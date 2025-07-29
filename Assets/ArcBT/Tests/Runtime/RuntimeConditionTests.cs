using ArcBT.Conditions;
using ArcBT.Core;
using ArcBT.TagSystem;
using NUnit.Framework;
using UnityEngine;

namespace ArcBT.Tests
{
    /// <summary>Runtime Conditionsの機能をテストするクラス</summary>
    [TestFixture]
    public class RuntimeConditionTests : BTTestBase
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

        #region ScanForInterestCondition Tests

        [Test][Description("ScanForInterestConditionのscan_rangeパラメータ設定機能の検証")]
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

        [Test][Description("興味対象オブジェクトが範囲内にある場合にScanForInterestConditionがSuccessを返しBlackBoardに情報を記録することを確認")]
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

        [Test][Description("RandomConditionのプロパティ設定で確率が正しく設定され、統計的に適切な結果を返すことを確認")]
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

        [Test][Description("RandomConditionのシード設定で同じシードでは同じ結果が返されることを確認")]
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

        [Test][Description("RandomConditionでBlackBoardから確率値を取得して判定する機能が正しく動作することを確認")]
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

        [Test][Description("RandomConditionで低い確率を設定した場合にほとんどFailureが返されることを確認")]
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

        [Test][Description("HasTargetConditionのtarget_tagパラメータでモンスター等のカスタムタグ指定機能の検証")]
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

        [Test][Description("HasTargetConditionでBlackBoardにターゲット情報がある場合にSuccessを返すことを確認")]
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

        [Test][Description("HasTargetConditionでシーン内にタグ付きターゲットがある場合にSuccessを返すことを確認")]
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

        [Test][Description("HasTargetCondition実行時にBlackBoardとシーンの両方にターゲットがない場合の動作")]
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

        [Test][Description("DistanceCheckConditionのtarget_nameパラメータでGameObject名指定機能の検証")]
        public void DistanceCheckCondition_SetProperty_SetsCorrectTargetName()
        {
            // Arrange
            var condition = new DistanceCheckCondition();

            // Act
            condition.SetProperty("target_name", "TestTarget");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // Assert - 実行して動作確認（ターゲットがないのでFailure）
            var result = condition.Execute();
            Assert.AreEqual(BTNodeResult.Failure, result, "指定した名前のターゲットが存在しない場合、Failureが返されるべき");
            
            // 注意: エラーログはLoggingBehaviorTestsで専用テストが行われます
        }

        [Test(Description = "DistanceCheckConditionのdistanceパラメータで\"<= 8.0\"等の比較演算子指定機能の検証")]
        public void DistanceCheckCondition_SetProperty_SetsCorrectDistance()
        {
            // Arrange
            var condition = new DistanceCheckCondition();

            // Act
            condition.SetProperty("distance", "<= 8.0");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // Assert - ログではなく実際の動作を検証
            var result = condition.Execute();
            Assert.AreEqual(BTNodeResult.Failure, result, "ターゲットが存在しない場合、Failureが返されるべき");
            
            // 注意: エラーログはLoggingBehaviorTestsで専用テストが行われます
        }

