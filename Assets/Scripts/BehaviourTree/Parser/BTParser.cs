using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BehaviourTree.Core;
using BehaviourTree.Nodes;
using UnityEngine;

namespace BehaviourTree.Parser
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
                Debug.LogError($"BT file not found: {filePath}");
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
            
            Debug.LogError("No tree definition found");
            return null;
        }

        List<Token> Tokenize(string content)
        {
            var tokens = new List<Token>();
            var lines = content.Split('\n');
            
            for (var lineNum = 0; lineNum < lines.Length; lineNum++)
            {
                var line = lines[lineNum].Trim();
                
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                {
                    continue;
                }

                var position = 0;
                while (position < line.Length)
                {
                    var c = line[position];
                    
                    if (char.IsWhiteSpace(c))
                    {
                        position++;
                        continue;
                    }
                    
                    if (c == '{')
                    {
                        tokens.Add(new Token("LBRACE", "{", lineNum));
                        position++;
                    }
                    else if (c == '}')
                    {
                        tokens.Add(new Token("RBRACE", "}", lineNum));
                        position++;
                    }
                    else if (c == ':')
                    {
                        tokens.Add(new Token("COLON", ":", lineNum));
                        position++;
                    }
                    else if (c == '"' || c == '\'')
                    {
                        // 文字列リテラル
                        var quote = c;
                        position++;
                        var start = position;
                        while (position < line.Length && line[position] != quote)
                        {
                            position++;
                        }
                        var str = line.Substring(start, position - start);
                        tokens.Add(new Token("STRING", str, lineNum));
                        if (position < line.Length)
                        {
                            position++; // 終端のクォートをスキップ
                        }
                    }
                    else if (char.IsLetter(c) || c == '_')
                    {
                        // 識別子またはキーワード
                        var start = position;
                        while (position < line.Length && (char.IsLetterOrDigit(line[position]) || line[position] == '_'))
                        {
                            position++;
                        }
                        var word = line.Substring(start, position - start);
                        
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
                        var start = position;
                        var hasDot = false;
                        while (position < line.Length && (char.IsDigit(line[position]) || 
                               (line[position] == '.' && !hasDot)))
                        {
                            if (line[position] == '.')
                            {
                                hasDot = true;
                            }

                            position++;
                        }
                        var number = line.Substring(start, position - start);
                        tokens.Add(new Token("NUMBER", number, lineNum));
                    }
                    else
                    {
                        position++;
                    }
                }
            }
            
            return tokens;
        }

        bool IsKeyword(string word)
        {
            return word == "tree" || word == "Sequence" || word == "Selector" || 
                   word == "Action" || word == "Condition" || word == "Parallel";
        }

        BTNode ParseTree()
        {
            // "tree" keyword
            currentTokenIndex++;
            
            // tree name
            if (currentTokenIndex >= tokens.Count || tokens[currentTokenIndex].Type != "IDENTIFIER")
            {
                Debug.LogError("Expected tree name");
                return null;
            }
            
            var treeName = tokens[currentTokenIndex].Value;
            currentTokenIndex++;
            
            // opening brace
            if (currentTokenIndex >= tokens.Count || tokens[currentTokenIndex].Type != "LBRACE")
            {
                Debug.LogError("Expected '{' after tree name");
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
                Debug.LogError("Expected node type keyword");
                return null;
            }
            
            var nodeType = tokens[currentTokenIndex].Value;
            currentTokenIndex++;
            
            // script name (for Action/Condition) or node name (for Sequence/Selector)
            if (currentTokenIndex >= tokens.Count || tokens[currentTokenIndex].Type != "IDENTIFIER")
            {
                Debug.LogError($"Expected script/node name after {nodeType}");
                return null;
            }
            
            var scriptOrNodeName = tokens[currentTokenIndex].Value;
            currentTokenIndex++;
            
            // opening brace
            if (currentTokenIndex >= tokens.Count || tokens[currentTokenIndex].Type != "LBRACE")
            {
                Debug.LogError("Expected '{' after node name");
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
                        Debug.LogError("Expected ':' after property name");
                        return null;
                    }
                    currentTokenIndex++;
                    
                    if (currentTokenIndex >= tokens.Count || 
                        (tokens[currentTokenIndex].Type != "STRING" && tokens[currentTokenIndex].Type != "NUMBER"))
                    {
                        Debug.LogError($"Expected property value, got: {(currentTokenIndex < tokens.Count ? tokens[currentTokenIndex].Type : "END_OF_TOKENS")}");
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
            Debug.Log($"🔍 Creating node: {nodeType} {scriptOrNodeName}");
            Debug.Log($"🔍 Properties: {string.Join(", ", properties.Select(p => $"{p.Key}={p.Value}"))}");
            
            if (nodeType == "Action" || nodeType == "Condition")
            {
                Debug.Log($"🚀 Creating {nodeType} with script '{scriptOrNodeName}'");
                node = CreateNodeFromScript(scriptOrNodeName, nodeType, properties);
            }
            else
            {
                Debug.Log($"🔧 Creating composite node: {nodeType}");
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
                Debug.LogError($"Failed to create node of type: {nodeType}");
                return null;
            }
            
            // Set name: for Action/Condition use script name, for others use node name
            if (nodeType == "Action" || nodeType == "Condition")
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
                Debug.LogError($"Expected ':' after property name '{propertyName}'");
                return;
            }
            currentTokenIndex++;
            
            // value
            if (currentTokenIndex >= tokens.Count)
            {
                Debug.LogError($"Expected value for property '{propertyName}'");
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
                    return new SequenceNode();
                case "Selector":
                    return new SelectorNode();
                case "Parallel":
                    // TODO: Implement ParallelNode if needed
                    Debug.LogWarning("Parallel node not implemented yet");
                    return null;
                default:
                    Debug.LogError($"Unknown composite node type: {nodeType}");
                    return null;
            }
        }

        BTNode CreateNodeFromScript(string scriptName, string nodeType, Dictionary<string, string> properties)
        {
            Debug.Log($"🔧 CreateNodeFromScript: script='{scriptName}', type='{nodeType}'");
            BTNode node = null;
            
            if (nodeType == "Action")
            {
                Debug.Log($"🔧 Creating ACTION node for script: {scriptName}");
                // Actionスクリプトから実際のクラスを作成
                switch (scriptName)
                {
                    case "MoveToPosition":
                        node = new MoveToPositionAction();
                        Debug.Log($"✅ Created MoveToPositionAction");
                        break;
                    case "Wait":
                        node = new WaitAction();
                        Debug.Log($"✅ Created WaitAction");
                        break;
                    case "AttackEnemy":
                        node = new AttackEnemyAction();
                        Debug.Log($"✅ Created AttackEnemyAction");
                        break;
                    case "Attack":
                    case "UseItem":
                    case "FleeToSafety":
                    case "Interact":
                    case "RandomWander":
                        // 未実装のアクションは CustomActionNode を使用
                        node = new CustomActionNode();
                        Debug.Log($"⚠️ Using CustomActionNode for {scriptName}");
                        break;
                    default:
                        Debug.LogWarning($"Unknown action script: {scriptName}, using CustomActionNode");
                        node = new CustomActionNode();
                        break;
                }
            }
            else if (nodeType == "Condition")
            {
                Debug.Log($"🔧 Creating CONDITION node for script: {scriptName}");
                // Conditionスクリプトから実際のクラスを作成
                switch (scriptName)
                {
                    case "HealthCheck":
                        node = new HealthCheckCondition();
                        Debug.Log($"✅ Created HealthCheckCondition");
                        break;
                    case "EnemyCheck":
                        node = new EnemyCheckCondition();
                        Debug.Log($"✅ Created EnemyCheckCondition");
                        break;
                    case "HasItem":
                        node = new HasItemCondition();
                        Debug.Log($"✅ Created HasItemCondition");
                        break;
                    case "IsInitialized":
                    case "HasTarget":
                    case "HasMana":
                    case "EnemyHealthCheck":
                        // 未実装の条件は CustomConditionNode を使用
                        node = new CustomConditionNode();
                        Debug.Log($"⚠️ Using CustomConditionNode for {scriptName}");
                        break;
                    default:
                        Debug.LogWarning($"Unknown condition script: {scriptName}, using CustomConditionNode");
                        node = new CustomConditionNode();
                        break;
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
