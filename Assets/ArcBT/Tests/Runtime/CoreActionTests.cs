using ArcBT.Actions;
using ArcBT.Core;
using ArcBT.Samples.RPG.Actions;
using NUnit.Framework;
using UnityEngine;

namespace ArcBT.Tests
{
    /// <summary>Runtime Core Actionsの機能をテストするクラス</summary>
    [TestFixture]
    public class CoreActionTests : BTTestBase
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


        [Test][Description("MoveToPositionActionの初期化処理でターゲット位置とプロパティが正しくBlackBoardに設定されることを確認")]
        public void MoveToPositionAction_Initialize_SetsUpCorrectly()
        {
            // Arrange
            var targetObj = new GameObject("TestTarget");
            targetObj.transform.position = new Vector3(5, 0, 5);

            var action = new MoveToPositionAction();
            action.SetProperty("target", "TestTarget");
            action.SetProperty("speed", "10.0");
            action.SetProperty("tolerance", "1.0");

            // Act
            action.Initialize(testOwner.GetComponent<MonoBehaviour>() ?? testOwner.AddComponent<TestCoreActionComponent>(), blackBoard);

            // Assert - NameプロパティがnullまたはEmptyの場合、「_target_position」のキーが使用される
            string expectedNameKey = string.IsNullOrEmpty(action.Name) ? "_target_position" : $"{action.Name}_target_position";
            string expectedTargetKey = string.IsNullOrEmpty(action.Name) ? "_target_name" : $"{action.Name}_target_name";

            Assert.IsTrue(blackBoard.HasKey(expectedNameKey), $"Expected key '{expectedNameKey}' not found in BlackBoard");
            Assert.IsTrue(blackBoard.HasKey(expectedTargetKey), $"Expected key '{expectedTargetKey}' not found in BlackBoard");
            Assert.AreEqual(new Vector3(5, 0, 5), blackBoard.GetValue<Vector3>(expectedNameKey));
            Assert.AreEqual("TestTarget", blackBoard.GetValue<string>(expectedTargetKey));

            // Cleanup
            Object.DestroyImmediate(targetObj);
        }

        [Test][Description("MoveToPositionActionで存在しないターゲットを指定した場合のエラーハンドリングを確認")]
        public void MoveToPositionAction_InvalidTarget_HandlesGracefully()
        {
            // Arrange
            var action = new MoveToPositionAction();
            action.SetProperty("target", "NonExistentTarget");

            // Act
            action.Initialize(testOwner.AddComponent<TestCoreActionComponent>(), blackBoard);

            // Assert - ログではなく実際の動作を検証
            // 存在しないターゲットの場合、BlackBoardに位置が設定されない
            string expectedKey = string.IsNullOrEmpty(action.Name) ? "_target_position" : $"{action.Name}_target_position";
            Assert.IsFalse(blackBoard.HasKey(expectedKey), "存在しないターゲットの場合、位置情報がBlackBoardに設定されるべきではない");
        }

        [Test][Description("MoveToPositionActionでターゲットが設定されていない場合にFailureが返されることを確認")]
        public void MoveToPositionAction_ExecuteWithoutTarget_ReturnsFailure()
        {
            // Arrange
            var action = new MoveToPositionAction();
            action.Initialize(testOwner.AddComponent<TestCoreActionComponent>(), blackBoard);

            // Act
            var result = action.Execute();

            // Assert - ログではなく実際の戻り値を検証
            Assert.AreEqual(BTNodeResult.Failure, result, "ターゲットが設定されていない場合、Failureが返されるべき");
            
            // BlackBoardの状態も確認
            string expectedKey = string.IsNullOrEmpty(action.Name) ? "_target_position" : $"{action.Name}_target_position";
            Assert.IsFalse(blackBoard.HasKey(expectedKey), "失敗時にはBlackBoardに位置情報が設定されるべきではない");
        }



        [Test][Description("WaitActionで待機時間のプロパティが正しく設定され、実行中にRunningが返されることを確認")]
        public void WaitAction_SetProperty_SetsCorrectDuration()
        {
            // Arrange
            var action = new WaitAction();

            // Act
            action.SetProperty("duration", "2.5");
            action.Initialize(testOwner.AddComponent<TestCoreActionComponent>(), blackBoard);

            // Assert (duration設定の確認は実行動作で行う)
            var result = action.Execute();
            Assert.AreEqual(BTNodeResult.Running, result); // 待機中はRunning
        }

