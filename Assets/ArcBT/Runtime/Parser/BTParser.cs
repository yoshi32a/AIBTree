using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
            public readonly TokenType Type; // byteサイズ、最後に配置

            public Token(TokenType type, string value, int line)
            {
                Type = type;
                Value = value;
                Line = (ushort)line;
            }

            // 高速比較用メソッド（インライン化）
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsKeyword(string keyword) => Type == TokenType.Keyword && Value == keyword;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsType(TokenType type) => Type == type;

            // デバッグ用のToString
            public override string ToString() => $"{Type}:{Value}@{Line}";
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


        Token[] tokens; // Listより配列の方が高速アクセス
        int currentTokenIndex;

        public BTNode ParseFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                BTLogger.LogError(LogCategory.Parser, $"BT file not found: {filePath}");
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

            while (currentTokenIndex < tokens.Length)
            {
                var token = tokens[currentTokenIndex];
                if (token.IsKeyword(TREE_KEYWORD))
                {
                    return ParseTree();
                }

                currentTokenIndex++;
            }

            BTLogger.LogError(LogCategory.Parser, "No tree definition found");
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
                            tokens.Add(new Token(TokenType.LeftBrace, LEFT_BRACE, lineNum));
                            linePos++;
                            continue;
                        case '}':
                            tokens.Add(new Token(TokenType.RightBrace, RIGHT_BRACE, lineNum));
                            linePos++;
                            continue;
                        case ':':
                            tokens.Add(new Token(TokenType.Colon, COLON, lineNum));
                            linePos++;
                            continue;
                    }

                    if (c is '"' or '\'')
                    {
                        // 文字列リテラル
                        var quote = c;
                        linePos++;
                        var start = linePos;
                        while (linePos < lineSpan.Length && lineSpan[linePos] != quote)
                        {
                            linePos++;
                        }

                        var str = linePos > start ? new string(lineSpan.Slice(start, linePos - start)) : string.Empty;
                        tokens.Add(new Token(TokenType.String, str, lineNum));
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
                            tokens.Add(new Token(TokenType.Keyword, word, lineNum));
                        }
                        else
                        {
                            var word = new string(wordSpan);
                            tokens.Add(new Token(TokenType.Identifier, word, lineNum));
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
                        tokens.Add(new Token(TokenType.Number, number, lineNum));
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
                BTLogger.LogError(LogCategory.Parser, "Expected tree name");
                return null;
            }

            var treeName = tokens[currentTokenIndex++].Value; // インクリメントを同時に実行

            BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"📋 Parsing tree: {treeName}");

            // opening brace
            if (currentTokenIndex >= tokens.Length || tokens[currentTokenIndex].Type != TokenType.LeftBrace)
            {
                BTLogger.LogError(LogCategory.Parser, "Expected '{' after tree name");
                return null;
            }

            currentTokenIndex++;

            // parse first node as root
            var rootNode = ParseNode();

            if (rootNode != null)
            {
                // ツリー名をルートノードのプロパティとして設定
                rootNode.SetProperty("treeName", treeName);
                BTLogger.Log(LogLevel.Debug, LogCategory.Parser, $"✅ Successfully parsed tree '{treeName}' with root node: {rootNode.Name}");
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
            if (currentTokenIndex >= tokens.Length || tokens[currentTokenIndex].Type != TokenType.Keyword)
            {
                BTLogger.LogError(LogCategory.Parser, "Expected node type keyword");
                return null;
            }

            var nodeType = tokens[currentTokenIndex++].Value; // インクリメントを同時に実行

            // script name (for Action/Condition) or node name (for Sequence/Selector)
            if (currentTokenIndex >= tokens.Length || tokens[currentTokenIndex].Type != TokenType.Identifier)
            {
                BTLogger.LogError(LogCategory.Parser, $"Expected script/node name after {nodeType}");
                return null;
            }

            var scriptOrNodeName = tokens[currentTokenIndex++].Value; // インクリメントを同時に実行

            // opening brace
            if (currentTokenIndex >= tokens.Length || tokens[currentTokenIndex].Type != TokenType.LeftBrace)
            {
                BTLogger.LogError(LogCategory.Parser, "Expected '{' after node name");
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
                    // property
                    if (TryParseProperty(out var propertyName, out var propertyValue))
                    {
                        properties[propertyName] = propertyValue;
                    }
                    else
                    {
                        return null;
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
            BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"🔍 Creating node: {nodeType} {scriptOrNodeName}");
            BTLogger.Log(LogLevel.Debug, LogCategory.Parser, $"🔍 Properties: {string.Join(", ", properties.Select(p => $"{p.Key}={p.Value}"))}");

            if (nodeType is "Action" or "Condition")
            {
                BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"🚀 Creating {nodeType} with script '{scriptOrNodeName}'");
                node = CreateNodeFromScript(scriptOrNodeName, nodeType, properties);
            }
            else if (decoratorNodeFactories.ContainsKey(nodeType))
            {
                BTLogger.Log(LogLevel.Debug, LogCategory.Parser, $"🔧 Creating decorator node: {nodeType}");
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
                BTLogger.Log(LogLevel.Debug, LogCategory.Parser, $"🔧 Creating composite node: {nodeType}");
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
                BTLogger.LogError(LogCategory.Parser, $"Failed to create node of type: {nodeType}");
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

            propertyName = tokens[currentTokenIndex++].Value; // インクリメントを同時に実行

            if (currentTokenIndex >= tokens.Length || tokens[currentTokenIndex].Type != TokenType.Colon)
            {
                BTLogger.LogError(LogCategory.Parser, "Expected ':' after property name");
                return false;
            }

            currentTokenIndex++;

            if (currentTokenIndex >= tokens.Length ||
                (tokens[currentTokenIndex].Type != TokenType.String && tokens[currentTokenIndex].Type != TokenType.Number))
            {
                BTLogger.LogError(LogCategory.Parser, $"Expected property value, got: {(currentTokenIndex < tokens.Length ? tokens[currentTokenIndex].Type.ToString() : "END_OF_TOKENS")}");
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

            BTLogger.LogError(LogCategory.Parser, $"Unknown composite node type: {nodeType}");
            return null;
        }

        BTNode CreateDecoratorNode(string nodeType)
        {
            if (decoratorNodeFactories.TryGetValue(nodeType, out var factory))
                return factory();
            
            BTLogger.LogError(LogCategory.Parser, $"Unknown decorator node type: {nodeType}");
            return null;
        }

        BTNode CreateNodeFromScript(string scriptName, string nodeType, Dictionary<string, string> properties)
        {
            BTLogger.Log(LogLevel.Debug, LogCategory.Parser, $"🔧 CreateNodeFromScript: script='{scriptName}', type='{nodeType}'");

            // 統一レジストリから作成（全ノードタイプ対応）
            var node = BTStaticNodeRegistry.CreateNode(nodeType, scriptName);

            if (node != null)
            {
                BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created {nodeType.ToLower()} for script '{scriptName}'");
                
                // プロパティを設定
                foreach (var prop in properties)
                {
                    node.SetProperty(prop.Key, prop.Value);
                }
            }
            else
            {
                BTLogger.Log(LogLevel.Error, LogCategory.Parser,
                    $"Unknown {nodeType.ToLower()} script: {scriptName}. Please register the node in BTStaticNodeRegistry or use source generator.");
            }

            return node;
        }
    }
}
