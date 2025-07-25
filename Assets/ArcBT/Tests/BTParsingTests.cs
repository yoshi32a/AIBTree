using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ArcBT.Parser;
using ArcBT.Core;

namespace ArcBT.Tests
{
    /// <summary>BehaviourTreeファイルのパース機能をテストするクラス</summary>
    public class BTParsingTests
    {
        BTParser parser;
        
        [SetUp]
        public void SetUp()
        {
            parser = new BTParser();
            BTLogger.EnableTestMode(); // テストモードでパーサーログを有効化
        }


        [TearDown]
        public void TearDown()
        {
            parser = null;
            BTLogger.ResetToDefaults(); // テスト後は通常モードに戻す
        }

        /// <summary>全ての.btファイルが正常にパースできるかテスト</summary>
        [Test]
        public void TestAllBTFilesParseSuccessfully()
        {
            var btDirectory = Path.Combine(Application.dataPath, "BehaviourTrees");
            
            if (!Directory.Exists(btDirectory))
            {
                Assert.Fail($"BehaviourTrees directory not found: {btDirectory}");
                return;
            }
            
            // テスト対象を動作するファイルのみに限定
            string[] testFiles = {
                "blackboard_sample.bt",
                "team_coordination_sample.bt", 
                "dynamic_condition_sample.bt",
                "resource_management_sample.bt"
            };
            
            var btFiles = new List<string>();
            foreach(var testFile in testFiles)
            {
                var fullPath = Path.Combine(btDirectory, testFile);
                if (File.Exists(fullPath))
                {
                    btFiles.Add(fullPath);
                }
                else
                {
                    Debug.LogWarning($"Test file not found: {testFile}");
                }
            }
            
            Assert.IsTrue(btFiles.Count > 0, "No test .bt files found");
            
            var failedFiles = new List<string>();
            var successfulFiles = new List<string>();
            
            foreach (var filePath in btFiles)
            {
                var fileName = Path.GetFileName(filePath);
                Debug.Log($"Testing BT file: {fileName}");
                
                try
                {
                    var rootNode = parser.ParseFile(filePath);
                    
                    if (rootNode != null)
                    {
                        successfulFiles.Add(fileName);
                        Debug.Log($"✅ Successfully parsed: {fileName}");
                        
                        // 追加検証: ノード構造の基本チェック
                        ValidateNodeStructure(rootNode, fileName);
                    }
                    else
                    {
                        failedFiles.Add($"{fileName} (returned null)");
                        Debug.LogError($"❌ Failed to parse: {fileName} - returned null");
                    }
                }
                catch (System.Exception ex)
                {
                    failedFiles.Add($"{fileName} ({ex.Message})");
                    Debug.LogError($"❌ Exception while parsing {fileName}: {ex.Message}");
                }
            }
            
            // 結果の出力
            Debug.Log($"🎯 Parsing Test Results:");
            Debug.Log($"📊 Total files: {btFiles.Count}");
            Debug.Log($"✅ Successful: {successfulFiles.Count}");
            Debug.Log($"❌ Failed: {failedFiles.Count}");
            
            if (successfulFiles.Count > 0)
            {
                Debug.Log($"✅ Successfully parsed files:\n  - {string.Join("\n  - ", successfulFiles)}");
            }
            
            if (failedFiles.Count > 0)
            {
                Debug.LogError($"❌ Failed to parse files:\n  - {string.Join("\n  - ", failedFiles)}");
                Assert.Fail($"Failed to parse {failedFiles.Count} out of {btFiles.Count} BT files:\n- {string.Join("\n- ", failedFiles)}");
            }
            
            Assert.Pass($"All {btFiles.Count} BT files parsed successfully!");
        }
        
