using System;
using System.Collections;
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ArcBT.Logger;

namespace ArcBT.Tests
{
    /// <summary>ZLoggerのメモリアロケーション性能を詳細測定するテストクラス</summary>
    public class ZLoggerMemoryBenchmarkTests
    {
        [SetUp]
        public void SetUp()
        {
            BTLogger.ResetToDefaults();
            BTLogger.ClearHistory();
            
            // メモリ測定前に2回GCを実行してベースラインを安定化
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        [TearDown] 
        public void TearDown()
        {
            BTLogger.ClearHistory();
            BTLogger.Dispose();
        }

        /// <summary>ZLoggerゼロアロケーション文字列補間のメモリ効率測定</summary>
        [UnityTest]
        [Description("ZLoggerのゼロアロケーション文字列補間によるメモリ効率性を詳細ベンチマーク")]
        public IEnumerator TestZLoggerZeroAllocationStringInterpolation()
        {
            // Arrange: 安定したベースライン取得
            yield return new WaitForEndOfFrame();
            var initialMemory = GC.GetTotalMemory(true);
            
            // Act: ゼロアロケーション文字列補間テスト
            const int logCount = 2500;
            for (int i = 0; i < logCount; i++)
            {
                BTLogger.LogSystem($"ZLogger zero allocation test {i} with float {i * 1.5f} and vector {new Vector3(i, i * 2, i * 3)}", "ZeroAllocation");
                
                if (i % 200 == 0)
                {
                    yield return null;
                }
            }
            
            // 複数回GCを実行して正確な測定
            GC.Collect();
            yield return new WaitForEndOfFrame();
            GC.Collect();
            var finalMemory = GC.GetTotalMemory(true);
            
            // Assert: ゼロアロケーションによる最小メモリ増加
            var memoryIncrease = finalMemory - initialMemory;
            var memoryIncreaseMB = memoryIncrease / (1024.0 * 1024.0);
            
            Assert.Less(memoryIncreaseMB, 8.0, 
                $"ZLoggerゼロアロケーション（{logCount}件）のメモリ増加が8MB以内（実測: {memoryIncreaseMB:F2}MB）");
            
            UnityEngine.Debug.Log($"ZLogger Zero Allocation Memory Test: {memoryIncreaseMB:F2}MB increase for {logCount} logs");
        }

        /// <summary>ZLogger vs 従来string.Format のメモリ使用量比較</summary>
        [UnityTest]
        [Description("ZLoggerと従来のstring.Formatによるメモリ使用量差を詳細比較")]
        public IEnumerator TestZLoggerVsStringFormatMemoryUsage()
        {
            const int logCount = 1500;
            
            // ZLoggerメモリ測定
            yield return new WaitForEndOfFrame();
            var zloggerInitialMemory = GC.GetTotalMemory(true);
            
            for (int i = 0; i < logCount; i++)
            {
                BTLogger.LogSystem($"ZLogger test {i} value {i * 2.5f} position {new Vector3(i, i, i)}", "MemoryComparison");
                
                if (i % 150 == 0)
                {
                    yield return null;
                }
            }
            
            GC.Collect();
            yield return new WaitForEndOfFrame();
            var zloggerFinalMemory = GC.GetTotalMemory(true);
            var zloggerMemoryIncrease = zloggerFinalMemory - zloggerInitialMemory;
            
            // ログ履歴をクリアして次の測定に備える
            BTLogger.ClearHistory();
            
            // 従来方式メモリ測定
            yield return new WaitForEndOfFrame();
            var traditionalInitialMemory = GC.GetTotalMemory(true);
            
            for (int i = 0; i < logCount; i++)
            {
                var message = string.Format("Traditional test {0} value {1} position {2}", i, i * 2.5f, new Vector3(i, i, i));
                BTLogger.LogSystem(message, "MemoryComparison");
                
                if (i % 150 == 0)
                {
                    yield return null;
                }
            }
            
            GC.Collect();
            yield return new WaitForEndOfFrame();
            var traditionalFinalMemory = GC.GetTotalMemory(true);
            var traditionalMemoryIncrease = traditionalFinalMemory - traditionalInitialMemory;
            
            // Assert: ZLoggerのメモリ効率性確認
            var memoryReduction = traditionalMemoryIncrease - zloggerMemoryIncrease;
            var memoryReductionPercent = (memoryReduction / (double)traditionalMemoryIncrease) * 100;
            
            Assert.Greater(memoryReduction, 0, 
                $"ZLoggerがメモリ効率的: ZLogger {zloggerMemoryIncrease / 1024}KB vs 従来 {traditionalMemoryIncrease / 1024}KB");
            
            Assert.Greater(memoryReductionPercent, 15, 
                $"ZLoggerによるメモリ削減率: {memoryReductionPercent:F1}% (目標: 15%以上)");
            
            UnityEngine.Debug.Log($"Memory Comparison - ZLogger: {zloggerMemoryIncrease / 1024}KB, Traditional: {traditionalMemoryIncrease / 1024}KB, Reduction: {memoryReductionPercent:F1}%");
        }

        /// <summary>ZLogger構造化ログのメモリ効率測定</summary>
        [UnityTest]
        [Description("ZLoggerの構造化ログによるメモリ効率性をベンチマーク")]
        public IEnumerator TestZLoggerStructuredLoggingMemoryEfficiency()
        {
            // Arrange
            yield return new WaitForEndOfFrame();
            var initialMemory = GC.GetTotalMemory(true);
            
            // Act: 構造化ログの大量生成
            const int logCount = 1200;
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
                
                BTLogger.LogStructured(LogLevel.Info, LogCategory.System, 
                    "Structured log {Index} with {Value} at {Position} named {Name} active {Active}", 
                    structuredData, "StructuredMemory");
                
                if (i % 120 == 0)
                {
                    yield return null;
                }
            }
            
            // 精密なメモリ測定
            GC.Collect();
            yield return new WaitForEndOfFrame();
            GC.Collect();
            var finalMemory = GC.GetTotalMemory(true);
            
            // Assert: 構造化ログの効率的なメモリ使用
            var memoryIncrease = finalMemory - initialMemory;
            var memoryIncreaseMB = memoryIncrease / (1024.0 * 1024.0);
            
            Assert.Less(memoryIncreaseMB, 6.0, 
                $"ZLogger構造化ログ（{logCount}件）のメモリ増加が6MB以内（実測: {memoryIncreaseMB:F2}MB）");
            
            UnityEngine.Debug.Log($"ZLogger Structured Logging Memory: {memoryIncreaseMB:F2}MB for {logCount} structured logs");
        }

