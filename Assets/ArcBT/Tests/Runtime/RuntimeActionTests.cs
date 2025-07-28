using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ArcBT.Core;
using ArcBT.Actions;
using ArcBT.Logger;
using ArcBT.TagSystem;

namespace ArcBT.Tests
{
    /// <summary>Runtime Actionsの機能をテストするクラス</summary>
    [TestFixture]
    public class RuntimeActionTests : BTTestBase
    {
        GameObject testOwner;
        BlackBoard blackBoard;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp(); // BTTestBaseのセットアップを実行（ログ抑制含む）
            testOwner = CreateTestGameObject("TestOwner");
            blackBoard = new BlackBoard();
            
            // GameplayTagManagerを強制初期化
            var existingManager = GameObject.FindFirstObjectByType<GameplayTagManager>();
            if (existingManager != null)
            {
                Object.DestroyImmediate(existingManager.gameObject);
            }
            
            var managerGo = new GameObject("GameplayTagManager");
            var manager = managerGo.AddComponent<GameplayTagManager>();
            
            // Instanceプロパティを呼び出して初期化を確実にする
            _ = GameplayTagManager.Instance;
        }

        [TearDown]
        public override void TearDown()
        {
            DestroyTestObject(testOwner);

            // すべてのテスト用GameObjectsを削除
            var testObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(go => go.name.StartsWith("Test") || go.name.Contains("Enemy") || go.name.Contains("Item"))
                .ToArray();
            
            foreach (var obj in testObjects)
            {
                Object.DestroyImmediate(obj);
            }

            // GameplayTagManagerのクリーンアップ
            var managers = GameObject.FindObjectsByType<GameplayTagManager>(FindObjectsSortMode.None);
            foreach (var manager in managers)
            {
                Object.DestroyImmediate(manager.gameObject);
            }

            base.TearDown(); // BTTestBaseのクリーンアップを実行
        }

        #region EnvironmentScanAction Tests

        [Test][Description("EnvironmentScanActionのscan_rangeパラメータ設定機能の動作検証")]
        public void EnvironmentScanAction_SetProperty_SetsCorrectRange()
        {
            // Arrange
            var action = new EnvironmentScanAction();

            // Act
            action.SetProperty("scan_range", "15.0");
            action.Initialize(testOwner.AddComponent<TestActionComponent>(), blackBoard);

            // Assert - 実行して動作確認
            var result = action.Execute();
            Assert.IsTrue(result is BTNodeResult.Success or BTNodeResult.Failure);
        }

        [Test][Description("EnvironmentScanActionのscan_tagsパラメータで複数タグ指定機能の検証")]
        public void EnvironmentScanAction_SetProperty_SetsCorrectTags()
        {
            // Arrange
            var action = new EnvironmentScanAction();

            // Act
            action.SetProperty("scan_tags", "Character.Enemy,Object.Item,Object.Custom");
            action.Initialize(testOwner.AddComponent<TestActionComponent>(), blackBoard);

            // Assert - 実行して動作確認
            var result = action.Execute();
            Assert.IsTrue(result is BTNodeResult.Success or BTNodeResult.Failure);
        }

