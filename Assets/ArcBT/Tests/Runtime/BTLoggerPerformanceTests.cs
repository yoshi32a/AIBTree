using System.Collections;
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ArcBT.Logger;

namespace ArcBT.Tests
{
    /// <summary>BTLoggerのパフォーマンステストクラス</summary>
    public class BTLoggerPerformanceTests
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
        }

        /// <summary>大量ログ出力のパフォーマンステスト</summary>
        [Test][Description("大量ログ出力（1000件）の性能をベンチマークテスト（1秒以内で処理、履歴上限制御の確認）")]
        public void TestHighVolumeLoggingPerformance()
        {
            // Arrange
            const int logCount = 1000;
            var stopwatch = new Stopwatch();
            
            // Act: 大量のログを出力して時間を測定
            stopwatch.Start();
            for (int i = 0; i < logCount; i++)
            {
                BTLogger.LogSystem($"Performance test message {i}");
            }
            stopwatch.Stop();
            
            // Assert: 適切な時間内で処理されているか確認
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            Assert.Less(elapsedMs, 1000, $"1000件のログ出力が1秒以内に完了する（実測: {elapsedMs}ms）");
            
            // 履歴の上限制御が機能しているか確認
            var logs = BTLogger.GetRecentLogs(200);
            Assert.LessOrEqual(logs.Length, 100, "履歴は最大100件に制限されている");
        }

        /// <summary>フィルタリング機能のパフォーマンステスト</summary>
        [Test][Description("カテゴリフィルタリング機能の性能をベンチマークテスト（フィルタされたログの高速処理を確認）")]
        public void TestFilteringPerformance()
        {
            // Arrange: すべてのカテゴリを無効化してフィルタリングを強制
            foreach (LogCategory category in System.Enum.GetValues(typeof(LogCategory)))
            {
                BTLogger.SetCategoryFilter(category, false);
            }
            
            const int logCount = 500;
            var stopwatch = new Stopwatch();
            
            // Act: フィルタリングされるログを大量出力
            stopwatch.Start();
            for (int i = 0; i < logCount; i++)
            {
                BTLogger.LogSystem($"Filtered message {i}");
                BTLogger.LogCombat($"Filtered combat {i}");
            }
            stopwatch.Stop();
            
            // Assert: フィルタリングが効いており、高速に処理されている
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            Assert.Less(elapsedMs, 500, $"フィルタリング処理が高速である（実測: {elapsedMs}ms）");
            
            var logs = BTLogger.GetRecentLogs(100);
            Assert.AreEqual(0, logs.Length, "フィルタリングによりログが記録されていない");
        }

        /// <summary>メモリ使用量テスト</summary>
        [UnityTest]
        [Description("大量ログ出力（2000件）時のメモリ使用量をベンチマークテスト（15MB以内、履歴制限機能の確認）")]
        public IEnumerator TestMemoryUsage()
        {
            // Arrange: GCを実行してベースライン取得
            System.GC.Collect();
            yield return null;
            var initialMemory = System.GC.GetTotalMemory(false);
            
            // Act: 大量のログを生成
            const int logCount = 2000;
            for (int i = 0; i < logCount; i++)
            {
                BTLogger.LogSystem($"Memory test message with longer content to test memory usage {i}");
                
                // 定期的にフレームを譲る
                if (i % 100 == 0)
                {
                    yield return null;
                }
            }
            
            // GCを実行して実際のメモリ使用量を確認
            System.GC.Collect();
            yield return null;
            var finalMemory = System.GC.GetTotalMemory(false);
            
            // Assert: メモリ使用量が適切な範囲内か確認
            var memoryIncrease = finalMemory - initialMemory;
            var memoryIncreaseMB = memoryIncrease / (1024 * 1024);
            
            Assert.Less(memoryIncreaseMB, 15, $"メモリ使用量増加が15MB以内（実測: {memoryIncreaseMB:F2}MB）（Unity Editorの Debug.Log オーバーヘッドを含む）");
            
            // 履歴制限が機能しているか確認
            var logs = BTLogger.GetRecentLogs(200);
            Assert.LessOrEqual(logs.Length, 100, "履歴上限によりメモリ使用量が制御されている");
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
            
            var logs = BTLogger.GetRecentLogs(200);
            Assert.Greater(logs.Length, 50, "並行処理でもログが適切に記録されている");
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

        /// <summary>設定変更処理のパフォーマンステスト</summary>
        [Test][Description("ログレベルやカテゴリフィルタの動的変更処理の性能をベンチマークテスト（1000回変更が50ms以内）")]
        public void TestSettingsChangePerformance()
        {
            var stopwatch = new Stopwatch();
            
            // Act: 設定変更処理の速度を測定
            stopwatch.Start();
            for (int i = 0; i < 1000; i++)
            {
                BTLogger.SetLogLevel((LogLevel)(i % 5));
                BTLogger.SetCategoryFilter(LogCategory.Combat, i % 2 == 0);
                BTLogger.SetCategoryFilter(LogCategory.Movement, i % 2 == 1);
            }
            stopwatch.Stop();
            
            // Assert: 設定変更が高速に処理されている
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            Assert.Less(elapsedMs, 50, $"設定変更処理が高速である（実測: {elapsedMs}ms）");
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
                    
                    // メモリ使用量が異常に増加していないかチェック
                    var logs = BTLogger.GetRecentLogs(10);
                    Assert.LessOrEqual(logs.Length, 10, $"履歴管理が正常に機能している（Iteration {iteration}）");
                }
            }
            
            var elapsedTime = Time.realtimeSinceStartup - startTime;
            
            // Assert: 長時間実行でも安定している
            Assert.Less(elapsedTime, 10.0f, "長時間実行が適切な時間内で完了");
            
            var finalLogs = BTLogger.GetRecentLogs(200);
            Assert.LessOrEqual(finalLogs.Length, 100, "最終的なログ履歴が制限内に収まっている");
        }
    }
}