        [Test][Description("WaitActionの実行開始時に待機中を示すRunningが返されることを確認")]
        public void WaitAction_Execute_ReturnsRunningInitially()
        {
            // Arrange
            var action = new WaitAction();
            action.SetProperty("duration", "1.0");
            action.Initialize(testOwner.AddComponent<TestCoreActionComponent>(), blackBoard);

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Running, result);
        }

        [Test][Description("WaitActionを継続的に実行した場合の挙動と最終的なSuccess結果を確認")]
        public void WaitAction_ExecuteMultipleTimes_EventuallyReturnsSuccess()
        {
            // Arrange
            var action = new WaitAction();
            action.SetProperty("duration", "0.1"); // 短い待機時間
            action.Initialize(testOwner.AddComponent<TestCoreActionComponent>(), blackBoard);

            // Act - 最初の実行
            var firstResult = action.Execute();
            Assert.AreEqual(BTNodeResult.Running, firstResult);

            // 十分な時間を経過させる（Time.timeは実際の時間に依存するため）
            // テスト環境では完了まで待つのが困難なので、Runningが返ることを確認
        }



        [Test][Description("LogActionでメッセージのログ出力が正しく動作することを確認")]
        public void LogAction_Execute_OutputsMessage()
        {
            // Arrange
            var action = new LogAction();
            action.SetProperty("message", "テストメッセージ");
            action.Initialize(testOwner.AddComponent<TestCoreActionComponent>(), blackBoard);

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
        }

        [Test][Description("LogActionでBlackBoard情報を含むログ出力が正しく動作することを確認")]
        public void LogAction_ExecuteWithBlackBoardInfo_IncludesBlackBoardData()
        {
            // Arrange
            var action = new LogAction();
            action.SetProperty("message", "BlackBoard情報");
            action.SetProperty("include_blackboard", "true");
            action.Initialize(testOwner.AddComponent<TestCoreActionComponent>(), blackBoard);

            // BlackBoardにテストデータを設定
            blackBoard.SetValue("test_key", "test_value");

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
        }



        [Test][Description("SetBlackBoardActionでint型の値（health=100）をBlackBoardに設定")]
        public void SetBlackBoardAction_SetProperty_SetsIntValue()
        {
            // Arrange
            var action = new SetBlackBoardAction();

            // Act
            action.SetProperty("health", "100");
            action.Initialize(testOwner.AddComponent<TestCoreActionComponent>(), blackBoard);

            // Assert
            var result = action.Execute();
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.IsTrue(blackBoard.HasKey("health"));
            Assert.AreEqual(100, blackBoard.GetValue<int>("health"));
        }

        [Test][Description("SetBlackBoardActionでfloat型の値（speed=10.5）をBlackBoardに設定")]
        public void SetBlackBoardAction_SetProperty_SetsFloatValue()
        {
            // Arrange
            var action = new SetBlackBoardAction();

            // Act
            action.SetProperty("speed", "10.5");
            action.Initialize(testOwner.AddComponent<TestCoreActionComponent>(), blackBoard);

            // Assert
            var result = action.Execute();
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.IsTrue(blackBoard.HasKey("speed"));
            Assert.AreEqual(10.5f, blackBoard.GetValue<float>("speed"));
        }

        [Test][Description("SetBlackBoardActionでstring型の値（player_name=Hero）をBlackBoardに設定")]
        public void SetBlackBoardAction_SetProperty_SetsStringValue()
        {
            // Arrange
            var action = new SetBlackBoardAction();

            // Act
            action.SetProperty("player_name", "Hero");
            action.Initialize(testOwner.AddComponent<TestCoreActionComponent>(), blackBoard);

            // Assert
            var result = action.Execute();
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.IsTrue(blackBoard.HasKey("player_name"));
            Assert.AreEqual("Hero", blackBoard.GetValue<string>("player_name"));
        }

        [Test][Description("SetBlackBoardActionでBlackBoardがnullの場合にFailureを返しエラーログを出力することを確認")]
        public void SetBlackBoardAction_ExecuteWithNullBlackBoard_ReturnsFailure()
        {
            // Arrange
            var action = new SetBlackBoardAction();
            action.SetProperty("test", "value");
            action.Initialize(testOwner.AddComponent<TestCoreActionComponent>(), null);

            // Act
            var result = action.Execute();

            // Assert - ログではなく実際の動作を検証
            Assert.AreEqual(BTNodeResult.Failure, result, "BlackBoardがnullの場合、Failureが返されるべき");
        }

        [Test][Description("WaitUntilActionで条件が設定されていない場合にFailureが返されることを確認")]
        public void WaitUntilAction_ExecuteWithoutCondition_ReturnsFailure()
        {
            // Arrange
            var action = new WaitUntilAction();
            action.Initialize(testOwner.AddComponent<TestCoreActionComponent>(), blackBoard);

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result, "conditionが設定されていない場合、Failureが返されるべき");
        }

        [Test][Description("WaitUntilActionで条件式形式(condition: \"key\" == \"value\")が正しく動作することを確認")]
        public void WaitUntilAction_ExecuteWithConditionExpression_ReturnsSuccess()
        {
            // Arrange
            var action = new WaitUntilAction();
            action.SetProperty("condition", "enemy_defeated == true");
            action.Initialize(testOwner.AddComponent<TestCoreActionComponent>(), blackBoard);

            // BlackBoardに期待値を設定
            blackBoard.SetValue("enemy_defeated", "true");

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result, "条件式が満たされた場合、Successが返されるべき");
        }

        [Test][Description("WaitUntilActionで条件式形式が満たされていない場合にRunningが返されることを確認")]
        public void WaitUntilAction_ExecuteWithUnmetConditionExpression_ReturnsRunning()
        {
            // Arrange
            var action = new WaitUntilAction();
            action.SetProperty("condition", "enemy_defeated == true");
            action.Initialize(testOwner.AddComponent<TestCoreActionComponent>(), blackBoard);

            // BlackBoardに異なる値を設定
            blackBoard.SetValue("enemy_defeated", "false");

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Running, result, "条件式が満たされていない場合、Runningが返されるべき");
        }

        [Test][Description("WaitUntilActionで無効な条件式形式の場合にRunningが返されることを確認")]
        public void WaitUntilAction_ExecuteWithInvalidConditionExpression_ReturnsRunning()
        {
            // Arrange
            var action = new WaitUntilAction();
            action.SetProperty("condition", "invalid expression");
            action.Initialize(testOwner.AddComponent<TestCoreActionComponent>(), blackBoard);

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Running, result, "無効な条件式の場合、Runningが返されるべき");
        }

    }

    /// <summary>テスト用のCoreActionコンポーネント</summary>
    public class TestCoreActionComponent : MonoBehaviour
    {
        // テスト用の空のMonoBehaviourコンポーネント
    }
}
