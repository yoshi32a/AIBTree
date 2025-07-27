using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ArcBT.Core;
using System.Linq;
using ArcBT.Logger;

namespace ArcBT.Tests
{
    /// <summary>BTLoggerクラスの機能をテストするクラス</summary>
    public class BTLoggerTests
    {
        [SetUp]
        public void SetUp()
        {
            // 各テスト前にBTLoggerをデフォルト状態にリセット
            BTLogger.ResetToDefaults();
            BTLogger.ClearHistory();
            
            // テスト用にすべてのカテゴリを有効化
            foreach (LogCategory category in System.Enum.GetValues(typeof(LogCategory)))
            {
                BTLogger.SetCategoryFilter(category, true);
            }
        }

        [TearDown]
        public void TearDown()
        {
            // テスト後のクリーンアップ
            BTLogger.ClearHistory();
        }

        /// <summary>ログレベル設定とフィルタリングのテスト</summary>
        [Test]
        public void TestLogLevelFiltering()
        {
            // Arrange: Unity Test環境でのログ期待設定
            LogAssert.Expect(LogType.Error, "[ERR][SYS]: Error message");
            LogAssert.Expect(LogType.Warning, "[WRN][SYS]: Warning message");
            
            // Arrange: ログレベルをWarningに設定
            BTLogger.SetLogLevel(LogLevel.Warning);
            
            // Act: 異なるレベルのログを出力
            BTLogger.Log(LogLevel.Error, LogCategory.System, "Error message");
            BTLogger.Log(LogLevel.Warning, LogCategory.System, "Warning message");
            BTLogger.Log(LogLevel.Info, LogCategory.System, "Info message");
            BTLogger.Log(LogLevel.Debug, LogCategory.System, "Debug message");
            
            // Assert: Warning以上のログのみが記録されているか確認
            var logs = BTLogger.GetRecentLogs(10);
            Assert.AreEqual(2, logs.Length, "Warning以上のログが2件記録されているべき");
            Assert.AreEqual(LogLevel.Error, logs[0].Level, "最初に記録されたログはErrorレベル");
            Assert.AreEqual(LogLevel.Warning, logs[1].Level, "2番目に記録されたログはWarningレベル");
        }

        /// <summary>カテゴリフィルタリングのテスト</summary>
        [Test]
        public void TestCategoryFiltering()
        {
            // Arrange: Combatカテゴリのみ有効にし、他を無効化
            BTLogger.SetCategoryFilter(LogCategory.Combat, true);
            BTLogger.SetCategoryFilter(LogCategory.Movement, false);
            BTLogger.SetCategoryFilter(LogCategory.System, false);
            
            // Act: 異なるカテゴリのログを出力
            BTLogger.Log(LogLevel.Info, LogCategory.Combat, "Combat message");
            BTLogger.Log(LogLevel.Info, LogCategory.Movement, "Movement message");
            BTLogger.Log(LogLevel.Info, LogCategory.System, "System message");
            
            // Assert: Combatカテゴリのログのみが記録されているか確認
            var logs = BTLogger.GetRecentLogs(10);
            Assert.AreEqual(1, logs.Length, "Combatカテゴリのログのみが記録されているべき");
            Assert.AreEqual(LogCategory.Combat, logs[0].Category, "記録されたログはCombatカテゴリ");
            Assert.AreEqual("Combat message", logs[0].Message, "メッセージが正しく記録されている");
        }

        /// <summary>ログ履歴管理のテスト</summary>
        [Test]
        public void TestLogHistoryManagement()
        {
            // Arrange & Act: 複数のログを追加
            for (int i = 0; i < 15; i++)
            {
                BTLogger.LogSystem($"Test message {i}");
            }
            
            // Assert: 指定した件数のログが取得できるか確認
            var recentLogs5 = BTLogger.GetRecentLogs(5);
            var recentLogs10 = BTLogger.GetRecentLogs(10);
            
            Assert.AreEqual(5, recentLogs5.Length, "最新5件が取得できる");
            Assert.AreEqual(10, recentLogs10.Length, "最新10件が取得できる");
            
            // 最新のログが配列の最後に来ているか確認（TakeLastの結果）
            Assert.IsTrue(recentLogs5[4].Message.Contains("14"), "最新のログが配列の最後に取得される");
            Assert.IsTrue(recentLogs5[0].Message.Contains("10"), "5件前のログが配列の最初に取得される");
        }

        /// <summary>カテゴリ別ログ取得のテスト</summary>
        [Test]
        public void TestCategorySpecificLogRetrieval()
        {
            // Arrange & Act: 異なるカテゴリのログを混在させる
            BTLogger.LogCombat("Attack message 1");
            BTLogger.LogMovement("Move message 1");
            BTLogger.LogCombat("Attack message 2");
            BTLogger.LogSystem("System message 1");
            BTLogger.LogCombat("Attack message 3");
            
            // Assert: Combatカテゴリのログのみが取得できるか確認
            var combatLogs = BTLogger.GetLogsByCategory(LogCategory.Combat, 10);
            Assert.AreEqual(3, combatLogs.Length, "Combatカテゴリのログが3件取得できる");
            
            foreach (var log in combatLogs)
            {
                Assert.AreEqual(LogCategory.Combat, log.Category, "すべてCombatカテゴリ");
                Assert.IsTrue(log.Message.Contains("Attack"), "メッセージにAttackが含まれる");
            }
        }

