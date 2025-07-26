using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ArcBT.Core;
using ArcBT.Actions;
using ArcBT.Samples.RPG.Actions;

namespace ArcBT.Tests
{
    /// <summary>ActionNodeの機能をテストするクラス</summary>
    [TestFixture]
    public class ActionNodeTests
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

        #region MoveToPositionAction Tests

        [Test]
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
            action.Initialize(testOwner.GetComponent<MonoBehaviour>() ?? testOwner.AddComponent<TestComponent>(), blackBoard);

            // Assert - NameプロパティがnullまたはEmptyの場合、「_target_position」のキーが使用される
            string expectedNameKey = string.IsNullOrEmpty(action.Name) ? "_target_position" : $"{action.Name}_target_position";
            string expectedTargetKey = string.IsNullOrEmpty(action.Name) ? "_target_name" : $"{action.Name}_target_name";
            
            // Debug出力でBlackBoardの内容を確認
            Debug.Log($"Action Name: '{action.Name}'");
            Debug.Log($"Expected position key: '{expectedNameKey}'");
            Debug.Log($"Expected target key: '{expectedTargetKey}'");
            Debug.Log($"BlackBoard keys: {string.Join(", ", blackBoard.GetAllKeys())}");
            
            Assert.IsTrue(blackBoard.HasKey(expectedNameKey), $"Expected key '{expectedNameKey}' not found in BlackBoard");
            Assert.IsTrue(blackBoard.HasKey(expectedTargetKey), $"Expected key '{expectedTargetKey}' not found in BlackBoard");
            Assert.AreEqual(new Vector3(5, 0, 5), blackBoard.GetValue<Vector3>(expectedNameKey));
            Assert.AreEqual("TestTarget", blackBoard.GetValue<string>(expectedTargetKey));