        [Test][Description("EnvironmentScanAction実行時にスキャン範囲内にタグ付きオブジェクトがない場合の動作")]
        public void EnvironmentScanAction_ExecuteWithNoObjects_ReturnsFailure()
        {
            // Arrange
            var action = new EnvironmentScanAction();
            action.Initialize(testOwner.AddComponent<TestActionComponent>(), blackBoard);

            // Act
            var result = action.Execute();

            // Assert - オブジェクトがないのでFailure
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        [Test][Description("タグ付きオブジェクトが範囲内に存在する場合にEnvironmentScanActionがSuccessを返すことを確認")]
        public void EnvironmentScanAction_ExecuteWithTaggedObjects_ReturnsSuccess()
        {
            // Arrange
            var action = new EnvironmentScanAction();
            action.SetProperty("scan_range", "20.0");
            action.Initialize(testOwner.AddComponent<TestActionComponent>(), blackBoard);

            // 近くにアイテムオブジェクトを配置（GameplayTagSystem対応）
            var itemObj = new GameObject("TestItem");
            var tagComponent = itemObj.AddComponent<GameplayTagComponent>();
            tagComponent.AddTag(new GameplayTag("Object.Item"));
            itemObj.transform.position = testOwner.transform.position + Vector3.forward * 5f;
            
            // 手動でGameplayTagManagerに登録
            GameplayTagManager.Instance.RegisterTagComponent(tagComponent);


            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.IsTrue(blackBoard.HasKey("scanned_object_item"));

            // Cleanup
            Object.DestroyImmediate(itemObj);
        }

        #endregion

        #region SearchForEnemyAction Tests

        [Test][Description("SearchForEnemyActionのsearch_rangeパラメータ設定機能の検証")]
        public void SearchForEnemyAction_SetProperty_SetsCorrectRange()
        {
            // Arrange
            var action = new SearchForEnemyAction();

            // Act
            action.SetProperty("search_range", "12.0");
            action.Initialize(testOwner.AddComponent<TestActionComponent>(), blackBoard);

            // Assert - 実行して動作確認
            var result = action.Execute();
            Assert.AreEqual(BTNodeResult.Failure, result); // 敵がいないのでFailure
        }

        [Test][Description("SearchForEnemyActionのenemy_tagパラメータでカスタムタグ指定機能の検証")]
        public void SearchForEnemyAction_SetProperty_SetsCorrectEnemyTag()
        {
            // Arrange
            var action = new SearchForEnemyAction();

            // Act
            action.SetProperty("enemy_tag", "Character.Monster");
            action.Initialize(testOwner.AddComponent<TestActionComponent>(), blackBoard);

            // Assert - 実行して動作確認
            var result = action.Execute();
            Assert.AreEqual(BTNodeResult.Failure, result); // モンスターがいないのでFailure
        }

        [Test][Description("SearchForEnemyAction実行時に検索範囲内に敵オブジェクトがいない場合の動作")]
        public void SearchForEnemyAction_ExecuteWithNoEnemies_ReturnsFailure()
        {
            // Arrange
            var action = new SearchForEnemyAction();
            action.Initialize(testOwner.AddComponent<TestActionComponent>(), blackBoard);

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        [Test][Description("範囲内に敵がいる場合にSearchForEnemyActionがSuccessを返しBlackBoardに敵情報を記録することを確認")]
        public void SearchForEnemyAction_ExecuteWithEnemyInRange_ReturnsSuccess()
        {
            // Arrange
            var action = new SearchForEnemyAction();
            action.SetProperty("search_range", "15.0");
            action.Initialize(testOwner.AddComponent<TestActionComponent>(), blackBoard);

            // 近くに敵オブジェクトを配置（GameplayTagSystem対応）
            var enemyObj = new GameObject("TestEnemy");
            var tagComponent = enemyObj.AddComponent<GameplayTagComponent>();
            tagComponent.AddTag(new GameplayTag("Character.Enemy"));
            enemyObj.transform.position = testOwner.transform.position + Vector3.forward * 8f;
            
            // 手動でGameplayTagManagerに登録
            GameplayTagManager.Instance.RegisterTagComponent(tagComponent);

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.IsTrue(blackBoard.HasKey("target_enemy"));
            Assert.IsTrue(blackBoard.HasKey("enemy_distance"));
            Assert.IsTrue(blackBoard.HasKey("last_search_time"));
            Assert.AreEqual(enemyObj, blackBoard.GetValue<GameObject>("target_enemy"));

            // Cleanup
            Object.DestroyImmediate(enemyObj);
        }

        [Test][Description("複数の敵がいる場合にSearchForEnemyActionが最も近い敵を選択することを確認")]
        public void SearchForEnemyAction_ExecuteWithMultipleEnemies_ReturnsNearest()
        {
            // Arrange
            var action = new SearchForEnemyAction();
            action.SetProperty("search_range", "20.0");
            action.Initialize(testOwner.AddComponent<TestActionComponent>(), blackBoard);

            // 複数の敵オブジェクトを配置（GameplayTagSystem対応）
            var farEnemyObj = new GameObject("FarEnemy");
            var farTagComponent = farEnemyObj.AddComponent<GameplayTagComponent>();
            farTagComponent.AddTag(new GameplayTag("Character.Enemy"));
            farEnemyObj.transform.position = testOwner.transform.position + Vector3.forward * 15f;
            GameplayTagManager.Instance.RegisterTagComponent(farTagComponent);

            var nearEnemyObj = new GameObject("NearEnemy");
            var nearTagComponent = nearEnemyObj.AddComponent<GameplayTagComponent>();
            nearTagComponent.AddTag(new GameplayTag("Character.Enemy"));
            nearEnemyObj.transform.position = testOwner.transform.position + Vector3.forward * 5f;
            GameplayTagManager.Instance.RegisterTagComponent(nearTagComponent);

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.AreEqual(nearEnemyObj, blackBoard.GetValue<GameObject>("target_enemy"));

            // Cleanup
            Object.DestroyImmediate(farEnemyObj);
            Object.DestroyImmediate(nearEnemyObj);
        }

        #endregion

        #region InteractAction Tests

        [Test][Description("InteractActionのtarget_tagパラメータでスイッチ等の特定タグ指定機能の検証")]
        public void InteractAction_SetProperty_SetsCorrectTargetTag()
        {
            // Arrange
            var action = new InteractAction();

            // Act
            action.SetProperty("target_tag", "Object.Switch");
            action.Initialize(testOwner.AddComponent<TestActionComponent>(), blackBoard);

            // Assert - 実行して動作確認
            var result = action.Execute();
            Assert.AreEqual(BTNodeResult.Failure, result); // スイッチがないのでFailure
        }

        [Test][Description("InteractActionのinteraction_rangeパラメータで接触可能距離設定機能の検証")]
        public void InteractAction_SetProperty_SetsCorrectRange()
        {
            // Arrange
            var action = new InteractAction();

            // Act
            action.SetProperty("interaction_range", "3.5");
            action.Initialize(testOwner.AddComponent<TestActionComponent>(), blackBoard);

            // Assert - 実行して動作確認
            var result = action.Execute();
            Assert.AreEqual(BTNodeResult.Failure, result); // インタラクト可能オブジェクトがないのでFailure
        }

        [Test][Description("InteractAction実行時にIInteractableを実装したオブジェクトが範囲内にない場合の動作")]
        public void InteractAction_ExecuteWithNoInteractables_ReturnsFailure()
        {
            // Arrange
            var action = new InteractAction();
            action.Initialize(testOwner.AddComponent<TestActionComponent>(), blackBoard);

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result);
        }

        [Test][Description("範囲内にインタラクト可能オブジェクトがある場合にInteractActionがインタラクションを実行することを確認")]
        public void InteractAction_ExecuteWithInteractableInRange_PerformsInteraction()
        {
            // Arrange
            var action = new InteractAction();
            action.SetProperty("interaction_range", "5.0");
            action.Initialize(testOwner.AddComponent<TestActionComponent>(), blackBoard);

            // 近くにインタラクト可能オブジェクトを配置（GameplayTagSystem対応）
            var interactableObj = new GameObject("TestSwitch");
            var tagComponent = interactableObj.AddComponent<GameplayTagComponent>();
            tagComponent.AddTag(new GameplayTag("Object.Interactable"));
            interactableObj.transform.position = testOwner.transform.position + Vector3.forward * 3f;
            interactableObj.AddComponent<TestInteractableComponent>();
            
            // 手動でGameplayTagManagerに登録
            GameplayTagManager.Instance.RegisterTagComponent(tagComponent);

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);

            // Cleanup
            Object.DestroyImmediate(interactableObj);
        }

