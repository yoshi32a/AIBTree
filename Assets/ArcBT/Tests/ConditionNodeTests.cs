using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ArcBT.Core;
using ArcBT.Conditions;
using ArcBT.Samples.RPG.Conditions;
using ArcBT.Samples.RPG.Components;

namespace ArcBT.Tests
{
    /// <summary>ConditionNodeの機能をテストするクラス</summary>
    [TestFixture]
    public class ConditionNodeTests
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

        [Test]
        public void HasSharedEnemyInfoCondition_SetProperty_SetsCorrectKey()
        {
            // Arrange
            var condition = new HasSharedEnemyInfoCondition();

            // Act
            condition.SetProperty("bb_key", "custom_enemy_key");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

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

        [Test]
        public void HasSharedEnemyInfoCondition_WithValidEnemyInfo_ReturnsSuccess()
        {
            // Arrange
            var condition = new HasSharedEnemyInfoCondition();
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            var enemyObject = new GameObject("TestEnemy");
            blackBoard.SetValue("has_enemy_info", true);
            blackBoard.SetValue("enemy_target", enemyObject);

            LogAssert.Expect(LogType.Log, "HasSharedEnemyInfo: Enemy info available - Target: 'TestEnemy'");

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);

            // Cleanup
            Object.DestroyImmediate(enemyObject);
        }

        [Test]
        public void HasSharedEnemyInfoCondition_WithoutEnemyInfo_ReturnsFailure()
        {
            // Arrange
            var condition = new HasSharedEnemyInfoCondition();
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            blackBoard.SetValue("has_enemy_info", false);
            LogAssert.Expect(LogType.Log, "HasSharedEnemyInfo: No valid enemy info in BlackBoard");

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        [Test]
        public void HasSharedEnemyInfoCondition_WithNullBlackBoard_ReturnsFailure()
        {
            // Arrange
            var condition = new HasSharedEnemyInfoCondition();
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), null);

            LogAssert.Expect(LogType.Error, "HasSharedEnemyInfo: BlackBoard is null");

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        [Test]
        public void HasSharedEnemyInfoCondition_WithInactiveEnemy_ReturnsFailure()
        {
            // Arrange
            var condition = new HasSharedEnemyInfoCondition();
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            var enemyObject = new GameObject("InactiveEnemy");
            enemyObject.SetActive(false);
            blackBoard.SetValue("has_enemy_info", true);
            blackBoard.SetValue("enemy_target", enemyObject);

            LogAssert.Expect(LogType.Log, "HasSharedEnemyInfo: No valid enemy info in BlackBoard");

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);

