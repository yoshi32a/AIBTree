using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ArcBT.Core;
using ArcBT.Conditions;
using ArcBT.Logger;

namespace ArcBT.Tests
{
    /// <summary>Runtime Core Conditionsの機能をテストするクラス</summary>
    [TestFixture]
    public class CoreConditionTests : BTTestBase
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

        #region HasSharedEnemyInfoCondition Tests

        [Test][Description("HasSharedEnemyInfoConditionのbb_keyパラメータでカスタムBlackBoardキー指定機能の検証")]
        public void HasSharedEnemyInfoCondition_SetProperty_SetsCorrectKey()
        {
            // Arrange
            var condition = new HasSharedEnemyInfoCondition();

            // Act
            condition.SetProperty("bb_key", "custom_enemy_key");
            condition.Initialize(testOwner.AddComponent<TestCoreConditionComponent>(), blackBoard);

            // BlackBoardにカスタムキーの情報を設定
            blackBoard.SetValue("custom_enemy_key", true);
            blackBoard.SetValue("enemy_target", new GameObject("Enemy"));

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);

            // Cleanup
            Object.DestroyImmediate(blackBoard.GetValue<GameObject>("enemy_target"));
        }

        [Test][Description("HasSharedEnemyInfoCondition実行時にBlackBoardに有効な敵情報とアクティブな敵オブジェクトがある場合の動作")]
        public void HasSharedEnemyInfoCondition_WithValidEnemyInfo_ReturnsSuccess()
        {
            // Arrange
            var condition = new HasSharedEnemyInfoCondition();
            condition.Initialize(testOwner.AddComponent<TestCoreConditionComponent>(), blackBoard);

            var enemyObject = new GameObject("TestEnemy");
            blackBoard.SetValue("has_enemy_info", true);
            blackBoard.SetValue("enemy_target", enemyObject);

            // Act
            var result = condition.Execute();

            // Assert - ログではなく実際の動作を検証
            Assert.AreEqual(BTNodeResult.Success, result, "有効な敵情報がある場合、Successが返されるべき");
            
            // BlackBoardの状態を確認
            Assert.IsTrue(blackBoard.GetValue<bool>("has_enemy_info"), "敵情報フラグが維持されているべき");
            Assert.AreEqual(enemyObject, blackBoard.GetValue<GameObject>("enemy_target"), "敵オブジェクトが維持されているべき");

            // Cleanup
            Object.DestroyImmediate(enemyObject);
        }

        [Test][Description("HasSharedEnemyInfoCondition実行時にBlackBoardに有効な敵情報フラグがない場合の動作")]
        public void HasSharedEnemyInfoCondition_WithoutEnemyInfo_ReturnsFailure()
        {
            // Arrange
            var condition = new HasSharedEnemyInfoCondition();
            condition.Initialize(testOwner.AddComponent<TestCoreConditionComponent>(), blackBoard);

            blackBoard.SetValue("has_enemy_info", false);

            // Act
            var result = condition.Execute();

            // Assert - ログではなく実際の動作を検証
            Assert.AreEqual(BTNodeResult.Failure, result, "敵情報がfalseの場合、Failureが返されるべき");
            Assert.IsFalse(blackBoard.GetValue<bool>("has_enemy_info"), "敵情報フラグがfalseのまま維持されているべき");
        }

        [Test][Description("HasSharedEnemyInfoCondition実行時にBlackBoard参照がnullの場合のエラーハンドリング")]
        public void HasSharedEnemyInfoCondition_WithNullBlackBoard_ReturnsFailure()
        {
            // Arrange
            var condition = new HasSharedEnemyInfoCondition();
            condition.Initialize(testOwner.AddComponent<TestCoreConditionComponent>(), null);

            // Act
            var result = condition.Execute();

            // Assert - ログではなく実際の動作を検証
            Assert.AreEqual(BTNodeResult.Failure, result, "BlackBoardがnullの場合、Failureが返されるべき");
            
            // 注意: エラーログはLoggingBehaviorTestsで専用テストが行われます
        }

        [Test][Description("HasSharedEnemyInfoCondition実行時に敵オブジェクトがSetActive(false)の場合のフィルタリング")]
        public void HasSharedEnemyInfoCondition_WithInactiveEnemy_ReturnsFailure()
        {
            // Arrange
            var condition = new HasSharedEnemyInfoCondition();
            condition.Initialize(testOwner.AddComponent<TestCoreConditionComponent>(), blackBoard);

            var enemyObject = new GameObject("InactiveEnemy");
            enemyObject.SetActive(false);
            blackBoard.SetValue("has_enemy_info", true);
            blackBoard.SetValue("enemy_target", enemyObject);

            // Act
            var result = condition.Execute();

            // Assert - ログではなく実際の動作を検証
            Assert.AreEqual(BTNodeResult.Failure, result, "非アクティブな敵オブジェクトの場合、Failureが返されるべき");
            
            // BlackBoardの状態確認（敵情報はあるが、オブジェクトが非アクティブ）
            Assert.IsTrue(blackBoard.GetValue<bool>("has_enemy_info"), "敵情報フラグは残っているべき");
            Assert.AreEqual(enemyObject, blackBoard.GetValue<GameObject>("enemy_target"), "敵オブジェクト参照は残っているべき");
            Assert.IsFalse(enemyObject.activeInHierarchy, "敵オブジェクトは非アクティブのままであるべき");

            // Cleanup
            Object.DestroyImmediate(enemyObject);
        }

        #endregion

        #region CompareBlackBoardCondition Tests

        [Test][Description("CompareBlackBoardConditionのプロパティ設定でシンプルな比較式が正しくパースされることを確認")]
        public void CompareBlackBoardCondition_SetProperty_ParsesSimpleExpression()
        {
            // Arrange
            var condition = new CompareBlackBoardCondition();
            condition.Initialize(testOwner.AddComponent<TestCoreConditionComponent>(), blackBoard);

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
            condition.Initialize(testOwner.AddComponent<TestCoreConditionComponent>(), blackBoard);

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
            condition.Initialize(testOwner.AddComponent<TestCoreConditionComponent>(), blackBoard);

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
            condition.Initialize(testOwner.AddComponent<TestCoreConditionComponent>(), blackBoard);

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
            condition.Initialize(testOwner.AddComponent<TestCoreConditionComponent>(), blackBoard);

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
            condition.Initialize(testOwner.AddComponent<TestCoreConditionComponent>(), blackBoard);

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
            condition.Initialize(testOwner.AddComponent<TestCoreConditionComponent>(), blackBoard);

            // Act
            condition.SetProperty("condition", "invalid expression without operator");
            var result = condition.Execute();

            // Assert - ログではなく実際の動作を検証
            Assert.AreEqual(BTNodeResult.Failure, result, "無効な式が指定された場合、Failureが返されるべき");
            
            // 注意: エラーログはLoggingBehaviorTestsで専用テストが行われます
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
            condition.Initialize(testOwner.AddComponent<TestCoreConditionComponent>(), blackBoard);

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

        [Test][Description("RandomConditionでBlackBoardから確率値を取得して判定する機能が正しく動作することを確認")]
        public void RandomCondition_ExecuteWithBlackBoardProbability_UsesBlackBoardValue()
        {
            // Arrange
            var condition = new RandomCondition();
            condition.SetProperty("probability_key", "dynamic_chance");
            condition.Initialize(testOwner.AddComponent<TestCoreConditionComponent>(), blackBoard);

            // BlackBoardに確率を設定
            blackBoard.SetValue("dynamic_chance", 1.0f); // 100%成功

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
        }

        #endregion
    }

    /// <summary>テスト用のCoreConditionコンポーネント</summary>
    public class TestCoreConditionComponent : MonoBehaviour
    {
        // テスト用の空のMonoBehaviourコンポーネント
    }
}
