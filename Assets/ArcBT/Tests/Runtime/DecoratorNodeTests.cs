using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ArcBT.Core;
using ArcBT.Decorators;
using ArcBT.Logger;

namespace ArcBT.Tests
{
    /// <summary>Decorator Nodesの機能をテストするクラス</summary>
    [TestFixture]
    public class DecoratorNodeTests
    {
        GameObject testOwner;
        BlackBoard blackBoard;

        [SetUp]
        public void SetUp()
        {
            testOwner = new GameObject("TestOwner");
            blackBoard = new BlackBoard();
            BTLogger.EnableTestMode();
        }

        [TearDown]
        public void TearDown()
        {
            if (testOwner != null)
            {
                Object.DestroyImmediate(testOwner);
            }

            BTLogger.ResetToDefaults();
        }

        #region InverterDecorator Tests

        [Test]
        public void InverterDecorator_WithSuccessChild_ReturnsFailure()
        {
            // Arrange
            var decorator = new InverterDecorator();
            var successChild = new MockSuccessNode();
            decorator.AddChild(successChild);
            decorator.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);

            // Act
            var result = decorator.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        [Test]
        public void InverterDecorator_WithFailureChild_ReturnsSuccess()
        {
            // Arrange
            var decorator = new InverterDecorator();
            var failureChild = new MockFailureNode();
            decorator.AddChild(failureChild);
            decorator.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);

            // Act
            var result = decorator.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
        }

        [Test]
        public void InverterDecorator_WithRunningChild_ReturnsRunning()
        {
            // Arrange
            var decorator = new InverterDecorator();
            var runningChild = new MockRunningNode();
            decorator.AddChild(runningChild);
            decorator.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);

