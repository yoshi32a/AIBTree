using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using ArcBT.Core;
using ArcBT.Decorators;
using ArcBT.Logger;

namespace ArcBT.Parser
{
    public class BTParser
    {
        enum TokenType : byte // byteで十分、メモリ節約
        {
            Keyword,
            Identifier,
            String,
            Number,
            LeftBrace,
            RightBrace,
            Colon
        }

        // 最適化されたToken構造体（フィールド順序を最適化）
        readonly struct Token
        {
            public readonly string Value; // 参照型を先頭に配置
            public readonly ushort Line; // 65535行まで対応、intより小さい
            public readonly ushort Column; // カラム位置
            public readonly TokenType Type; // byteサイズ、最後に配置

            public Token(TokenType type, string value, int line, int column = 0)
            {
                Type = type;
                Value = value;
                Line = (ushort)line;
                Column = (ushort)column;
            }

            // 高速比較用メソッド（インライン化）
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsKeyword(string keyword) => Type == TokenType.Keyword && Value == keyword;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsType(TokenType type) => Type == type;

            // デバッグ用のToString
            public override string ToString() => $"{Type}:{Value}@{Line}:{Column}";
        }

        // よく使用されるキーワードを静的参照として保持（FrozenSetで高速化）
        static readonly HashSet<string> keywords = new()
        {
            "tree", "Sequence", "Selector", "Action", "Condition", "Parallel",
            "Inverter", "Repeat", "Retry", "Timeout"
        };

        // ノードタイプの高速マッピング
        static readonly Dictionary<string, Func<BTNode>> compositeNodeFactories = new()
        {
            ["Sequence"] = () => new BTSequenceNode(),
            ["Selector"] = () => new BTSelectorNode(),
            ["Parallel"] = () => new BTParallelNode()
        };

        // デコレーターノードの高速マッピング
        static readonly Dictionary<string, Func<BTNode>> decoratorNodeFactories = new()
        {
            ["Inverter"] = () => new InverterDecorator(),
            ["Repeat"] = () => new RepeatDecorator(),
            ["Retry"] = () => new RetryDecorator(),
            ["Timeout"] = () => new TimeoutDecorator()
        };

        // よく使われる文字列の事前割り当て（GC負荷軽減）
        const string TREE_KEYWORD = "tree";
        const string LEFT_BRACE = "{";
        const string RIGHT_BRACE = "}";
        const string COLON = ":";

        // 再帰深度制限（StackOverflowException防止）
        const int MaxNestingDepth = 128;

        Token[] tokens; // Listより配列の方が高速アクセス
        int currentTokenIndex;
        int currentDepth;
        readonly List<string> parseErrors = new();

        public BTNode ParseFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                BTLogger.LogSystemError("Parser", $"BT file not found: {filePath}");
                return null;
            }

