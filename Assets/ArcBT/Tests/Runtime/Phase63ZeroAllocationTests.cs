using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using System.Diagnostics;
using ArcBT.Logger;
using Microsoft.Extensions.Logging;
using ZLogger;
using LogCategory = ArcBT.Logger.LogCategory;

namespace ArcBT.Tests
{
    /// <summary>Phase 6.3: ゼロアロケーション最適化の検証テスト</summary>
    public class Phase63ZeroAllocationTests : BTTestBase
    {
        ILoggerFactory testLoggerFactory;
        
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            // Unity EditorTest環境用LoggerFactory（外部プロバイダーなし）
            testLoggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                // EditorTestでは外部プロバイダーを使わずシンプルに設定
                builder.AddZLoggerConsole();
            });
            
            BTLogger.Configure(testLoggerFactory);
        }
        
        [TearDown]
        public override void TearDown()
        {
            testLoggerFactory?.Dispose();
            base.TearDown();
        }
        
        /// <summary>基本ログ出力のゼロアロケーション検証</summary>
        [Test][Description("基本ログメソッドがゼロアロケーションで動作することを確認")]
        public void TestBasicLoggingZeroAllocation()
        {
            // GCを強制実行してベースライン確立
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            var initialMemory = GC.GetTotalMemory(false);
            
            // ログ出力（ゼロアロケーションが期待される）
            BTLogger.LogCombat("Test combat message", "TestNode");
            BTLogger.LogMovement("Test movement message");
            BTLogger.LogSystem("Test system message");
            
            var finalMemory = GC.GetTotalMemory(false);
            var allocatedBytes = finalMemory - initialMemory;
            
            // 小さなアロケーション（100bytes以下）は許容
            Assert.Less(allocatedBytes, 100, 
                $"基本ログ出力で予想以上のメモリアロケーションが発生: {allocatedBytes} bytes");
            
            UnityEngine.Debug.Log($"Phase 6.3 基本ログアロケーション: {allocatedBytes} bytes");
        }
        
        /// <summary>構造化ログのゼロアロケーション検証</summary>
        [Test][Description("構造化ログがゼロアロケーションで動作することを確認")]
        public void TestStructuredLoggingZeroAllocation()
        {
            // GCベースライン確立
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            var initialMemory = GC.GetTotalMemory(false);
            
            // 構造化ログ出力
            var testData = new { PlayerId = 123, Score = 456.7f, Level = "TestLevel" };
            BTLogger.LogStructured(Microsoft.Extensions.Logging.LogLevel.Information, 
                LogCategory.System, "Player {PlayerId} scored {Score} in level {Level}", 
                testData, "StructuredTest");
            
            var finalMemory = GC.GetTotalMemory(false);
            var allocatedBytes = finalMemory - initialMemory;
            
            // 構造化ログでも最小限のアロケーション
            Assert.Less(allocatedBytes, 200, 
                $"構造化ログで予想以上のメモリアロケーションが発生: {allocatedBytes} bytes");
            
            UnityEngine.Debug.Log($"Phase 6.3 構造化ログアロケーション: {allocatedBytes} bytes");
        }
        
        /// <summary>フォーマットログの最適化検証</summary>
        [Test][Description("ZLoggerネイティブフォーマットへの最適化効果を確認")]
        public void TestFormatLoggingOptimization()
        {
            // GCベースライン確立
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            var initialMemory = GC.GetTotalMemory(false);
            
            // フォーマットログ出力（ZLoggerネイティブ最適化）
            BTLogger.LogCombatFormat("Attack {0} with damage", "Enemy01", "AttackNode");
            BTLogger.LogMovementFormat("Moving to position {0}", "Vector3(1,2,3)", "MoveNode");
            
            var finalMemory = GC.GetTotalMemory(false);
            var allocatedBytes = finalMemory - initialMemory;
            
            // フォーマットログの最適化効果確認
            Assert.Less(allocatedBytes, 150, 
                $"フォーマットログで予想以上のメモリアロケーションが発生: {allocatedBytes} bytes");
            
            UnityEngine.Debug.Log($"Phase 6.3 フォーマットログアロケーション: {allocatedBytes} bytes");
        }
        
        /// <summary>大量ログ処理のパフォーマンス検証</summary>
        [Test][Description("大量ログ処理でのメモリ効率とパフォーマンスを確認")]
        public void TestHighVolumeLoggingPerformance()
        {
            const int logCount = 1000;
            
            // GCベースライン確立
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            var initialMemory = GC.GetTotalMemory(false);
            var stopwatch = Stopwatch.StartNew();
            
            // 大量ログ出力
            for (int i = 0; i < logCount; i++)
            {
                BTLogger.LogSystem($"Bulk log message {i}", $"Node{i}");
                
                if (i % 100 == 0)
                {
                    BTLogger.LogStructured(Microsoft.Extensions.Logging.LogLevel.Information, 
                        LogCategory.System, "Progress: {iteration}/{total}", 
                        new { iteration = i, total = logCount }, "BulkTest");
                }
            }
            
            stopwatch.Stop();
            var finalMemory = GC.GetTotalMemory(false);
            var allocatedBytes = finalMemory - initialMemory;
            var avgTimePerLog = (float)stopwatch.ElapsedMilliseconds / logCount;
            
            // パフォーマンス基準
            Assert.Less(avgTimePerLog, 0.1f, 
                $"ログ1件あたりの処理時間が遅すぎます: {avgTimePerLog:F3}ms/log");
            
            Assert.Less(allocatedBytes, logCount * 50, 
                $"大量ログ処理でメモリ効率が悪すぎます: {allocatedBytes} bytes for {logCount} logs");
            
            UnityEngine.Debug.Log($"Phase 6.3 大量ログパフォーマンス: {stopwatch.ElapsedMilliseconds}ms for {logCount} logs");
            UnityEngine.Debug.Log($"Phase 6.3 平均処理時間: {avgTimePerLog:F3}ms/log");
            UnityEngine.Debug.Log($"Phase 6.3 メモリ効率: {allocatedBytes} bytes ({(float)allocatedBytes/logCount:F1} bytes/log)");
        }
        
        /// <summary>後方互換性APIの動作確認</summary>
        [Test][Description("後方互換性APIが正常に動作することを確認")]
        public void TestBackwardCompatibilityAPIs()
        {
            // 後方互換性メソッドの実行（例外が発生しないことを確認）
            Assert.DoesNotThrow(() =>
            {
                BTLogger.SetLogLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
                BTLogger.SetCategoryFilter(LogCategory.Combat, true);
                BTLogger.ResetToDefaults();
                BTLogger.ClearHistory();
            }, "後方互換性APIで例外が発生しました");
            
            // 戻り値の確認
            var logLevel = BTLogger.GetCurrentLogLevel();
            Assert.AreEqual(Microsoft.Extensions.Logging.LogLevel.Information, logLevel, 
                "GetCurrentLogLevel()が期待される固定値を返さない");
            
            var isEnabled = BTLogger.IsCategoryEnabled(LogCategory.Combat);
            Assert.IsTrue(isEnabled, "IsCategoryEnabled()が常にtrueを返さない");
            
            var recentLogs = BTLogger.GetRecentLogs(10);
            Assert.AreEqual(0, recentLogs.Length, "GetRecentLogs()が空配列を返さない");
            
            var categoryLogs = BTLogger.GetLogsByCategory(LogCategory.System, 5);
            Assert.AreEqual(0, categoryLogs.Length, "GetLogsByCategory()が空配列を返さない");
        }
        
        /// <summary>LoggerFactory設定状況の確認</summary>
        [Test][Description("LoggerFactory設定状況が正しく検出されることを確認")]
        public void TestLoggerFactoryConfigurationDetection()
        {
            // 設定済み状態の確認（NullLoggerFactoryでも設定済みとして扱われる）
            Assert.IsTrue(BTLogger.IsConfigured, "LoggerFactory設定済み状態が検出されない");
            Assert.IsNotNull(BTLogger.Instance, "Loggerインスタンスが取得できない");
            
            // 実際にログが出力されることを確認（例外なし）
            Assert.DoesNotThrow(() =>
            {
                BTLogger.LogSystem("Configuration test message");
            }, "設定済みLoggerでログ出力に失敗");
        }
    }
}