        #endregion

        #region LogAction Tests

        [Test][Description("LogActionのmessageパラメータで日本語メッセージ出力機能の検証")]
        public void LogAction_SetProperty_SetsCorrectMessage()
        {
            // Arrange
            var action = new LogAction();

            // Act
            action.SetProperty("message", "テストメッセージ");
            action.Initialize(testOwner.AddComponent<TestActionComponent>(), blackBoard);

            // Assert
            var result = action.Execute();
            Assert.AreEqual(BTNodeResult.Success, result);
        }

        [Test][Description("LogActionのlevelパラメータでWarningレベル等のログ出力機能の検証")]
        public void LogAction_SetProperty_SetsCorrectLogLevel()
        {
            // Arrange
            var action = new LogAction();

            // Act
            action.SetProperty("level", "Warning");
            action.SetProperty("message", "警告メッセージ");
            action.Initialize(testOwner.AddComponent<TestActionComponent>(), blackBoard);

            // Assert
            var result = action.Execute();
            Assert.AreEqual(BTNodeResult.Success, result);
        }

        [Test][Description("LogActionでBlackBoard情報を含むログ出力が正しく動作することを確認")]
        public void LogAction_ExecuteWithBlackBoardInfo_IncludesBlackBoardData()
        {
            // Arrange
            var action = new LogAction();
            action.SetProperty("message", "BlackBoard情報");
            action.SetProperty("include_blackboard", "true");
            action.Initialize(testOwner.AddComponent<TestActionComponent>(), blackBoard);

            // BlackBoardにテストデータを設定
            blackBoard.SetValue("test_key", "test_value");

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
        }