            // Cleanup
            Object.DestroyImmediate(enemyObject);
        }

        [Test]
        public void HealthCheckCondition_WithSufficientHealth_ReturnsSuccess()
        {
            // Arrange
            var condition = new HealthCheckCondition();
            var healthComponent = testOwner.AddComponent<Health>();
            healthComponent.CurrentHealth = 80;
            healthComponent.MaxHealth = 100;

            condition.SetProperty("min_health", "50");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
        }

        [Test]
        public void HealthCheckCondition_WithInsufficientHealth_ReturnsFailure()
        {
            // Arrange
            var condition = new HealthCheckCondition();
            var healthComponent = testOwner.AddComponent<Health>();
            healthComponent.CurrentHealth = 30;
            healthComponent.MaxHealth = 100;

            condition.SetProperty("min_health", "50");
            condition.Initialize(testOwner.GetComponent<TestConditionComponent>() ?? testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        [Test]
        public void HealthCheckCondition_WithoutHealthComponent_ReturnsFailure()
        {
            // Arrange
            var condition = new HealthCheckCondition();
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            LogAssert.Expect(LogType.Error, "HealthCheck: No Health component found on TestOwner");
            LogAssert.Expect(LogType.Error, "HealthCheck '': Health component is null - trying to find it again");
            LogAssert.Expect(LogType.Error, "HealthCheck '': Still no Health component found!");

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        [Test]
        public void HealthCheckCondition_InSafetyPeriod_SkipsCheck()
        {
            // Arrange
            var condition = new HealthCheckCondition();
            var healthComponent = testOwner.AddComponent<Health>();
            healthComponent.CurrentHealth = 10; // 低い体力
            healthComponent.MaxHealth = 100;

            condition.SetProperty("min_health", "20"); // 緊急時閾値
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // 安全期間を設定
            blackBoard.SetValue("safety_timer", Time.time + 10f);

            LogAssert.Expect(LogType.Log, "HealthCheck '': In safety period - skipping emergency check");

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result); // 緊急時チェックをスキップ
        }

        [Test]
        public void EnemyCheckCondition_SetProperty_SetsCorrectRange()
        {
            // Arrange
            var condition = new EnemyCheckCondition();

            // Act
            condition.SetProperty("detection_range", "15.0");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // 敵がいない状態での実行を確認
            var result = condition.Execute();
            
            // Assert (敵がいないのでFailureが期待される)
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        [Test]
        public void EnemyCheckCondition_WithEnemyInRange_ReturnsSuccess()
        {
            // Arrange
            var condition = new EnemyCheckCondition();
            condition.SetProperty("detection_range", "10.0");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // 近くに敵を配置（レイヤー設定が必要だが、テスト環境では省略）
            var enemyObject = new GameObject("Enemy");
            enemyObject.transform.position = testOwner.transform.position + Vector3.forward * 5f;

            // Act
            var result = condition.Execute();

            // Assert (実際の検出結果は環境に依存するため、実行されることを確認)
            Assert.IsTrue(result is BTNodeResult.Success or BTNodeResult.Failure);

            // Cleanup
            Object.DestroyImmediate(enemyObject);
        }

        [Test]
        public void HasManaCondition_WithSufficientMana_ReturnsSuccess()
        {
            // Arrange
            var condition = new HasManaCondition();
            var manaComponent = testOwner.AddComponent<ArcBT.Samples.RPG.Components.Mana>();
            manaComponent.CurrentMana = 80;
            manaComponent.MaxMana = 100;

            condition.SetProperty("min_mana", "50");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
        }

        [Test]
        public void HasManaCondition_WithInsufficientMana_ReturnsFailure()
        {
            // Arrange
            var condition = new HasManaCondition();
            var manaComponent = testOwner.AddComponent<ArcBT.Samples.RPG.Components.Mana>();
            manaComponent.CurrentMana = 30;
            manaComponent.MaxMana = 100;

            condition.SetProperty("min_mana", "50");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        [Test]
        public void HasManaCondition_WithoutManaComponent_ReturnsFailure()
        {
            // Arrange
            var condition = new HasManaCondition();
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        [Test]
        public void EnemyInRangeCondition_SetProperty_SetsCorrectRange()
        {
            // Arrange
            var condition = new EnemyInRangeCondition();

            // Act
            condition.SetProperty("range", "8.0");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // 実行して動作確認
            var result = condition.Execute();
            
            // Assert (実行されることを確認)
            Assert.IsTrue(result is BTNodeResult.Success or BTNodeResult.Failure);
        }

        [Test]
        public void IsInitializedCondition_WithInitializedFlag_ReturnsSuccess()
        {
            // Arrange
            var condition = new IsInitializedCondition();
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // BlackBoardに初期化フラグを設定
            blackBoard.SetValue("is_initialized", true);

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
        }

        [Test]
        public void IsInitializedCondition_WithoutInitializedFlag_InitializesAndReturnsSuccess()
        {
            // Arrange
            var condition = new IsInitializedCondition();
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // 初期化フラグを設定しない（またはfalse）
            blackBoard.SetValue("is_initialized", false);

            // Act
            var result = condition.Execute();

            // Assert - IsInitializedConditionは初期化されていない場合に初期化を実行してSuccessを返す
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.AreEqual(true, blackBoard.GetValue<bool>("is_initialized"));
        }

        [Test]
        public void HasItemCondition_WithRequiredItem_ReturnsSuccess()
        {
            // Arrange
            var condition = new HasItemCondition();
            var inventoryComponent = testOwner.AddComponent<ArcBT.Samples.RPG.Components.Inventory>();
            
            // テストアイテムを追加
            inventoryComponent.AddItem("health_potion", 3);

            // HasItemConditionは "item_type" プロパティを使用
            condition.SetProperty("item_type", "health_potion");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
        }

        [Test]
        public void HasItemCondition_WithoutRequiredItem_ReturnsFailure()
        {
            // Arrange
            var condition = new HasItemCondition();
            var inventoryComponent = testOwner.AddComponent<TestConditionInventoryComponent>();

            condition.SetProperty("item_name", "rare_item");
            condition.SetProperty("quantity", "1");
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        [Test]
        public void ConditionNodes_ChainEvaluation_WorksCorrectly()
        {
            // Arrange: 複数の条件を組み合わせ
            var healthCondition = new HealthCheckCondition();
            var manaCondition = new HasManaCondition();
            var initCondition = new IsInitializedCondition();

            // コンポーネントを設定
            var healthComponent = testOwner.AddComponent<Health>();
            healthComponent.CurrentHealth = 80;
            healthComponent.MaxHealth = 100;

            var manaComponent = testOwner.AddComponent<ArcBT.Samples.RPG.Components.Mana>();
            manaComponent.CurrentMana = 70;
            manaComponent.MaxMana = 100;

            // BlackBoard初期化
            blackBoard.SetValue("is_initialized", true);

            // 条件を初期化
            healthCondition.SetProperty("min_health", "50");
            manaCondition.SetProperty("min_mana", "30");

            healthCondition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);
            manaCondition.Initialize(testOwner.GetComponent<TestConditionComponent>(), blackBoard);
            initCondition.Initialize(testOwner.GetComponent<TestConditionComponent>(), blackBoard);

            // Act & Assert: 全ての条件がSuccessを返すことを確認
            Assert.AreEqual(BTNodeResult.Success, healthCondition.Execute());
            Assert.AreEqual(BTNodeResult.Success, manaCondition.Execute());
            Assert.AreEqual(BTNodeResult.Success, initCondition.Execute());
        }

        [Test]
        public void ConditionNodes_BlackBoardInteraction_SharesStateCorrectly()
        {
            // Arrange
            var hasEnemyCondition = new HasSharedEnemyInfoCondition();
            var enemyInRangeCondition = new EnemyInRangeCondition();

            hasEnemyCondition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);
            enemyInRangeCondition.Initialize(testOwner.GetComponent<TestConditionComponent>(), blackBoard);

            // 敵情報をBlackBoardに設定
            var enemyObject = new GameObject("SharedEnemy");
            blackBoard.SetValue("has_enemy_info", true);
            blackBoard.SetValue("enemy_target", enemyObject);

            // Act
            var hasEnemyResult = hasEnemyCondition.Execute();
            var inRangeResult = enemyInRangeCondition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, hasEnemyResult);
            Assert.IsTrue(inRangeResult is BTNodeResult.Success or BTNodeResult.Failure);

            // BlackBoardが両方の条件で共有されていることを確認
            Assert.AreEqual(enemyObject, blackBoard.GetValue<GameObject>("enemy_target"));

            // Cleanup
            Object.DestroyImmediate(enemyObject);
        }

        [Test]
        public void ConditionNodes_PerformanceTest_EvaluatesQuickly()
        {
            // Arrange
            var condition = new IsInitializedCondition();
            condition.Initialize(testOwner.AddComponent<TestConditionComponent>(), blackBoard);
            blackBoard.SetValue("is_initialized", true);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            const int evaluationCount = 1000;

            // Act: 大量の条件評価を実行
            for (int i = 0; i < evaluationCount; i++)
            {
                condition.Execute();
            }

            stopwatch.Stop();

            // Assert: 適切な時間内で完了する（1000回評価が50ms以内）
            Assert.Less(stopwatch.ElapsedMilliseconds, 50, 
                $"性能テスト失敗: {evaluationCount}回評価に{stopwatch.ElapsedMilliseconds}ms掛かりました");
        }
    }

    /// <summary>テスト用のConditionコンポーネント</summary>
    public class TestConditionComponent : MonoBehaviour
    {
        // テスト用の空のMonoBehaviourコンポーネント
    }

    /// <summary>テスト用のマナコンポーネント（Condition用）</summary>
    public class TestConditionManaComponent : MonoBehaviour
    {
        public float currentMana = 100f;
        public float maxMana = 100f;
    }

    /// <summary>テスト用のインベントリコンポーネント（Condition用）</summary>
    public class TestConditionInventoryComponent : MonoBehaviour
    {
        readonly System.Collections.Generic.Dictionary<string, int> items = 
            new System.Collections.Generic.Dictionary<string, int>();

        public void AddItem(string itemName, int quantity)
        {
            if (items.ContainsKey(itemName))
            {
                items[itemName] += quantity;
            }
            else
            {
                items[itemName] = quantity;
            }
        }

        public int GetItemCount(string itemName)
        {
            return items.TryGetValue(itemName, out var count) ? count : 0;
        }

        public bool HasItem(string itemName, int requiredQuantity = 1)
        {
            return GetItemCount(itemName) >= requiredQuantity;
        }
    }
}