        /// <summary>便利メソッドのテスト</summary>
        [Test]
        public void TestConvenienceMethods()
        {
            // Arrange: Debugレベルまでのログをすべて記録するよう設定
            BTLogger.SetLogLevel(LogLevel.Debug);
            
            // Act: 各便利メソッドを実行
            BTLogger.LogCombat("Combat test", "TestNode");
            BTLogger.LogMovement("Movement test");
            BTLogger.LogCondition("Condition test");
            BTLogger.LogBlackBoard("BlackBoard test");
            BTLogger.LogSystem("System test");
            
            // Assert: 適切なカテゴリでログが記録されているか確認
            var logs = BTLogger.GetRecentLogs(10);
            Assert.AreEqual(5, logs.Length, "5件のログが記録されている");
            
            var combatLog = logs.FirstOrDefault(l => l.Category == LogCategory.Combat);
            var movementLog = logs.FirstOrDefault(l => l.Category == LogCategory.Movement);
            var conditionLog = logs.FirstOrDefault(l => l.Category == LogCategory.Condition);
            var blackBoardLog = logs.FirstOrDefault(l => l.Category == LogCategory.BlackBoard);
            var systemLog = logs.FirstOrDefault(l => l.Category == LogCategory.System);
            
            Assert.IsNotNull(combatLog, "Combatログが記録されている");
            Assert.IsNotNull(movementLog, "Movementログが記録されている");
            Assert.IsNotNull(conditionLog, "Conditionログが記録されている");
            Assert.IsNotNull(blackBoardLog, "BlackBoardログが記録されている");
            Assert.IsNotNull(systemLog, "Systemログが記録されている");
            
            Assert.AreEqual("TestNode", combatLog.NodeName, "ノード名が正しく記録されている");
        }

        /// <summary>ログクリア機能のテスト</summary>
        [Test]
        public void TestLogClearFunctionality()
        {
            // Arrange: いくつかのログを追加
            BTLogger.LogSystem("Test message 1");
            BTLogger.LogSystem("Test message 2");
            BTLogger.LogSystem("Test message 3");
            
            // 追加されたことを確認
            var logsBeforeClear = BTLogger.GetRecentLogs(10);
            Assert.AreEqual(3, logsBeforeClear.Length, "3件のログが追加されている");
            
            // Act: ログをクリア
            BTLogger.ClearHistory();
            
            // Assert: ログが空になっているか確認
            var logsAfterClear = BTLogger.GetRecentLogs(10);
            Assert.AreEqual(0, logsAfterClear.Length, "ログがクリアされている");
        }

        /// <summary>デフォルト設定リセットのテスト</summary>
        [Test]
        public void TestDefaultSettingsReset()
        {
            // Arrange: 設定を変更
            BTLogger.SetLogLevel(LogLevel.Error);
            BTLogger.SetCategoryFilter(LogCategory.Combat, false);
            BTLogger.SetCategoryFilter(LogCategory.System, false);
            
            // 変更されたことを確認
            Assert.AreEqual(LogLevel.Error, BTLogger.GetCurrentLogLevel(), "ログレベルが変更されている");
            Assert.IsFalse(BTLogger.IsCategoryEnabled(LogCategory.Combat), "Combatカテゴリが無効化されている");
            
            // Act: デフォルト設定にリセット
            BTLogger.ResetToDefaults();
            
            // Assert: デフォルト値に戻っているか確認
            Assert.AreEqual(LogLevel.Info, BTLogger.GetCurrentLogLevel(), "ログレベルがInfoに戻っている");
            Assert.IsTrue(BTLogger.IsCategoryEnabled(LogCategory.Combat), "Combatカテゴリが有効に戻っている");
            Assert.IsTrue(BTLogger.IsCategoryEnabled(LogCategory.System), "Systemカテゴリが有効に戻っている");
            Assert.IsFalse(BTLogger.IsCategoryEnabled(LogCategory.Parser), "Parserカテゴリはデフォルトで無効");
            Assert.IsFalse(BTLogger.IsCategoryEnabled(LogCategory.Debug), "Debugカテゴリはデフォルトで無効");
        }

        /// <summary>エラーログ機能のテスト</summary>
        [Test]
        public void TestErrorLogging()
        {
            // Arrange: Unity Test環境でのエラーログ期待設定
            LogAssert.Expect(LogType.Error, "[ERR][ATK][ErrorNode]: Critical error occurred");
            
            // Act: エラーログを出力
            BTLogger.LogError(LogCategory.Combat, "Critical error occurred", "ErrorNode");
            
            // Assert: エラーレベルで記録されているか確認
            var logs = BTLogger.GetRecentLogs(1);
            Assert.AreEqual(1, logs.Length, "エラーログが記録されている");
            Assert.AreEqual(LogLevel.Error, logs[0].Level, "エラーレベルで記録されている");
            Assert.AreEqual(LogCategory.Combat, logs[0].Category, "指定したカテゴリで記録されている");
            Assert.AreEqual("Critical error occurred", logs[0].Message, "メッセージが正しく記録されている");
            Assert.AreEqual("ErrorNode", logs[0].NodeName, "ノード名が正しく記録されている");
        }

        /// <summary>タイムスタンプ機能のテスト</summary>
        [Test]
        public void TestTimestampFunctionality()
        {
            // Arrange: 現在時刻を記録
            var beforeTime = System.DateTime.Now;
            
            // Act: ログを出力
            BTLogger.LogSystem("Timestamp test");
            
            // Arrange: 現在時刻を記録
            var afterTime = System.DateTime.Now;
            
            // Assert: タイムスタンプが適切な範囲内か確認
            var logs = BTLogger.GetRecentLogs(1);
            Assert.AreEqual(1, logs.Length, "ログが記録されている");
            
            var logTime = logs[0].Timestamp;
            Assert.IsTrue(logTime >= beforeTime && logTime <= afterTime, 
                "タイムスタンプが適切な時間範囲内に記録されている");
        }
    }
}