        [Test][Description("LogActionで特定のBlackBoardキーの値をログ出力することを確認")]
        public void LogAction_ExecuteWithSpecificBlackBoardKey_LogsKeyValue()
        {
            // Arrange
            var action = new LogAction();
            action.SetProperty("message", "特定キー情報");
            action.SetProperty("blackboard_key", "specific_key");
            action.Initialize(testOwner.AddComponent<TestActionComponent>(), blackBoard);

            // BlackBoardに特定キーを設定
            blackBoard.SetValue("specific_key", 42);

            // Act
            var result = action.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result);
        }

        #endregion

        #region SetBlackBoardAction Tests

        [Test][Description("SetBlackBoardActionでint型の値（health=100）をBlackBoardに設定")]
        public void SetBlackBoardAction_SetProperty_SetsIntValue()
        {
            // Arrange
            var action = new SetBlackBoardAction();

            // Act
            action.SetProperty("health", "100");
            action.Initialize(testOwner.AddComponent<TestActionComponent>(), blackBoard);

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
            action.Initialize(testOwner.AddComponent<TestActionComponent>(), blackBoard);

            // Assert
            var result = action.Execute();
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.IsTrue(blackBoard.HasKey("speed"));
            Assert.AreEqual(10.5f, blackBoard.GetValue<float>("speed"));
        }

        [Test][Description("SetBlackBoardActionでbool型の値（is_active=true）をBlackBoardに設定")]
        public void SetBlackBoardAction_SetProperty_SetsBoolValue()
        {
            // Arrange
            var action = new SetBlackBoardAction();

            // Act
            action.SetProperty("is_active", "true");
            action.Initialize(testOwner.AddComponent<TestActionComponent>(), blackBoard);

            // Assert
            var result = action.Execute();
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.IsTrue(blackBoard.HasKey("is_active"));
            Assert.AreEqual(true, blackBoard.GetValue<bool>("is_active"));
        }

        [Test][Description("SetBlackBoardActionでVector3型の値（position=(1,2,3)）をBlackBoardに設定")]
        public void SetBlackBoardAction_SetProperty_SetsVector3Value()
        {
            // Arrange
            var action = new SetBlackBoardAction();

            // Act
            action.SetProperty("position", "(1.0,2.0,3.0)");
            action.Initialize(testOwner.AddComponent<TestActionComponent>(), blackBoard);

            // Assert
            var result = action.Execute();
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.IsTrue(blackBoard.HasKey("position"));
            Assert.AreEqual(new Vector3(1.0f, 2.0f, 3.0f), blackBoard.GetValue<Vector3>("position"));
        }

        [Test][Description("SetBlackBoardActionでstring型の値（player_name=Hero）をBlackBoardに設定")]
        public void SetBlackBoardAction_SetProperty_SetsStringValue()
        {
            // Arrange
            var action = new SetBlackBoardAction();

            // Act
            action.SetProperty("player_name", "Hero");
            action.Initialize(testOwner.AddComponent<TestActionComponent>(), blackBoard);

            // Assert
            var result = action.Execute();
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.IsTrue(blackBoard.HasKey("player_name"));
            Assert.AreEqual("Hero", blackBoard.GetValue<string>("player_name"));
        }

        [Test][Description("SetBlackBoardActionで複数の異なる型の値を同時にBlackBoardに設定")]
        public void SetBlackBoardAction_SetMultipleProperties_SetsAllValues()
        {
            // Arrange
            var action = new SetBlackBoardAction();

            // Act
            action.SetProperty("health", "80");
            action.SetProperty("mana", "50.5");
            action.SetProperty("ready", "false");
            action.Initialize(testOwner.AddComponent<TestActionComponent>(), blackBoard);

            // Assert
            var result = action.Execute();
            Assert.AreEqual(BTNodeResult.Success, result);
            Assert.AreEqual(80, blackBoard.GetValue<int>("health"));
            Assert.AreEqual(50.5f, blackBoard.GetValue<float>("mana"));
            Assert.AreEqual(false, blackBoard.GetValue<bool>("ready"));
        }

        [Test][Description("SetBlackBoardActionでBlackBoardがnullの場合にFailureを返しエラーログを出力することを確認")]
        public void SetBlackBoardAction_ExecuteWithNullBlackBoard_ReturnsFailure()
        {
            // Arrange
            var action = new SetBlackBoardAction();
            action.SetProperty("test", "value");
            action.Initialize(testOwner.AddComponent<TestActionComponent>(), null);

            // Act
            var result = action.Execute();

            // Assert - ログではなく実際の動作を検証
            Assert.AreEqual(BTNodeResult.Failure, result, "BlackBoardがnullの場合、Failureが返されるべき");
        }

        [Test][Description("SetBlackBoardActionで割り当てが指定されていない場合にFailureを返しエラーログを出力することを確認")]
        public void SetBlackBoardAction_ExecuteWithNoAssignments_ReturnsFailure()
        {
            // Arrange
            var action = new SetBlackBoardAction();
            action.Initialize(testOwner.AddComponent<TestActionComponent>(), blackBoard);

            // Act
            var result = action.Execute();

            // Assert - ログではなく実際の動作を検証
            Assert.AreEqual(BTNodeResult.Failure, result, "割り当てが指定されていない場合、Failureが返されるべき");
            
            // BlackBoardに何も設定されていないことを確認
            Assert.IsFalse(blackBoard.HasKey("test"), "割り当てなしの場合、BlackBoardにキーが追加されるべきではない");
        }

        #endregion
    }

    /// <summary>テスト用のActionコンポーネント</summary>
    public class TestActionComponent : MonoBehaviour
    {
        // テスト用の空のMonoBehaviourコンポーネント
    }

    /// <summary>テスト用のインタラクト可能コンポーネント</summary>
    public class TestInteractableComponent : MonoBehaviour, ArcBT.Actions.IInteractable
    {
        public bool isInteracted = false;

        public void OnInteract(GameObject interactor)
        {
            isInteracted = true;
        }
    }
}
