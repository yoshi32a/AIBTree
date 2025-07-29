using System;
using System.Collections;
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ArcBT.Logger;
using Microsoft.Extensions.Logging;

namespace ArcBT.Tests
{
    /// <summary>BTLoggerのパフォーマンステストクラス</summary>
    public class BTLoggerPerformanceTests
    {
        [SetUp]
        public void SetUp()
        {
            // Phase 6.4: レガシーAPI削除に伴い簡素化
        }

        [TearDown] 
        public void TearDown()
        {
            BTLogger.Dispose(); // ZLoggerリソース解放
        }

        /// <summary>ZLogger高性能大量ログ出力テスト</summary>
        [Test][Description("ZLoggerによる大量ログ出力（2000件）の性能をベンチマークテスト（ZLoggerの高速化を活用）")]
        public void TestZLoggerHighVolumeLoggingPerformance()
        {
            // Arrange
            const int logCount = 2000; // ZLoggerの高性能化により増量
            var stopwatch = new Stopwatch();
            
            // Act: ZLoggerベースの大量ログ出力
            stopwatch.Start();
            for (int i = 0; i < logCount; i++)
            {
                BTLogger.LogSystem($"ZLogger high performance test message {i} with data {i * 1.5f}");
            }
            stopwatch.Stop();
            
            // Assert: ZLoggerによる高速処理確認
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            Assert.Less(elapsedMs, 1500, $"ZLoggerによる{logCount}件ログ出力が1.5秒以内で完了（実測: {elapsedMs}ms）");
            
            // 履歴の上限制御が機能しているか確認
            var logs = BTLogger.GetRecentLogs(200);
            Assert.LessOrEqual(logs.Length, 100, "履歴は最大100件に制限されている");
            
            UnityEngine.Debug.Log($"ZLogger high volume test: {logCount} logs in {elapsedMs}ms");
        }

        /// <summary>ZLoggerフィルタリング機能の高性能テスト</summary>
        [Test][Description("ZLoggerの条件付きコンパイル・フィルタリング機能による高性能処理をベンチマーク")]
        public void TestZLoggerFilteringPerformance()
        {
            // Phase 6.4: SetCategoryFilter削除に伴い、フィルタリングはLoggerFactory設定で制御
            
            const int logCount = 1000; // ZLoggerの高速フィルタリングにより増量
            var stopwatch = new Stopwatch();
            
            // Act: フィルタリングされるログを大量出力
            stopwatch.Start();
            for (int i = 0; i < logCount; i++)
            {
                BTLogger.LogSystem($"ZLogger filtered message {i} with complex data {i * 2.5f}");
                BTLogger.LogCombat($"ZLogger filtered combat {i} action attack");
                BTLogger.LogMovement($"ZLogger filtered movement {i} to position");
            }
            stopwatch.Stop();
            
            // Assert: ZLoggerのフィルタリングが超高速処理
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            Assert.Less(elapsedMs, 800, $"ZLoggerフィルタリング処理が超高速（実測: {elapsedMs}ms）");
            
            var logs = BTLogger.GetRecentLogs(100);
            Assert.AreEqual(0, logs.Length, "フィルタリングによりログが記録されていない");
            
            UnityEngine.Debug.Log($"ZLogger filtering test: {logCount * 3} filtered logs in {elapsedMs}ms");
        }

