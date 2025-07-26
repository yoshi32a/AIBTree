using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArcBT.Core;

namespace ArcBT.Parser
{
    public class BTParser
    {
        struct Token
        {
            public string Type;
            public string Value;
            public int Line;

            public Token(string type, string value, int line)
            {
                Type = type;
                Value = value;
                Line = line;
            }
        }

        List<Token> tokens;
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
            tokens = Tokenize(content);
            currentTokenIndex = 0;

            while (currentTokenIndex < tokens.Count)
            {
                var token = tokens[currentTokenIndex];
                if (token.Type == "KEYWORD" && token.Value == "tree")
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
            var tokens = new List<Token>();
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
                var commentIndex = -1;
                for (var i = 0; i < lineSpan.Length; i++)
                {
                    if (lineSpan[i] == '#')
                    {
                        commentIndex = i;
                        break;
                    }
                }

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

                    if (c == '{')
                    {
                        tokens.Add(new Token("LBRACE", "{", lineNum));
                        linePos++;
                    }
                    else if (c == '}')
                    {
                        tokens.Add(new Token("RBRACE", "}", lineNum));
                        linePos++;
                    }
                    else if (c == ':')
                    {
                        tokens.Add(new Token("COLON", ":", lineNum));
                        linePos++;
                    }
                    else if (c is '"' or '\'')
                    {
                        // 文字列リテラル
                        var quote = c;
                        linePos++;
                        var start = linePos;
                        while (linePos < lineSpan.Length && lineSpan[linePos] != quote)
                        {
                            linePos++;
                        }

                        var str = linePos > start ? lineSpan.Slice(start, linePos - start).ToString() : string.Empty;
                        tokens.Add(new Token("STRING", str, lineNum));
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
                        var word = wordSpan.ToString();

                        if (IsKeyword(word))
                        {
                            tokens.Add(new Token("KEYWORD", word, lineNum));
                        }
                        else
                        {
                            tokens.Add(new Token("IDENTIFIER", word, lineNum));
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

                        var number = lineSpan.Slice(start, linePos - start).ToString();
                        tokens.Add(new Token("NUMBER", number, lineNum));
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

        // よく使用されるキーワードを静的参照として保持
        static readonly HashSet<string> Keywords = new HashSet<string>
        {
            "tree", "Sequence", "Selector", "Action", "Condition", "Parallel"
        };

        bool IsKeyword(string word)
        {
            return Keywords.Contains(word);
        }

        BTNode ParseTree()
        {
            // "tree" keyword
            currentTokenIndex++;

            // tree name
            if (currentTokenIndex >= tokens.Count || tokens[currentTokenIndex].Type != "IDENTIFIER")
            {
                BTLogger.LogError(LogCategory.Parser, "Expected tree name");
                return null;
            }

            var treeName = tokens[currentTokenIndex].Value;
            currentTokenIndex++;

            // opening brace
            if (currentTokenIndex >= tokens.Count || tokens[currentTokenIndex].Type != "LBRACE")
            {
                BTLogger.LogError(LogCategory.Parser, "Expected '{' after tree name");
                return null;
            }

            currentTokenIndex++;

            // parse first node as root
            var rootNode = ParseNode();

            // closing brace
            if (currentTokenIndex < tokens.Count && tokens[currentTokenIndex].Type == "RBRACE")
            {
                currentTokenIndex++;
            }

            return rootNode;
        }

        BTNode ParseNode()
        {
            if (currentTokenIndex >= tokens.Count || tokens[currentTokenIndex].Type != "KEYWORD")
            {
                BTLogger.LogError(LogCategory.Parser, "Expected node type keyword");
                return null;
            }

            var nodeType = tokens[currentTokenIndex].Value;
            currentTokenIndex++;

            // script name (for Action/Condition) or node name (for Sequence/Selector)
            if (currentTokenIndex >= tokens.Count || tokens[currentTokenIndex].Type != "IDENTIFIER")
            {
                BTLogger.LogError(LogCategory.Parser, $"Expected script/node name after {nodeType}");
                return null;
            }

            var scriptOrNodeName = tokens[currentTokenIndex].Value;
            currentTokenIndex++;

            // opening brace
            if (currentTokenIndex >= tokens.Count || tokens[currentTokenIndex].Type != "LBRACE")
            {
                BTLogger.LogError(LogCategory.Parser, "Expected '{' after node name");
                return null;
            }

            currentTokenIndex++;

            // まずプロパティを収集
            var properties = new Dictionary<string, string>();
            var childNodes = new List<BTNode>();

            // parse properties and child nodes
            while (currentTokenIndex < tokens.Count && tokens[currentTokenIndex].Type != "RBRACE")
            {
                var token = tokens[currentTokenIndex];

                if (token.Type == "KEYWORD")
                {
                    // child node
                    var childNode = ParseNode();
                    if (childNode != null)
                    {
                        childNodes.Add(childNode);
                    }
                }
                else if (token.Type == "IDENTIFIER")
                {
                    // property
                    var propertyName = tokens[currentTokenIndex].Value;
                    currentTokenIndex++;

                    if (currentTokenIndex >= tokens.Count || tokens[currentTokenIndex].Type != "COLON")
                    {
                        BTLogger.LogError(LogCategory.Parser, "Expected ':' after property name");
                        return null;
                    }

                    currentTokenIndex++;

                    if (currentTokenIndex >= tokens.Count ||
                        (tokens[currentTokenIndex].Type != "STRING" && tokens[currentTokenIndex].Type != "NUMBER"))
                    {
                        BTLogger.LogError(LogCategory.Parser,
                            $"Expected property value, got: {(currentTokenIndex < tokens.Count ? tokens[currentTokenIndex].Type : "END_OF_TOKENS")}");
                        return null;
                    }

                    var propertyValue = tokens[currentTokenIndex].Value;
                    // Remove quotes from string value only
                    if (tokens[currentTokenIndex].Type == "STRING" &&
                        propertyValue.StartsWith("\"") && propertyValue.EndsWith("\""))
                    {
                        propertyValue = propertyValue.Substring(1, propertyValue.Length - 2);
                    }

                    currentTokenIndex++;

                    properties[propertyName] = propertyValue;
                }
                else
                {
                    currentTokenIndex++;
                }
            }

            // closing brace
            if (currentTokenIndex < tokens.Count && tokens[currentTokenIndex].Type == "RBRACE")
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

        void ParseProperty(BTNode node)
        {
            if (currentTokenIndex >= tokens.Count || tokens[currentTokenIndex].Type != "IDENTIFIER")
            {
                return;
            }

            var propertyName = tokens[currentTokenIndex].Value;
            currentTokenIndex++;

            // colon
            if (currentTokenIndex >= tokens.Count || tokens[currentTokenIndex].Type != "COLON")
            {
                BTLogger.LogError(LogCategory.Parser, $"Expected ':' after property name '{propertyName}'");
                return;
            }

            currentTokenIndex++;

            // value
            if (currentTokenIndex >= tokens.Count)
            {
                BTLogger.LogError(LogCategory.Parser, $"Expected value for property '{propertyName}'");
                return;
            }

            var valueToken = tokens[currentTokenIndex];
            var propertyValue = valueToken.Value;
            currentTokenIndex++;

            // set property
            node.SetProperty(propertyName, propertyValue);
        }

        BTNode CreateNode(string nodeType)
        {
            switch (nodeType)
            {
                case "Sequence":
                    return new BTSequenceNode();
                case "Selector":
                    return new BTSelectorNode();
                case "Parallel":
                    return new BTParallelNode();
                default:
                    BTLogger.LogError(LogCategory.Parser, $"Unknown composite node type: {nodeType}");
                    return null;
            }
        }

        BTNode CreateNodeFromScript(string scriptName, string nodeType, Dictionary<string, string> properties)
        {
            BTLogger.Log(LogLevel.Debug, LogCategory.Parser, $"🔧 CreateNodeFromScript: script='{scriptName}', type='{nodeType}'");
            BTNode node = null;

            if (nodeType == "Action")
            {
                BTLogger.Log(LogLevel.Debug, LogCategory.Parser, $"🔧 Creating ACTION node for script: {scriptName}");
                
                // 静的レジストリから作成（リフレクション不使用）
                node = BTStaticNodeRegistry.CreateAction(scriptName);
                
                if (node != null)
                {
                    BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created action for script '{scriptName}'");
                }
                else
                {
                    BTLogger.Log(LogLevel.Error, LogCategory.Parser, $"Unknown action script: {scriptName}. Please register the action in BTNodeRegistry.");
                    return null;
                }
            }
            else if (nodeType == "Condition")
            {
                BTLogger.Log(LogLevel.Debug, LogCategory.Parser, $"🔧 Creating CONDITION node for script: {scriptName}");
                
                // 静的レジストリから作成（リフレクション不使用）
                node = BTStaticNodeRegistry.CreateCondition(scriptName);
                
                if (node != null)
                {
                    BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created condition for script '{scriptName}'");
                }
                else
                {
                    BTLogger.Log(LogLevel.Error, LogCategory.Parser, $"Unknown condition script: {scriptName}. Please register the condition in BTNodeRegistry.");
                    return null;
                }
            }

            if (node != null)
            {
                // プロパティを設定
                foreach (var prop in properties)
                {
                    node.SetProperty(prop.Key, prop.Value);
                }
            }

            return node;
        }
    }
}
