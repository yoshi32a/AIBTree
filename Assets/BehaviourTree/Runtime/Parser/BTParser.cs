using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BehaviourTree.Core;
using BehaviourTree.Nodes;
using BehaviourTree.Actions;
using BehaviourTree.Conditions;
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
            var lines = content.Split('\n');

            for (var lineNum = 0; lineNum < lines.Length; lineNum++)
            {
                var line = lines[lineNum].Trim();

                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                {
                    continue;
                }

                if (line.Contains("#"))
                {
                    line = line[..line.IndexOf("#", StringComparison.Ordinal)];
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

            if (nodeType == "Action" || nodeType == "Condition")
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
                    return new SequenceNode();
                case "Selector":
                    return new SelectorNode();
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
                // Actionスクリプトから実際のクラスを作成
                switch (scriptName)
                {
                    case "MoveToPosition":
                        node = new MoveToPositionAction();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created MoveToPositionAction");
                        break;
                    case "Wait":
                        node = new WaitAction();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created WaitAction");
                        break;
                    case "AttackEnemy":
                        node = new AttackEnemyAction();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created AttackEnemyAction");
                        break;
                    case "ScanEnvironment":
                        node = new ScanEnvironmentAction();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created ScanEnvironmentAction");
                        break;
                    case "MoveToEnemy":
                        node = new MoveToEnemyAction();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created MoveToEnemyAction");
                        break;
                    case "AttackTarget":
                        node = new AttackTargetAction();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created AttackTargetAction");
                        break;
                    case "RandomWander":
                        node = new RandomWanderAction();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created RandomWanderAction");
                        break;
                    case "CastSpell":
                        node = new CastSpellAction();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created CastSpellAction");
                        break;
                    case "Attack":
                        node = new AttackAction();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created AttackAction");
                        break;
                    case "NormalAttack":
                        node = new NormalAttackAction();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created NormalAttackAction");
                        break;
                    case "UseItem":
                        node = new UseItemAction();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created UseItemAction");
                        break;
                    case "FleeToSafety":
                        node = new FleeToSafetyAction();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created FleeToSafetyAction");
                        break;
                    case "Interact":
                        node = new InteractAction();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created InteractAction");
                        break;
                    case "MoveToTarget":
                        node = new MoveToTargetAction();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created MoveToTargetAction");
                        break;
                    case "EnvironmentScan":
                        node = new EnvironmentScanAction();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created EnvironmentScanAction");
                        break;
                    case "InitializeResources":
                        node = new InitializeResourcesAction();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created InitializeResourcesAction");
                        break;
                    case "RestoreSmallMana":
                        node = new RestoreSmallManaAction();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created RestoreSmallManaAction");
                        break;
                    case "SearchForEnemy":
                        node = new SearchForEnemyAction();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created SearchForEnemyAction");
                        break;
                    default:
                        BTLogger.Log(LogLevel.Warning, LogCategory.Parser, $"Unknown action script: {scriptName}, using CustomActionNode");
                        node = new CustomActionNode();
                        break;
                }
            }
            else if (nodeType == "Condition")
            {
                BTLogger.Log(LogLevel.Debug, LogCategory.Parser, $"🔧 Creating CONDITION node for script: {scriptName}");
                // Conditionスクリプトから実際のクラスを作成
                switch (scriptName)
                {
                    case "HealthCheck":
                        node = new HealthCheckCondition();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created HealthCheckCondition");
                        break;
                    case "EnemyCheck":
                        node = new EnemyCheckCondition();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created EnemyCheckCondition");
                        break;
                    case "HasItem":
                        node = new HasItemCondition();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created HasItemCondition");
                        break;
                    case "HasSharedEnemyInfo":
                        node = new HasSharedEnemyInfoCondition();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created HasSharedEnemyInfoCondition");
                        break;
                    case "EnemyInRange":
                        node = new EnemyInRangeCondition();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created EnemyInRangeCondition");
                        break;
                    case "IsInitialized":
                        node = new IsInitializedCondition();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created IsInitializedCondition");
                        break;
                    case "HasTarget":
                        node = new HasTargetCondition();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created HasTargetCondition");
                        break;
                    case "HasMana":
                        node = new HasManaCondition();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created HasManaCondition");
                        break;
                    case "EnemyHealthCheck":
                        node = new EnemyHealthCheckCondition();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created EnemyHealthCheckCondition");
                        break;
                    case "ScanForInterest":
                        node = new ScanForInterestCondition();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created ScanForInterestCondition");
                        break;
                    case "CheckManaResource":
                        node = new CheckManaResourceCondition();
                        BTLogger.Log(LogLevel.Info, LogCategory.Parser, $"✅ Created CheckManaResourceCondition");
                        break;
                    default:
                        BTLogger.Log(LogLevel.Warning, LogCategory.Parser, $"Unknown condition script: {scriptName}, using CustomConditionNode");
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