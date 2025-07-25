using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using BehaviourTree.Parser;
using BehaviourTree.Core;

namespace BehaviourTree.Tests
{
    /// <summary>
    /// BTファイルの内容検証を行う詳細テスト
    /// </summary>
    public class BTFileValidationTests
    {
        BTParser parser;
        string btDirectory;
        
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            parser = new BTParser();
            btDirectory = Path.Combine(Application.dataPath, "BehaviourTrees");
        }
        
        /// <summary>
        /// blackboard_sample.btの詳細検証
        /// </summary>
        [Test]
        public void ValidateBlackBoardSample()
        {
            string filePath = Path.Combine(btDirectory, "blackboard_sample.bt");
            
            if (!File.Exists(filePath))
            {
                Assert.Inconclusive("blackboard_sample.bt not found");
                return;
            }
            
            BTNode root = parser.ParseFile(filePath);
            Assert.IsNotNull(root, "blackboard_sample.bt should parse successfully");
            
            // ルートはSequence 'main'
            Assert.AreEqual("main", root.Name, "Root should be named 'main'");
            Assert.AreEqual(2, root.Children.Count, "Root should have 2 children");
            
            // 1番目の子: ScanEnvironment Action
            BTNode scanAction = root.Children[0];
            Assert.IsTrue(scanAction.Name.Contains("ScanEnvironment"), "First child should be ScanEnvironment");
            
            // 2番目の子: Selector 'movement_behavior'
            BTNode movementSelector = root.Children[1];
            Assert.AreEqual("movement_behavior", movementSelector.Name, "Second child should be movement_behavior selector");
            Assert.AreEqual(2, movementSelector.Children.Count, "movement_behavior should have 2 children");
            
            // movement_behaviorの子ノード検証
            BTNode moveToEnemySequence = movementSelector.Children[0];
            BTNode randomWanderAction = movementSelector.Children[1];
            
            Assert.AreEqual("move_to_enemy", moveToEnemySequence.Name, "First child of selector should be move_to_enemy");
            Assert.AreEqual(3, moveToEnemySequence.Children.Count, "move_to_enemy should have 3 children");
            
            // move_to_enemyの子ノード検証
            Assert.IsTrue(moveToEnemySequence.Children[0].Name.Contains("HasSharedEnemyInfo"), "Should have HasSharedEnemyInfo condition");
            Assert.IsTrue(moveToEnemySequence.Children[1].Name.Contains("MoveToEnemy"), "Should have MoveToEnemy action");
            Assert.IsTrue(moveToEnemySequence.Children[2].Name.Contains("AttackTarget"), "Should have AttackTarget action");
            
            Assert.IsTrue(randomWanderAction.Name.Contains("RandomWander"), "Should have RandomWander action");
            
            Debug.Log("✅ blackboard_sample.bt validation passed");
        }
        
        /// <summary>
        /// team_coordination_sample.btの詳細検証
        /// </summary>
        [Test]
        public void ValidateTeamCoordinationSample()
        {
            string filePath = Path.Combine(btDirectory, "team_coordination_sample.bt");
            
            if (!File.Exists(filePath))
            {
                Assert.Inconclusive("team_coordination_sample.bt not found");
                return;
            }
            
            BTNode root = parser.ParseFile(filePath);
            Assert.IsNotNull(root, "team_coordination_sample.bt should parse successfully");
            
            // ルートはParallel 'team_behavior'
            Assert.AreEqual("team_behavior", root.Name, "Root should be named 'team_behavior'");
            Assert.AreEqual(3, root.Children.Count, "Parallel should have 3 roles");
            
            // 各役割の検証
            var roleNames = new List<string> { "scout_role", "combat_role", "support_role" };
            
            for (int i = 0; i < 3; i++)
            {
                BTNode role = root.Children[i];
                Assert.IsTrue(roleNames.Contains(role.Name), $"Role {i} should have valid name, got: {role.Name}");
                Assert.IsTrue(role.Children.Count > 0, $"Role {role.Name} should have children");
            }
            
            Debug.Log("✅ team_coordination_sample.bt validation passed");
        }
        
