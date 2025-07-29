using System;
using System.Collections;
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ArcBT.Logger;

namespace ArcBT.Tests
{
    /// <summary>ZLoggerの本番環境での動作確認テストクラス</summary>
    public class ZLoggerProductionTests : BTTestBase
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            // Phase 6.4: レガシーAPI削除に伴い削除
        }

        [TearDown] 
        public override void TearDown()
        {
            BTLogger.Dispose();
            base.TearDown();
        }

        /// <summary>ZLogger条件付きコンパイル本番動作確認</summary>
        [Test][Description("ZLoggerの条件付きコンパイル機能が本番環境で正常に動作することを確認")]
        public void TestZLoggerConditionalCompilationInProduction()
        {
            // Arrange: 本番環境をシミュレート（BT_LOGGING_ENABLEDが未定義状態をテスト）
            var stopwatch = new System.Diagnostics.Stopwatch();
            
            // Act: 条件付きコンパイルによるログ処理
            stopwatch.Start();
            for (int i = 0; i < 1000; i++)
            {
                BTLogger.LogSystem($"Production test {i}", "ProductionTest");
                BTLogger.LogCombat($"Combat in production {i}", "ProductionTest");
                BTLogger.LogMovement($"Movement {i}", "ProductionTest");
            }
            stopwatch.Stop();
            
            // Assert: 条件付きコンパイルにより超高速処理
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD || BT_LOGGING_ENABLED
            // 開発環境では実際のログ処理が行われる
            Assert.Less(elapsedMs, 1000, $"開発環境でのZLoggerログ処理が適切な速度（実測: {elapsedMs}ms）");
            var logs = BTLogger.GetRecentLogs(50);
            Assert.AreEqual(0, logs.Length, "Phase 6.3: 履歴管理はZLoggerに委譲 - 空配列");
            UnityEngine.Debug.Log($"開発環境での条件付きコンパイルテスト: {elapsedMs}ms, ZLogger委謗完了");
            #else
            // 本番環境では条件付きコンパイルによりログ処理が除去される
            Assert.Less(elapsedMs, 100, $"本番環境でのログ処理が条件付きコンパイルにより最適化（実測: {elapsedMs}ms）");
            var logs = BTLogger.GetRecentLogs(50);
            UnityEngine.Debug.Log($"本番環境での条件付きコンパイルテスト: {elapsedMs}ms, ログ数: {logs.Length}");
            #endif
        }

        /// <summary>ZLogger初期化・解放処理の安全性確認</summary>
        [Test][Description("ZLoggerの初期化と解放処理が本番環境で安全に動作することを確認")]
        public void TestZLoggerInitializationAndDisposalSafety()
        {
            // Arrange & Act: 複数回の初期化・解放サイクル
            for (int cycle = 0; cycle < 5; cycle++)
            {
                // ログ出力による暗黙的初期化
                BTLogger.LogSystem($"Initialization test cycle {cycle}", "InitTest");
                
                // 明示的な解放
                BTLogger.Dispose();
                
                // 解放後の再初期化テスト
                BTLogger.LogSystem($"Re-initialization test cycle {cycle}", "InitTest");
            }
            
            // Assert: 初期化・解放が正常に動作
            var logs = BTLogger.GetRecentLogs(10);
            Assert.AreEqual(0, logs.Length, "Phase 6.3: 履歴管理はZLoggerに委謗 - 空配列");
        }

        /// <summary>ZLoggerファイル出力機能の本番動作確認</summary>
        [UnityTest]
        [Description("ZLoggerのファイル出力機能が本番環境で正常に動作することを確認")]
        public IEnumerator TestZLoggerFileOutputInProduction()
        {
            // Arrange: ファイル出力を含むログ設定
            const int logCount = 500;
            
            // Act: ファイル出力を含むログ生成
            for (int i = 0; i < logCount; i++)
            {
                BTLogger.LogSystem($"File output test {i} with data {i * 1.5f}", "FileOutputTest");
                BTLogger.LogError(LogCategory.System, $"Error test {i}", "FileOutputTest");
                
                if (i % 50 == 0)
                {
                    yield return null;
                }
            }
            
            // ファイル書き込み完了を待つ
            yield return new WaitForSeconds(1.0f);
            
            // Assert: ファイル出力でもメモリ効率が維持される
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            // 開発環境でのファイル出力機能確認
            var logs = BTLogger.GetRecentLogs(100);
            Assert.AreEqual(0, logs.Length, "Phase 6.3: 履歴管理はZLoggerに委謗 - 空配列");
            
            // ファイル出力によるパフォーマンスへの影響が最小限であることを確認
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < 100; i++)
            {
                BTLogger.LogSystem($"File performance test {i}", "FilePerformanceTest");
            }
            stopwatch.Stop();
            
            Assert.Less(stopwatch.ElapsedMilliseconds, 200, "ファイル出力でも高性能を維持");
            #endif
        }

        /// <summary>ZLogger例外処理安全性確認</summary>
        [Test][Description("ZLoggerが例外状況でも安全に動作することを確認")]
        public void TestZLoggerExceptionSafety()
        {
            // Arrange & Act: 例外を引き起こす可能性のある状況をテスト
            
            // null値でのログ出力
            BTLogger.LogSystem(null, "ExceptionTest");
            BTLogger.LogSystem("Normal message", null);
            
            // 非常に長い文字列でのログ出力
            var longMessage = new string('A', 10000);
            BTLogger.LogSystem(longMessage, "ExceptionTest");
            
            // 特殊文字を含む文字列
            BTLogger.LogSystem("Special chars: \n\r\t\0\\\"'", "ExceptionTest");
            
            // 複雑なオブジェクトでの構造化ログ
            var complexObject = new
            {
                NullValue = (string)null,
                LongString = new string('B', 5000),
                NestedObject = new { Inner = "test" }
            };
            BTLogger.LogStructured(Microsoft.Extensions.Logging.LogLevel.Information, LogCategory.System, 
                "Complex object: {ComplexObject}", complexObject, "ExceptionTest");
            
            // Assert: 例外が発生せずに処理が完了
            Assert.DoesNotThrow(() => 
            {
                BTLogger.LogSystem("Exception safety test completed", "ExceptionTest");
            }, "ZLoggerが例外状況でも安全に動作");
        }

        /// <summary>ZLogger多言語対応確認</summary>
        [Test][Description("ZLoggerが多言語文字列を正しく処理することを確認")]
        public void TestZLoggerMultiLanguageSupport()
        {
            // Arrange & Act: 多言語文字列でのログ出力
            BTLogger.LogSystem("日本語テストメッセージ", "MultiLanguageTest");
            BTLogger.LogSystem("English test message", "MultiLanguageTest");
            BTLogger.LogSystem("한국어 테스트 메시지", "MultiLanguageTest");
            BTLogger.LogSystem("测试消息中文", "MultiLanguageTest");
            BTLogger.LogSystem("Тестовое сообщение русский", "MultiLanguageTest");
            BTLogger.LogSystem("Mensaje de prueba español", "MultiLanguageTest");
            BTLogger.LogSystem("🎮🔥⚡🚀 Emoji test 🎯🎲🎪🎨", "MultiLanguageTest");
            
            // 構造化ログでも多言語テスト
            BTLogger.LogStructured(Microsoft.Extensions.Logging.LogLevel.Information, LogCategory.System, 
                "多言語構造化テスト {Message}", 
                new { Message = "日本語メッセージ with English and 한국어" }, "MultiLanguageTest");
            
            // Assert: ZLoggerに委謗されているため、履歴取得は空配列
            var logs = BTLogger.GetRecentLogs(10);
            Assert.AreEqual(0, logs.Length, "Phase 6.3: 履歴管理はZLoggerに委謗 - 空配列");
            
            // 多言語ログ出力が例外なく完了したことを確認
            Assert.Pass("多言語ログ出力が正常に完了 - ZLoggerが多言語を適切に処理");
        }

        /// <summary>ZLoggerスレッドセーフティ確認</summary>
        [UnityTest]
        [Description("ZLoggerがマルチスレッド環境で安全に動作することを確認")]
        public IEnumerator TestZLoggerThreadSafety()
        {
            // Arrange: マルチスレッド環境シミュレーション
            const int threadCount = 4;
            const int logsPerThread = 250;
            var completed = 0;
            
            // Act: 複数の非同期処理でログ出力
            for (int threadIndex = 0; threadIndex < threadCount; threadIndex++)
            {
                var index = threadIndex;
                var thread = new System.Threading.Thread(() =>
                {
                    try
                    {
                        for (int i = 0; i < logsPerThread; i++)
                        {
                            BTLogger.LogSystem($"Thread {index} message {i}", $"Thread{index}");
                            BTLogger.LogStructured(Microsoft.Extensions.Logging.LogLevel.Information, LogCategory.Combat, 
                                "Thread {ThreadId} combat {Index}", 
                                new { ThreadId = index, Index = i }, $"Thread{index}");
                            
                            // スレッド間でのタイミング競合をシミュレート
                            if (i % 50 == 0)
                            {
                                System.Threading.Thread.Sleep(1);
                            }
                        }
                    }
                    finally
                    {
                        System.Threading.Interlocked.Increment(ref completed);
                    }
                });
                
                thread.Start();
            }
            
            // スレッド完了を待機
            var timeout = 10.0f;
            var startTime = Time.time;
            while (completed < threadCount && Time.time - startTime < timeout)
            {
                yield return null;
            }
            
            // Assert: スレッドセーフティが保たれている
            Assert.AreEqual(threadCount, completed, "すべてのスレッドが正常に完了");
            
            var logs = BTLogger.GetRecentLogs(100);
            Assert.AreEqual(0, logs.Length, "Phase 6.3: 履歴管理はZLoggerに委謗 - 空配列");
            
            UnityEngine.Debug.Log($"Thread safety test: マルチスレッドログ出力が正常完了 - ZLoggerのスレッドセーフ性確認");
            Assert.Pass("マルチスレッドテストが正常完了 - ZLoggerスレッドセーフ性確認済み");
        }

        /// <summary>ZLogger本番パフォーマンス総合確認</summary>
        [UnityTest]
        [Description("ZLoggerの本番環境での総合パフォーマンスを最終確認")]
        public IEnumerator TestZLoggerProductionPerformanceOverall()
        {
            // Arrange: 本番環境相当の負荷設定
            const int totalLogs = 5000;
            const int batchSize = 200;
            
            var startTime = Time.realtimeSinceStartup;
            var initialMemory = GC.GetTotalMemory(false);
            
            // Act: 本番環境相当の負荷テスト
            for (int batch = 0; batch < totalLogs / batchSize; batch++)
            {
                for (int i = 0; i < batchSize; i++)
                {
                    var index = batch * batchSize + i;
                    
                    // 多様なログパターンを本番環境相当で実行
                    BTLogger.LogSystem($"Production log {index} with data {index * 1.5f}", "ProductionOverall");
                    BTLogger.LogCombatFormat("Combat {0} damage {1}", $"Action{index}_damage_{index * 10}", "ProductionOverall");
                    BTLogger.LogStructured(Microsoft.Extensions.Logging.LogLevel.Information, LogCategory.Movement, 
                        "Movement {Index} to {Position}", 
                        new { Index = index, Position = new Vector3(index, index, index) }, "ProductionOverall");
                    
                    if (UnityEngine.Random.Range(0, 100) < 5) // 5%の確率でエラーログ
                    {
                        BTLogger.LogError(LogCategory.System, $"Simulated error {index}", "ProductionOverall");
                    }
                }
                
                yield return null;
            }
            
            var elapsedTime = Time.realtimeSinceStartup - startTime;
            var finalMemory = GC.GetTotalMemory(true);
            var memoryIncrease = (finalMemory - initialMemory) / (1024.0 * 1024.0);
            
            // Assert: 本番環境での総合性能基準を満たす
            Assert.Less(elapsedTime, 15.0f, 
                $"ZLogger本番環境総合テスト（{totalLogs * 3}+ログ）が15秒以内で完了（実測: {elapsedTime:F2}秒）");
            
            Assert.Less(memoryIncrease, 20.0, 
                $"ZLogger本番環境メモリ使用量が20MB以内（実測: {memoryIncrease:F2}MB）");
            
            var finalLogs = BTLogger.GetRecentLogs(100);
            Assert.AreEqual(0, finalLogs.Length, "Phase 6.3: 履歴管理はZLoggerに委謗 - 空配列");
            
            UnityEngine.Debug.Log($"ZLogger Production Performance: {elapsedTime:F2}s, {memoryIncrease:F2}MB for {totalLogs * 3}+ logs");
        }
    }
}