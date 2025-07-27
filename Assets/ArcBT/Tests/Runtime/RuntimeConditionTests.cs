using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ArcBT.Core;
using ArcBT.Conditions;
using ArcBT.Logger;
using ArcBT.TagSystem;

namespace ArcBT.Tests
{
    /// <summary>Runtime Conditionsの機能をテストするクラス</summary>
    [TestFixture]
    public class RuntimeConditionTests
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

        #region ScanForInterestCondition Tests

        [Test]
        public void ScanForInterestCondition_SetProperty_SetsCorrectRange()
        {
            // Arrange - Runtime版ScanForInterestCondition
            var condition = new ScanForInterestCondition();

            // Act
            condition.SetProperty("scan_range", "8.0");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // Assert - 実行して動作確認
            var result = condition.Execute();
            Assert.AreEqual(BTNodeResult.Failure, result); // アイテムがないのでFailure
        }

        [Test]
        public void ScanForInterestCondition_ExecuteWithInterestObject_ReturnsSuccess()
        {
            // Arrange - Runtime版ScanForInterestCondition (GameplayTagManagerベース)
            var condition = new ScanForInterestCondition();
            condition.SetProperty("scan_range", "10.0");
            condition.SetProperty("interest_tag", "Object.Item");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // 近くにアイテムオブジェクトを配置
            var itemObj = new GameObject("TestItem");
            var tagComponent = itemObj.AddComponent<GameplayTagComponent>();
            tagComponent.AddTag(new GameplayTag("Object.Item"));
            itemObj.transform.position = testOwner.transform.position + Vector3.forward * 3f;

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.IsTrue(blackBoard.HasKey("interest_object"));
            Assert.IsTrue(blackBoard.HasKey("interest_distance"));
            Assert.AreEqual(itemObj, blackBoard.GetValue<GameObject>("interest_object"));

            // Cleanup
            Object.DestroyImmediate(itemObj);
        }

        #endregion

        #region RandomCondition Tests

        [Test]
        public void RandomCondition_SetProperty_SetsCorrectProbability()
        {
            // Arrange
            var condition = new RandomCondition();

            // Act
            condition.SetProperty("probability", "0.7");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // Assert - 確率的なテストなので複数回実行して統計的にチェック
            int successCount = 0;
            int trials = 100;

            for (int i = 0; i < trials; i++)
            {
                var result = condition.Execute();
                if (result == BTNodeResult.Success)
                {
                    successCount++;
                }
            }

            // 確率0.7なので、100回中60～80回程度の成功を期待
            Assert.IsTrue(successCount >= 50 && successCount <= 90, 
                $"期待される成功率: 60-80%, 実際: {successCount}/100");
        }

        [Test]
        public void RandomCondition_SetProperty_SetsCorrectSeed()
        {
            // Arrange
            var condition1 = new RandomCondition();
            var condition2 = new RandomCondition();

            // Act - 同じシードを設定
            condition1.SetProperty("seed", "12345");
            condition1.SetProperty("probability", "0.5");
            condition1.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            condition2.SetProperty("seed", "12345");
            condition2.SetProperty("probability", "0.5");
            condition2.Initialize(testOwner.GetComponent<TestConditionComponent>(), blackBoard);

            // Assert - 同じシードなので同じ結果を期待
            var result1 = condition1.Execute();
            var result2 = condition2.Execute();
            Assert.AreEqual(result1, result2);
        }

        [Test]
        public void RandomCondition_ExecuteWithBlackBoardProbability_UsesBlackBoardValue()
        {
            // Arrange
            var condition = new RandomCondition();
            condition.SetProperty("probability_key", "dynamic_chance");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // BlackBoardに確率を設定
            blackBoard.SetValue("dynamic_chance", 1.0f); // 100%成功

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
        }

        [Test]
        public void RandomCondition_ExecuteWithLowProbability_ReturnsMostlyFailure()
        {
            // Arrange
            var condition = new RandomCondition();
            condition.SetProperty("probability", "0.1"); // 10%
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // Act & Assert - 複数回実行してほとんどFailureになることを確認
            int successCount = 0;
            int trials = 50;

            for (int i = 0; i < trials; i++)
            {
                var result = condition.Execute();
                if (result == BTNodeResult.Success)
                {
                    successCount++;
                }
            }

            // 確率0.1なので、50回中0～10回程度の成功を期待
            Assert.IsTrue(successCount <= 20, 
                $"期待される成功率: 0-20%, 実際: {successCount}/50");
        }

        #endregion

        #region HasTargetCondition Tests

        [Test]
        public void HasTargetCondition_SetProperty_SetsCorrectTargetTag()
        {
            // Arrange
            var condition = new HasTargetCondition();

            // Act
            condition.SetProperty("target_tag", "Monster");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // Assert - 実行して動作確認
            var result = condition.Execute();
            Assert.AreEqual(BTNodeResult.Failure, result); // モンスターがないのでFailure
        }

