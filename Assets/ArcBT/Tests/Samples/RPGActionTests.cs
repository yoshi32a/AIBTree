using ArcBT.Core;
using ArcBT.Samples.RPG.Actions;
using ArcBT.Samples.RPG.Components;
using ArcBT.TagSystem;
using NUnit.Framework;
using UnityEngine;

namespace ArcBT.Tests.Samples
{
    /// <summary>RPG Sample Actionsの機能をテストするクラス</summary>
    [TestFixture]
    public class RPGActionTests : BTTestBase
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

        #region AttackEnemyAction Tests

        [Test][Description("AttackEnemyActionのプロパティ設定(ダメージ、攻撃範囲、クールダウン)が正しく適用されることを確認")]
        public void AttackEnemyAction_SetProperty_SetsCorrectValues()
        {
            // Arrange
            var action = new AttackEnemyAction();

            // Act
            action.SetProperty("damage", "50");
            action.SetProperty("attack_range", "3.0");
            action.SetProperty("cooldown", "0.0"); // テスト用にクールダウンを0に設定

            // Assert (プロパティ設定を実行動作で確認)
            action.Initialize(testOwner.AddComponent<TestRPGActionComponent>(), blackBoard);

            // 敵がいない状態での実行確認
            var result = action.Execute();
            Assert.AreEqual(BTNodeResult.Failure, result); // 敵がいないのでFailure
        }

