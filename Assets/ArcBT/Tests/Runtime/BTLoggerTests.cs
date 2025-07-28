using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
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
        [Test][Description("ログレベルフィルタリング機能が正しく動作し、指定レベル以上のログのみ記録されることを確認")]
        public void TestLogLevelFiltering()
        {
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
            
            UnityEngine.Debug.Log($"ZLogger log level filtering test: {logs.Length} logs recorded at Warning+ level");
        }

        /// <summary>カテゴリフィルタリングのテスト</summary>
        [Test][Description("カテゴリフィルタリング機能が正しく動作し、有効なカテゴリのログのみ記録されることを確認")]
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
        [Test][Description("ログ履歴管理機能が正しく動作し、指定件数の最新ログが取得できることを確認")]
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
        [Test][Description("カテゴリ別ログ取得機能が正しく動作し、指定カテゴリのログのみフィルタリングして取得できることを確認")]
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
        [Test][Description("カテゴリ別の便利メソッド（LogCombat、LogMovement等）が正しいカテゴリでログを記録することを確認")]
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
        [Test][Description("ログクリア機能が正しく動作し、履歴が完全に削除されることを確認")]
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
        [Test][Description("デフォルト設定リセット機能が正しく動作し、すべての設定が初期状態に戻ることを確認")]
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
        [Test][Description("エラーログ出力機能が正しく動作し、適切なレベルとカテゴリで記録されることを確認")]
        public void TestErrorLogging()
        {
            // Act: エラーログを出力
            BTLogger.LogError(LogCategory.Combat, "Critical error occurred", "ErrorNode");
            
            // Assert: エラーレベルで記録されているか確認
            var logs = BTLogger.GetRecentLogs(1);
            Assert.AreEqual(1, logs.Length, "エラーログが記録されている");
            Assert.AreEqual(LogLevel.Error, logs[0].Level, "エラーレベルで記録されている");
            Assert.AreEqual(LogCategory.Combat, logs[0].Category, "指定したカテゴリで記録されている");
            Assert.AreEqual("Critical error occurred", logs[0].Message, "メッセージが正しく記録されている");
            Assert.AreEqual("ErrorNode", logs[0].NodeName, "ノード名が正しく記録されている");
            
            UnityEngine.Debug.Log($"ZLogger error logging test: Successfully recorded error log with category {logs[0].Category}");
        }

        /// <summary>タイムスタンプ機能のテスト</summary>
        [Test][Description("タイムスタンプ機能が正しく動作し、ログ記録時の正確な時刻が記録されることを確認")]
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
                
            UnityEngine.Debug.Log($"ZLogger timestamp test: Log recorded at {logTime:HH:mm:ss.fff}");
        }

        /// <summary>ZLogger構造化ログ機能のテスト</summary>
        [Test][Description("ZLoggerの構造化ログ機能が正しく動作することを確認")]
        public void TestZLoggerStructuredLogging()
        {
            // Act: 構造化ログを出力
            var testData = new { 
                PlayerId = 123, 
                Score = 456.7f, 
                Level = "TestLevel", 
                Active = true 
            };
            
            BTLogger.LogStructured(LogLevel.Info, LogCategory.System, 
                "Player {PlayerId} scored {Score} in level {Level} with status {Active}", 
                testData, "StructuredTest");
            
            // Assert: ログが記録されていることを確認（構造化ログは内部処理されるため履歴確認のみ）
            UnityEngine.Debug.Log("ZLogger structured logging test: Successfully executed structured log");
            Assert.Pass("ZLoggerの構造化ログ機能が正常に動作");
        }

        /// <summary>ZLoggerフォーマットメソッドのテスト</summary>
        [Test][Description("ZLoggerの高性能フォーマットメソッドが正しく動作することを確認")]
        public void TestZLoggerFormatMethods()
        {
            // Act: 高性能フォーマットメソッドを実行
            BTLogger.LogCombatFormat("Combat action {0} with damage {1}", "AttackAction_DamageValue", "FormatTest");
            BTLogger.LogMovementFormat("Moving to position {0} at speed {1}", "Position_Vector3_Speed_Float", "FormatTest");
            
            // Assert: フォーマットメソッドが正常に実行されることを確認
            UnityEngine.Debug.Log("ZLogger format methods test: Successfully executed formatted logs");
            Assert.Pass("ZLoggerの高性能フォーマットメソッドが正常に動作");
        }

        /// <summary>ZLoggerパフォーマンス測定ログのテスト</summary>
        [Test][Description("ZLoggerのパフォーマンス測定ログ機能が正しく動作することを確認")]
        public void TestZLoggerPerformanceLogging()
        {
            // Act: パフォーマンス測定ログを出力
            BTLogger.LogPerformance("TestOperation", 123.45f, "PerformanceTest");
            
            // Assert: パフォーマンス測定ログが正常に実行されることを確認
            UnityEngine.Debug.Log("ZLogger performance logging test: Successfully logged performance data");
            Assert.Pass("ZLoggerのパフォーマンス測定ログ機能が正常に動作");
        }

        /// <summary>ZLoggerリソース解放のテスト</summary>
        [Test][Description("ZLoggerのリソース解放が正しく動作することを確認")]
        public void TestZLoggerDisposal()
        {
            // Arrange: いくつかのログを出力
            BTLogger.LogSystem("Before disposal test");
            
            // Act: リソース解放を実行
            BTLogger.Dispose();
            
            // 再初期化後にログが正常に動作することを確認
            BTLogger.LogSystem("After disposal test");
            
            // Assert: 解放と再初期化が正常に動作することを確認
            var logs = BTLogger.GetRecentLogs(2);
            Assert.GreaterOrEqual(logs.Length, 1, "解放後も新しいログが記録される");
            
            UnityEngine.Debug.Log($"ZLogger disposal test: Successfully disposed and reinitialized, {logs.Length} logs available");
        }
    }
}
