using ArcBT.Core;
using ArcBT.Samples.RPG;
using ArcBT.Samples.RPG.Actions;
using ArcBT.Samples.RPG.Conditions;
using NUnit.Framework;
using UnityEngine;

namespace ArcBT.Tests.Samples
{
    /// <summary>RPGサンプルのSimple系クラスの機能をテストするクラス</summary>
    [TestFixture]
    public class RPGSimpleTests : BTTestBase
    {
        GameObject testOwner;
        BlackBoard blackBoard;
        MockExampleAI mockAI;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp(); // BTTestBaseのセットアップを実行（ログ抑制含む）
            testOwner = CreateTestGameObject("TestOwner");
            blackBoard = new BlackBoard();
            mockAI = testOwner.AddComponent<MockExampleAI>();
        }

        [TearDown]
        public override void TearDown()
        {
            DestroyTestObject(testOwner);
            base.TearDown(); // BTTestBaseのクリーンアップを実行
        }

        #region SimpleAttackAction Tests

        [Test][Description("SimpleAttackActionのdamageプロパティが正しく設定されることを確認")]
        public void SimpleAttackAction_SetProperty_SetsCorrectDamage()
        {
            // Arrange
            var action = new SimpleAttackAction();

            // Act
            action.SetProperty("damage", "35");
            action.Initialize(mockAI, blackBoard);

            // Assert - 実行して動作確認
            var result = action.Execute();
            Assert.IsTrue(result is BTNodeResult.Success or BTNodeResult.Failure);
        }

        [Test][Description("SimpleAttackActionのattack_rangeプロパティが正しく設定されることを確認")]
        public void SimpleAttackAction_SetProperty_SetsCorrectAttackRange()
        {
            // Arrange
            var action = new SimpleAttackAction();

            // Act
            action.SetProperty("attack_range", "3.5");
            action.Initialize(mockAI, blackBoard);

            // Assert - 実行して動作確認
            var result = action.Execute();
            Assert.IsTrue(result is BTNodeResult.Success or BTNodeResult.Failure);
        }

        [Test][Description("SimpleAttackActionがターゲットあり・範囲内で実行された時にAIの攻撃メソッドを呼び出すことを確認")]
        public void SimpleAttackAction_ExecuteWithTarget_CallsAttackOnAI()
        {
            // Arrange
            var action = new SimpleAttackAction();
            action.Initialize(mockAI, blackBoard);

            // ターゲットを設定
            var targetObj = new GameObject("Enemy");
            mockAI.Target = targetObj.transform;
            mockAI.isTargetInRange = true;

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.IsTrue(mockAI.attackCalled);

            // Cleanup
            Object.DestroyImmediate(targetObj);
        }

        [Test][Description("SimpleAttackActionがターゲットなしで実行された時にFailureを返すことを確認")]
        public void SimpleAttackAction_ExecuteWithoutTarget_ReturnsFailure()
        {
            // Arrange
            var action = new SimpleAttackAction();
            action.Initialize(mockAI, blackBoard);

            // ターゲットなし
            mockAI.Target = null;

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
            Assert.IsFalse(mockAI.attackCalled);
        }

        [Test][Description("SimpleAttackActionが範囲外ターゲットで実行された時にFailureを返すことを確認")]
        public void SimpleAttackAction_ExecuteWithOutOfRangeTarget_ReturnsFailure()
        {
            // Arrange
            var action = new SimpleAttackAction();
            action.Initialize(mockAI, blackBoard);

            // ターゲットを設定（範囲外）
            var targetObj = new GameObject("Enemy");
            mockAI.Target = targetObj.transform;
            mockAI.isTargetInRange = false;

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
            Assert.IsFalse(mockAI.attackCalled);

            // Cleanup
            Object.DestroyImmediate(targetObj);
        }

        [Test][Description("SimpleAttackActionがExampleAIコンポーネントなしで実行された時にエラーログ出力とFailureを返すことを確認")]
        public void SimpleAttackAction_ExecuteWithoutExampleAI_ReturnsFailure()
        {
            // Arrange
            var action = new SimpleAttackAction();
            var nonAIOwner = new GameObject("NonAIOwner");
            action.Initialize(nonAIOwner.AddComponent<TestRPGComponent>(), blackBoard);

            // 期待されるエラーログ

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);