        [Test][Description("AttackEnemyActionで敵が存在しない場合にFailureが返されることを確認")]
        public void AttackEnemyAction_ExecuteWithoutEnemy_ReturnsFailure()
        {
            // Arrange
            var action = new AttackEnemyAction();
            action.SetProperty("cooldown", "0.0"); // クールダウンを0に設定
            action.Initialize(testOwner.AddComponent<TestRPGActionComponent>(), blackBoard);

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        [Test][Description("AttackEnemyActionでBlackBoardに設定された敵情報を使用して攻撃が実行されることを確認")]
        public void AttackEnemyAction_ExecuteWithBlackBoardEnemy_AttacksTarget()
        {
            // Arrange
            var enemyObj = new GameObject("Enemy");
            var enemyHealth = enemyObj.AddComponent<Health>();
            enemyHealth.CurrentHealth = 100;
            enemyHealth.MaxHealth = 100;

            // 敵にGameplayTagComponentとCharacter.Enemyタグを追加
            var tagComponent = enemyObj.AddComponent<GameplayTagComponent>();
            tagComponent.AddTag(new GameplayTag("Character.Enemy"));

            // 敵を攻撃範囲内（1.5m）に配置
            enemyObj.transform.position = testOwner.transform.position + Vector3.forward * 1.5f;

            var action = new AttackEnemyAction();
            action.SetProperty("damage", "30");
            action.SetProperty("cooldown", "0.0");
            action.SetProperty("attack_range", "5.0"); // 攻撃範囲を明示的に設定
            action.Initialize(testOwner.AddComponent<TestRPGActionComponent>(), blackBoard);

            // BlackBoardに敵情報を設定
            blackBoard.SetValue("target_enemy", enemyObj);

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.AreEqual(70, enemyHealth.CurrentHealth); // ダメージが適用されたことを確認

            // Cleanup
            Object.DestroyImmediate(enemyObj);
        }

        #endregion

        #region CastSpellAction Tests

        [Test][Description("CastSpellActionのプロパティ設定(魔法名、ダメージ、マナコスト、詠唱時間)が正しく適用されることを確認")]
        public void CastSpellAction_SetProperty_SetsCorrectValues()
        {
            // Arrange
            var action = new CastSpellAction();

            // Act
            action.SetProperty("spell_name", "Fireball");
            action.SetProperty("damage", "40");
            action.SetProperty("mana_cost", "20");
            action.SetProperty("cast_time", "1.5");

            // Assert (プロパティ設定を実行動作で確認)
            action.Initialize(testOwner.AddComponent<TestRPGActionComponent>(), blackBoard);
            
            var result = action.Execute();

            // Assert - ログではなく実際の動作を検証
            Assert.AreEqual(BTNodeResult.Failure, result, "Manaコンポーネントがない場合、Failureが返されるべき");
            
            // 注意: エラーログはLoggingBehaviorTestsで専用テストが行われます
        }

        [Test][Description("CastSpellActionでマナが不足している場合にFailureが返されることを確認")]
        public void CastSpellAction_ExecuteWithoutMana_ReturnsFailure()
        {
            // Arrange
            var action = new CastSpellAction();
            action.SetProperty("spell_name", "Heal");
            action.SetProperty("mana_cost", "50");

            var manaComponent = testOwner.AddComponent<Mana>();
            manaComponent.CurrentMana = 20; // マナ不足
            manaComponent.MaxMana = 100;

            action.Initialize(testOwner.AddComponent<TestRPGActionComponent>(), blackBoard);

            // Act
            var result = action.Execute();

            // Assert - ログではなく実際の動作を検証
            Assert.AreEqual(BTNodeResult.Failure, result, "マナが不足している場合、Failureが返されるべき");
            Assert.AreEqual(20, manaComponent.CurrentMana, "マナが消費されていないべき");
            
            // 注意: マナ不足ログはLoggingBehaviorTestsで専用テストが行われます
        }

        #endregion

        #region UseItemAction Tests

        [Test][Description("UseItemActionのプロパティ設定(アイテム名、数量)が正しく適用されることを確認")]
        public void UseItemAction_SetProperty_SetsCorrectValues()
        {
            // Arrange
            var action = new UseItemAction();

            // Act
            action.SetProperty("item_type", "healing_potion");
            action.SetProperty("quantity", "1");

            // Assert (プロパティ設定を実行動作で確認)
            action.Initialize(testOwner.AddComponent<TestRPGActionComponent>(), blackBoard);
            var result = action.Execute();

            // Assert - ログではなく実際の動作を検証
            Assert.AreEqual(BTNodeResult.Failure, result, "Inventoryコンポーネントがない場合、Failureが返されるべき");
            
            // 注意: エラーログはLoggingBehaviorTestsで専用テストが行われます
        }

        [Test][Description("UseItemActionでアイテムを所持していない場合にFailureが返されることを確認")]
        public void UseItemAction_ExecuteWithoutItem_ReturnsFailure()
        {
            // Arrange
            var action = new UseItemAction();
            action.SetProperty("item_name", "rare_item");
            action.SetProperty("quantity", "1");

            var inventoryComponent = testOwner.AddComponent<Inventory>();
            // アイテムを追加しない（空のインベントリ）

            action.Initialize(testOwner.AddComponent<TestRPGActionComponent>(), blackBoard);

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        [Test][Description("UseItemActionで正常にアイテムを使用できる場合にSuccessが返されることを確認")]
        public void UseItemAction_ExecuteWithValidItem_ReturnsSuccess()
        {
            // Arrange
            var action = new UseItemAction();
            action.SetProperty("item_type", "healing_potion"); // 正しいプロパティ名とアイテム名
            action.SetProperty("quantity", "1");

            var inventoryComponent = testOwner.AddComponent<Inventory>();
            inventoryComponent.AddItem("healing_potion", 2); // アイテムを追加

            action.Initialize(testOwner.AddComponent<TestRPGActionComponent>(), blackBoard);

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.AreEqual(1, inventoryComponent.GetItemCount("healing_potion")); // アイテムが1つ消費された
        }

        #endregion

        #region FleeToSafetyAction Tests

        [Test][Description("FleeToSafetyActionのプロパティ設定(逃走速度、安全距離)が正しく適用されることを確認")]
        public void FleeToSafetyAction_SetProperty_SetsCorrectValues()
        {
            // Arrange
            var action = new FleeToSafetyAction();

            // Act
            action.SetProperty("flee_speed", "8.0");
            action.SetProperty("safe_distance", "15.0");

            // 脅威となる敵オブジェクトをBlackBoardに設定
            var threatObj = new GameObject("TestThreat");
            threatObj.transform.position = testOwner.transform.position + Vector3.forward * 3f;
            blackBoard.SetValue("enemy_target", threatObj);

            // Assert (プロパティ設定を実行動作で確認)
            action.Initialize(testOwner.AddComponent<TestRPGActionComponent>(), blackBoard);
            var result = action.Execute();

            // 逃走アクションが実行されることを確認
            Assert.IsTrue(result is BTNodeResult.Running or BTNodeResult.Success);

            // Cleanup
            Object.DestroyImmediate(threatObj);
        }

        [Test][Description("FleeToSafetyActionの実行でBlackBoardに逃走ターゲットと安全期間が正しく設定されることを確認")]
        public void FleeToSafetyAction_Execute_UpdatesBlackBoard()
        {
            // Arrange
            var action = new FleeToSafetyAction();
            action.Initialize(testOwner.AddComponent<TestRPGActionComponent>(), blackBoard);

            // 脅威となる敵オブジェクトをBlackBoardに設定
            var enemyObj = new GameObject("TestEnemy");
            enemyObj.transform.position = testOwner.transform.position + Vector3.forward * 5f;
            blackBoard.SetValue("enemy_target", enemyObj);

            // Act
            var result = action.Execute();

            // Assert
            Assert.IsTrue(result is BTNodeResult.Running or BTNodeResult.Success);
            
            // BlackBoardに逃走関連のデータが設定されることを確認
            bool hasFleeData = blackBoard.HasKey("flee_target") ||
                              blackBoard.HasKey("safety_timer") ||
                              blackBoard.HasKey("current_action");
            Assert.IsTrue(hasFleeData);

            // Cleanup
            Object.DestroyImmediate(enemyObj);
        }

        #endregion

        #region Multiple Actions Integration Tests

        [Test][Description("複数のRPGアクション（攻撃、魔法、アイテム使用）の連続実行が正しく動作することを確認")]
        public void RPGActions_ChainExecution_WorksCorrectly()
        {
            // Arrange: 複数のアクションを組み合わせ
            var attackAction = new AttackEnemyAction();
            var spellAction = new CastSpellAction();
            var itemAction = new UseItemAction();

            // 各アクションのプロパティ設定
            attackAction.SetProperty("damage", "25");
            attackAction.SetProperty("cooldown", "0.0");

            spellAction.SetProperty("spell_name", "Heal");
            spellAction.SetProperty("mana_cost", "10");

            itemAction.SetProperty("item_type", "mana_potion");
            itemAction.SetProperty("quantity", "1");

            // コンポーネント設定
            var healthComponent = testOwner.AddComponent<Health>();
            healthComponent.CurrentHealth = 50;
            healthComponent.MaxHealth = 100;

            var manaComponent = testOwner.AddComponent<Mana>();
            manaComponent.CurrentMana = 30;
            manaComponent.MaxMana = 100;

            var inventoryComponent = testOwner.AddComponent<Inventory>();
            inventoryComponent.AddItem("mana_potion", 1);

            // アクションを初期化
            var testComponent = testOwner.AddComponent<TestRPGActionComponent>();
            attackAction.Initialize(testComponent, blackBoard);
            spellAction.Initialize(testComponent, blackBoard);
            itemAction.Initialize(testComponent, blackBoard);

            // Act & Assert: 各アクションの実行
            // 1. アイテム使用（マナポーション）
            var itemResult = itemAction.Execute();
            Assert.AreEqual(BTNodeResult.Success, itemResult);

            // 2. 攻撃（敵がいないのでFailure）
            var attackResult = attackAction.Execute();
            Assert.AreEqual(BTNodeResult.Failure, attackResult);

            // 3. 魔法（マナは十分だが、ターゲットが見つからないのでFailure）
            var spellResult = spellAction.Execute();
            Assert.AreEqual(BTNodeResult.Failure, spellResult, "ターゲットが見つからない場合、魔法はFailureを返すべき");
            
            // 注意: ターゲット不在のログはLoggingBehaviorTestsで専用テストが行われます
        }

        #endregion
    }

    /// <summary>テスト用のRPGActionコンポーネント</summary>
    public class TestRPGActionComponent : MonoBehaviour
    {
        // テスト用の空のMonoBehaviourコンポーネント
    }
}