        /// <summary>
        /// dynamic_condition_sample.btの詳細検証
        /// </summary>
        [Test]
        public void ValidateDynamicConditionSample()
        {
            string filePath = Path.Combine(btDirectory, "dynamic_condition_sample.bt");
            
            if (!File.Exists(filePath))
            {
                Assert.Inconclusive("dynamic_condition_sample.bt not found");
                return;
            }
            
            BTNode root = parser.ParseFile(filePath);
            Assert.IsNotNull(root, "dynamic_condition_sample.bt should parse successfully");
            
            // ルートはSelector 'main'
            Assert.AreEqual("main", root.Name, "Root should be named 'main'");
            Assert.AreEqual(3, root.Children.Count, "Selector should have 3 behaviors");
            
            // 各行動パターンの検証
            BTNode healthyCombat = root.Children[0];
            BTNode safeHealing = root.Children[1];
            BTNode flee = root.Children[2];
            
            Assert.AreEqual("healthy_combat", healthyCombat.Name, "First behavior should be healthy_combat");
            Assert.AreEqual("safe_healing", safeHealing.Name, "Second behavior should be safe_healing");
            Assert.IsTrue(flee.Name.Contains("MoveToPosition"), "Third behavior should be MoveToPosition (flee)");
            
            // healthy_combatの詳細検証
            Assert.AreEqual(3, healthyCombat.Children.Count, "healthy_combat should have 3 children");
            Assert.IsTrue(healthyCombat.Children[0].Name.Contains("HealthCheck"), "Should start with HealthCheck");
            Assert.IsTrue(healthyCombat.Children[1].Name.Contains("EnemyCheck"), "Should have EnemyCheck");
            Assert.IsTrue(healthyCombat.Children[2].Name.Contains("AttackEnemy"), "Should end with AttackEnemy");
            
            Debug.Log("✅ dynamic_condition_sample.bt validation passed");
        }
        
        /// <summary>
        /// 全BTファイルで使用されているスクリプト名の検証
        /// </summary>
        [Test]
        public void ValidateAllScriptReferences()
        {
            if (!Directory.Exists(btDirectory))
            {
                Assert.Inconclusive("BehaviourTrees directory not found");
                return;
            }
            
            string[] btFiles = Directory.GetFiles(btDirectory, "*.bt");
            var usedScripts = new HashSet<string>();
            var unknownScripts = new List<string>();
            
            // 既知のスクリプト一覧
            var knownActionScripts = new HashSet<string>
            {
                "MoveToPosition", "Wait", "AttackEnemy", "ScanEnvironment",
                "MoveToEnemy", "AttackTarget", "RandomWander"
            };
            
            var knownConditionScripts = new HashSet<string>
            {
                "HealthCheck", "EnemyCheck", "HasItem", "HasSharedEnemyInfo"
            };
            
            foreach (string filePath in btFiles)
            {
                string fileName = Path.GetFileName(filePath);
                Debug.Log($"🔍 Analyzing script references in: {fileName}");
                
                BTNode root = parser.ParseFile(filePath);
                if (root != null)
                {
                    CollectScriptReferences(root, usedScripts, unknownScripts, knownActionScripts, knownConditionScripts);
                }
            }
            
            // 結果の出力
            Debug.Log($"📊 Script Reference Analysis:");
            Debug.Log($"📝 Used scripts: {string.Join(", ", usedScripts)}");
            
            if (unknownScripts.Count > 0)
            {
                Debug.LogWarning($"⚠️ Unknown scripts found: {string.Join(", ", unknownScripts)}");
                Assert.Fail($"Unknown scripts referenced in BT files: {string.Join(", ", unknownScripts)}");
            }
            
            Assert.IsTrue(usedScripts.Count > 0, "Should have found at least some script references");
            Debug.Log("✅ All script references are valid");
        }
        