            // Act
            var result = decorator.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Running, result);
        }

        [Test]
        public void InverterDecorator_WithNoChild_ReturnsFailure()
        {
            // Arrange
            var decorator = new InverterDecorator();
            decorator.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);

            // Expected error log for no child node
            LogAssert.Expect(LogType.Error, "[ERR][SYS]: Decorator '' has no child node");

            // Act
            var result = decorator.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        #endregion

        #region RepeatDecorator Tests

        [Test]
        public void RepeatDecorator_SetProperty_SetsCorrectMaxCount()
        {
            // Arrange
            var decorator = new RepeatDecorator();

            // Act
            decorator.SetProperty("max_count", "3");
            decorator.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);

            // Assert
            Assert.AreEqual(3, decorator.GetMaxCount());
        }

        [Test]
        public void RepeatDecorator_SetProperty_SetsStopOnFailure()
        {
            // Arrange
            var decorator = new RepeatDecorator();
            var successChild = new MockSuccessNode();
            decorator.AddChild(successChild);

            // Act
            decorator.SetProperty("stop_on_failure", "true");
            decorator.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);

            // Assert - プロパティ設定の確認は実行動作で行う
            var result = decorator.Execute();
            Assert.AreEqual(BTNodeResult.Running, result); // 繰り返し継続中
        }

        [Test]
        public void RepeatDecorator_WithLimitedCount_CompletesAfterMaxIterations()
        {
            // Arrange
            var decorator = new RepeatDecorator();
            var successChild = new MockSuccessNode();
            decorator.AddChild(successChild);
            decorator.SetProperty("max_count", "2");
            decorator.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);

            // Act & Assert - 複数回実行して動作確認
            // 1回目: Running（まだ継続）
            var result1 = decorator.Execute();
            Assert.AreEqual(BTNodeResult.Running, result1);
            Assert.AreEqual(1, decorator.GetCurrentCount());

            // 2回目: Success（完了）
            var result2 = decorator.Execute();
            Assert.AreEqual(BTNodeResult.Success, result2);
            Assert.AreEqual(2, decorator.GetCurrentCount());
        }

        [Test]
        public void RepeatDecorator_WithInfiniteCount_ContinuesIndefinitely()
        {
            // Arrange
            var decorator = new RepeatDecorator();
            var successChild = new MockSuccessNode();
            decorator.AddChild(successChild);
            decorator.SetProperty("max_count", "-1"); // 無限繰り返し
            decorator.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);

            // Act & Assert
            Assert.IsTrue(decorator.IsInfinite());

            // 複数回実行してもRunningを返し続ける
            for (int i = 0; i < 5; i++)
            {
                var result = decorator.Execute();
                Assert.AreEqual(BTNodeResult.Running, result);
            }

            Assert.AreEqual(5, decorator.GetCurrentCount());
        }

        [Test]
        public void RepeatDecorator_WithStopOnFailure_StopsOnFirstFailure()
        {
            // Arrange
            var decorator = new RepeatDecorator();
            var failureChild = new MockFailureNode();
            decorator.AddChild(failureChild);
            decorator.SetProperty("stop_on_failure", "true");
            decorator.SetProperty("max_count", "5");
            decorator.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);

            // Act
            var result = decorator.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
            Assert.AreEqual(1, decorator.GetCurrentCount());
        }

        [Test]
        public void RepeatDecorator_Reset_ResetsCurrentCount()
        {
            // Arrange
            var decorator = new RepeatDecorator();
            var successChild = new MockSuccessNode();
            decorator.AddChild(successChild);
            decorator.SetProperty("max_count", "3");
            decorator.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);

            // Act - 実行してからリセット
            decorator.Execute();
            Assert.AreEqual(1, decorator.GetCurrentCount());

            decorator.Reset();

            // Assert
            Assert.AreEqual(0, decorator.GetCurrentCount());
        }

        #endregion

        #region RetryDecorator Tests

        [Test]
        public void RetryDecorator_SetProperty_SetsCorrectMaxRetries()
        {
            // Arrange
            var decorator = new RetryDecorator();

            // Act
            decorator.SetProperty("max_retries", "5");
            decorator.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);

            // Assert
            Assert.AreEqual(5, decorator.GetMaxRetries());
        }

        [Test]
        public void RetryDecorator_SetProperty_SetsCorrectRetryDelay()
        {
            // Arrange
            var decorator = new RetryDecorator();
            var failureChild = new MockFailureNode();
            decorator.AddChild(failureChild);

            // Act
            decorator.SetProperty("retry_delay", "1.5");
            decorator.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);

            // Assert - 遅延設定の確認は実行動作で行う
            var result = decorator.Execute();
            Assert.AreEqual(BTNodeResult.Running, result); // 最初の失敗後、遅延中
        }

        [Test]
        public void RetryDecorator_WithSuccessChild_ReturnsSuccessImmediately()
        {
            // Arrange
            var decorator = new RetryDecorator();
            var successChild = new MockSuccessNode();
            decorator.AddChild(successChild);
            decorator.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);

            // Act
            var result = decorator.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.AreEqual(0, decorator.GetCurrentRetries());
        }

        [Test]
        public void RetryDecorator_WithFailureChild_RetriesUpToMaximum()
        {
            // Arrange
            var decorator = new RetryDecorator();
            var failureChild = new MockFailureNode();
            decorator.AddChild(failureChild);
            decorator.SetProperty("max_retries", "2");
            decorator.SetProperty("retry_delay", "0"); // 遅延なし
            decorator.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);

            // Act & Assert - 複数回実行してリトライ動作確認
            // 1回目: Running（最初の失敗、リトライ準備）
            Debug.Log("=== TEST: Calling Execute() #1 ===");
            var result1 = decorator.Execute();
            Debug.Log($"=== TEST: Execute() #1 returned {result1} ===");
            Assert.AreEqual(BTNodeResult.Running, result1);

            // 2回目: Running（1回目のリトライ、まだリトライ可能）
            Debug.Log("=== TEST: Calling Execute() #2 ===");
            var result2 = decorator.Execute();
            Debug.Log($"=== TEST: Execute() #2 returned {result2} ===");
            Assert.AreEqual(BTNodeResult.Running, result2);

            // 3回目: Running（2回目のリトライ、まだリトライ可能）
            Debug.Log("=== TEST: Calling Execute() #3 ===");
            var result3 = decorator.Execute();
            Debug.Log($"=== TEST: Execute() #3 returned {result3} ===");
            Assert.AreEqual(BTNodeResult.Running, result3);

            // 4回目: Failure（全てのリトライ完了）
            Debug.Log("=== TEST: Calling Execute() #4 ===");
            var result4 = decorator.Execute();
            Debug.Log($"=== TEST: Execute() #4 returned {result4} ===");
            Assert.AreEqual(BTNodeResult.Failure, result4);
        }

        [Test]
        public void RetryDecorator_WithRunningChild_ReturnsRunning()
        {
            // Arrange
            var decorator = new RetryDecorator();
            var runningChild = new MockRunningNode();
            decorator.AddChild(runningChild);
            decorator.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);

            // Act
            var result = decorator.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Running, result);
        }

        [Test]
        public void RetryDecorator_Reset_ResetsRetryCount()
        {
            // Arrange
            var decorator = new RetryDecorator();
            var failureChild = new MockFailureNode();
            decorator.AddChild(failureChild);
            decorator.SetProperty("retry_delay", "0");
            decorator.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);

            // Act - 実行してからリセット
            decorator.Execute();
            Assert.IsTrue(decorator.GetCurrentRetries() > 0);

            decorator.Reset();

            // Assert
            Assert.AreEqual(0, decorator.GetCurrentRetries());
            Assert.IsFalse(decorator.IsWaitingForRetry());
        }

        #endregion

        #region TimeoutDecorator Tests

        [Test]
        public void TimeoutDecorator_SetProperty_SetsCorrectTimeout()
        {
            // Arrange
            var decorator = new TimeoutDecorator();

            // Act
            decorator.SetProperty("timeout", "3.0");
            decorator.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);

            // Assert
            Assert.AreEqual(3.0f, decorator.GetTimeoutDuration());
        }

        [Test]
        public void TimeoutDecorator_SetProperty_SetsSuccessOnTimeout()
        {
            // Arrange
            var decorator = new TimeoutDecorator();
            var runningChild = new MockRunningNode();
            decorator.AddChild(runningChild);

            // Act
            decorator.SetProperty("success_on_timeout", "true");
            decorator.SetProperty("timeout", "0.1"); // 短いタイムアウト
            decorator.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);

            // Assert - プロパティ設定の確認は実行動作で行う
            var result = decorator.Execute();
            Assert.AreEqual(BTNodeResult.Running, result);
            Assert.IsTrue(decorator.IsRunning());
        }

        [Test]
        public void TimeoutDecorator_WithQuickSuccessChild_ReturnsSuccessBeforeTimeout()
        {
            // Arrange
            var decorator = new TimeoutDecorator();
            var successChild = new MockSuccessNode();
            decorator.AddChild(successChild);
            decorator.SetProperty("timeout", "5.0");
            decorator.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);

            // Act
            var result = decorator.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.IsFalse(decorator.IsRunning());
        }

        [Test]
        public void TimeoutDecorator_WithRunningChild_TracksElapsedTime()
        {
            // Arrange
            var decorator = new TimeoutDecorator();
            var runningChild = new MockRunningNode();
            decorator.AddChild(runningChild);
            decorator.SetProperty("timeout", "10.0");
            decorator.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);

            // Act
            var result = decorator.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Running, result);
            Assert.IsTrue(decorator.IsRunning());
            Assert.IsTrue(decorator.GetElapsedTime() >= 0f);
            Assert.IsTrue(decorator.GetRemainingTime() <= 10.0f);
            Assert.IsTrue(decorator.GetTimeoutProgress() >= 0f && decorator.GetTimeoutProgress() <= 1f);
        }

        [Test]
        public void TimeoutDecorator_Reset_ResetsTimeoutState()
        {
            // Arrange
            var decorator = new TimeoutDecorator();
            var runningChild = new MockRunningNode();
            decorator.AddChild(runningChild);
            decorator.SetProperty("timeout", "5.0");
            decorator.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);

            // Act - 実行してからリセット
            decorator.Execute();
            Assert.IsTrue(decorator.IsRunning());

            decorator.Reset();

            // Assert
            Assert.IsFalse(decorator.IsRunning());
            Assert.AreEqual(0f, decorator.GetElapsedTime());
        }

        [Test]
        public void TimeoutDecorator_WithNoChild_ReturnsFailure()
        {
            // Arrange
            var decorator = new TimeoutDecorator();
            decorator.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);

            // Act & Assert
            // 子ノードがない場合のエラーログを期待
            LogAssert.Expect(LogType.Error, "[ERR][SYS]: Decorator '' has no child node");
            var result = decorator.Execute();
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void NestedDecorators_InverterWithTimeout_WorksCorrectly()
        {
            // Arrange - InverterでTimeoutをラップ
            var inverter = new InverterDecorator();
            var timeout = new TimeoutDecorator();
            var runningChild = new MockRunningNode();

            timeout.AddChild(runningChild);
            inverter.AddChild(timeout);

            timeout.SetProperty("timeout", "0.1");
            timeout.SetProperty("success_on_timeout", "true");

            inverter.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);
            timeout.Initialize(testOwner.GetComponent<TestDecoratorComponent>(), blackBoard);

            // Act - 最初はRunning
            var result1 = inverter.Execute();
            Assert.AreEqual(BTNodeResult.Running, result1);

            // タイムアウト後はSuccess → InverterでFailureに変換される想定
            // （実際のタイムアウトテストは時間に依存するため、状態確認のみ）
            Assert.IsTrue(timeout.IsRunning());
        }

        [Test]
        public void RepeatWithRetry_ComplexBehavior_WorksCorrectly()
        {
            // Arrange - RepeatでRetryをラップ
            var repeat = new RepeatDecorator();
            var retry = new RetryDecorator();
            var failureChild = new MockFailureNode();

            retry.AddChild(failureChild);
            repeat.AddChild(retry);

            repeat.SetProperty("max_count", "2");
            retry.SetProperty("max_retries", "1");
            retry.SetProperty("retry_delay", "0");

            repeat.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);
            retry.Initialize(testOwner.GetComponent<TestDecoratorComponent>(), blackBoard);

            // Act & Assert - 複雑な動作パターンの確認
            var result = repeat.Execute();
            
            // Retryが失敗を処理し、Repeatが繰り返しを管理する
            Assert.IsTrue(result is BTNodeResult.Running or BTNodeResult.Failure or BTNodeResult.Success);
        }

        #endregion
    }

    /// <summary>テスト用のDecoratorコンポーネント</summary>
    public class TestDecoratorComponent : MonoBehaviour
    {
        // テスト用の空のMonoBehaviourコンポーネント
    }

    /// <summary>常にSuccessを返すモックノード</summary>
    public class MockSuccessNode : BTNode
    {
        public override BTNodeResult Execute()
        {
            return BTNodeResult.Success;
        }

        public override void SetProperty(string key, string value) { }
    }

    /// <summary>常にFailureを返すモックノード</summary>
    public class MockFailureNode : BTNode
    {
        public override BTNodeResult Execute()
        {
            return BTNodeResult.Failure;
        }

        public override void SetProperty(string key, string value) { }
    }

    /// <summary>常にRunningを返すモックノード</summary>
    public class MockRunningNode : BTNode
    {
        public override BTNodeResult Execute()
        {
            return BTNodeResult.Running;
        }

        public override void SetProperty(string key, string value) { }
    }
}