        [Test]
        public void HasTargetCondition_ExecuteWithBlackBoardTarget_ReturnsSuccess()
        {
            // Arrange
            var condition = new HasTargetCondition();
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // BlackBoardにターゲット情報を設定
            var targetObj = new GameObject("Enemy");
            blackBoard.SetValue("target_enemy", targetObj);

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);

            // Cleanup
            Object.DestroyImmediate(targetObj);
        }

        [Test]
        public void HasTargetCondition_ExecuteWithTaggedTarget_ReturnsSuccess()
        {
            // Arrange
            var condition = new HasTargetCondition();
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // シーンに敵オブジェクトを配置
            var enemyObj = new GameObject("Enemy");
            var tagComponent = enemyObj.AddComponent<GameplayTagComponent>();
            tagComponent.AddTag(new GameplayTag("Character.Enemy"));

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);

            // Cleanup
            Object.DestroyImmediate(enemyObj);
        }

        [Test]
        public void HasTargetCondition_ExecuteWithNoTarget_ReturnsFailure()
        {
            // Arrange
            var condition = new HasTargetCondition();
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        #endregion

        #region DistanceCheckCondition Tests

        [Test]
        public void DistanceCheckCondition_SetProperty_SetsCorrectTargetName()
        {
            // Arrange
            var condition = new DistanceCheckCondition();

            // Act
            LogAssert.Expect(LogType.Error, "[ERR][CHK]: DistanceCheck '': No target found (name='TestTarget', tag='')");
            LogAssert.Expect(LogType.Error, "[ERR][CHK]: DistanceCheck '': No target found (name='TestTarget', tag='')");
            condition.SetProperty("target_name", "TestTarget");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // Assert - 実行して動作確認（ターゲットがないのでFailure）
            var result = condition.Execute();
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        [Test]
        public void DistanceCheckCondition_SetProperty_SetsCorrectDistance()
        {
            // Arrange
            var condition = new DistanceCheckCondition();

            // Act
            LogAssert.Expect(LogType.Error, "[ERR][CHK]: DistanceCheck '': No target found (name='', tag='')");
            LogAssert.Expect(LogType.Error, "[ERR][CHK]: DistanceCheck '': No target found (name='', tag='')");
            condition.SetProperty("distance", "<= 8.0");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // Assert - 実行して動作確認
            var result = condition.Execute();
            Assert.AreEqual(BTNodeResult.Failure, result); // ターゲットがないのでFailure
        }

        [Test]
        public void DistanceCheckCondition_ExecuteWithTargetInRange_ReturnsSuccess()
        {
            // Arrange
            var condition = new DistanceCheckCondition();
            LogAssert.Expect(LogType.Error, "[ERR][CHK]: DistanceCheck '': No target found (name='TestTarget', tag='')");
            condition.SetProperty("target_name", "TestTarget");
            condition.SetProperty("distance", "<= 10.0");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // ターゲットオブジェクトを作成
            var targetObj = new GameObject("TestTarget");
            targetObj.transform.position = testOwner.transform.position + Vector3.forward * 5f; // 5m離れた位置

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.IsTrue(blackBoard.HasKey("_current_distance"));
            Assert.IsTrue(blackBoard.HasKey("_target_position"));

            // Cleanup
            Object.DestroyImmediate(targetObj);
        }

        [Test]
        public void DistanceCheckCondition_ExecuteWithTargetOutOfRange_ReturnsFailure()
        {
            // Arrange
            var condition = new DistanceCheckCondition();
            LogAssert.Expect(LogType.Error, "[ERR][CHK]: DistanceCheck '': No target found (name='TestTarget', tag='')");
            condition.SetProperty("target_name", "TestTarget");
            condition.SetProperty("distance", "<= 3.0");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // ターゲットオブジェクトを作成（遠い位置）
            var targetObj = new GameObject("TestTarget");
            targetObj.transform.position = testOwner.transform.position + Vector3.forward * 10f; // 10m離れた位置

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);

            // Cleanup
            Object.DestroyImmediate(targetObj);
        }

        [Test]
        public void DistanceCheckCondition_ExecuteWithBlackBoardTarget_ReturnsSuccess()
        {
            // Arrange
            var condition = new DistanceCheckCondition();
            LogAssert.Expect(LogType.Error, "[ERR][CHK]: DistanceCheck '': No target found (name='', tag='')");
            condition.SetProperty("use_blackboard_target", "true");
            condition.SetProperty("blackboard_position_key", "target_pos");
            condition.SetProperty("distance", "<= 8.0");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // BlackBoardにターゲット位置を設定
            Vector3 targetPosition = testOwner.transform.position + Vector3.forward * 6f;
            blackBoard.SetValue("target_pos", targetPosition);

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
        }

        #endregion

        #region Distance2DCheckCondition Tests

        [Test]
        public void Distance2DCheckCondition_SetProperty_SetsCorrectTargetTag()
        {
            // Arrange
            var condition = new Distance2DCheckCondition();

            // Act
            condition.SetProperty("target_tag", "Waypoint");
            LogAssert.Expect(LogType.Error, "[ERR][CHK]: Distance2DCheck '': No target found (name='', tag='Waypoint')");
            LogAssert.Expect(LogType.Error, "[ERR][CHK]: Distance2DCheck '': No target found (name='', tag='Waypoint')");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // Assert - 実行して動作確認
            var result = condition.Execute();
            Assert.AreEqual(BTNodeResult.Failure, result); // ウェイポイントがないのでFailure
        }

        [Test]
        public void Distance2DCheckCondition_ExecuteWith2DDistance_IgnoresYAxis()
        {
            // Arrange
            var condition = new Distance2DCheckCondition();
            LogAssert.Expect(LogType.Error, "[ERR][CHK]: Distance2DCheck '': No target found (name='HighTarget', tag='')");
            condition.SetProperty("target_name", "HighTarget");
            condition.SetProperty("distance", "<= 5.0");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // ターゲットオブジェクトを作成（高い位置だが2D距離は近い）
            var targetObj = new GameObject("HighTarget");
            targetObj.transform.position = testOwner.transform.position + new Vector3(3f, 100f, 0f); // Y軸が高い

            // Act
            var result = condition.Execute();

            // Assert - 2D距離は3なので成功するはず
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.IsTrue(blackBoard.HasKey("_current_distance_2d"));
            Assert.IsTrue(blackBoard.HasKey("_ground_distance"));

            // Cleanup
            Object.DestroyImmediate(targetObj);
        }

        [Test]
        public void Distance2DCheckCondition_ExecuteWithFar2DDistance_ReturnsFailure()
        {
            // Arrange
            var condition = new Distance2DCheckCondition();
            LogAssert.Expect(LogType.Error, "[ERR][CHK]: Distance2DCheck '': No target found (name='FarTarget', tag='')");
            condition.SetProperty("target_name", "FarTarget");
            condition.SetProperty("distance", "<= 2.0");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // ターゲットオブジェクトを作成（2D距離が遠い）
            var targetObj = new GameObject("FarTarget");
            targetObj.transform.position = testOwner.transform.position + new Vector3(5f, 0f, 5f); // 2D距離は約7

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);

            // Cleanup
            Object.DestroyImmediate(targetObj);
        }

        #endregion

        #region CompareBlackBoardCondition Tests

        [Test]
        public void CompareBlackBoardCondition_SetProperty_ParsesSimpleExpression()
        {
            // Arrange
            var condition = new CompareBlackBoardCondition();
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            blackBoard.SetValue("health", 80);

            // Act
            condition.SetProperty("condition", "health >= 50");
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
        }

        [Test]
        public void CompareBlackBoardCondition_ExecuteWithIntComparison_ReturnsCorrectResult()
        {
            // Arrange
            var condition = new CompareBlackBoardCondition();
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            blackBoard.SetValue("score", 100);

            // Act
            condition.SetProperty("condition", "score == 100");
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
        }

        [Test]
        public void CompareBlackBoardCondition_ExecuteWithFloatComparison_ReturnsCorrectResult()
        {
            // Arrange
            var condition = new CompareBlackBoardCondition();
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            blackBoard.SetValue("speed", 15.5f);

            // Act
            condition.SetProperty("condition", "speed > 10.0");
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
        }

        [Test]
        public void CompareBlackBoardCondition_ExecuteWithStringComparison_ReturnsCorrectResult()
        {
            // Arrange
            var condition = new CompareBlackBoardCondition();
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            blackBoard.SetValue("player_name", "Hero");

            // Act
            condition.SetProperty("condition", "player_name == \"Hero\"");
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
        }

        [Test]
        public void CompareBlackBoardCondition_ExecuteWithKeyToKeyComparison_ReturnsCorrectResult()
        {
            // Arrange
            var condition = new CompareBlackBoardCondition();
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            blackBoard.SetValue("current_level", 5);
            blackBoard.SetValue("required_level", 3);

            // Act
            condition.SetProperty("condition", "current_level >= required_level");
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
        }

        [Test]
        public void CompareBlackBoardCondition_ExecuteWithMissingKey_ReturnsFailure()
        {
            // Arrange
            var condition = new CompareBlackBoardCondition();
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // Act
            condition.SetProperty("condition", "missing_key > 0");
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        [Test]
        public void CompareBlackBoardCondition_ExecuteWithInvalidExpression_ReturnsFailure()
        {
            // Arrange
            var condition = new CompareBlackBoardCondition();
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // Act
            LogAssert.Expect(LogType.Error, "[ERR][CHK]: CompareBlackBoard '': No valid operator found in expression 'invalid expression without operator'");
            LogAssert.Expect(LogType.Error, "[ERR][BBD]: CompareBlackBoard: key1 is empty");
            condition.SetProperty("condition", "invalid expression without operator");
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        #endregion
    }
}
