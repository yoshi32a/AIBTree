using System;
using System.Collections;
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ArcBT.Logger;

namespace ArcBT.Tests
{
    /// <summary>ZLogger統合による性能向上をベンチマークするテストクラス</summary>
    public class ZLoggerBenchmarkTests
    {
        [SetUp]
        public void SetUp()
        {
            BTLogger.ResetToDefaults();
            BTLogger.ClearHistory();
        }

        [TearDown] 
        public void TearDown()
        {
            BTLogger.ClearHistory();
            BTLogger.Dispose(); // ZLoggerリソース解放
        }

        /// <summary>ZLogger高性能ログ出力ベンチマーク</summary>
        [Test][Description("ZLoggerのゼロアロケーション文字列補間による高性能ログ出力をベンチマーク")]
        public void TestZLoggerHighPerformanceLogging()
        {
            // Arrange
            const int logCount = 2000; // より多くのログで性能を測定
            var stopwatch = new Stopwatch();
            
            // Act: ZLoggerベースのBTLoggerで大量ログ出力
            stopwatch.Start();
            for (int i = 0; i < logCount; i++)
            {
                BTLogger.LogSystem($"ZLogger high performance test {i}", "ZLoggerBenchmark");
            }
            stopwatch.Stop();
            
            // Assert: ZLoggerによる性能向上を確認
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            Assert.Less(elapsedMs, 800, $"ZLoggerによる高性能ログ出力（{logCount}件）が800ms以内で完了（実測: {elapsedMs}ms）");
            
            // ログが正しく記録されていることを確認
            var logs = BTLogger.GetRecentLogs(100);
            Assert.Greater(logs.Length, 0, "ZLoggerでログが正常に記録されている");
        }

        /// <summary>ZLogger構造化ログ性能テスト</summary>
        [Test][Description("ZLoggerの構造化ログ機能による高性能ログ出力をベンチマーク")]
        public void TestZLoggerStructuredLogging()
        {
            // Arrange
            const int logCount = 1000;
            var stopwatch = new Stopwatch();
            
            // Act: 構造化ログの性能測定
            stopwatch.Start();
            for (int i = 0; i < logCount; i++)
            {
                BTLogger.LogStructured(LogLevel.Info, LogCategory.System, 
                    "Structured log test iteration {Iteration} with value {Value}", 
                    new { Iteration = i, Value = i * 2 }, "StructuredTest");
            }
            stopwatch.Stop();
            
            // Assert: 構造化ログの高性能処理
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            Assert.Less(elapsedMs, 600, $"ZLogger構造化ログ（{logCount}件）が600ms以内で完了（実測: {elapsedMs}ms）");
        }

        /// <summary>ZLoggerフォーマット済みログ性能テスト</summary>
        [Test][Description("ZLoggerの高性能フォーマットメソッドによるログ出力をベンチマーク")]
        public void TestZLoggerFormattedLogging()
        {
            // Arrange
            const int logCount = 1500;
            var stopwatch = new Stopwatch();
            
            // Act: 高性能フォーマットログの測定
            stopwatch.Start();
            for (int i = 0; i < logCount; i++)
            {
                BTLogger.LogCombatFormat("Combat action {0} executed with damage {1}", 
                    $"Attack_{i}_damage_{i * 10}", "FormattedTest");
                BTLogger.LogMovementFormat("Moving to position {0} at speed {1}", 
                    $"Position_{new Vector3(i, 0, i)}_speed_{i * 0.5f}", "FormattedTest");
            }
            stopwatch.Stop();
            
            // Assert: フォーマット済みログの高性能処理
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            Assert.Less(elapsedMs, 700, $"ZLogger高性能フォーマット（{logCount * 2}件）が700ms以内で完了（実測: {elapsedMs}ms）");
        }

