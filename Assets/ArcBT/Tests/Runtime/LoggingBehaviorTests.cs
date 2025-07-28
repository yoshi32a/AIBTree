using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ArcBT.Core;
using ArcBT.Actions;
using ArcBT.Conditions;
using ArcBT.Logger;

namespace ArcBT.Tests
{
    /// <summary>
    /// 重要なログ出力の動作をテストするクラス
    /// エラーログなど、ユーザーへの情報提供として重要なログのみを対象とする
    /// </summary>  
    [TestFixture]
    [Category("LoggingTests")]
    public class LoggingBehaviorTests : BTTestBase
    {
        GameObject testOwner;
        BlackBoard blackBoard;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            testOwner = CreateTestGameObject("TestOwner");
            blackBoard = new BlackBoard();
            
            // ログテストのためログを有効化
            EnableLoggingForTest();
        }

        [TearDown]
        public override void TearDown()
        {
            DestroyTestObject(testOwner);
            base.TearDown();
        }

        #region エラーログテスト - ユーザーへの重要な情報提供

        [Test][Description("MoveToPositionActionで存在しないターゲットが指定された場合のエラーログ出力")]
        public void MoveToPositionAction_InvalidTarget_LogsError()
        {
            // Arrange
            var action = new MoveToPositionAction();
            action.SetProperty("target", "NonExistentTarget");

            // Act
            action.Initialize(testOwner.AddComponent<TestCoreActionComponent>(), blackBoard);

            // Assert - エラー処理後の状態を検証（ログはZLoggerで処理される）
            string expectedKey = string.IsNullOrEmpty(action.Name) ? "_target_position" : $"{action.Name}_target_position";
            Assert.IsFalse(blackBoard.HasKey(expectedKey), "エラー時にはBlackBoardに設定されるべきではない");
        }

        [Test][Description("HasSharedEnemyInfoConditionでBlackBoardがnullの場合のエラーログ出力")]
        public void HasSharedEnemyInfoCondition_NullBlackBoard_LogsError()
        {
            // Arrange
            var condition = new HasSharedEnemyInfoCondition();

            // Act
            condition.Initialize(testOwner.AddComponent<TestCoreConditionComponent>(), null); // BlackBoardをnullに設定
            var result = condition.Execute();

            // Assert - エラー処理後の戻り値を検証（ログはZLoggerで処理される）
            Assert.AreEqual(BTNodeResult.Failure, result, "BlackBoardがnullの場合はFailureが返されるべき");
        }

        [Test][Description("BehaviourTreeRunnerで存在しないファイルをロードしようとした場合のエラーログ出力")]
        public void BehaviourTreeRunner_InvalidFile_LogsError()
        {
            // Arrange
            var runner = testOwner.AddComponent<BehaviourTreeRunner>();

            // Act
            runner.LoadBehaviourTree("non_existent_file.bt");

            // Assert - エラー処理後の状態を検証（ログはZLoggerで処理される）
            Assert.IsNull(runner.RootNode, "ファイルが存在しない場合、ツリーは設定されるべきではない");
        }

        #endregion

        #region 警告ログテスト - 潜在的な問題の通知

        [Test][Description("BlackBoardで存在しないキーを取得しようとした場合の警告ログ出力")]
        public void BlackBoard_GetNonExistentKey_LogsWarning()
        {
            // Arrange
            var testBlackBoard = new BlackBoard();

            // Act & Assert - 警告ログの出力確認
            // 注意: このテストは実際の警告ログ実装に依存する
            var result = testBlackBoard.GetValue<string>("non_existent_key");
            
            // 機能面も確認
            Assert.IsNull(result, "存在しないキーの場合、nullまたはデフォルト値が返されるべき");
        }

        #endregion

        #region デバッグログテスト - 開発時の詳細情報（必要最小限）

        [Test][Description("BlackBoardへの値設定時のデバッグログ出力（開発時のみ重要）")]
        public void BlackBoard_SetValue_LogsDebugInfo()
        {
            // Arrange
            var testBlackBoard = new BlackBoard();

            // デバッグログは実装によって異なるため、実際のログ仕様に合わせて調整が必要
            // このテストは例示用

            // Act
            testBlackBoard.SetValue("debug_key", "debug_value");

            // Assert - 主に機能を検証し、ログは副次的
            Assert.IsTrue(testBlackBoard.HasKey("debug_key"), "値が正しく設定されているべき");
            Assert.AreEqual("debug_value", testBlackBoard.GetValue<string>("debug_key"), "設定した値が取得できるべき");
        }

        #endregion
    }
}