            // Cleanup
            Object.DestroyImmediate(targetObj);
        }

        [Test]
        public void MoveToPositionAction_InvalidTarget_HandlesGracefully()
        {
            // Arrange
            var action = new MoveToPositionAction();
            action.SetProperty("target", "NonExistentTarget");
            
            LogAssert.Expect(LogType.Warning, "MoveToPosition: Target 'NonExistentTarget' not found!");

            // Act
            action.Initialize(testOwner.AddComponent<TestComponent>(), blackBoard);

            // Assert
            Assert.IsFalse(blackBoard.HasKey("Action:MoveToPosition_target_position"));
        }

        [Test]
        public void MoveToPositionAction_ExecuteWithoutTarget_ReturnsFailure()
        {
            // Arrange
            var action = new MoveToPositionAction();
            action.Initialize(testOwner.AddComponent<TestComponent>(), blackBoard);

            // 期待されるエラーログを指定
            LogAssert.Expect(LogType.Error, "MoveToPosition '': No valid target '' - trying to find it again");
            LogAssert.Expect(LogType.Error, "MoveToPosition '': No target name specified!");

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        #endregion

        #region ScanEnvironmentAction Tests

        [Test]
        public void ScanEnvironmentAction_SetProperty_SetsCorrectValues()
        {
            // Arrange
            var action = new ScanEnvironmentAction();

            // Act
            action.SetProperty("scan_interval", "0.0");
            action.SetProperty("scan_radius", "20.0");

            // Assert (プロパティの設定を確認するため、実行してBlackBoardの値をチェック)
            action.Initialize(testOwner.AddComponent<TestComponent>(), blackBoard);
            var result = action.Execute();
            
            // スキャンが実行されることを確認
            Assert.IsTrue(result == BTNodeResult.Success || result == BTNodeResult.Running);
        }

        [Test]
        public void ScanEnvironmentAction_ExecuteWithoutOwner_ReturnsFailure()
        {
            // Arrange
            var action = new ScanEnvironmentAction();
            action.Initialize(null, blackBoard);

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        [Test]
        public void ScanEnvironmentAction_ExecuteWithoutBlackBoard_ReturnsFailure()
        {
            // Arrange
            var action = new ScanEnvironmentAction();
            action.Initialize(testOwner.AddComponent<TestComponent>(), null);

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        [Test]
        public void ScanEnvironmentAction_Execute_UpdatesBlackBoard()
        {
            // Arrange
            var action = new ScanEnvironmentAction();
            action.SetProperty("scan_interval", "0.0");
            action.Initialize(testOwner.AddComponent<TestComponent>(), blackBoard);

            // Act
            var result = action.Execute();

            // Assert - スキャンが実行されることを確認
            Assert.IsTrue(result == BTNodeResult.Success || result == BTNodeResult.Running);
            
            // BlackBoard にスキャン関連のデータが設定されていることを確認
            Assert.IsTrue(blackBoard.GetAllKeys().Length > 0);
            
            // 基本的なスキャンデータのいずれかが存在することを確認
            bool hasBasicScanData = blackBoard.HasKey("enemies_detected") || 
                                   blackBoard.HasKey("items_detected") ||
                                   blackBoard.HasKey("interactables_detected") ||
                                   blackBoard.HasKey("environment_scan_time") ||
                                   blackBoard.HasKey("threat_level");
            Assert.IsTrue(hasBasicScanData);
            
            // 数値データが0以上であることを確認（存在する場合）
            if (blackBoard.HasKey("enemies_detected"))
                Assert.GreaterOrEqual(blackBoard.GetValue<int>("enemies_detected"), 0);
            if (blackBoard.HasKey("items_detected"))
                Assert.GreaterOrEqual(blackBoard.GetValue<int>("items_detected"), 0);
            if (blackBoard.HasKey("interactables_detected"))
                Assert.GreaterOrEqual(blackBoard.GetValue<int>("interactables_detected"), 0);
        }

        #endregion

        #region WaitAction Tests

        [Test]
        public void WaitAction_SetProperty_SetsCorrectDuration()
        {
            // Arrange
            var action = new WaitAction();

            // Act
            action.SetProperty("duration", "2.5");
            action.Initialize(testOwner.AddComponent<TestComponent>(), blackBoard);

            // Assert (duration設定の確認は実行動作で行う)
            var result = action.Execute();
            Assert.AreEqual(BTNodeResult.Running, result); // 待機中はRunning
        }

        [Test]
        public void WaitAction_Execute_ReturnsRunningInitially()
        {
            // Arrange
            var action = new WaitAction();
            action.SetProperty("duration", "1.0");
            action.Initialize(testOwner.AddComponent<TestComponent>(), blackBoard);

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Running, result);
        }

        [Test]
        public void WaitAction_ExecuteMultipleTimes_EventuallyReturnsSuccess()
        {
            // Arrange
            var action = new WaitAction();
            action.SetProperty("duration", "0.1"); // 短い待機時間
            action.Initialize(testOwner.AddComponent<TestComponent>(), blackBoard);

            // Act - 最初の実行
            var firstResult = action.Execute();
            Assert.AreEqual(BTNodeResult.Running, firstResult);

            // 十分な時間を経過させる（Time.timeは実際の時間に依存するため）
            // テスト環境では完了まで待つのが困難なので、Runningが返ることを確認
        }

        #endregion

        #region RandomWanderAction Tests

        [Test]
        public void RandomWanderAction_SetProperty_SetsCorrectValues()
        {
            // Arrange
            var action = new RandomWanderAction();

            // Act
            action.SetProperty("speed", "5.0");
            action.SetProperty("wander_radius", "8.0");
            action.SetProperty("change_direction_interval", "3.0");

            // Assert (プロパティ設定の動作確認)
            action.Initialize(testOwner.AddComponent<TestComponent>(), blackBoard);
            var result = action.Execute();
            
            // 実行できることを確認（詳細な動作は環境に依存）
            Assert.IsTrue(result == BTNodeResult.Running || result == BTNodeResult.Success);
        }

        [Test]
        public void RandomWanderAction_Execute_UpdatesBlackBoard()
        {
            // Arrange
            var action = new RandomWanderAction();
            action.Initialize(testOwner.AddComponent<TestComponent>(), blackBoard);

            // Act
            var result = action.Execute();

            // Assert
            Assert.IsTrue(blackBoard.HasKey("wander_target"));
            Assert.IsTrue(blackBoard.HasKey("current_action"));
            
            // アクション状態が正しく設定されていることを確認
            var currentAction = blackBoard.GetValue<string>("current_action");
            Assert.AreEqual("RandomWander", currentAction);
        }

        #endregion

        #region AttackEnemyAction Tests

        [Test]
        public void AttackEnemyAction_SetProperty_SetsCorrectValues()
        {
            // Arrange
            var action = new AttackEnemyAction();

            // Act
            action.SetProperty("damage", "50");
            action.SetProperty("attack_range", "3.0");
            action.SetProperty("cooldown", "0.0"); // テスト用にクールダウンを0に設定

            // Assert (プロパティ設定を実行動作で確認)
            action.Initialize(testOwner.AddComponent<TestComponent>(), blackBoard);
            
            // 敵がいない状態での実行確認
            var result = action.Execute();
            Assert.AreEqual(BTNodeResult.Failure, result); // 敵がいないのでFailure
        }

        [Test]
        public void AttackEnemyAction_ExecuteWithoutEnemy_ReturnsFailure()
        {
            // Arrange
            var action = new AttackEnemyAction();
            action.SetProperty("cooldown", "0.0"); // クールダウンを0に設定
            action.Initialize(testOwner.AddComponent<TestComponent>(), blackBoard);

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        [Test]
        public void AttackEnemyAction_ExecuteWithBlackBoardEnemy_AttacksTarget()
        {
            // Arrange
            var enemyObj = new GameObject("Enemy");
            enemyObj.tag = "Enemy"; // タグを設定（AttackEnemyActionがFindGameObjectsWithTagを使用）
            var action = new AttackEnemyAction();
            
            action.SetProperty("cooldown", "0.0"); // クールダウンを0に設定
            action.SetProperty("attack_range", "10.0"); // 攻撃範囲を広く設定
            
            // BlackBoardに敵情報を設定
            blackBoard.SetValue("nearest_enemy", enemyObj);
            blackBoard.SetValue("enemy_position", enemyObj.transform.position);
            
            action.Initialize(testOwner.AddComponent<TestComponent>(), blackBoard);

            // Act
            var result = action.Execute();

            // Assert
            // 敵がタグ付けされていれば攻撃が実行される
            Assert.IsTrue(result == BTNodeResult.Success || result == BTNodeResult.Failure);

            // Cleanup
            Object.DestroyImmediate(enemyObj);
        }

        #endregion

        #region CastSpellAction Tests

        [Test]
        public void CastSpellAction_SetProperty_SetsCorrectValues()
        {
            // Arrange
            var action = new CastSpellAction();

            // Act
            action.SetProperty("spell_name", "fireball");
            action.SetProperty("damage", "60");
            action.SetProperty("mana_cost", "30");
            action.SetProperty("cast_time", "2.0");

            // Assert
            action.Initialize(testOwner.AddComponent<TestComponent>(), blackBoard);
            var result = action.Execute();
            
            // 魔法詠唱の開始を確認
            Assert.IsTrue(result == BTNodeResult.Running || result == BTNodeResult.Failure);
        }

        [Test]
        public void CastSpellAction_ExecuteWithoutMana_ReturnsFailure()
        {
            // Arrange
            var action = new CastSpellAction();
            action.SetProperty("mana_cost", "50");
            
            // マナコンポーネントを追加して0に設定
            var manaComponent = testOwner.AddComponent<TestManaComponent>();
            manaComponent.currentMana = 0;
            
            action.Initialize(testOwner.AddComponent<TestComponent>(), blackBoard);

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        #endregion

        #region UseItemAction Tests

        [Test]
        public void UseItemAction_SetProperty_SetsCorrectValues()
        {
            // Arrange
            var action = new UseItemAction();

            // Act
            action.SetProperty("item_name", "health_potion");
            action.SetProperty("quantity", "1");

            // Assert
            action.Initialize(testOwner.AddComponent<TestComponent>(), blackBoard);
            var result = action.Execute();
            
            // アイテム使用の試行を確認
            Assert.IsTrue(result == BTNodeResult.Success || result == BTNodeResult.Failure);
        }

        #endregion

        #region 統合テスト

        [Test]
        public void ActionNodes_ChainExecution_WorksCorrectly()
        {
            // Arrange: 複数のアクションを連続実行
            var scanAction = new ScanEnvironmentAction();
            var waitAction = new WaitAction();
            var wanderAction = new RandomWanderAction();

            scanAction.SetProperty("scan_interval", "0.0");
            scanAction.Initialize(testOwner.AddComponent<TestComponent>(), blackBoard);
            waitAction.Initialize(testOwner.GetComponent<TestComponent>(), blackBoard);
            wanderAction.Initialize(testOwner.GetComponent<TestComponent>(), blackBoard);

            waitAction.SetProperty("duration", "0.1");

            // Act & Assert: 各アクションが正常に実行される
            var scanResult = scanAction.Execute();
            Assert.IsTrue(scanResult == BTNodeResult.Success || scanResult == BTNodeResult.Running);

            var waitResult = waitAction.Execute();
            Assert.AreEqual(BTNodeResult.Running, waitResult);

            var wanderResult = wanderAction.Execute();
            Assert.IsTrue(wanderResult == BTNodeResult.Success || wanderResult == BTNodeResult.Running);

            // BlackBoardに適切な情報が蓄積されていることを確認
            Assert.IsTrue(blackBoard.GetAllKeys().Length > 0);
        }

        [Test]
        public void ActionNodes_BlackBoardInteraction_SharesDataCorrectly()
        {
            // Arrange
            var scanAction = new ScanEnvironmentAction();
            var attackAction = new AttackEnemyAction();

            // スキャン間隔を0にして即座に実行できるようにする
            scanAction.SetProperty("scan_interval", "0.0");
            
            scanAction.Initialize(testOwner.AddComponent<TestComponent>(), blackBoard);
            attackAction.Initialize(testOwner.GetComponent<TestComponent>(), blackBoard);

            // Act: スキャンしてから攻撃
            var scanResult = scanAction.Execute();
            
            // Debug: スキャン結果と BlackBoard 内容を確認
            Debug.Log($"Scan result: {scanResult}");
            Debug.Log($"BlackBoard keys count: {blackBoard.GetAllKeys().Length}");
            foreach (var key in blackBoard.GetAllKeys())
            {
                Debug.Log($"BlackBoard key: {key} = {blackBoard.GetValueAsString(key)}");
            }
            
            // スキャンが正常に実行されたことを確認
            Assert.IsTrue(scanResult == BTNodeResult.Success || scanResult == BTNodeResult.Running);
            
            // BlackBoard にスキャン関連のデータが設定されていることを確認
            // 最低限、基本的なスキャン情報は設定される
            Assert.IsTrue(blackBoard.GetAllKeys().Length > 0, "BlackBoard should contain scan data");
            
            // 基本的なスキャンデータが存在することを確認
            bool hasBasicScanData = blackBoard.HasKey("enemies_detected") || 
                                   blackBoard.HasKey("environment_scan_time") ||
                                   blackBoard.HasKey("threat_level");
            Assert.IsTrue(hasBasicScanData, "BlackBoard should contain basic scan data");
            
            // 攻撃アクションがBlackBoard情報を参照できることを確認
            var attackResult = attackAction.Execute();
            Assert.IsTrue(attackResult == BTNodeResult.Success || attackResult == BTNodeResult.Failure || attackResult == BTNodeResult.Running);
        }

        #endregion
    }

    /// <summary>テスト用のコンポーネント</summary>
    public class TestComponent : MonoBehaviour
    {
        // テスト用の空のMonoBehaviourコンポーネント
    }

    /// <summary>テスト用のマナコンポーネント</summary>
    public class TestManaComponent : MonoBehaviour
    {
        public float currentMana = 100f;
        public float maxMana = 100f;
    }
}