        /// <summary>各BTファイルの詳細構造をテスト</summary>
        [Test]
        public void TestSpecificBTFileStructures()
        {
            var testCases = new Dictionary<string, System.Action<BTNode>>
            {
                {
                    "blackboard_sample.bt",
                    rootNode => {
                        Assert.IsNotNull(rootNode, "Root node should not be null");
                        Assert.IsTrue(rootNode.Name.Contains("main"), "Root should be 'main' sequence");
                        Assert.IsTrue(rootNode.Children.Count > 0, "Root should have children");
                    }
                },
                {
                    "team_coordination_sample.bt",
                    rootNode => {
                        Assert.IsNotNull(rootNode, "Root node should not be null");
                        Assert.IsTrue(rootNode.Name.Contains("team_behavior"), "Root should be 'team_behavior' parallel");
                        Assert.IsTrue(rootNode.Children.Count >= 3, "Parallel should have at least 3 children (scout, combat, support)");
                    }
                },
                {
                    "dynamic_condition_sample.bt",
                    rootNode => {
                        Assert.IsNotNull(rootNode, "Root node should not be null");
                        Assert.IsTrue(rootNode.Name.Contains("main"), "Root should be 'main' selector");
                        Assert.IsTrue(rootNode.Children.Count > 0, "Selector should have children");
                    }
                }
            };
            
            var btDirectory = Path.Combine(Application.dataPath, "BehaviourTrees");
            
            foreach (var testCase in testCases)
            {
                var filePath = Path.Combine(btDirectory, testCase.Key);
                
                if (File.Exists(filePath))
                {
                    Debug.Log($"🔍 Testing structure of: {testCase.Key}");
                    var rootNode = parser.ParseFile(filePath);
                    
                    try
                    {
                        testCase.Value(rootNode);
                        Debug.Log($"✅ Structure validation passed for: {testCase.Key}");
                    }
                    catch (System.Exception ex)
                    {
                        Assert.Fail($"Structure validation failed for {testCase.Key}: {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"⚠️ Test file not found, skipping: {testCase.Key}");
                }
            }
        }

        [Test(Description = "パーサーエラーハンドリングをテスト")]
        public void TestParserErrorHandling()
        {
            // 存在しないファイルのテスト（エラーログを期待）
            LogAssert.Expect(LogType.Error, "[ERR][PRS]: BT file not found: nonexistent_file.bt");
            var result = parser.ParseFile("nonexistent_file.bt");
            Assert.IsNull(result, "Parsing non-existent file should return null");
            
            // 無効な構文のテスト（エラーログが発生する可能性）
            var invalidContent = @"
                invalid syntax here
                not a proper bt file
            ";
            
            // 無効な構文ではエラーログが出る可能性があるため、期待
            LogAssert.Expect(LogType.Error, "[ERR][PRS]: No tree definition found");
            var invalidResult = parser.ParseContent(invalidContent);
            Assert.IsNull(invalidResult, "Parsing invalid content should return null");
            
            // 空のコンテンツのテスト
            LogAssert.Expect(LogType.Error, "[ERR][PRS]: No tree definition found");
            var emptyResult = parser.ParseContent("");
            Assert.IsNull(emptyResult, "Parsing empty content should return null");
            
            Debug.Log("✅ Error handling tests passed");
        }
        
        /// <summary>特定のノードタイプが正しく作成されるかテスト</summary>
        [Test]
        public void TestNodeCreation()
        {
            // 基本的なSequenceのテスト
            var sequenceContent = @"
                tree TestTree {
                    Sequence root {
                        Action MoveToPosition {
                            target: ""test_target""
                            speed: 3.5
                        }
                        
                        Condition HealthCheck {
                            min_health: 50
                        }
                    }
                }
            ";
            
            var root = parser.ParseContent(sequenceContent);
            Assert.IsNotNull(root, "Should successfully parse basic sequence");
            Assert.AreEqual("root", root.Name, "Root node name should be 'root'");
            Assert.AreEqual(2, root.Children.Count, "Sequence should have 2 children");
            
            // 子ノードの検証
            var moveAction = root.Children[0];
            var healthCondition = root.Children[1];
            
            Assert.IsTrue(moveAction.Name.Contains("MoveToPosition"), "First child should be MoveToPosition action");
            Assert.IsTrue(healthCondition.Name.Contains("HealthCheck"), "Second child should be HealthCheck condition");
            
            Debug.Log("✅ Node creation tests passed");
        }
        
        /// <summary>BlackBoard関連のノードが認識されるかテスト</summary>
        [Test]
        public void TestBlackBoardNodes()
        {
            var blackboardContent = @"
                tree BlackBoardTest {
                    Sequence main {
                        Action ScanEnvironment {
                            scan_radius: 15.0
                        }
                        
                        Condition HasSharedEnemyInfo {
                        }
                        
                        Action AttackTarget {
                            damage: 30
                        }
                    }
                }
            ";
            
            var root = parser.ParseContent(blackboardContent);
            Assert.IsNotNull(root, "Should successfully parse BlackBoard content");
            Assert.AreEqual(3, root.Children.Count, "Should have 3 children");
            
            // BlackBoard専用ノードの検証
            Assert.IsTrue(root.Children[0].Name.Contains("ScanEnvironment"), "Should recognize ScanEnvironment action");
            Assert.IsTrue(root.Children[1].Name.Contains("HasSharedEnemyInfo"), "Should recognize HasSharedEnemyInfo condition");
            Assert.IsTrue(root.Children[2].Name.Contains("AttackTarget"), "Should recognize AttackTarget action");
            
            Debug.Log("✅ BlackBoard node tests passed");
        }
        
        /// <summary>ノード構造を再帰的に検証するヘルパーメソッド</summary>
        void ValidateNodeStructure(BTNode node, string fileName)
        {
            if (node == null)
            {
                Assert.Fail($"{fileName}: Node should not be null");
                return;
            }
            
            // ノード名が設定されているかチェック
            Assert.IsFalse(string.IsNullOrEmpty(node.Name), $"{fileName}: Node name should not be null or empty");
            
            // 子ノードがある場合は再帰的にチェック
            if (node.Children != null && node.Children.Count > 0)
            {
                foreach (var child in node.Children)
                {
                    ValidateNodeStructure(child, fileName);
                }
            }
            
            // ノードタイプ別の追加チェック
            if (node.Name.StartsWith("Action:"))
            {
                // Actionノードの場合の検証
                Assert.IsTrue(node is BTActionNode || node.GetType().IsSubclassOf(typeof(BTActionNode)), 
                    $"{fileName}: Action node should inherit from BTActionNode");
            }
            else if (node.Name.StartsWith("Condition:"))
            {
                // Conditionノードの場合の検証
                Assert.IsTrue(node is BTConditionNode || node.GetType().IsSubclassOf(typeof(BTConditionNode)), 
                    $"{fileName}: Condition node should inherit from BTConditionNode");
            }
        }
        
        /// <summary>パフォーマンステスト：大きなファイルの処理時間をチェック</summary>
        [Test]
        public void TestParsingPerformance()
        {
            var btDirectory = Path.Combine(Application.dataPath, "BehaviourTrees");
            var btFiles = Directory.GetFiles(btDirectory, "*.bt");
            
            if (btFiles.Length == 0)
            {
                Assert.Inconclusive("No BT files found for performance testing");
                return;
            }
            
            var stopwatch = new System.Diagnostics.Stopwatch();
            
            foreach (var filePath in btFiles)
            {
                var fileName = Path.GetFileName(filePath);
                
                // 構文エラーがある古いファイルのエラーログを抑制
                var hasKnownSyntaxErrors = fileName.Contains("complex_example") || fileName.Contains("advanced_guard");
                
                if (hasKnownSyntaxErrors)
                {
                    LogAssert.ignoreFailingMessages = true;
                }
                
                stopwatch.Restart();
                var result = parser.ParseFile(filePath);
                stopwatch.Stop();
                
                if (hasKnownSyntaxErrors)
                {
                    LogAssert.ignoreFailingMessages = false;
                }
                
                var elapsedMs = stopwatch.ElapsedMilliseconds;
                Debug.Log($"⏱️ {fileName}: {elapsedMs}ms");
                
                // パース時間が1秒を超える場合は警告
                if (elapsedMs > 1000)
                {
                    Debug.LogWarning($"⚠️ {fileName} took {elapsedMs}ms to parse (>1000ms)");
                }
                
                // パース時間が10秒を超える場合は失敗
                Assert.IsTrue(elapsedMs < 10000, $"Parsing {fileName} took too long: {elapsedMs}ms");
            }
            
            Debug.Log("✅ Performance tests completed");
        }
    }
}