        /// <summary>ZLogger長時間実行時のメモリリーク検出</summary>
        [UnityTest]
        [Description("ZLoggerの長時間実行時にメモリリークが発生しないことを確認")]
        public IEnumerator TestZLoggerMemoryLeakDetection()
        {
            // Arrange: 初期メモリ状態記録
            yield return new WaitForEndOfFrame();
            var initialMemory = GC.GetTotalMemory(true);
            
            // Act: 長時間実行シミュレーション（複数サイクル）
            const int cycles = 10;
            const int logsPerCycle = 300;
            
            for (int cycle = 0; cycle < cycles; cycle++)
            {
                for (int i = 0; i < logsPerCycle; i++)
                {
                    BTLogger.LogSystem($"Leak test cycle {cycle} log {i}", "LeakDetection");
                    BTLogger.LogCombatFormat("Combat cycle {0} iteration {1} with damage {2}", 
                        $"cycle_{cycle}_iter_{i}_damage_{i * 10}", "LeakDetection");
                    BTLogger.LogStructured(LogLevel.Debug, LogCategory.Movement, 
                        "Movement cycle {Cycle} step {Step}", 
                        new { Cycle = cycle, Step = i }, "LeakDetection");
                }
                
                // 各サイクル後にログ履歴をクリア（実際の運用パターンをシミュレート）
                BTLogger.ClearHistory();
                
                // 定期的なGCとメモリチェック
                if (cycle % 3 == 0)
                {
                    GC.Collect();
                    yield return null;
                    
                    var currentMemory = GC.GetTotalMemory(false);
                    var currentIncrease = currentMemory - initialMemory;
                    var currentIncreaseMB = currentIncrease / (1024.0 * 1024.0);
                    
                    Assert.Less(currentIncreaseMB, 15.0, 
                        $"サイクル {cycle} でメモリリークなし（増加: {currentIncreaseMB:F2}MB）");
                }
            }
            
            // 最終メモリリーク確認
            GC.Collect();
            yield return new WaitForEndOfFrame();
            GC.Collect();
            var finalMemory = GC.GetTotalMemory(true);
            
            // Assert: メモリリークが発生していないことを確認
            var totalMemoryIncrease = finalMemory - initialMemory;
            var totalMemoryIncreaseMB = totalMemoryIncrease / (1024.0 * 1024.0);
            
            Assert.Less(totalMemoryIncreaseMB, 10.0, 
                $"ZLogger長時間実行（{cycles * logsPerCycle * 3}ログ）でメモリリークなし（増加: {totalMemoryIncreaseMB:F2}MB）");
            
            UnityEngine.Debug.Log($"ZLogger Memory Leak Test: {totalMemoryIncreaseMB:F2}MB increase after {cycles} cycles");
        }

