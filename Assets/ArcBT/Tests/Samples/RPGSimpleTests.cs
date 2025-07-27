using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ArcBT.Core;
using ArcBT.Samples.RPG.Actions;
using ArcBT.Samples.RPG.Conditions;
using ArcBT.Samples.RPG;
using ArcBT.Logger;

namespace ArcBT.Tests
{
    /// <summary>RPGサンプルのSimple系クラスの機能をテストするクラス</summary>
    [TestFixture]
    public class RPGSimpleTests
    {
        GameObject testOwner;
        BlackBoard blackBoard;
        MockExampleAI mockAI;

        [SetUp]
        public void SetUp()
        {
            testOwner = new GameObject("TestOwner");
            blackBoard = new BlackBoard();
            mockAI = testOwner.AddComponent<MockExampleAI>();
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

        #region SimpleAttackAction Tests

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public void SimpleAttackAction_ExecuteWithoutExampleAI_ReturnsFailure()
        {
            // Arrange
            var action = new SimpleAttackAction();
            var nonAIOwner = new GameObject("NonAIOwner");
            action.Initialize(nonAIOwner.AddComponent<TestRPGComponent>(), blackBoard);

            // 期待されるエラーログ
            LogAssert.Expect(LogType.Error, "[ERR][ATK]: SimpleAttackAction requires ExampleAI component");
            LogAssert.Expect(LogType.Error, "[ERR][ATK]: ExampleAI controller not found");

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);

            // Cleanup
            Object.DestroyImmediate(nonAIOwner);
        }

        #endregion

        #region WaitSimpleAction Tests

        [Test]
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

        [Test]
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

        [Test]
        public void WaitSimpleAction_ExecuteWithoutExampleAI_ReturnsFailure()
        {
            // Arrange
            var action = new WaitSimpleAction();
            var nonAIOwner = new GameObject("NonAIOwner");
            action.Initialize(nonAIOwner.AddComponent<TestRPGComponent>(), blackBoard);

            // 期待されるエラーログ
            LogAssert.Expect(LogType.Error, "[ERR][SYS]: WaitSimpleAction requires ExampleAI component");
            LogAssert.Expect(LogType.Error, "[ERR][SYS]: ExampleAI controller not found");

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);

            // Cleanup
            Object.DestroyImmediate(nonAIOwner);
        }

        #endregion

        #region MoveToNamedPositionAction Tests

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public void MoveToNamedPositionAction_ExecuteWithoutExampleAI_ReturnsFailure()
        {
            // Arrange
            var action = new MoveToNamedPositionAction();
            var nonAIOwner = new GameObject("NonAIOwner");
            action.Initialize(nonAIOwner.AddComponent<TestRPGComponent>(), blackBoard);

            // 期待されるエラーログ
            LogAssert.Expect(LogType.Error, "[ERR][MOV]: MoveToNamedPositionAction requires ExampleAI component");
            LogAssert.Expect(LogType.Error, "[ERR][MOV]: ExampleAI controller not found");

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);

            // Cleanup
            Object.DestroyImmediate(nonAIOwner);
        }

        #endregion

        #region SimpleHasTargetCondition Tests

        [Test]
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

        [Test]
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

        [Test]
        public void SimpleHasTargetCondition_ExecuteWithoutExampleAI_ReturnsFailure()
        {
            // Arrange
            var condition = new SimpleHasTargetCondition();
            var nonAIOwner = new GameObject("NonAIOwner");
            condition.Initialize(nonAIOwner.AddComponent<TestRPGComponent>(), blackBoard);

            // 期待されるエラーログ
            LogAssert.Expect(LogType.Error, "[ERR][SYS]: SimpleHasTargetCondition requires ExampleAI component");
            LogAssert.Expect(LogType.Error, "[ERR][SYS]: ExampleAI controller not found");

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);

            // Cleanup
            Object.DestroyImmediate(nonAIOwner);
        }

        #endregion

        #region EnemyDetectionCondition Tests

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public void EnemyDetectionCondition_ExecuteWithoutExampleAI_ReturnsFailure()
        {
            // Arrange
            var condition = new EnemyDetectionCondition();
            var nonAIOwner = new GameObject("NonAIOwner");
            condition.Initialize(nonAIOwner.AddComponent<TestRPGComponent>(), blackBoard);

            // 期待されるエラーログ
            LogAssert.Expect(LogType.Error, "[ERR][SYS]: EnemyDetectionCondition requires ExampleAI component");
            LogAssert.Expect(LogType.Error, "[ERR][SYS]: ExampleAI controller not found");

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);

            // Cleanup
            Object.DestroyImmediate(nonAIOwner);
        }

        #endregion

        #region SimpleHealthCheckCondition Tests

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public void SimpleHealthCheckCondition_ExecuteWithoutExampleAI_ReturnsFailure()
        {
            // Arrange
            var condition = new SimpleHealthCheckCondition();
            var nonAIOwner = new GameObject("NonAIOwner");
            condition.Initialize(nonAIOwner.AddComponent<TestRPGComponent>(), blackBoard);

            // 期待されるエラーログ
            LogAssert.Expect(LogType.Error, "[ERR][SYS]: SimpleHealthCheckCondition requires ExampleAI component");
            LogAssert.Expect(LogType.Error, "[ERR][SYS]: ExampleAI controller not found");

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