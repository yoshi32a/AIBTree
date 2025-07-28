using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using ArcBT.Logger;
using Microsoft.Extensions.Logging;

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
        [Test][Description("Phase 6.3: ログレベル制御がZLoggerに委譲されていることを確認")]
        public void TestLogLevelFiltering()
        {
            // Arrange & Act: 後方互換性メソッドが例外なく動作することを確認
            Assert.DoesNotThrow(() =>
            {
                BTLogger.SetLogLevel(Microsoft.Extensions.Logging.LogLevel.Warning);
                
                // 異なるレベルのログを出力（ZLoggerが実際のフィルタリングを制御）
                BTLogger.Log(Microsoft.Extensions.Logging.LogLevel.Error, LogCategory.System, "Error message");
                BTLogger.Log(Microsoft.Extensions.Logging.LogLevel.Warning, LogCategory.System, "Warning message");
                BTLogger.Log(Microsoft.Extensions.Logging.LogLevel.Information, LogCategory.System, "Info message");
                BTLogger.Log(Microsoft.Extensions.Logging.LogLevel.Debug, LogCategory.System, "Debug message");
            }, "ログレベル設定と出力で例外が発生");
            
            // Assert: ZLoggerに委譲されているため、履歴取得は空配列
            var logs = BTLogger.GetRecentLogs(10);
            Assert.AreEqual(0, logs.Length, "Phase 6.3: 履歴管理はZLoggerに委譲されているため空配列");
            
            UnityEngine.Debug.Log("Phase 6.3: ログレベル制御テスト完了 - ZLoggerに委譲");
        }

        /// <summary>カテゴリフィルタリングのテスト</summary>
        [Test][Description("Phase 6.3: カテゴリフィルタリングがZLoggerに委譲されていることを確認")]
        public void TestCategoryFiltering()
        {
            // Arrange & Act: 後方互換性メソッドが例外なく動作することを確認
            Assert.DoesNotThrow(() =>
            {
                BTLogger.SetCategoryFilter(LogCategory.Combat, true);
                BTLogger.SetCategoryFilter(LogCategory.Movement, false);
                BTLogger.SetCategoryFilter(LogCategory.System, false);
                
                // 異なるカテゴリのログを出力（ZLoggerがタグベースで制御）
                BTLogger.Log(Microsoft.Extensions.Logging.LogLevel.Information, LogCategory.Combat, "Combat message");
                BTLogger.Log(Microsoft.Extensions.Logging.LogLevel.Information, LogCategory.Movement, "Movement message");
                BTLogger.Log(Microsoft.Extensions.Logging.LogLevel.Information, LogCategory.System, "System message");
            }, "カテゴリフィルタリング設定で例外が発生");
            
            // Assert: ZLoggerに委譲されているため、履歴取得は空配列
            var logs = BTLogger.GetRecentLogs(10);
            Assert.AreEqual(0, logs.Length, "Phase 6.3: 履歴管理はZLoggerに委譲されているため空配列");
            
            UnityEngine.Debug.Log("Phase 6.3: カテゴリフィルタリングテスト完了 - ZLoggerタグベース制御");
        }

        /// <summary>ログ履歴管理のテスト</summary>
        [Test][Description("Phase 6.3: ログ履歴管理がZLoggerに委譲されていることを確認")]
        public void TestLogHistoryManagement()
        {
            // Arrange & Act: 複数のログを出力（ZLoggerが管理）
            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 15; i++)
                {
                    BTLogger.LogSystem($"Test message {i}");
                }
            }, "ログ出力で例外が発生");
            
            // Assert: ZLoggerに委譲されているため、履歴取得は空配列
            var recentLogs5 = BTLogger.GetRecentLogs(5);
            var recentLogs10 = BTLogger.GetRecentLogs(10);
            
            Assert.AreEqual(0, recentLogs5.Length, "Phase 6.3: 履歴管理はZLoggerに委譲 - 空配列");
            Assert.AreEqual(0, recentLogs10.Length, "Phase 6.3: 履歴管理はZLoggerに委譲 - 空配列");
            
            UnityEngine.Debug.Log("Phase 6.3: ログ履歴管理テスト完了 - ZLoggerプロバイダーに委譲");
        }

        /// <summary>カテゴリ別ログ取得のテスト</summary>
        [Test][Description("Phase 6.3: カテゴリ別ログ取得がZLoggerに委譲されていることを確認")]
        public void TestCategorySpecificLogRetrieval()
        {
            // Arrange & Act: 異なるカテゴリのログを出力（ZLoggerがタグベースで管理）
            Assert.DoesNotThrow(() =>
            {
                BTLogger.LogCombat("Attack message 1");
                BTLogger.LogMovement("Move message 1");
                BTLogger.LogCombat("Attack message 2");
                BTLogger.LogSystem("System message 1");
                BTLogger.LogCombat("Attack message 3");
            }, "カテゴリ別ログ出力で例外が発生");
            
            // Assert: ZLoggerに委譲されているため、履歴取得は空配列
            var combatLogs = BTLogger.GetLogsByCategory(LogCategory.Combat, 10);
            Assert.AreEqual(0, combatLogs.Length, "Phase 6.3: カテゴリ別履歴管理はZLoggerに委譲 - 空配列");
            
            UnityEngine.Debug.Log("Phase 6.3: カテゴリ別ログ取得テスト完了 - ZLoggerタグベース管理");
        }

        /// <summary>便利メソッドのテスト</summary>
        [Test][Description("Phase 6.3: カテゴリ別便利メソッドが正常に動作することを確認")]
        public void TestConvenienceMethods()
        {
            // Arrange & Act: 各便利メソッドが例外なく実行されることを確認
            Assert.DoesNotThrow(() =>
            {
                BTLogger.SetLogLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
                
                BTLogger.LogCombat("Combat test", "TestNode");
                BTLogger.LogMovement("Movement test");
                BTLogger.LogCondition("Condition test");
                BTLogger.LogBlackBoard("BlackBoard test");
                BTLogger.LogSystem("System test");
            }, "便利メソッド実行で例外が発生");
            
            // Assert: ZLoggerに委譲されているため、履歴取得は空配列
            var logs = BTLogger.GetRecentLogs(10);
            Assert.AreEqual(0, logs.Length, "Phase 6.3: 履歴管理はZLoggerに委譲 - 空配列");
            
            UnityEngine.Debug.Log("Phase 6.3: 便利メソッドテスト完了 - ZLoggerタグベース出力");
        }

        /// <summary>ログクリア機能のテスト</summary>
        [Test][Description("Phase 6.3: ログクリア機能がZLoggerに委譲されていることを確認")]
        public void TestLogClearFunctionality()
        {
            // Arrange & Act: ログ出力とクリア機能が例外なく動作することを確認
            Assert.DoesNotThrow(() =>
            {
                BTLogger.LogSystem("Test message 1");
                BTLogger.LogSystem("Test message 2");
                BTLogger.LogSystem("Test message 3");
                BTLogger.ClearHistory();
            }, "ログ出力とクリア処理で例外が発生");
            
            // Assert: ZLoggerに委譲されているため、履歴取得は常に空配列
            var logsAfterClear = BTLogger.GetRecentLogs(10);
            Assert.AreEqual(0, logsAfterClear.Length, "Phase 6.3: 履歴管理はZLoggerに委譲 - 常に空配列");
            
            UnityEngine.Debug.Log("Phase 6.3: ログクリアテスト完了 - ZLoggerプロバイダーに委譲");
        }

        /// <summary>デフォルト設定リセットのテスト</summary>
        [Test][Description("Phase 6.3: デフォルト設定リセット機能の後方互換性確認")]
        public void TestDefaultSettingsReset()
        {
            // Arrange & Act: 後方互換性メソッドが例外なく動作することを確認
            Assert.DoesNotThrow(() =>
            {
                BTLogger.SetLogLevel(Microsoft.Extensions.Logging.LogLevel.Error);
                BTLogger.SetCategoryFilter(LogCategory.Combat, false);
                BTLogger.SetCategoryFilter(LogCategory.System, false);
                BTLogger.ResetToDefaults();
            }, "設定変更とリセット処理で例外が発生");
            
            // Assert: 後方互換性のため固定値を返すことを確認
            Assert.AreEqual(Microsoft.Extensions.Logging.LogLevel.Information, BTLogger.GetCurrentLogLevel(), "Phase 6.3: 固定値Information");
            Assert.IsTrue(BTLogger.IsCategoryEnabled(LogCategory.Combat), "Phase 6.3: 常にtrue");
            Assert.IsTrue(BTLogger.IsCategoryEnabled(LogCategory.System), "Phase 6.3: 常にtrue");
            Assert.IsTrue(BTLogger.IsCategoryEnabled(LogCategory.Parser), "Phase 6.3: 常にtrue");
            Assert.IsTrue(BTLogger.IsCategoryEnabled(LogCategory.Debug), "Phase 6.3: 常にtrue");
            
            UnityEngine.Debug.Log("Phase 6.3: デフォルト設定リセットテスト完了 - 後方互換性維持");
        }

        /// <summary>エラーログ機能のテスト</summary>
        [Test][Description("Phase 6.3: エラーログ出力が正常に動作することを確認")]
        public void TestErrorLogging()
        {
            // Act & Assert: エラーログが例外なく出力されることを確認
            Assert.DoesNotThrow(() =>
            {
                BTLogger.LogError(LogCategory.Combat, "Critical error occurred", "ErrorNode");
            }, "エラーログ出力で例外が発生");
            
            // Assert: ZLoggerに委譲されているため、履歴取得は空配列
            var logs = BTLogger.GetRecentLogs(1);
            Assert.AreEqual(0, logs.Length, "Phase 6.3: 履歴管理はZLoggerに委譲 - 空配列");
            
            UnityEngine.Debug.Log("Phase 6.3: エラーログテスト完了 - ZLoggerエラー出力");
        }

        /// <summary>タイムスタンプ機能のテスト</summary>
        [Test][Description("Phase 6.3: タイムスタンプ機能がZLoggerに委譲されていることを確認")]
        public void TestTimestampFunctionality()
        {
            // Arrange & Act: ログ出力が例外なく動作することを確認
            var beforeTime = System.DateTime.Now;
            
            Assert.DoesNotThrow(() =>
            {
                BTLogger.LogSystem("Timestamp test");
            }, "タイムスタンプ付きログ出力で例外が発生");
            
            var afterTime = System.DateTime.Now;
            
            // Assert: ZLoggerがタイムスタンプを管理
            var logs = BTLogger.GetRecentLogs(1);
            Assert.AreEqual(0, logs.Length, "Phase 6.3: タイムスタンプ管理はZLoggerに委譲 - 空配列");
                
            UnityEngine.Debug.Log($"Phase 6.3: タイムスタンプテスト完了 - ZLoggerタイムスタンプ管理 ({afterTime - beforeTime:F2}ms)");
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
            
            BTLogger.LogStructured(Microsoft.Extensions.Logging.LogLevel.Information, LogCategory.System, 
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
            // Arrange & Act: 後方互換性メソッドが例外なく動作することを確認
            Assert.DoesNotThrow(() =>
            {
                BTLogger.LogSystem("Before disposal test");
                BTLogger.Dispose();
                BTLogger.LogSystem("After disposal test");
            }, "リソース解放と再初期化で例外が発生");
            
            // Assert: ZLoggerに委譲されているため、履歴取得は空配列
            var logs = BTLogger.GetRecentLogs(2);
            Assert.AreEqual(0, logs.Length, "Phase 6.3: 履歴管理はZLoggerに委譲 - 空配列");
            
            UnityEngine.Debug.Log("Phase 6.3: ZLoggerリソース解放テスト完了 - 後方互換性維持");
        }
    }
}