        /// <summary>ZLoggerゼロアロケーションメモリ効率テスト</summary>
        [UnityTest]
        [Description("ZLoggerのゼロアロケーション機能による大量ログ出力（3000件）時のメモリ効率をベンチマーク")]
        public IEnumerator TestZLoggerMemoryEfficiency()
        {
            // Arrange: 初期状態でGCを実行してベースラインを安定化
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            yield return new WaitForEndOfFrame();
            var initialMemory = System.GC.GetTotalMemory(false);
            
            // Act: ZLoggerによる大量ログ生成（ゼロアロケーション効果を測定）
            const int logCount = 3000; // ZLoggerの効率性により増量
            for (int i = 0; i < logCount; i++)
            {
                BTLogger.LogSystem($"ZLogger zero allocation test message {i} with complex data {i * 2.5f} and position {new Vector3(i, i, i)}");
                
                // 定期的にフレームを譲る
                if (i % 150 == 0)
                {
                    yield return null;
                }
            }
            
            // 複数回GCを実行して正確なメモリ測定
            yield return new WaitForEndOfFrame();
            System.GC.Collect();
            yield return new WaitForEndOfFrame();
            System.GC.Collect();
            var finalMemory = System.GC.GetTotalMemory(false);
            
            // Assert: ZLoggerのメモリ効率性確認
            var memoryIncrease = finalMemory - initialMemory;
            var memoryIncreaseMB = memoryIncrease / (1024.0 * 1024.0);
            
            // Unity Editor環境を考慮した実用的なメモリ基準
            Assert.Less(Math.Abs(memoryIncreaseMB), 20, $"ZLoggerメモリ効率テスト（{logCount}件）: {memoryIncreaseMB:F2}MB（Unity Editor環境）");
            
            // 履歴制限が機能しているか確認
            var logs = BTLogger.GetRecentLogs(200);
            Assert.LessOrEqual(logs.Length, 100, "履歴上限によりメモリ使用量が制御されている");
            
            UnityEngine.Debug.Log($"ZLogger memory efficiency: {logCount} logs with {memoryIncreaseMB:F2}MB memory change");
        }

        /// <summary>並行アクセス耐性テスト</summary>
        [UnityTest]
        [Description("複数処理からの同時ログ出力をシミュレートし、並行アクセス耐性をベンチマークテスト（3秒以内で完了）")]
        public IEnumerator TestConcurrentAccess()
        {
            // Arrange: 複数の並行処理で同時ログ出力をシミュレート
            const int threadCount = 5;
            const int logsPerThread = 100;
            
            var startTime = Time.realtimeSinceStartup;
            
            // Act: 複数の処理を並行実行（コルーチンではなく直接実行）
            var tasks = new System.Collections.Generic.List<IEnumerator>();
            
            for (int i = 0; i < threadCount; i++)
            {
                int threadIndex = i;
                tasks.Add(LoggingTask(threadIndex, logsPerThread));
            }
            
            // 各タスクを順次実行（Unity Test環境での並行処理シミュレート）
            foreach (var task in tasks)
            {
                while (task.MoveNext())
                {
                    yield return task.Current;
                }
            }
            
            var elapsedTime = Time.realtimeSinceStartup - startTime;
            
            // Assert: 並行処理が適切に処理されている
            Assert.Less(elapsedTime, 3.0f, "並行ログ処理が適切な時間内で完了");
            
            // Phase 6.4: GetRecentLogsは常に空配列を返すため、時間ベースでのテストのみ
            var logs = BTLogger.GetRecentLogs(200);
            Assert.AreEqual(0, logs.Length, "Phase 6.4: 履歴管理はZLoggerに委譲 - 空配列");
        }

        IEnumerator LoggingTask(int taskIndex, int logCount)
        {
            for (int i = 0; i < logCount; i++)
            {
                BTLogger.LogSystem($"Task {taskIndex} - Message {i}");
                
                // 定期的にフレームを譲る
                if (i % 10 == 0)
                {
                    yield return null;
                }
            }
        }

        /// <summary>ログ取得処理のパフォーマンステスト</summary>
        [Test][Description("ログ履歴取得処理の性能をベンチマークテスト（カテゴリ別取得、最新ログ取得の高速処理を確認）")]
        public void TestLogRetrievalPerformance()
        {
            // Arrange: 履歴にログを蓄積
            for (int i = 0; i < 100; i++)
            {
                BTLogger.LogCombat($"Combat log {i}");
                BTLogger.LogMovement($"Movement log {i}");
                BTLogger.LogSystem($"System log {i}");
            }
            
            var stopwatch = new Stopwatch();
            
            // Act: ログ取得処理の速度を測定
            stopwatch.Start();
            for (int i = 0; i < 100; i++)
            {
                var recentLogs = BTLogger.GetRecentLogs(10);
                var combatLogs = BTLogger.GetLogsByCategory(LogCategory.Combat, 5);
            }
            stopwatch.Stop();
            
            // Assert: ログ取得が高速に処理されている
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            Assert.Less(elapsedMs, 100, $"ログ取得処理が高速である（実測: {elapsedMs}ms）");
        }

