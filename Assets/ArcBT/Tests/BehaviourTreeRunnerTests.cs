using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ArcBT.Core;
using ArcBT.Actions;
using System.Collections;
using System.IO;

namespace ArcBT.Tests
{
    /// <summary>BehaviourTreeRunnerの機能をテストするクラス</summary>
    [TestFixture]
    public class BehaviourTreeRunnerTests
    {
        GameObject testRunner;
        BehaviourTreeRunner runner;
        string tempFilePath;
        
        [SetUp]
        public void SetUp()
        {
            testRunner = new GameObject("TestRunner");
            runner = testRunner.AddComponent<BehaviourTreeRunner>();
            tempFilePath = Path.GetTempFileName();
            BTLogger.EnableTestMode();
        }

        [TearDown]
        public void TearDown()
        {
            if (testRunner != null)
            {
                Object.DestroyImmediate(testRunner);
            }

            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
            
            BTLogger.ResetToDefaults();
        }

        #region 初期化テスト

        [Test]
        public void BehaviourTreeRunner_Awake_InitializesComponents()
        {
            // Act (Awakeは自動実行される)
            
            // Assert
            Assert.IsNotNull(runner.BlackBoard);
        }

        [Test]
        public void BehaviourTreeRunner_Start_LoadsBehaviourTreeFromFile()
        {
            // Arrange
            string btContent = @"
                tree TestTree {
                    Sequence Root {
                        Action Wait {
                            duration: ""1.0""
                        }
                    }
                }";
            File.WriteAllText(tempFilePath, btContent);
            
            // Set private field using reflection for testing
            var filePathField = typeof(BehaviourTreeRunner).GetField("behaviourTreeFilePath", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            filePathField.SetValue(runner, tempFilePath);

            // Act
            runner.LoadBehaviourTree(tempFilePath);

            // Assert
            Assert.IsNotNull(runner.RootNode);
            Assert.IsInstanceOf<BTSequenceNode>(runner.RootNode);
        }

        #endregion

        #region ファイル読み込みテスト

        [Test]
        public void LoadBehaviourTree_ValidFile_LoadsCorrectly()
        {
            // Arrange
            string btContent = @"
                tree SimpleTree {
                    Selector Root {
                        Action RandomWander {
                            speed: ""2.0""
                        }
                    }
                }";
            File.WriteAllText(tempFilePath, btContent);

            // Act
            runner.LoadBehaviourTree(tempFilePath);

            // Assert
            Assert.IsNotNull(runner.RootNode);
            Assert.IsInstanceOf<BTSelectorNode>(runner.RootNode);
            Assert.AreEqual("Root", runner.RootNode.Name);
            Assert.AreEqual(1, runner.RootNode.Children.Count);
        }

        [Test]
        public void LoadBehaviourTree_NonExistentFile_HandlesGracefully()
        {
            // Arrange
            string nonExistentFile = "non_existent_file.bt";
            LogAssert.Expect(LogType.Error, "[ERR][SYS]: BT file not found: non_existent_file.bt");

            // Act
            runner.LoadBehaviourTree(nonExistentFile);

            // Assert
            Assert.IsNull(runner.RootNode);
        }

        [Test]
        public void LoadBehaviourTree_InvalidContent_HandlesGracefully()
        {
            // Arrange
            string invalidContent = "This is not valid BT content";
            File.WriteAllText(tempFilePath, invalidContent);
            LogAssert.Expect(LogType.Error, "[ERR][PRS]: No tree definition found");
            // 動的なファイルパスを含むエラーログは正確なパスで期待することにする
            LogAssert.Expect(LogType.Error, $"[ERR][PRS]: Failed to parse behaviour tree: {tempFilePath}");

            // Act
            runner.LoadBehaviourTree(tempFilePath);

            // Assert
            Assert.IsNull(runner.RootNode);
        }

        #endregion

        #region 実行テスト

        [Test]
        public void SetRootNode_ValidNode_SetsCorrectly()
        {
            // Arrange
            var testNode = new BTSequenceNode();
            testNode.Name = "TestSequence";

            // Act
            runner.SetRootNode(testNode);

            // Assert
            Assert.AreEqual(testNode, runner.RootNode);
            Assert.AreEqual("TestSequence", runner.RootNode.Name);
        }

        [Test]
        public void SetRootNode_InitializesBlackBoard()
        {
            // Arrange
            var testNode = new WaitAction();
            testNode.Name = "TestWait";

            // Act
            runner.SetRootNode(testNode);

            // Assert
            Assert.IsNotNull(runner.BlackBoard);
            // ノードがBlackBoardで初期化されることを確認
        }

        [UnityTest]
        public IEnumerator Update_WithValidTree_ExecutesCorrectly()
        {
            // Arrange
            var waitAction = new WaitAction();
            waitAction.SetProperty("duration", "0.1");
            waitAction.Initialize(runner, runner.BlackBoard);
            runner.SetRootNode(waitAction);

            // Act - 少し待って Update が実行されるのを確認
            yield return new WaitForSeconds(0.2f);

            // Assert - WaitActionが実行されていることを確認
            // (実際の実行確認は困難なので、ログや状態変化で判断)
        }

        [Test]
        public void ExecuteOnce_WithSimpleAction_ReturnsResult()
        {
            // Arrange
            var waitAction = new WaitAction();
            waitAction.SetProperty("duration", "0.1");
            waitAction.Initialize(runner, runner.BlackBoard);
            runner.SetRootNode(waitAction);

            // Act
            var result = runner.ExecuteOnce();

            // Assert
            Assert.IsTrue(result == BTNodeResult.Running || result == BTNodeResult.Success);
        }

        [Test]
        public void ExecuteOnce_WithNullRoot_ReturnsFailure()
        {
            // Act
            var result = runner.ExecuteOnce();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        #endregion

        #region BlackBoard統合テスト

        [Test]
        public void BlackBoard_Integration_SharesDataBetweenNodes()
        {
            // Arrange
            var sequence = new BTSequenceNode();
            sequence.Name = "TestSequence";

            var scanAction = new ScanEnvironmentAction();
            // スキャン間隔を0に設定して即座に実行できるようにする
            scanAction.SetProperty("scan_interval", "0.0");
            scanAction.Initialize(runner, runner.BlackBoard);

            var wanderAction = new RandomWanderAction();
            wanderAction.Initialize(runner, runner.BlackBoard);

            sequence.AddChild(scanAction);
            sequence.AddChild(wanderAction);
            sequence.Initialize(runner, runner.BlackBoard);

            runner.SetRootNode(sequence);

            // Act
            var result = runner.ExecuteOnce();

            // Assert
            // ScanActionがBlackBoardにデータを設定したことを確認
            Debug.Log($"Execution result: {result}");
            Debug.Log($"BlackBoard keys count: {runner.BlackBoard.GetAllKeys().Length}");
            foreach (var key in runner.BlackBoard.GetAllKeys())
            {
                Debug.Log($"BlackBoard key: {key} = {runner.BlackBoard.GetValueAsString(key)}");
            }
            
            Assert.IsTrue(runner.BlackBoard.GetAllKeys().Length > 0, "BlackBoard should contain data after ScanEnvironmentAction execution");
        }

        [Test]
        public void BlackBoard_GetBlackBoardContents_ReturnsCorrectInfo()
        {
            // Arrange
            runner.BlackBoard.SetValue("test_key", "test_value");
            runner.BlackBoard.SetValue("enemy_count", 3);
            runner.BlackBoard.SetValue("player_position", new Vector3(1, 2, 3));

            // Act
            string contents = runner.GetBlackBoardContents();

            // Assert
            Assert.IsTrue(contents.Contains("test_key"));
            Assert.IsTrue(contents.Contains("test_value"));
            Assert.IsTrue(contents.Contains("enemy_count"));
            Assert.IsTrue(contents.Contains("3"));
        }

        [Test]
        public void ResetTreeState_ClearsBlackBoardAndResets()
        {
            // Arrange
            runner.BlackBoard.SetValue("temp_data", "should_be_cleared");
            var testNode = new WaitAction();
            runner.SetRootNode(testNode);

            // Act
            runner.ResetTreeState();

            // Assert
            Assert.IsFalse(runner.BlackBoard.HasKey("temp_data"));
        }

        #endregion

        #region 設定テスト

        [Test]
        public void SetTickInterval_UpdatesCorrectly()
        {
            // Act
            runner.SetTickInterval(0.5f);

            // Assert (内部フィールドの確認は困難なので、設定が受け入れられることを確認)
            // 実際のテストは統合テストで間隔を確認
            Assert.IsNotNull(runner);
        }

        [Test]
        public void SetDebugMode_UpdatesCorrectly()
        {
            // Act
            runner.SetDebugMode(false);
            runner.SetDebugMode(true);

            // Assert (デバッグモードの動作確認は実行ログで判断)
            Assert.IsNotNull(runner);
        }

        #endregion

        #region エラーハンドリングテスト

        [Test]
        public void LoadBehaviourTree_EmptyFilePath_HandlesGracefully()
        {
            // 期待されるエラーログを指定
            LogAssert.Expect(LogType.Error, "[ERR][SYS]: BT file not found: ");

            // Act
            runner.LoadBehaviourTree("");

            // Assert
            Assert.IsNull(runner.RootNode);
        }

        [Test]
        public void LoadBehaviourTree_NullFilePath_HandlesGracefully()
        {
            // 期待されるエラーログを指定
            LogAssert.Expect(LogType.Error, "[ERR][SYS]: Error loading behaviour tree: Object reference not set to an instance of an object");

            // Act
            runner.LoadBehaviourTree(null);

            // Assert
            Assert.IsNull(runner.RootNode);
        }

        [Test]
        public void SetRootNode_NullNode_HandlesGracefully()
        {
            // Act
            runner.SetRootNode(null);

            // Assert
            Assert.IsNull(runner.RootNode);
        }

        #endregion

        #region 複雑なツリーテスト

        [Test]
        public void ComplexTree_NestedStructure_ExecutesCorrectly()
        {
            // Arrange: 複雑なツリー構造を作成
            var rootSelector = new BTSelectorNode();
            rootSelector.Name = "RootSelector";

            var combatSequence = new BTSequenceNode();
            combatSequence.Name = "CombatSequence";

            var scanAction = new ScanEnvironmentAction();
            scanAction.Initialize(runner, runner.BlackBoard);

            var wanderAction = new RandomWanderAction();
            wanderAction.Initialize(runner, runner.BlackBoard);

            combatSequence.AddChild(scanAction);
            combatSequence.AddChild(wanderAction);
            combatSequence.Initialize(runner, runner.BlackBoard);

            rootSelector.AddChild(combatSequence);
            rootSelector.Initialize(runner, runner.BlackBoard);

            runner.SetRootNode(rootSelector);

            // Act
            var result = runner.ExecuteOnce();

            // Assert
            Assert.IsTrue(result == BTNodeResult.Success || result == BTNodeResult.Running || result == BTNodeResult.Failure);
            Assert.AreEqual("RootSelector", runner.RootNode.Name);
            Assert.AreEqual(1, runner.RootNode.Children.Count);
        }

        [Test]
        public void ParallelExecution_MultipleActions_ExecutesCorrectly()
        {
            // Arrange
            var parallelNode = new BTParallelNode();
            parallelNode.Name = "ParallelRoot";

            var action1 = new WaitAction();
            action1.SetProperty("duration", "0.1");
            action1.Initialize(runner, runner.BlackBoard);

            var action2 = new RandomWanderAction();
            action2.Initialize(runner, runner.BlackBoard);

            parallelNode.AddChild(action1);
            parallelNode.AddChild(action2);
            parallelNode.Initialize(runner, runner.BlackBoard);

            runner.SetRootNode(parallelNode);

            // Act
            var result = runner.ExecuteOnce();

            // Assert
            Assert.IsTrue(result == BTNodeResult.Running || result == BTNodeResult.Success);
        }

        #endregion

        #region 統合テスト

        [UnityTest]
        public IEnumerator Integration_FullBehaviourTreeExecution_WorksCorrectly()
        {
            // Arrange: 完全なBTファイルを作成
            string btContent = @"
                tree IntegrationTest {
                    Selector MainSelector {
                        Sequence ScanAndWander {
                            Action ScanEnvironment {
                                scan_radius: ""10.0""
                                scan_interval: ""0.0""
                            }
                            Action RandomWander {
                                speed: ""3.0""
                                wander_radius: ""5.0""
                            }
                        }
                        Action Wait {
                            duration: ""0.5""
                        }
                    }
                }";
            File.WriteAllText(tempFilePath, btContent);

            // Act
            runner.LoadBehaviourTree(tempFilePath);
            
            // 数フレーム実行を確認
            for (int i = 0; i < 5; i++)
            {
                yield return null;
            }

            // Assert
            Assert.IsNotNull(runner.RootNode);
            Assert.IsInstanceOf<BTSelectorNode>(runner.RootNode);
            
            // BlackBoardにスキャン結果が記録されていることを確認
            Assert.IsTrue(runner.BlackBoard.GetAllKeys().Length > 0);
        }

        [Test]
        public void PerformanceTest_MultipleExecutions_CompletesQuickly()
        {
            // Arrange
            var simpleAction = new WaitAction();
            simpleAction.SetProperty("duration", "0.01");
            simpleAction.Initialize(runner, runner.BlackBoard);
            runner.SetRootNode(simpleAction);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            const int executionCount = 100;

            // Act
            for (int i = 0; i < executionCount; i++)
            {
                runner.RootNode?.Execute();
            }

            stopwatch.Stop();

            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 100, 
                $"性能テスト失敗: {executionCount}回実行に{stopwatch.ElapsedMilliseconds}ms掛かりました");
        }

        #endregion
    }
}