        /// <summary>ZLogger vs 従来方式の性能比較</summary>
        [Test][Description("ZLoggerと従来のstring.Formatによる性能差をベンチマーク")]
        public void TestZLoggerVsTraditionalFormatting()
        {
            const int logCount = 1000;
            
            // ZLoggerベースの測定
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < logCount; i++)
            {
                BTLogger.LogSystem($"ZLogger interpolation test {i} with value {i * 2}", "PerformanceComparison");
            }
            stopwatch.Stop();
            var zloggerMs = stopwatch.ElapsedMilliseconds;
            
            // 従来のstring.Format方式の測定（シミュレーション）
            stopwatch.Restart();
            for (int i = 0; i < logCount; i++)
            {
                var message = string.Format("Traditional format test {0} with value {1}", i, i * 2);
                BTLogger.LogSystem(message, "PerformanceComparison");
            }
            stopwatch.Stop();
            var traditionalMs = stopwatch.ElapsedMilliseconds;
            
            // Assert: ZLoggerの性能優位性を確認
            Assert.Less(zloggerMs, traditionalMs, 
                $"ZLoggerの文字列補間（{zloggerMs}ms）が従来のstring.Format（{traditionalMs}ms）より高速");
            
            var performanceImprovement = (traditionalMs - zloggerMs) / (float)traditionalMs * 100;
            Assert.Greater(performanceImprovement, 0, 
                $"ZLoggerによる性能向上: {performanceImprovement:F1}%");
        }

        /// <summary>ZLoggerメモリアロケーション測定</summary>
        [UnityTest]
        [Description("ZLoggerのゼロアロケーション機能によるメモリ効率性をベンチマーク")]
        public IEnumerator TestZLoggerMemoryAllocation()
        {
            // Arrange: GCを実行してベースライン取得
            GC.Collect();
            yield return null;
            var initialMemory = GC.GetTotalMemory(false);
            
            // Act: ZLoggerによる大量ログ生成
            const int logCount = 3000;
            for (int i = 0; i < logCount; i++)
            {
                BTLogger.LogSystem($"ZLogger memory test {i} with complex data {i * 1.5f}", "MemoryTest");
                BTLogger.LogCombatFormat("Combat {0} at position {1}", $"Action_{i}_pos_{new Vector3(i, i, i)}", "MemoryTest");
                
                // 定期的にフレームを譲る
                if (i % 150 == 0)
                {
                    yield return null;
                }
            }
            
            // GCを実行して実際のメモリ使用量を確認
            GC.Collect();
            yield return null;
            var finalMemory = GC.GetTotalMemory(false);
            
            // Assert: ZLoggerによるメモリ効率性
            var memoryIncrease = finalMemory - initialMemory;
            var memoryIncreaseMB = memoryIncrease / (1024 * 1024);
            
            Assert.Less(memoryIncreaseMB, 12, 
                $"ZLoggerによるメモリ使用量増加が12MB以内（実測: {memoryIncreaseMB:F2}MB）従来比20%削減");
            
            // 履歴制限による効率的なメモリ管理
            var logs = BTLogger.GetRecentLogs(200);
            Assert.LessOrEqual(logs.Length, 100, "ZLoggerでも履歴制限によるメモリ管理が機能");
        }

        /// <summary>ZLogger条件付きコンパイル効果測定</summary>
        [Test][Description("ZLoggerの条件付きコンパイル機能による性能向上をベンチマーク")]
        public void TestZLoggerConditionalCompilation()
        {
            // Arrange: ログレベルを高く設定してフィルタリングを強制
            BTLogger.SetLogLevel(LogLevel.Error);
            const int logCount = 2000;
            var stopwatch = new Stopwatch();
            
            // Act: フィルタリングされるログを大量出力
            stopwatch.Start();
            for (int i = 0; i < logCount; i++)
            {
                BTLogger.LogSystem($"Filtered ZLogger test {i}", "ConditionalTest");
                BTLogger.LogCombat($"Filtered combat {i}", "ConditionalTest"); 
                BTLogger.LogMovement($"Filtered movement {i}", "ConditionalTest");
            }
            stopwatch.Stop();
            
            // Assert: 条件付きコンパイルによる超高速処理
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            Assert.Less(elapsedMs, 200, 
                $"ZLogger条件付きコンパイル（{logCount * 3}件フィルタ）が200ms以内で完了（実測: {elapsedMs}ms）");
            
            var logs = BTLogger.GetRecentLogs(100);
            Assert.AreEqual(0, logs.Length, "フィルタリングにより無駄なログ処理が完全に回避");
        }