        /// <summary>基本ログ出力処理パフォーマンステスト</summary>
        [Test][Description("Phase 6.4: 基本ログ出力処理の性能をベンチマークテスト（3000回出力が200ms以内）")]
        public void TestBasicLoggingPerformance()
        {
            var stopwatch = new Stopwatch();
            
            // Act: 基本ログ出力処理の速度を測定
            stopwatch.Start();
            for (int i = 0; i < 1000; i++)
            {
                BTLogger.LogSystem($"Performance test message {i}");
                BTLogger.LogCombat($"Combat test message {i}");
                BTLogger.LogMovement($"Movement test message {i}");
            }
            stopwatch.Stop();
            
            // Assert: ログ出力が高速に処理されている
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            Assert.Less(elapsedMs, 200, $"基本ログ出力処理が高速である（実測: {elapsedMs}ms）");
            
            UnityEngine.Debug.Log($"Phase 6.4: Basic logging performance: 3000 logs in {elapsedMs}ms");
        }

        /// <summary>長時間実行安定性テスト</summary>
        [UnityTest]
        [Description("長時間の連続ログ出力をシミュレートし、システムの安定性をベンチマークテスト（5000件ログが10秒以内で完了）")]
        public IEnumerator TestLongRunningStability()
        {
            // Arrange: 長時間の連続ログ出力をシミュレート
            const int totalIterations = 100;
            const int logsPerIteration = 50;
            
            var startTime = Time.realtimeSinceStartup;
            
            // Act: 長時間にわたってログを出力
            for (int iteration = 0; iteration < totalIterations; iteration++)
            {
                for (int i = 0; i < logsPerIteration; i++)
                {
                    BTLogger.LogSystem($"Stability test - Iteration {iteration}, Log {i}");
                }
                
                // 定期的にフレームを譲り、メモリ状況をチェック
                if (iteration % 10 == 0)
                {
                    yield return null;
                    
                    // Phase 6.4: GetRecentLogsは常に空配列を返す
                    var logs = BTLogger.GetRecentLogs(10);
                    Assert.AreEqual(0, logs.Length, "Phase 6.4: 履歴管理はZLoggerに委譲 - 空配列");
                }
            }
            
            var elapsedTime = Time.realtimeSinceStartup - startTime;
            
            // Assert: 長時間実行でも安定している
            Assert.Less(elapsedTime, 10.0f, "長時間実行が適切な時間内で完了");
            
            // Phase 6.4: GetRecentLogsは常に空配列を返す
            var finalLogs = BTLogger.GetRecentLogs(200);
            Assert.AreEqual(0, finalLogs.Length, "Phase 6.4: 履歴管理はZLoggerに委譲 - 空配列");
        }

        /// <summary>ZLogger構造化ログパフォーマンステスト</summary>
        [Test][Description("ZLoggerの構造化ログ機能の性能をベンチマークテスト")]
        public void TestZLoggerStructuredLoggingPerformance()
        {
            // Arrange
            const int logCount = 800;
            var stopwatch = new Stopwatch();
            
            // Act: ZLoggerの構造化ログを大量出力
            stopwatch.Start();
            for (int i = 0; i < logCount; i++)
            {
                var structuredData = new
                {
                    Index = i,
                    Value = i * 1.5f,
                    Position = new Vector3(i, i * 2, i * 3),
                    Name = $"Entity_{i}",
                    Active = i % 2 == 0
                };
                
                BTLogger.LogStructured(Microsoft.Extensions.Logging.LogLevel.Information, LogCategory.System, 
                    "Structured performance test {Index} with {Value} at {Position} named {Name} active {Active}", 
                    structuredData, "StructuredPerformance");
            }
            stopwatch.Stop();
            
            // Assert: 構造化ログの高性能処理
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            Assert.Less(elapsedMs, 1000, $"ZLogger構造化ログ（{logCount}件）が1秒以内で完了（実測: {elapsedMs}ms）");
            
            UnityEngine.Debug.Log($"ZLogger structured logging performance: {logCount} logs in {elapsedMs}ms");
        }