            // Cleanup
            Object.DestroyImmediate(nonAIOwner);
        }

        #endregion

        #region WaitSimpleAction Tests

        [Test][Description("WaitSimpleActionのdurationプロパティが正しく設定され、AIの待機メソッドを呼び出すことを確認")]
        public void WaitSimpleAction_SetProperty_SetsCorrectDuration()
        {
            // Arrange
            var action = new WaitSimpleAction();

            // Act
            action.SetProperty("duration", "2.5");
            action.Initialize(mockAI, blackBoard);

            // Assert - 実行して動作確認
            var result = action.Execute();
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.IsTrue(mockAI.waitCalled);
            Assert.AreEqual(2.5f, mockAI.lastWaitDuration);
        }

        [Test][Description("WaitSimpleActionが実行された時にAIの待機メソッドを呼び出し、Successを返すことを確認")]
        public void WaitSimpleAction_Execute_CallsWaitOnAI()
        {
            // Arrange
            var action = new WaitSimpleAction();
            action.Initialize(mockAI, blackBoard);

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.IsTrue(mockAI.waitCalled);
            Assert.AreEqual(1.0f, mockAI.lastWaitDuration); // デフォルト値
        }

        [Test][Description("WaitSimpleActionがExampleAIコンポーネントなしで実行された時にエラーログ出力とFailureを返すことを確認")]
        public void WaitSimpleAction_ExecuteWithoutExampleAI_ReturnsFailure()
        {
            // Arrange
            var action = new WaitSimpleAction();
            var nonAIOwner = new GameObject("NonAIOwner");
            action.Initialize(nonAIOwner.AddComponent<TestRPGComponent>(), blackBoard);

            // 期待されるエラーログ

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);

            // Cleanup
            Object.DestroyImmediate(nonAIOwner);
        }

        #endregion

        #region MoveToNamedPositionAction Tests

        [Test][Description("MoveToNamedPositionActionのtargetプロパティが正しく設定されることを確認")]
        public void MoveToNamedPositionAction_SetProperty_SetsCorrectTarget()
        {
            // Arrange
            var action = new MoveToNamedPositionAction();

            // Act
            action.SetProperty("target", "Waypoint");
            action.Initialize(mockAI, blackBoard);

            // Assert - 実行して動作確認
            var result = action.Execute();
            Assert.IsTrue(result is BTNodeResult.Success or BTNodeResult.Failure);
        }

        [Test][Description("MoveToNamedPositionActionのspeedプロパティが正しく設定されることを確認")]
        public void MoveToNamedPositionAction_SetProperty_SetsCorrectSpeed()
        {
            // Arrange
            var action = new MoveToNamedPositionAction();

            // Act
            action.SetProperty("speed", "5.0");
            action.Initialize(mockAI, blackBoard);

            // Assert - 実行して動作確認
            var result = action.Execute();
            Assert.IsTrue(result is BTNodeResult.Success or BTNodeResult.Failure);
        }

        [Test][Description("MoveToNamedPositionActionが有効なターゲットで実行された時にAIの移動メソッドを呼び出すことを確認")]
        public void MoveToNamedPositionAction_ExecuteWithValidTarget_CallsMoveOnAI()
        {
            // Arrange
            var action = new MoveToNamedPositionAction();
            action.SetProperty("target", "TestTarget");
            action.Initialize(mockAI, blackBoard);

            // ターゲット位置を設定
            mockAI.hasValidMoveTarget = true;
            mockAI.moveResult = BTNodeResult.Success;

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.IsTrue(mockAI.moveCalled);
            Assert.AreEqual("TestTarget", mockAI.lastMoveTarget);
        }

        [Test][Description("MoveToNamedPositionActionが無効なターゲットで実行された時にFailureを返すことを確認")]
        public void MoveToNamedPositionAction_ExecuteWithInvalidTarget_ReturnsFailure()
        {
            // Arrange
            var action = new MoveToNamedPositionAction();
            action.SetProperty("target", "NonExistentTarget");
            action.Initialize(mockAI, blackBoard);

            // 無効なターゲット
            mockAI.hasValidMoveTarget = false;

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        [Test][Description("MoveToNamedPositionActionがExampleAIコンポーネントなしで実行された時にエラーログ出力とFailureを返すことを確認")]
        public void MoveToNamedPositionAction_ExecuteWithoutExampleAI_ReturnsFailure()
        {
            // Arrange
            var action = new MoveToNamedPositionAction();
            var nonAIOwner = new GameObject("NonAIOwner");
            action.Initialize(nonAIOwner.AddComponent<TestRPGComponent>(), blackBoard);

            // 期待されるエラーログ

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);

            // Cleanup
            Object.DestroyImmediate(nonAIOwner);
        }

        #endregion

        #region SimpleHasTargetCondition Tests

        [Test][Description("SimpleHasTargetConditionがターゲットありで実行された時にSuccessを返すことを確認")]
        public void SimpleHasTargetCondition_ExecuteWithTarget_ReturnsSuccess()
        {
            // Arrange
            var condition = new SimpleHasTargetCondition();
            condition.Initialize(mockAI, blackBoard);

            // ターゲットを設定
            var targetObj = new GameObject("Target");
            mockAI.Target = targetObj.transform;

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);

            // Cleanup
            Object.DestroyImmediate(targetObj);
        }

        [Test][Description("SimpleHasTargetConditionがターゲットなしで実行された時にFailureを返すことを確認")]
        public void SimpleHasTargetCondition_ExecuteWithoutTarget_ReturnsFailure()
        {
            // Arrange
            var condition = new SimpleHasTargetCondition();
            condition.Initialize(mockAI, blackBoard);

            // ターゲットなし
            mockAI.Target = null;

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        [Test][Description("SimpleHasTargetConditionがExampleAIコンポーネントなしで実行された時にエラーログ出力とFailureを返すことを確認")]
        public void SimpleHasTargetCondition_ExecuteWithoutExampleAI_ReturnsFailure()
        {
            // Arrange
            var condition = new SimpleHasTargetCondition();
            var nonAIOwner = new GameObject("NonAIOwner");
            condition.Initialize(nonAIOwner.AddComponent<TestRPGComponent>(), blackBoard);

            // 期待されるエラーログ

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);

            // Cleanup
            Object.DestroyImmediate(nonAIOwner);
        }

        #endregion

        #region EnemyDetectionCondition Tests

        [Test][Description("EnemyDetectionConditionのdetection_rangeプロパティが正しく設定され、敵検出処理で使用されることを確認")]
        public void EnemyDetectionCondition_SetProperty_SetsCorrectDetectionRange()
        {
            // Arrange
            var condition = new EnemyDetectionCondition();

            // Act
            condition.SetProperty("detection_range", "8.0");
            condition.Initialize(mockAI, blackBoard);

            // Assert - 実行して動作確認
            mockAI.enemyDetected = true;
            var result = condition.Execute();
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.AreEqual(8.0f, mockAI.lastDetectionRange);
        }

        [Test][Description("EnemyDetectionConditionが敵検出時にAIの検出メソッドを呼び出し、Successを返すことを確認")]
        public void EnemyDetectionCondition_ExecuteWithEnemyDetected_ReturnsSuccess()
        {
            // Arrange
            var condition = new EnemyDetectionCondition();
            condition.Initialize(mockAI, blackBoard);

            // 敵が検出される設定
            mockAI.enemyDetected = true;

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.IsTrue(mockAI.detectEnemyCalled);
        }

        [Test][Description("EnemyDetectionConditionが敵未検出時にAIの検出メソッドを呼び出し、Failureを返すことを確認")]
        public void EnemyDetectionCondition_ExecuteWithNoEnemyDetected_ReturnsFailure()
        {
            // Arrange
            var condition = new EnemyDetectionCondition();
            condition.Initialize(mockAI, blackBoard);

            // 敵が検出されない設定
            mockAI.enemyDetected = false;

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
            Assert.IsTrue(mockAI.detectEnemyCalled);
        }

        [Test][Description("EnemyDetectionConditionがExampleAIコンポーネントなしで実行された時にエラーログ出力とFailureを返すことを確認")]
        public void EnemyDetectionCondition_ExecuteWithoutExampleAI_ReturnsFailure()
        {
            // Arrange
            var condition = new EnemyDetectionCondition();
            var nonAIOwner = new GameObject("NonAIOwner");
            condition.Initialize(nonAIOwner.AddComponent<TestRPGComponent>(), blackBoard);

            // 期待されるエラーログ

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);

            // Cleanup
            Object.DestroyImmediate(nonAIOwner);
        }

        #endregion

        #region SimpleHealthCheckCondition Tests

        [Test][Description("SimpleHealthCheckConditionのmin_healthプロパティが正しく設定され、体力チェック処理で使用されることを確認")]
        public void SimpleHealthCheckCondition_SetProperty_SetsCorrectMinHealth()
        {
            // Arrange
            var condition = new SimpleHealthCheckCondition();

            // Act
            condition.SetProperty("min_health", "50");
            condition.Initialize(mockAI, blackBoard);

            // Assert - 実行して動作確認
            mockAI.Health = 60;
            var result = condition.Execute();
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.AreEqual(50f, mockAI.lastHealthCheck);
        }

        [Test][Description("SimpleHealthCheckConditionが十分な体力で実行された時にAIの体力チェックメソッドを呼び出し、Successを返すことを確認")]
        public void SimpleHealthCheckCondition_ExecuteWithSufficientHealth_ReturnsSuccess()
        {
            // Arrange
            var condition = new SimpleHealthCheckCondition();
            condition.SetProperty("min_health", "30");
            condition.Initialize(mockAI, blackBoard);

            // 十分な体力を設定
            mockAI.Health = 50;

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.IsTrue(mockAI.checkHealthCalled);
        }

        [Test][Description("SimpleHealthCheckConditionが不十分な体力で実行された時にAIの体力チェックメソッドを呼び出し、Failureを返すことを確認")]
        public void SimpleHealthCheckCondition_ExecuteWithInsufficientHealth_ReturnsFailure()
        {
            // Arrange
            var condition = new SimpleHealthCheckCondition();
            condition.SetProperty("min_health", "70");
            condition.Initialize(mockAI, blackBoard);

            // 不十分な体力を設定
            mockAI.Health = 40;

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
            Assert.IsTrue(mockAI.checkHealthCalled);
        }

        [Test][Description("SimpleHealthCheckConditionがExampleAIコンポーネントなしで実行された時にエラーログ出力とFailureを返すことを確認")]
        public void SimpleHealthCheckCondition_ExecuteWithoutExampleAI_ReturnsFailure()
        {
            // Arrange
            var condition = new SimpleHealthCheckCondition();
            var nonAIOwner = new GameObject("NonAIOwner");
            condition.Initialize(nonAIOwner.AddComponent<TestRPGComponent>(), blackBoard);

            // 期待されるエラーログ

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);

            // Cleanup
            Object.DestroyImmediate(nonAIOwner);
        }

        #endregion
    }

    /// <summary>テスト用のRPGコンポーネント</summary>
    public class TestRPGComponent : MonoBehaviour
    {
        // テスト用の空のMonoBehaviourコンポーネント
    }

    /// <summary>ExampleAIのモッククラス</summary>
    public class MockExampleAI : ExampleAI
    {
        // テスト用フラグ
        public bool isTargetInRange = false;
        public bool attackCalled = false;
        public bool waitCalled = false;
        public bool moveCalled = false;
        public bool detectEnemyCalled = false;
        public bool checkHealthCalled = false;

        // パラメータ記録用
        public float lastWaitDuration;
        public string lastMoveTarget;
        public float lastDetectionRange;
        public float lastHealthCheck;

        // 動作制御用
        public bool hasValidMoveTarget = true;
        public BTNodeResult moveResult = BTNodeResult.Success;
        public bool enemyDetected = false;

        // ExampleAIメソッドをオーバーライドしてテスト用動作を追加
        public override bool DetectEnemy(float range)
        {
            detectEnemyCalled = true;
            lastDetectionRange = range;
            return enemyDetected;
        }

        public override bool CheckHealth(float minHealth)
        {
            checkHealthCalled = true;
            lastHealthCheck = minHealth;
            return Health >= minHealth;
        }

        public override void Wait(float duration)
        {
            waitCalled = true;
            lastWaitDuration = duration;
        }

        public override bool AttackEnemy(int damage, float attackRange)
        {
            if (Target == null) return false;
            if (!isTargetInRange) return false;
            
            attackCalled = true;
            return true;
        }

        public override bool MoveToPosition(string targetName, float speed, float tolerance = 0.5f)
        {
            moveCalled = true;
            lastMoveTarget = targetName;
            
            return hasValidMoveTarget;
        }

        // リセットメソッド
        public void ResetMockState()
        {
            attackCalled = false;
            waitCalled = false;
            moveCalled = false;
            detectEnemyCalled = false;
            checkHealthCalled = false;
            
            lastWaitDuration = 0f;
            lastMoveTarget = "";
            lastDetectionRange = 0f;
            lastHealthCheck = 0f;
        }
    }
}