        /// <summary>ZLoggerフィルタリング時のメモリ効率測定</summary>
        [UnityTest]
        [Description("ZLoggerのログフィルタリングによるメモリ効率性をベンチマーク")]
        public IEnumerator TestZLoggerFilteringMemoryEfficiency()
        {
            // Arrange: フィルタリング設定で無効ログを大量生成
            BTLogger.SetLogLevel(LogLevel.Error); // InfoとDebugをフィルタ
            foreach (LogCategory category in Enum.GetValues(typeof(LogCategory)))
            {
                if (category != LogCategory.System)
                {
                    BTLogger.SetCategoryFilter(category, false);
                }
            }
            
            yield return new WaitForEndOfFrame();
            var initialMemory = GC.GetTotalMemory(true);
            
            // Act: フィルタリングされるログを大量生成
            const int logCount = 3000;
            for (int i = 0; i < logCount; i++)
            {
                // これらのログはフィルタリングされるため処理されない
                BTLogger.LogCombat($"Filtered combat log {i}", "FilterMemory");
                BTLogger.LogMovement($"Filtered movement log {i}", "FilterMemory");
                BTLogger.Log(LogLevel.Info, LogCategory.System, $"Filtered info log {i}", "FilterMemory");
                BTLogger.Log(LogLevel.Debug, LogCategory.Debug, $"Filtered debug log {i}", "FilterMemory");
                
                if (i % 250 == 0)
                {
                    yield return null;
                }
            }
            
            GC.Collect();
            yield return new WaitForEndOfFrame();
            var finalMemory = GC.GetTotalMemory(true);
            
            // Assert: フィルタリングによる最小メモリ使用
            var memoryIncrease = finalMemory - initialMemory;
            var memoryIncreaseMB = memoryIncrease / (1024.0 * 1024.0);
            
            Assert.Less(memoryIncreaseMB, 2.0, 
                $"ZLoggerフィルタリング（{logCount * 4}件フィルタ）のメモリ増加が2MB以内（実測: {memoryIncreaseMB:F2}MB）");
            
            // フィルタリングによりログ履歴が空であることを確認
            var logs = BTLogger.GetRecentLogs(100);
            Assert.AreEqual(0, logs.Length, "フィルタリングによりメモリにログが残っていない");
            
            UnityEngine.Debug.Log($"ZLogger Filtering Memory: {memoryIncreaseMB:F2}MB increase for {logCount * 4} filtered logs");
        }

        /// <summary>ZLoggerガベージコレクション負荷測定</summary>
        [UnityTest]
        [Description("ZLoggerがGCに与える負荷を測定してGCフレンドリーであることを確認")]
        public IEnumerator TestZLoggerGarbageCollectionImpact()
        {
            // Arrange: GC統計の初期状態
            var initialGCGen0 = GC.CollectionCount(0);
            var initialGCGen1 = GC.CollectionCount(1);
            var initialGCGen2 = GC.CollectionCount(2);
            
            // Act: ZLoggerによる大量ログ生成
            const int logCount = 4000;
            for (int i = 0; i < logCount; i++)
            {
                BTLogger.LogSystem($"GC impact test {i} with data {i * 2.5f}", "GCImpact");
                BTLogger.LogStructured(LogLevel.Info, LogCategory.Combat, 
                    "GC test {Index} value {Value}", 
                    new { Index = i, Value = i * 1.5f }, "GCImpact");
                
                if (i % 300 == 0)
                {
                    yield return null;
                }
            }
            
            yield return new WaitForEndOfFrame();
            
            // Assert: GC発生頻度が最小限であることを確認
            var finalGCGen0 = GC.CollectionCount(0);
            var finalGCGen1 = GC.CollectionCount(1);
            var finalGCGen2 = GC.CollectionCount(2);
            
            var gen0Increases = finalGCGen0 - initialGCGen0;
            var gen1Increases = finalGCGen1 - initialGCGen1;
            var gen2Increases = finalGCGen2 - initialGCGen2;
            
            Assert.Less(gen0Increases, 20, 
                $"ZLoggerのGen0 GC発生が最小限（{gen0Increases}回）");
            Assert.Less(gen1Increases, 5, 
                $"ZLoggerのGen1 GC発生が最小限（{gen1Increases}回）");
            Assert.Less(gen2Increases, 2, 
                $"ZLoggerのGen2 GC発生が最小限（{gen2Increases}回）");
            
            UnityEngine.Debug.Log($"ZLogger GC Impact - Gen0: {gen0Increases}, Gen1: {gen1Increases}, Gen2: {gen2Increases} for {logCount * 2} logs");
        }
    }
}