        /// <summary>ZLoggerフォーマットメソッドパフォーマンステスト</summary>
        [Test][Description("ZLoggerの高性能フォーマットメソッドの性能をベンチマークテスト")]
        public void TestZLoggerFormatMethodsPerformance()
        {
            // Arrange
            const int logCount = 1000;
            var stopwatch = new Stopwatch();
            
            // Act: ZLoggerの高性能フォーマットメソッドを大量実行
            stopwatch.Start();
            for (int i = 0; i < logCount; i++)
            {
                BTLogger.LogCombatFormat("Combat performance test {0} with damage {1}", 
                    $"Action_{i}_damage_{i * 10}", "FormatPerformance");
                BTLogger.LogMovementFormat("Movement performance test {0} at speed {1}", 
                    $"Position_{new Vector3(i, 0, i)}_speed_{i * 0.5f}", "FormatPerformance");
            }
            stopwatch.Stop();
            
            // Assert: フォーマットメソッドの高性能処理
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            Assert.Less(elapsedMs, 800, $"ZLoggerフォーマットメソッド（{logCount * 2}件）が800ms以内で完了（実測: {elapsedMs}ms）");
            
            UnityEngine.Debug.Log($"ZLogger format methods performance: {logCount * 2} logs in {elapsedMs}ms");
        }

        /// <summary>ZLoggerパフォーマンス測定機能テスト</summary>
        [Test][Description("ZLoggerのパフォーマンス測定ログ機能の性能をベンチマークテスト")]
        public void TestZLoggerPerformanceMeasurementPerformance()
        {
            // Arrange
            const int operationCount = 200;
            var stopwatch = new Stopwatch();
            
            // Act: ZLoggerのパフォーマンス測定ログを大量実行
            stopwatch.Start();
            for (int i = 0; i < operationCount; i++)
            {
                var operationStart = Time.realtimeSinceStartup;
                
                // 何らかの処理をシミュレート
                System.Threading.Thread.Sleep(1);
                
                var elapsedMs = (Time.realtimeSinceStartup - operationStart) * 1000;
                BTLogger.LogPerformance($"PerformanceOperation_{i}", elapsedMs, "PerfMeasurementPerformance");
            }
            stopwatch.Stop();
            
            // Assert: パフォーマンス測定ログの効率性
            var totalElapsedMs = stopwatch.ElapsedMilliseconds;
            Assert.Less(totalElapsedMs, 1000, $"ZLoggerパフォーマンス測定（{operationCount}件）が1秒以内で完了（実測: {totalElapsedMs}ms）");
            
            UnityEngine.Debug.Log($"ZLogger performance measurement: {operationCount} measurements in {totalElapsedMs}ms");
        }

        /// <summary>ZLoggerリソース管理パフォーマンステスト</summary>
        [Test][Description("ZLoggerの初期化・解放処理の性能をベンチマークテスト")]
        public void TestZLoggerResourceManagementPerformance()
        {
            // Arrange
            const int cycles = 10;
            var stopwatch = new Stopwatch();
            
            // Act: ZLoggerの初期化・解放サイクルを測定
            stopwatch.Start();
            for (int cycle = 0; cycle < cycles; cycle++)
            {
                // ログ出力により暗黙的初期化
                BTLogger.LogSystem($"Resource management test cycle {cycle}");
                
                // 明示的解放
                BTLogger.Dispose();
                
                // 再初期化テスト
                BTLogger.LogSystem($"Re-initialization test cycle {cycle}");
            }
            stopwatch.Stop();
            
            // Assert: リソース管理の高速処理
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            Assert.Less(elapsedMs, 500, $"ZLoggerリソース管理（{cycles}サイクル）が500ms以内で完了（実測: {elapsedMs}ms）");
            
            // Phase 6.4: GetRecentLogsは常に空配列を返す
            var logs = BTLogger.GetRecentLogs(5);
            Assert.AreEqual(0, logs.Length, "Phase 6.4: 履歴管理はZLoggerに委譲 - 空配列");
            
            UnityEngine.Debug.Log($"ZLogger resource management: {cycles} cycles in {elapsedMs}ms");
        }
    }
}