        /// <summary>
        /// スクリプト参照を再帰的に収集するヘルパーメソッド
        /// </summary>
        void CollectScriptReferences(BTNode node, HashSet<string> usedScripts, List<string> unknownScripts,
                                   HashSet<string> knownActions, HashSet<string> knownConditions)
        {
            if (node == null) return;
            
            // ノード名からスクリプト名を抽出
            if (node.Name.StartsWith("Action:"))
            {
                string scriptName = node.Name.Substring("Action:".Length);
                usedScripts.Add(scriptName);
                
                if (!knownActions.Contains(scriptName) && !unknownScripts.Contains(scriptName))
                {
                    unknownScripts.Add($"Action:{scriptName}");
                }
            }
            else if (node.Name.StartsWith("Condition:"))
            {
                string scriptName = node.Name.Substring("Condition:".Length);
                usedScripts.Add(scriptName);
                
                if (!knownConditions.Contains(scriptName) && !unknownScripts.Contains(scriptName))
                {
                    unknownScripts.Add($"Condition:{scriptName}");
                }
            }
            
            // 子ノードも再帰的にチェック
            if (node.Children != null)
            {
                foreach (BTNode child in node.Children)
                {
                    CollectScriptReferences(child, usedScripts, unknownScripts, knownActions, knownConditions);
                }
            }
        }
        
        /// <summary>
        /// BTファイルの基本構文チェック
        /// </summary>
        [Test]
        public void ValidateBTFileSyntax()
        {
            if (!Directory.Exists(btDirectory))
            {
                Assert.Inconclusive("BehaviourTrees directory not found");
                return;
            }
            
            string[] btFiles = Directory.GetFiles(btDirectory, "*.bt");
            var syntaxErrors = new List<string>();
            
            foreach (string filePath in btFiles)
            {
                string fileName = Path.GetFileName(filePath);
                string content = File.ReadAllText(filePath);
                
                // 基本的な構文チェック
                if (!content.Contains("tree "))
                {
                    syntaxErrors.Add($"{fileName}: Missing 'tree' declaration");
                }
                
                // 波括弧のバランスチェック
                int openBraces = 0;
                int closeBraces = 0;
                
                foreach (char c in content)
                {
                    if (c == '{') openBraces++;
                    if (c == '}') closeBraces++;
                }
                
                if (openBraces != closeBraces)
                {
                    syntaxErrors.Add($"{fileName}: Mismatched braces (open: {openBraces}, close: {closeBraces})");
                }
                
                // パース可能かチェック
                BTNode root = parser.ParseFile(filePath);
                if (root == null)
                {
                    syntaxErrors.Add($"{fileName}: Failed to parse (returned null)");
                }
            }
            
            if (syntaxErrors.Count > 0)
            {
                Debug.LogError($"❌ Syntax errors found:\n  - {string.Join("\n  - ", syntaxErrors)}");
                Assert.Fail($"Syntax errors in BT files:\n- {string.Join("\n- ", syntaxErrors)}");
            }
            
            Debug.Log("✅ All BT files have valid syntax");
        }
        
        /// <summary>
        /// 必須ファイルの存在チェック
        /// </summary>
        [Test]
        public void ValidateRequiredFilesExist()
        {
            var requiredFiles = new string[]
            {
                "blackboard_sample.bt",
                "team_coordination_sample.bt", 
                "dynamic_condition_sample.bt"
            };
            
            var missingFiles = new List<string>();
            
            foreach (string fileName in requiredFiles)
            {
                string filePath = Path.Combine(btDirectory, fileName);
                if (!File.Exists(filePath))
                {
                    missingFiles.Add(fileName);
                }
            }
            
            if (missingFiles.Count > 0)
            {
                Assert.Fail($"Required BT files missing: {string.Join(", ", missingFiles)}");
            }
            
            Debug.Log("✅ All required BT files exist");
        }
    }
}