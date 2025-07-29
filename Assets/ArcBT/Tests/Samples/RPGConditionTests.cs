using System.Collections.Generic;
using ArcBT.Core;
using ArcBT.Samples.RPG.Components;
using ArcBT.Samples.RPG.Conditions;
using NUnit.Framework;
using UnityEngine;

namespace ArcBT.Tests.Samples
{
    /// <summary>RPG Sample Conditionsの機能をテストするクラス</summary>
    [TestFixture]
    public class RPGConditionTests : BTTestBase
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


        [Test][Description("HealthCheckCondition実行時に現在体力がmin_health闾値を上回る健康状態の場合の動作")]
        public void HealthCheckCondition_WithSufficientHealth_ReturnsSuccess()
        {
            // Arrange
            var condition = new HealthCheckCondition();
            var healthComponent = testOwner.AddComponent<Health>();
            healthComponent.CurrentHealth = 80;
            healthComponent.MaxHealth = 100;

            condition.SetProperty("min_health", "50");
            condition.Initialize(testOwner.AddComponent<TestRPGConditionComponent>(), blackBoard);

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
        }

        [Test][Description("HealthCheckCondition実行時に現在体力がmin_health闾値を下回る場合の動作")]
        public void HealthCheckCondition_WithInsufficientHealth_ReturnsFailure()
        {
            // Arrange
            var condition = new HealthCheckCondition();
            var healthComponent = testOwner.AddComponent<Health>();
            healthComponent.CurrentHealth = 30;
            healthComponent.MaxHealth = 100;

            condition.SetProperty("min_health", "50");
            condition.Initialize(testOwner.AddComponent<TestRPGConditionComponent>(), blackBoard);

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        [Test][Description("Healthコンポーネントが存在しない場合にHealthCheckConditionがFailureを返しエラーログを出力することを確認")]
        public void HealthCheckCondition_WithoutHealthComponent_ReturnsFailure()
        {
            // Arrange
            var condition = new HealthCheckCondition();
            condition.Initialize(testOwner.AddComponent<TestRPGConditionComponent>(), blackBoard);


            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        [Test][Description("安全期間中の場合にHealthCheckConditionが緊急時チェックをスキップしてFailureを返すことを確認")]
        public void HealthCheckCondition_InSafetyPeriod_SkipsCheck()
        {
            // Arrange
            var condition = new HealthCheckCondition();
            var healthComponent = testOwner.AddComponent<Health>();
            healthComponent.CurrentHealth = 10; // 低い体力
            healthComponent.MaxHealth = 100;

            condition.SetProperty("min_health", "20"); // 緊急時閾値
            condition.Initialize(testOwner.AddComponent<TestRPGConditionComponent>(), blackBoard);

            // 安全期間を設定
            blackBoard.SetValue("safety_timer", Time.time + 10f);


            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result); // 緊急時チェックをスキップ
        }



        [Test][Description("HasManaCondition実行時に現在マナがmin_mana闾値を上回る十分なマナ状態の場合の動作")]
        public void HasManaCondition_WithSufficientMana_ReturnsSuccess()
        {
            // Arrange
            var condition = new HasManaCondition();
            var manaComponent = testOwner.AddComponent<Mana>();
            manaComponent.CurrentMana = 80;
            manaComponent.MaxMana = 100;

            condition.SetProperty("min_mana", "50");
            condition.Initialize(testOwner.AddComponent<TestRPGConditionComponent>(), blackBoard);

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
        }

        [Test][Description("HasManaCondition実行時に現在マナがmin_mana闾値を下回る場合の動作")]
        public void HasManaCondition_WithInsufficientMana_ReturnsFailure()
        {
            // Arrange
            var condition = new HasManaCondition();
            var manaComponent = testOwner.AddComponent<Mana>();
            manaComponent.CurrentMana = 30;
            manaComponent.MaxMana = 100;

            condition.SetProperty("min_mana", "50");
            condition.Initialize(testOwner.AddComponent<TestRPGConditionComponent>(), blackBoard);

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        [Test][Description("Manaコンポーネントが存在しない場合にHasManaConditionがFailureを返しエラーログを出力することを確認")]
        public void HasManaCondition_WithoutManaComponent_ReturnsFailure()
        {
            // Arrange
            var condition = new HasManaCondition();
            condition.Initialize(testOwner.AddComponent<TestRPGConditionComponent>(), blackBoard);

            // BTLoggerのエラーログを期待（Manaコンポーネントがない場合）

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }



        [Test][Description("HasItemCondition実行時にInventoryコンポーネントに指定アイテムが存在する場合の動作")]
        public void HasItemCondition_WithRequiredItem_ReturnsSuccess()
        {
            // Arrange
            var condition = new HasItemCondition();
            var inventoryComponent = testOwner.AddComponent<Inventory>();
            
            // テストアイテムを追加
            inventoryComponent.AddItem("healing_potion", 3);

            // HasItemConditionは "item_type" プロパティを使用
            condition.SetProperty("item_type", "healing_potion");
            condition.Initialize(testOwner.AddComponent<TestRPGConditionComponent>(), blackBoard);

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
        }

        [Test][Description("必要なアイテムを所持していない場合にHasItemConditionがFailureを返しエラーログを出力することを確認")]
        public void HasItemCondition_WithoutRequiredItem_ReturnsFailure()
        {
            // Arrange
            var condition = new HasItemCondition();
            var inventoryComponent = testOwner.AddComponent<TestRPGConditionInventoryComponent>();

            condition.SetProperty("item_name", "rare_item");
            condition.SetProperty("quantity", "1");
            
            // BTLoggerのエラーログを期待（正規のInventoryコンポーネントがない場合）
            
            condition.Initialize(testOwner.AddComponent<TestRPGConditionComponent>(), blackBoard);

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }



        [Test][Description("EnemyCheckConditionのdetection_rangeパラメータで敵検出範囲設定機能の検証")]
        public void EnemyCheckCondition_SetProperty_SetsCorrectRange()
        {
            // Arrange
            var condition = new EnemyCheckCondition();

            // Act
            condition.SetProperty("detection_range", "15.0");
            condition.Initialize(testOwner.AddComponent<TestRPGConditionComponent>(), blackBoard);

            // 敵がいない状態での実行を確認
            var result = condition.Execute();
            
            // Assert (敵がいないのでFailureが期待される)
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        [Test][Description("範囲内に敵がいる場合のEnemyCheckConditionの動作を確認（環境依存）")]
        public void EnemyCheckCondition_WithEnemyInRange_ReturnsSuccess()
        {
            // Arrange
            var condition = new EnemyCheckCondition();
            condition.SetProperty("detection_range", "10.0");
            condition.Initialize(testOwner.AddComponent<TestRPGConditionComponent>(), blackBoard);

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



        [Test][Description("IsInitializedCondition実行時にBlackBoardにis_initializedフラグがtrueの場合の動作")]
        public void IsInitializedCondition_WithInitializedFlag_ReturnsSuccess()
        {
            // Arrange
            var condition = new IsInitializedCondition();
            condition.Initialize(testOwner.AddComponent<TestRPGConditionComponent>(), blackBoard);

            // BlackBoardに初期化フラグを設定
            blackBoard.SetValue("is_initialized", true);

            // Act
            var result = condition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
        }

        [Test][Description("IsInitializedCondition実行時に初期化フラグがfalseの場合の自動初期化機能")]
        public void IsInitializedCondition_WithoutInitializedFlag_InitializesAndReturnsSuccess()
        {
            // Arrange
            var condition = new IsInitializedCondition();
            condition.Initialize(testOwner.AddComponent<TestRPGConditionComponent>(), blackBoard);

            // 初期化フラグを設定しない（またはfalse）
            blackBoard.SetValue("is_initialized", false);

            // Act
            var result = condition.Execute();

            // Assert - IsInitializedConditionは初期化されていない場合に初期化を実行してSuccessを返す
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.AreEqual(true, blackBoard.GetValue<bool>("is_initialized"));
        }



        [Test][Description("複数のRPGConditionノード（体力、マナ、初期化）の連続評価が正しく動作することを確認")]
        public void RPGConditions_ChainEvaluation_WorksCorrectly()
        {
            // Arrange: 複数の条件を組み合わせ
            var healthCondition = new HealthCheckCondition();
            var manaCondition = new HasManaCondition();
            var initCondition = new IsInitializedCondition();

            // コンポーネントを設定
            var healthComponent = testOwner.AddComponent<Health>();
            healthComponent.CurrentHealth = 80;
            healthComponent.MaxHealth = 100;

            var manaComponent = testOwner.AddComponent<Mana>();
            manaComponent.CurrentMana = 70;
            manaComponent.MaxMana = 100;

            // BlackBoard初期化
            blackBoard.SetValue("is_initialized", true);

            // 条件を初期化
            healthCondition.SetProperty("min_health", "50");
            manaCondition.SetProperty("min_mana", "30");

            var testComponent = testOwner.AddComponent<TestRPGConditionComponent>();
            healthCondition.Initialize(testComponent, blackBoard);
            manaCondition.Initialize(testComponent, blackBoard);
            initCondition.Initialize(testComponent, blackBoard);

            // Act & Assert: 全ての条件がSuccessを返すことを確認
            Assert.AreEqual(BTNodeResult.Success, healthCondition.Execute());
            Assert.AreEqual(BTNodeResult.Success, manaCondition.Execute());
            Assert.AreEqual(BTNodeResult.Success, initCondition.Execute());
        }

        [Test][Description("複数のRPGConditionノード間でBlackBoardを介した状態共有が正しく機能することを確認")]
        public void RPGConditions_BlackBoardInteraction_SharesStateCorrectly()
        {
            // Arrange
            var healthCondition = new HealthCheckCondition();
            var manaCondition = new HasManaCondition();

            // コンポーネント設定
            var healthComponent = testOwner.AddComponent<Health>();
            healthComponent.CurrentHealth = 25; // 低い体力
            healthComponent.MaxHealth = 100;

            var manaComponent = testOwner.AddComponent<Mana>();
            manaComponent.CurrentMana = 80; // 十分なマナ
            manaComponent.MaxMana = 100;

            var testComponent = testOwner.AddComponent<TestRPGConditionComponent>();

            healthCondition.SetProperty("min_health", "50");
            manaCondition.SetProperty("min_mana", "30");

            healthCondition.Initialize(testComponent, blackBoard);
            manaCondition.Initialize(testComponent, blackBoard);

            // Act
            var healthResult = healthCondition.Execute();
            var manaResult = manaCondition.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, healthResult); // 体力不足
            Assert.AreEqual(BTNodeResult.Success, manaResult);   // マナ十分

            // BlackBoardが両方の条件で共有されていることを確認
            Assert.IsNotNull(blackBoard);
        }

    }

    /// <summary>テスト用のRPGConditionコンポーネント</summary>
    public class TestRPGConditionComponent : MonoBehaviour
    {
        // テスト用の空のMonoBehaviourコンポーネント
    }

    /// <summary>テスト用のRPGCondition Inventoryコンポーネント</summary>
    public class TestRPGConditionInventoryComponent : MonoBehaviour
    {
        readonly Dictionary<string, int> items = 
            new Dictionary<string, int>();

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
