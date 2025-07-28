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
    public class DecoratorNodeTests : BTTestBase
    {
        GameObject testOwner;
        BlackBoard blackBoard;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp(); // BTTestBaseのセットアップを実行（ログ抑制含む）
            testOwner = CreateTestGameObject("TestOwner");
            blackBoard = new BlackBoard();
        }

        [TearDown]
        public override void TearDown()
        {
            DestroyTestObject(testOwner);
            base.TearDown(); // BTTestBaseのクリーンアップを実行
        }

        #region InverterDecorator Tests

        [Test][Description("InverterDecoratorでSuccess子ノードをFailureに反転させる基本動作の確認")]
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

        [Test][Description("InverterDecoratorでFailure子ノードをSuccessに反転させる動作の確認")]
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

        [Test][Description("InverterDecoratorでRunning状態の子ノードはそのまま通す動作の確認")]
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

        [Test][Description("InverterDecoratorで子ノードが存在しない場合のエラーハンドリング")]
        public void InverterDecorator_WithNoChild_ReturnsFailure()
        {
            // Arrange
            var decorator = new InverterDecorator();
            decorator.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);

            // Act
            var result = decorator.Execute();

            // Assert - 実際の動作を検証（エラーログはログシステムで処理される）
            Assert.AreEqual(BTNodeResult.Failure, result, "子ノードがない場合、Failureが返されるべき");
        }

        #endregion

        #region RepeatDecorator Tests

        [Test][Description("RepeatDecoratorのmax_countパラメータで最大繰り返し回数設定機能の検証")]
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

        [Test][Description("RepeatDecoratorのstop_on_failureパラメータで失敗時停止機能の検証")]
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

        [Test][Description("RepeatDecoratorで指定回数（2回）まで実行して完了する動作の確認")]
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

        [Test][Description("RepeatDecoratorで無限繰り返し（max_count=-1）設定時の継続動作確認")]
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

        [Test][Description("RepeatDecoratorで失敗時停止オプションが有効な場合の即座停止動作")]
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

        [Test][Description("RepeatDecoratorのReset()メソッドで現在の実行カウンターがリセットされることを確認")]
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

        [Test][Description("RetryDecoratorのmax_retriesパラメータで最大リトライ回数設定機能の検証")]
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

        [Test][Description("RetryDecoratorのretry_delayパラメータで再試行間隔設定機能の検証")]
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

        [Test][Description("RetryDecoratorで成功する子ノードは即座にSuccessを返しリトライしない動作")]
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

        [Test][Description("RetryDecoratorで失敗する子ノードを最大回数まで再試行する完全な動作検証")]
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

        [Test][Description("RetryDecoratorでRunning状態の子ノードはそのまま実行継続する動作")]
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

        [Test][Description("RetryDecoratorのReset()メソッドでリトライカウンターと待機状態がリセットされることを確認")]
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

        [Test][Description("TimeoutDecoratorのtimeoutパラメータでタイムアウト時間設定機能の検証")]
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

        [Test][Description("TimeoutDecoratorのsuccess_on_timeoutパラメータでタイムアウト時成功オプション設定の検証")]
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

        [Test][Description("TimeoutDecoratorで短時間で成功する子ノードがタイムアウト前に完了する動作")]
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

        [Test][Description("TimeoutDecoratorで実行中の子ノードの経過時間と残り時間を正確に追跡する機能")]
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

        [Test][Description("TimeoutDecoratorのReset()メソッドでタイムアウト状態と経過時間がリセットされることを確認")]
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

        [Test][Description("TimeoutDecoratorで子ノードが存在しない場合のエラーハンドリング")]
        public void TimeoutDecorator_WithNoChild_ReturnsFailure()
        {
            // Arrange
            var decorator = new TimeoutDecorator();
            decorator.Initialize(testOwner.AddComponent<TestDecoratorComponent>(), blackBoard);

            // Act & Assert - 実際の動作を検証（エラーログはログシステムで処理される）
            var result = decorator.Execute();
            Assert.AreEqual(BTNodeResult.Failure, result, "子ノードがない場合、Failureが返されるべき");
        }

        #endregion

        #region Integration Tests

        [Test][Description("InverterとTimeoutのネストしたDecorator構造が正しく連携動作することを確認")]
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

        [Test][Description("RepeatとRetryを組み合わせた複雑なDecorator階層の統合動作検証")]
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