            var content = File.ReadAllText(filePath);
            return ParseContent(content);
        }

        public BTNode ParseContent(string content)
        {
            var tokenList = Tokenize(content);
            // 配列への変換（Unity互換性のためToArrayを使用）
            tokens = tokenList.ToArray();
            currentTokenIndex = 0;
            currentDepth = 0;
            parseErrors.Clear();

            while (currentTokenIndex < tokens.Length)
            {
                var token = tokens[currentTokenIndex];
                if (token.IsKeyword(TREE_KEYWORD))
                {
                    var result = ParseTree();

                    // 収集したエラーをまとめて報告
                    foreach (var error in parseErrors)
                    {
                        BTLogger.LogSystemError("Parser", error);
                    }

                    return result;
                }

                currentTokenIndex++;
            }

            BTLogger.LogSystemError("Parser", "No tree definition found");
            return null;
        }

        List<Token> Tokenize(string content)
        {
            var tokens = new List<Token>(content.Length / 10); // 初期容量を推定して割り当て
            var span = content.AsSpan();
            var lineNum = 0;
            var position = 0;

            while (position < span.Length)
            {
                // 現在の行を取得
                var lineStart = position;
                while (position < span.Length && span[position] != '\n' && span[position] != '\r')
                    position++;

                var lineSpan = span.Slice(lineStart, position - lineStart);

                // 改行文字をスキップ
                if (position < span.Length)
                {
                    if (span[position] == '\r' && position + 1 < span.Length && span[position + 1] == '\n')
                        position += 2; // Windows形式の改行 (\r\n)
                    else
                        position++; // Unix形式の改行 (\n) または Mac形式の改行 (\r)
                }

                // 空行やコメント行をスキップ
                lineSpan = lineSpan.Trim();
                if (lineSpan.IsEmpty || lineSpan[0] == '#')
                {
                    lineNum++;
                    continue;
                }

                // コメントがある場合、コメント前までの部分を処理
                var commentIndex = lineSpan.IndexOf('#');
                if (commentIndex >= 0)
                    lineSpan = lineSpan.Slice(0, commentIndex).Trim();

                // 行内の各トークンを処理
                var linePos = 0;
                while (linePos < lineSpan.Length)
                {
                    var c = lineSpan[linePos];

                    if (char.IsWhiteSpace(c))
                    {
                        linePos++;
                        continue;
                    }

                    // switch式で高速化
                    switch (c)
                    {
                        case '{':
                            tokens.Add(new Token(TokenType.LeftBrace, LEFT_BRACE, lineNum, linePos));
                            linePos++;
                            continue;
                        case '}':
                            tokens.Add(new Token(TokenType.RightBrace, RIGHT_BRACE, lineNum, linePos));
                            linePos++;
                            continue;
                        case ':':
                            tokens.Add(new Token(TokenType.Colon, COLON, lineNum, linePos));
                            linePos++;
                            continue;
                    }

                    if (c is '"' or '\'')
                    {
                        // 文字列リテラル（エスケープシーケンス対応）
                        var quote = c;
                        var tokenColumn = linePos;
                        linePos++;
                        var start = linePos;
                        var hasEscape = false;

                        // エスケープの有無を先に確認
                        var scanPos = linePos;
                        while (scanPos < lineSpan.Length && lineSpan[scanPos] != quote)
                        {
                            if (lineSpan[scanPos] == '\\' && scanPos + 1 < lineSpan.Length)
                            {
                                hasEscape = true;
                                scanPos += 2;
                            }
                            else
                            {
                                scanPos++;
                            }
                        }

                        string str;
                        if (hasEscape)
                        {
                            // エスケープシーケンスを処理
                            var sb = new StringBuilder();
                            while (linePos < lineSpan.Length && lineSpan[linePos] != quote)
                            {
                                if (lineSpan[linePos] == '\\' && linePos + 1 < lineSpan.Length)
                                {
                                    linePos++;
                                    sb.Append(lineSpan[linePos] switch
                                    {
                                        'n' => '\n',
                                        't' => '\t',
                                        '\\' => '\\',
                                        '"' => '"',
                                        '\'' => '\'',
                                        _ => lineSpan[linePos]
                                    });
                                    linePos++;
                                }
                                else
                                {
                                    sb.Append(lineSpan[linePos]);
                                    linePos++;
                                }
                            }
                            str = sb.ToString();
                        }
                        else
                        {
                            // エスケープなしの高速パス
                            while (linePos < lineSpan.Length && lineSpan[linePos] != quote)
                            {
                                linePos++;
                            }
                            str = linePos > start ? new string(lineSpan.Slice(start, linePos - start)) : string.Empty;
                        }

                        tokens.Add(new Token(TokenType.String, str, lineNum, tokenColumn));
                        if (linePos < lineSpan.Length)
                        {
                            linePos++; // 終端のクォートをスキップ
                        }
                    }
                    else if (char.IsLetter(c) || c == '_')
                    {
                        // 識別子またはキーワード
                        var start = linePos;
                        while (linePos < lineSpan.Length &&
                               (char.IsLetterOrDigit(lineSpan[linePos]) || lineSpan[linePos] == '_'))
                        {
                            linePos++;
                        }

                        var wordSpan = lineSpan.Slice(start, linePos - start);

                        // キーワードチェックを先に行い、文字列生成を最小化
                        if (IsKeywordSpan(wordSpan))
                        {
                            var word = new string(wordSpan);
                            tokens.Add(new Token(TokenType.Keyword, word, lineNum, start));
                        }
                        else
                        {
                            var word = new string(wordSpan);
                            tokens.Add(new Token(TokenType.Identifier, word, lineNum, start));
                        }
                    }
                    else if (char.IsDigit(c) || c == '.')
                    {
                        // 数値
                        var start = linePos;
                        var hasDot = false;
                        while (linePos < lineSpan.Length &&
                               (char.IsDigit(lineSpan[linePos]) ||
                                (lineSpan[linePos] == '.' && !hasDot)))
                        {
                            if (lineSpan[linePos] == '.')
                            {
                                hasDot = true;
                            }

                            linePos++;
                        }

                        var number = new string(lineSpan.Slice(start, linePos - start));
                        tokens.Add(new Token(TokenType.Number, number, lineNum, start));
                    }
                    else
                    {
                        linePos++;
                    }
                }

                lineNum++;
            }

            return tokens;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool IsKeyword(string word)
        {
            return keywords.Contains(word);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool IsKeywordSpan(ReadOnlySpan<char> span)
        {
            // 長さベースの早期リターン
            return span.Length switch
            {
                4 => span.SequenceEqual("tree".AsSpan()),
                5 => span.SequenceEqual("Retry".AsSpan()),
                6 => span.SequenceEqual("Action".AsSpan()) || span.SequenceEqual("Repeat".AsSpan()),
                7 => span.SequenceEqual("Timeout".AsSpan()),
                8 => span.SequenceEqual("Sequence".AsSpan()) || span.SequenceEqual("Selector".AsSpan()) || span.SequenceEqual("Parallel".AsSpan()) || span.SequenceEqual("Inverter".AsSpan()),
                9 => span.SequenceEqual("Condition".AsSpan()),
                _ => false
            };
        }

        BTNode ParseTree()
        {
            // "tree" keyword
            currentTokenIndex++;

            // tree name
            if (currentTokenIndex >= tokens.Length || tokens[currentTokenIndex].Type != TokenType.Identifier)
            {
                BTLogger.LogSystemError("Parser", "Expected tree name");
                return null;
            }

            var treeName = tokens[currentTokenIndex++].Value; // インクリメントを同時に実行

            BTLogger.LogSystem("Parser", $"📋 Parsing tree: {treeName}");

            // opening brace
            if (currentTokenIndex >= tokens.Length || tokens[currentTokenIndex].Type != TokenType.LeftBrace)
            {
                BTLogger.LogSystemError("Parser", "Expected '{' after tree name");
                return null;
            }

            currentTokenIndex++;

            // parse first node as root
            var rootNode = ParseNode();

            if (rootNode != null)
            {
                // ツリー名をルートノードのプロパティとして設定
                rootNode.SetProperty("treeName", treeName);
                BTLogger.LogSystem("Parser", $"✅ Successfully parsed tree '{treeName}' with root node: {rootNode.Name}");
            }

            // closing brace
            if (currentTokenIndex < tokens.Length && tokens[currentTokenIndex].Type == TokenType.RightBrace)
            {
                currentTokenIndex++;
            }

            return rootNode;
        }

        BTNode ParseNode()
        {
            // 再帰深度チェック（Issue #3: StackOverflowException防止）
            if (currentDepth >= MaxNestingDepth)
            {
                var depthToken = currentTokenIndex < tokens.Length ? tokens[currentTokenIndex] : default;
                BTLogger.LogSystemError("Parser", $"Maximum nesting depth ({MaxNestingDepth}) exceeded at line {depthToken.Line + 1}, column {depthToken.Column + 1}");
                return null;
            }

            currentDepth++;
            try
            {
                return ParseNodeInternal();
            }
            finally
            {
                currentDepth--;
            }
        }

        BTNode ParseNodeInternal()
        {
            if (currentTokenIndex >= tokens.Length || tokens[currentTokenIndex].Type != TokenType.Keyword)
            {
                var errorToken = currentTokenIndex < tokens.Length ? tokens[currentTokenIndex] : default;
                BTLogger.LogSystemError("Parser", $"Expected node type keyword at line {errorToken.Line + 1}, column {errorToken.Column + 1}");
                return null;
            }

            var nodeTypeToken = tokens[currentTokenIndex++];
            var nodeType = nodeTypeToken.Value;

            // Issue #14: 未知のノードタイプを解析時に検出
            if (nodeType is not ("Action" or "Condition") &&
                !decoratorNodeFactories.ContainsKey(nodeType) &&
                !compositeNodeFactories.ContainsKey(nodeType))
            {
                var suggestion = FindSimilarNodeType(nodeType);
                var warningMsg = $"Unknown node type '{nodeType}' at line {nodeTypeToken.Line + 1}, column {nodeTypeToken.Column + 1}";
                if (suggestion != null)
                {
                    warningMsg += $". Did you mean '{suggestion}'?";
                }
                parseErrors.Add(warningMsg);
                // 警告のみ、パースは継続
            }

            // script name (for Action/Condition) or node name (for Sequence/Selector)
            if (currentTokenIndex >= tokens.Length || tokens[currentTokenIndex].Type != TokenType.Identifier)
            {
                var errorToken = currentTokenIndex < tokens.Length ? tokens[currentTokenIndex] : default;
                BTLogger.LogSystemError("Parser", $"Expected script/node name after {nodeType} at line {errorToken.Line + 1}, column {errorToken.Column + 1}");
                return null;
            }

            var scriptOrNodeName = tokens[currentTokenIndex++].Value; // インクリメントを同時に実行

            // opening brace
            if (currentTokenIndex >= tokens.Length || tokens[currentTokenIndex].Type != TokenType.LeftBrace)
            {
                var errorToken = currentTokenIndex < tokens.Length ? tokens[currentTokenIndex] : default;
                BTLogger.LogSystemError("Parser", $"Expected '{{' after node name at line {errorToken.Line + 1}, column {errorToken.Column + 1}");
                return null;
            }

            currentTokenIndex++;

            // まずプロパティを収集（初期容量設定で高速化）
            var properties = new Dictionary<string, string>(4);
            var childNodes = new List<BTNode>(8);

            // parse properties and child nodes
            while (currentTokenIndex < tokens.Length && tokens[currentTokenIndex].Type != TokenType.RightBrace)
            {
                var token = tokens[currentTokenIndex];

                if (token.Type == TokenType.Keyword)
                {
                    // child node
                    var childNode = ParseNode();
                    if (childNode != null)
                    {
                        childNodes.Add(childNode);
                    }
                }
                else if (token.Type == TokenType.Identifier)
                {
                    // property（Issue #16: エラー回復付き）
                    if (TryParseProperty(out var propertyName, out var propertyValue))
                    {
                        properties[propertyName] = propertyValue;
                    }
                    else
                    {
                        // パース失敗時、次の行または '}' までスキップして継続
                        var failedLine = currentTokenIndex < tokens.Length ? tokens[currentTokenIndex].Line : -1;
                        while (currentTokenIndex < tokens.Length &&
                               tokens[currentTokenIndex].Type != TokenType.RightBrace &&
                               tokens[currentTokenIndex].Line == failedLine)
                        {
                            currentTokenIndex++;
                        }
                    }
                }
                else
                {
                    currentTokenIndex++;
                }
            }

            // closing brace
            if (currentTokenIndex < tokens.Length && tokens[currentTokenIndex].Type == TokenType.RightBrace)
            {
                currentTokenIndex++;
            }

            // 新フォーマット: Action/Condition は直接スクリプト名、Sequence/Selector は従来通り
            BTNode node = null;
            BTLogger.LogSystem("Parser", $"🔍 Creating node: {nodeType} {scriptOrNodeName}");
            BTLogger.LogSystem("Parser", $"🔍 Properties: {string.Join(", ", properties.Select(p => $"{p.Key}={p.Value}"))}");

            if (nodeType is "Action" or "Condition")
            {
                BTLogger.LogSystem("Parser", $"🚀 Creating {nodeType} with script '{scriptOrNodeName}'");
                node = CreateNodeFromScript(scriptOrNodeName, nodeType, properties);
            }
            else if (decoratorNodeFactories.ContainsKey(nodeType))
            {
                BTLogger.LogSystem("Parser", $"🔧 Creating decorator node: {nodeType}");
                node = CreateDecoratorNode(nodeType);
                if (node != null)
                {
                    foreach (var prop in properties)
                    {
                        node.SetProperty(prop.Key, prop.Value);
                    }
                }
            }
            else
            {
                BTLogger.LogSystem("Parser", $"🔧 Creating composite node: {nodeType}");
                node = CreateNode(nodeType);
                if (node != null)
                {
                    foreach (var prop in properties)
                    {
                        node.SetProperty(prop.Key, prop.Value);
                    }
                }
            }

            if (node == null)
            {
                BTLogger.LogSystemError("Parser", $"Failed to create node of type: {nodeType}");
                return null;
            }

            // Set name: for Action/Condition use script name, for others use node name
            if (nodeType is "Action" or "Condition")
            {
                node.Name = $"{nodeType}:{scriptOrNodeName}";
            }
            else
            {
                node.Name = scriptOrNodeName;
            }

            // 子ノードを追加
            foreach (var childNode in childNodes)
            {
                node.AddChild(childNode);
            }

            return node;
        }

        bool TryParseProperty(out string propertyName, out string propertyValue)
        {
            propertyName = null;
            propertyValue = null;

            if (currentTokenIndex >= tokens.Length || tokens[currentTokenIndex].Type != TokenType.Identifier)
            {
                return false;
            }

            var nameToken = tokens[currentTokenIndex++];
            propertyName = nameToken.Value;

            if (currentTokenIndex >= tokens.Length || tokens[currentTokenIndex].Type != TokenType.Colon)
            {
                parseErrors.Add($"Expected ':' after property name '{propertyName}' at line {nameToken.Line + 1}, column {nameToken.Column + 1}");
                return false;
            }

            currentTokenIndex++;

            if (currentTokenIndex >= tokens.Length ||
                (tokens[currentTokenIndex].Type != TokenType.String && tokens[currentTokenIndex].Type != TokenType.Number))
            {
                var errorToken = currentTokenIndex < tokens.Length ? tokens[currentTokenIndex] : default;
                var gotType = currentTokenIndex < tokens.Length ? errorToken.Type.ToString() : "END_OF_TOKENS";
                parseErrors.Add($"Expected property value for '{propertyName}', got: {gotType} at line {errorToken.Line + 1}, column {errorToken.Column + 1}");
                return false;
            }

            var currentToken = tokens[currentTokenIndex++]; // 値の取得とインクリメントを同時に実行
            propertyValue = currentToken.Value;

            // Remove quotes from string value only
            if (currentToken.Type == TokenType.String &&
                propertyValue.StartsWith("\"") && propertyValue.EndsWith("\""))
            {
                propertyValue = propertyValue.Substring(1, propertyValue.Length - 2);
            }

            return true;
        }

        BTNode CreateNode(string nodeType)
        {
            if (compositeNodeFactories.TryGetValue(nodeType, out var factory))
                return factory();

            BTLogger.LogSystemError("Parser", $"Unknown composite node type: {nodeType}");
            return null;
        }

        BTNode CreateDecoratorNode(string nodeType)
        {
            if (decoratorNodeFactories.TryGetValue(nodeType, out var factory))
                return factory();
            
            BTLogger.LogSystemError("Parser", $"Unknown decorator node type: {nodeType}");
            return null;
        }

        BTNode CreateNodeFromScript(string scriptName, string nodeType, Dictionary<string, string> properties)
        {
            BTLogger.LogSystem("Parser", $"🔧 CreateNodeFromScript: script='{scriptName}', type='{nodeType}'");

            // 統一レジストリから作成（全ノードタイプ対応）
            var node = BTStaticNodeRegistry.CreateNode(nodeType, scriptName);

            if (node != null)
            {
                BTLogger.LogSystem("Parser", $"✅ Created {nodeType.ToLower()} for script '{scriptName}'");
                
                // プロパティを設定
                foreach (var prop in properties)
                {
                    node.SetProperty(prop.Key, prop.Value);
                }
            }
            else
            {
                BTLogger.LogSystemError("Parser",
                    $"Unknown {nodeType.ToLower()} script: {scriptName}. Please register the node in BTStaticNodeRegistry or use source generator.");
            }

            return node;
        }

        /// <summary>
        /// 未知のノードタイプに対して類似の既知ノード名を提案する
        /// </summary>
        string FindSimilarNodeType(string unknown)
        {
            string bestMatch = null;
            int bestDistance = int.MaxValue;
            var threshold = Math.Max(2, unknown.Length / 2);

            // 組み込みノードタイプを検索
            var knownTypes = new List<string>(compositeNodeFactories.Keys);
            knownTypes.AddRange(decoratorNodeFactories.Keys);
            knownTypes.Add("Action");
            knownTypes.Add("Condition");

            // BTStaticNodeRegistryの登録ノードも検索
            foreach (var name in BTStaticNodeRegistry.GetRegisteredNodeNames())
            {
                knownTypes.Add(name);
            }

            foreach (var known in knownTypes)
            {
                // プレフィックスマッチ
                if (known.StartsWith(unknown, StringComparison.OrdinalIgnoreCase) ||
                    unknown.StartsWith(known, StringComparison.OrdinalIgnoreCase))
                {
                    return known;
                }

                // 簡易Levenshtein距離
                var dist = ComputeLevenshteinDistance(unknown.ToLowerInvariant(), known.ToLowerInvariant());
                if (dist < bestDistance && dist <= threshold)
                {
                    bestDistance = dist;
                    bestMatch = known;
                }
            }

            return bestMatch;
        }

        /// <summary>
        /// 簡易Levenshtein距離の計算
        /// </summary>
        static int ComputeLevenshteinDistance(string a, string b)
        {
            if (a.Length == 0) return b.Length;
            if (b.Length == 0) return a.Length;

            var costs = new int[b.Length + 1];
            for (int j = 0; j <= b.Length; j++)
            {
                costs[j] = j;
            }

            for (int i = 1; i <= a.Length; i++)
            {
                int prev = costs[0];
                costs[0] = i;

                for (int j = 1; j <= b.Length; j++)
                {
                    int temp = costs[j];
                    costs[j] = a[i - 1] == b[j - 1]
                        ? prev
                        : Math.Min(Math.Min(costs[j] + 1, costs[j - 1] + 1), prev + 1);
                    prev = temp;
                }
            }

            return costs[b.Length];
        }
    }
}