        [Test][Description("範囲内にターゲットがある場合にDistanceCheckConditionがSuccessを返しBlackBoardに距離情報を記録することを確認")]
        public void DistanceCheckCondition_ExecuteWithTargetInRange_ReturnsSuccess()
        {
            // Arrange
            var condition = new DistanceCheckCondition();
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

        [Test][Description("DistanceCheckCondition実行時にターゲットが指定距離より遠い位置にある場合の動作")]
        public void DistanceCheckCondition_ExecuteWithTargetOutOfRange_ReturnsFailure()
        {
            // Arrange
            var condition = new DistanceCheckCondition();
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

        [Test][Description("DistanceCheckConditionでBlackBoardのターゲット位置を使用した距離チェックが正しく動作することを確認")]
        public void DistanceCheckCondition_ExecuteWithBlackBoardTarget_ReturnsSuccess()
        {
            // Arrange
            var condition = new DistanceCheckCondition();
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

        [Test][Description("Distance2DCheckConditionのtarget_tagパラメータでウェイポイント等のタグ指定機能の検証")]
        public void Distance2DCheckCondition_SetProperty_SetsCorrectTargetTag()
        {
            // Arrange
            var condition = new Distance2DCheckCondition();

            // Act
            condition.SetProperty("target_tag", "Waypoint");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // Assert - ログではなく実際の動作を検証
            var result = condition.Execute();
            Assert.AreEqual(BTNodeResult.Failure, result, "指定したタグのオブジェクトが存在しない場合、Failureが返されるべき");
            
            // 注意: エラーログはLoggingBehaviorTestsで専用テストが行われます
        }

        [Test][Description("Distance2DCheckConditionで2D距離で判定しY軸を無視して適切に動作することを確認")]
        public void Distance2DCheckCondition_ExecuteWith2DDistance_IgnoresYAxis()
        {
            // Arrange
            var condition = new Distance2DCheckCondition();
            condition.SetProperty("target_name", "HighTarget");
            condition.SetProperty("distance", "<= 5.0");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // ターゲットオブジェクトを作成（高い位置だが2D距離は近い）
            var targetObj = new GameObject("HighTarget");
            targetObj.transform.position = testOwner.transform.position + new Vector3(3f, 100f, 0f); // Y軸が高い

            // Act
            var result = condition.Execute();

            // Assert - ログではなく実際の動作を検証（2D距離は3なので成功するはず）
            Assert.AreEqual(BTNodeResult.Success, result, "2D距離が範囲内の場合、Successが返されるべき");
            Assert.IsTrue(blackBoard.HasKey("_current_distance_2d"), "2D距離がBlackBoardに記録されるべき");
            Assert.IsTrue(blackBoard.HasKey("_ground_distance"), "地面距離がBlackBoardに記録されるべき");
            
            // 注意: エラーログはLoggingBehaviorTestsで専用テストが行われます

            // Cleanup
            Object.DestroyImmediate(targetObj);
        }

        [Test][Description("Distance2DCheckCondition実行時に2D平面上の距離が闾値を超える場合の動作")]
        public void Distance2DCheckCondition_ExecuteWithFar2DDistance_ReturnsFailure()
        {
            // Arrange
            var condition = new Distance2DCheckCondition();
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

        [Test][Description("CompareBlackBoardConditionのプロパティ設定でシンプルな比較式が正しくパースされることを確認")]
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

        [Test][Description("CompareBlackBoardConditionで整数値の比較が正しく動作することを確認")]
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

        [Test][Description("CompareBlackBoardConditionで浮動小数点値の比較が正しく動作することを確認")]
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

        [Test][Description("CompareBlackBoardConditionで文字列の比較が正しく動作することを確認")]
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

        [Test][Description("CompareBlackBoardConditionでBlackBoardキー同士の比較が正しく動作することを確認")]
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

        [Test][Description("CompareBlackBoardCondition実行時に比較式で未定義キーを参照した場合のエラーハンドリング")]
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

        [Test][Description("CompareBlackBoardConditionで無効な式を指定した場合にFailureを返しエラーログを出力することを確認")]
        public void CompareBlackBoardCondition_ExecuteWithInvalidExpression_ReturnsFailure()
        {
            // Arrange
            var condition = new CompareBlackBoardCondition();
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // Act
            condition.SetProperty("condition", "invalid expression without operator");
            var result = condition.Execute();

            // Assert - ログではなく実際の動作を検証
            Assert.AreEqual(BTNodeResult.Failure, result, "無効な式が指定された場合、Failureが返されるべき");
            
            // 注意: エラーログはLoggingBehaviorTestsで専用テストが行われます
        }

        #endregion
    }

    /// <summary>Runtime Condition Tests用のテストコンポーネント</summary>
    public class TestConditionComponent : MonoBehaviour
    {
        // Runtime条件テスト用の基本的なMonoBehaviourコンポーネント
    }
}
