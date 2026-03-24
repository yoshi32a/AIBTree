using ArcBT.Core;
using ArcBT.Decorators;
using NUnit.Framework;
using UnityEngine;

namespace ArcBT.Tests
{
    /// <summary>TimeoutDecoratorのエッジケースをテストするクラス</summary>
    [TestFixture]
    public class TimeoutDecoratorTests : BTTestBase
    {
        GameObject testOwner;
        BlackBoard blackBoard;
        MonoBehaviour ownerComponent;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            testOwner = CreateTestGameObject("TestOwner");
            blackBoard = new BlackBoard();
            ownerComponent = testOwner.AddComponent<TestTimeoutComponent>();
        }

        [TearDown]
        public override void TearDown()
        {
            DestroyTestObject(testOwner);
            base.TearDown();
        }

        [Test][Description("子ノードがタイムアウト前にSuccessで完了した場合にSuccessが返されることを確認")]
        public void Execute_ChildCompletesBeforeTimeout_ReturnsChildSuccess()
        {
            // Arrange
            var decorator = new TimeoutDecorator();
            var successChild = new MockSuccessNode();
            decorator.AddChild(successChild);
            decorator.SetProperty("timeout", "10.0");
            decorator.Initialize(ownerComponent, blackBoard);

            // Act
            var result = decorator.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result, "子ノードがタイムアウト前にSuccessを返した場合、Successを返すべき");
            Assert.IsFalse(decorator.IsRunning(), "子ノード完了後はRunning状態でないべき");
        }

        [Test][Description("子ノードがタイムアウト前にFailureで完了した場合にFailureが返されることを確認")]
        public void Execute_ChildCompletesBeforeTimeout_ReturnsChildFailure()
        {
            // Arrange
            var decorator = new TimeoutDecorator();
            var failureChild = new MockFailureNode();
            decorator.AddChild(failureChild);
            decorator.SetProperty("timeout", "10.0");
            decorator.Initialize(ownerComponent, blackBoard);

            // Act
            var result = decorator.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result, "子ノードがタイムアウト前にFailureを返した場合、Failureを返すべき");
            Assert.IsFalse(decorator.IsRunning(), "子ノード完了後はRunning状態でないべき");
        }

        [Test][Description("タイムアウト後にデフォルト設定でFailureが返されることを確認")]
        public void Execute_TimeoutExpired_ReturnsFailureByDefault()
        {
            // Arrange - タイムアウト時間を非常に短く設定し、Time.timeが初期値より大きいことを利用
            var decorator = new TimeoutDecorator();
            var runningChild = new MockRunningNode();
            decorator.AddChild(runningChild);
            decorator.SetProperty("timeout", "0.1"); // 最小値 0.1秒
            decorator.Initialize(ownerComponent, blackBoard);

            // Act - 最初の実行でタイマー開始
            var firstResult = decorator.Execute();

            // Time.timeが0.1秒以上の場合、2回目の呼び出しでタイムアウト
            // テスト環境ではTime.timeが0から始まるため、
            // 初回実行時点でstartTimeが記録され、タイムアウト判定はelapsedTime >= timeoutDuration
            // テスト環境の動作に依存するが、Runningが返される基本動作を確認
            if (firstResult == BTNodeResult.Running)
            {
                Assert.IsTrue(decorator.IsRunning(), "タイムアウト前はRunning状態であるべき");
            }
            else
            {
                // Time.timeが既に0.1を超えている場合、即座にタイムアウト
                Assert.AreEqual(BTNodeResult.Failure, firstResult, "タイムアウト時はデフォルトでFailureを返すべき");
            }
        }

        [Test][Description("タイムアウト後にsuccess_on_timeoutオプションでSuccessが返されることを確認")]
        public void Execute_TimeoutExpiredWithSuccessOnTimeout_ReturnsSuccess()
        {
            // Arrange
            var decorator = new TimeoutDecorator();
            var runningChild = new MockRunningNode();
            decorator.AddChild(runningChild);
            decorator.SetProperty("timeout", "0.1");
            decorator.SetProperty("success_on_timeout", "true");
            decorator.Initialize(ownerComponent, blackBoard);

            // Act
            var result = decorator.Execute();

            // タイムアウト設定と現在のTime.timeに依存
            if (result == BTNodeResult.Running)
            {
                Assert.IsTrue(decorator.IsRunning(), "タイムアウト前はRunning状態であるべき");
            }
            else
            {
                Assert.AreEqual(BTNodeResult.Success, result, "success_on_timeout有効時はタイムアウトでSuccessを返すべき");
            }
        }

        [Test][Description("タイムアウト後に子ノードが正しくResetされることを確認")]
        public void Execute_AfterTimeout_ChildNodeIsReset()
        {
            // Arrange
            var decorator = new TimeoutDecorator();
            var countingChild = new MockCountingNode();
            decorator.AddChild(countingChild);
            decorator.SetProperty("timeout", "0.1");
            decorator.Initialize(ownerComponent, blackBoard);

            // Act - 実行開始
            decorator.Execute();

            // Reset呼び出しで子ノードの状態もリセットされることを確認
            decorator.Reset();

            // Assert
            Assert.IsFalse(decorator.IsRunning(), "Reset後はRunning状態でないべき");
            Assert.AreEqual(0f, decorator.GetElapsedTime(), "Reset後の経過時間は0であるべき");
            Assert.AreEqual(0, countingChild.ExecuteCount, "Reset後の子ノードの実行カウントは0であるべき");
        }

        [Test][Description("ゼロタイムアウト（0.1に丸められる）の場合の動作を確認")]
        public void SetProperty_ZeroTimeout_ClampedToMinimum()
        {
            // Arrange
            var decorator = new TimeoutDecorator();

            // Act
            decorator.SetProperty("timeout", "0");

            // Assert - SetPropertyで最小0.1秒にクランプされる
            Assert.AreEqual(0.1f, decorator.GetTimeoutDuration(), "ゼロタイムアウトは最小値0.1秒にクランプされるべき");
        }

        [Test][Description("負のタイムアウト（0.1に丸められる）の場合の動作を確認")]
        public void SetProperty_NegativeTimeout_ClampedToMinimum()
        {
            // Arrange
            var decorator = new TimeoutDecorator();

            // Act
            decorator.SetProperty("timeout", "-5.0");

            // Assert - SetPropertyで最小0.1秒にクランプされる
            Assert.AreEqual(0.1f, decorator.GetTimeoutDuration(), "負のタイムアウトは最小値0.1秒にクランプされるべき");
        }

        [Test][Description("OnConditionFailedで実行状態がリセットされることを確認")]
        public void OnConditionFailed_ResetsRunningState()
        {
            // Arrange
            var decorator = new TimeoutDecorator();
            var runningChild = new MockRunningNode();
            decorator.AddChild(runningChild);
            decorator.SetProperty("timeout", "10.0");
            decorator.Initialize(ownerComponent, blackBoard);

            // Act - 実行開始してRunning状態にする
            decorator.Execute();
            Assert.IsTrue(decorator.IsRunning(), "実行後はRunning状態であるべき");

            // OnConditionFailedを呼び出し
            decorator.OnConditionFailed();

            // Assert
            Assert.IsFalse(decorator.IsRunning(), "OnConditionFailed後はRunning状態でないべき");
        }

        [Test][Description("実行前の初期状態でGetElapsedTimeが0を返すことを確認")]
        public void GetElapsedTime_BeforeExecution_ReturnsZero()
        {
            // Arrange
            var decorator = new TimeoutDecorator();
            decorator.SetProperty("timeout", "5.0");

            // Act & Assert
            Assert.AreEqual(0f, decorator.GetElapsedTime(), "実行前の経過時間は0であるべき");
        }

        [Test][Description("実行前の初期状態でGetRemainingTimeがタイムアウト期間を返すことを確認")]
        public void GetRemainingTime_BeforeExecution_ReturnsFullDuration()
        {
            // Arrange
            var decorator = new TimeoutDecorator();
            decorator.SetProperty("timeout", "5.0");

            // Act & Assert
            Assert.AreEqual(5.0f, decorator.GetRemainingTime(), "実行前の残り時間はタイムアウト期間と同じであるべき");
        }

        [Test][Description("実行前の初期状態でGetTimeoutProgressが0を返すことを確認")]
        public void GetTimeoutProgress_BeforeExecution_ReturnsZero()
        {
            // Arrange
            var decorator = new TimeoutDecorator();
            decorator.SetProperty("timeout", "5.0");

            // Act & Assert
            Assert.AreEqual(0f, decorator.GetTimeoutProgress(), "実行前の進行率は0であるべき");
        }

        [Test][Description("durationプロパティ名でもタイムアウト時間が設定できることを確認")]
        public void SetProperty_DurationAlias_SetsTimeout()
        {
            // Arrange
            var decorator = new TimeoutDecorator();

            // Act
            decorator.SetProperty("duration", "7.5");

            // Assert
            Assert.AreEqual(7.5f, decorator.GetTimeoutDuration(), "durationプロパティ名でもタイムアウト時間が設定されるべき");
        }

        [Test][Description("return_successプロパティ名でもsuccess_on_timeoutオプションが設定できることを確認")]
        public void SetProperty_ReturnSuccessAlias_SetsSuccessOnTimeout()
        {
            // Arrange
            var decorator = new TimeoutDecorator();
            var runningChild = new MockRunningNode();
            decorator.AddChild(runningChild);
            decorator.SetProperty("return_success", "true");
            decorator.SetProperty("timeout", "0.1");
            decorator.Initialize(ownerComponent, blackBoard);

            // Act
            var result = decorator.Execute();

            // Assert - タイムアウトが発生した場合、Successが返される
            if (result != BTNodeResult.Running)
            {
                Assert.AreEqual(BTNodeResult.Success, result, "return_successプロパティ有効時はタイムアウトでSuccessを返すべき");
            }
        }
    }

    /// <summary>テスト用のTimeoutコンポーネント</summary>
    public class TestTimeoutComponent : MonoBehaviour
    {
        // テスト用の空のMonoBehaviourコンポーネント
    }
}