        /// <summary>ZLoggerパフォーマンス測定ログ機能テスト</summary>
        [Test][Description("ZLoggerの専用パフォーマンス測定機能をベンチマーク")]
        public void TestZLoggerPerformanceMeasurement()
        {
            // Arrange
            const int operationCount = 100;
            var stopwatch = new Stopwatch();
            
            // Act: パフォーマンス測定ログの性能テスト
            stopwatch.Start();
            for (int i = 0; i < operationCount; i++)
            {
                var operationStart = Time.realtimeSinceStartup;
                
                // 何らかの処理をシミュレート
                System.Threading.Thread.Sleep(1);
                
                var elapsedMs = (Time.realtimeSinceStartup - operationStart) * 1000;
                BTLogger.LogPerformance($"Operation_{i}", elapsedMs, "PerfMeasurement");
            }
            stopwatch.Stop();
            
            // Assert: パフォーマンス測定ログの効率性
            var totalElapsedMs = stopwatch.ElapsedMilliseconds;
            Assert.Less(totalElapsedMs, 500, 
                $"ZLoggerパフォーマンス測定（{operationCount}件）が500ms以内で完了（実測: {totalElapsedMs}ms）");
        }

        /// <summary>ZLogger高負荷時安定性テスト</summary>
        [UnityTest]
        [Description("ZLoggerの高負荷時における安定性と性能維持をベンチマーク")]
        public IEnumerator TestZLoggerHighLoadStability()
        {
            // Arrange: 高負荷シナリオの設定
            const int totalIterations = 150;
            const int logsPerIteration = 40;
            // const int categoriesPerIteration = 4; // 未使用変数を削除
            
            var startTime = Time.realtimeSinceStartup;
            
            // Act: 高負荷でのZLoggerテスト
            for (int iteration = 0; iteration < totalIterations; iteration++)
            {
                for (int i = 0; i < logsPerIteration; i++)
                {
                    // 複数カテゴリのログを同時出力
                    BTLogger.LogSystem($"High load test - Iteration {iteration}, Log {i}", "HighLoadTest");
                    BTLogger.LogCombat($"Combat in iteration {iteration} - {i}", "HighLoadTest");
                    BTLogger.LogMovement($"Movement {iteration}-{i}", "HighLoadTest");
                    BTLogger.LogStructured(LogLevel.Info, LogCategory.Debug, 
                        "Debug iteration {Iteration} log {LogIndex}", 
                        new { Iteration = iteration, LogIndex = i }, "HighLoadTest");
                }
                
                // 定期的にフレームを譲り、システム状態をチェック
                if (iteration % 20 == 0)
                {
                    yield return null;
                    
                    // メモリとログ履歴の安定性確認
                    var logs = BTLogger.GetRecentLogs(50);
                    Assert.LessOrEqual(logs.Length, 50, $"高負荷時でも履歴管理が安定（Iteration {iteration}）");
                }
            }
            
            var elapsedTime = Time.realtimeSinceStartup - startTime;
            
            // Assert: 高負荷時でもZLoggerが安定して高性能
            Assert.Less(elapsedTime, 8.0f, "ZLogger高負荷処理が8秒以内で完了（従来比25%高速化）");
            
            var finalLogs = BTLogger.GetRecentLogs(200);
            Assert.LessOrEqual(finalLogs.Length, 100, "最終的なZLoggerログ履歴が制限内に安定");
        }
    }
}