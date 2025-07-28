using NUnit.Framework;
using System.Diagnostics;
using System.Text;
using ArcBT.Parser;
using UnityEngine.Profiling;
using ArcBT.Logger;

namespace ArcBT.Tests
{
#if ENABLE_PERFORMANCE_TESTS || UNITY_EDITOR
    [TestFixture]
    [Category("Heavy")]  // 重いパフォーマンステスト
    public class BTParserPerformanceTests
    {
        const int WARMUP_RUNS = 5;
        const int TEST_RUNS = 100;
        
        // テスト用のBTコンテンツ
        readonly string smallBTContent = @"
tree TestTree {
    Sequence root {
        Condition HasTarget {}
        Action MoveToTarget {
            speed: 5.0
            range: 2.0
        }
        Action Attack {}
    }
}";

        readonly string mediumBTContent = @"
tree ComplexTree {
    Selector main {
        Sequence movement {
            Condition HasSharedEnemyInfo {}
            Action MoveToPosition {
                x: 10.0
                y: 0.0
                z: 5.0
                speed: 3.0
            }
        }
        Sequence scan {
            Action ScanEnvironment {
                range: 10.0
            }
            Action Wait {
                duration: 2.0
            }
        }
        Action RandomWander {
            speed: 2.0
            range: 5.0
        }
    }
}";

        string largeBTContent;

        [OneTimeSetUp]
        public void Setup()
        {
            // 大きなBTコンテンツを生成
            var sb = new StringBuilder();
            sb.AppendLine("tree LargeTree {");
            sb.AppendLine("    Selector root {");
            
            for (int i = 0; i < 50; i++)
            {
                sb.AppendLine($"        Sequence sequence{i} {{");
                sb.AppendLine($"            Condition HasSharedEnemyInfo {{}}");
                
                for (int j = 0; j < 5; j++)
                {
                    var actionType = j % 3 == 0 ? "MoveToPosition" : j % 3 == 1 ? "ScanEnvironment" : "RandomWander";
                    sb.AppendLine($"            Action {actionType} {{");
                    
                    if (actionType == "MoveToPosition")
                    {
                        sb.AppendLine($"                x: {i * 2.0f}");
                        sb.AppendLine($"                y: 0.0");
                        sb.AppendLine($"                z: {j * 3.0f}");
                        sb.AppendLine($"                speed: {3 + i * 0.1f}");
                    }
                    else if (actionType == "ScanEnvironment")
                    {
                        sb.AppendLine($"                range: {10 + i * 0.5f}");
                    }
                    else
                    {
                        sb.AppendLine($"                speed: {2 + j * 0.2f}");
                        sb.AppendLine($"                range: {5 + i * 0.1f}");
                    }
                    
                    sb.AppendLine("            }}");
                }
                
                sb.AppendLine("        }}");
            }
            
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            largeBTContent = sb.ToString();
        }

        [Test][Description("小さなBTファイルの解析性能をベンチマークテスト（1000文字以下のファイル）")]
        [Category("Heavy")]
        public void MeasureParserPerformance_SmallFile()
        {
            MeasurePerformance("Small BT", smallBTContent);
        }

        [Test][Description("中規模BTファイルの解析性能をベンチマークテスト（1000-2000文字程度のファイル）")]
        [Category("Heavy")]
        public void MeasureParserPerformance_MediumFile()
        {
            MeasurePerformance("Medium BT", mediumBTContent);
        }

        [Test][Description("大規模BTファイルの解析性能をベンチマークテスト（複雑な階層構造を持つファイル）")]
        [Category("Heavy")]
        public void MeasureParserPerformance_LargeFile()
        {
            MeasurePerformance("Large BT", largeBTContent);
        }

        void MeasurePerformance(string testName, string content)
        {
            var parser = new BTParser();
            
            // テスト中はログを抑制（Errorレベルに設定でログを最小化）
            var originalLogLevel = BTLogger.GetCurrentLogLevel();
            var originalParserCategory = BTLogger.IsCategoryEnabled(LogCategory.Parser);
            var originalSystemCategory = BTLogger.IsCategoryEnabled(LogCategory.System);
            
            BTLogger.SetLogLevel(Microsoft.Extensions.Logging.LogLevel.Error);
            BTLogger.SetCategoryFilter(LogCategory.Parser, false);
            BTLogger.SetCategoryFilter(LogCategory.System, false);
            
            try
            {
                // ウォームアップ
                for (int i = 0; i < WARMUP_RUNS; i++)
                {
                    parser.ParseContent(content);
                }

                // GCの影響を最小化
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.GC.Collect();

                // メモリとGCの初期状態を記録
                long initialMemory = System.GC.GetTotalMemory(false);
                int initialGen0 = System.GC.CollectionCount(0);
                int initialGen1 = System.GC.CollectionCount(1);
                int initialGen2 = System.GC.CollectionCount(2);

                // Profilerサンプラーを使用（Unity固有）
                var sampler = CustomSampler.Create($"BTParser.ParseContent.{testName}");
                
                // 実行時間計測
                var stopwatch = new Stopwatch();
                long totalTicks = 0;
                long minTicks = long.MaxValue;
                long maxTicks = 0;

                for (int i = 0; i < TEST_RUNS; i++)
                {
                    sampler.Begin();
                    stopwatch.Restart();
                    
                    var result = parser.ParseContent(content);
                    
                    stopwatch.Stop();
                    sampler.End();
                    
                    totalTicks += stopwatch.ElapsedTicks;
                    minTicks = System.Math.Min(minTicks, stopwatch.ElapsedTicks);
                    maxTicks = System.Math.Max(maxTicks, stopwatch.ElapsedTicks);
                    
                    Assert.IsNotNull(result, $"Parse failed on iteration {i}");
                }

                // メモリとGCの最終状態
                long finalMemory = System.GC.GetTotalMemory(false);
                int finalGen0 = System.GC.CollectionCount(0);
                int finalGen1 = System.GC.CollectionCount(1);
                int finalGen2 = System.GC.CollectionCount(2);

                // 結果を計算
                double avgMs = (totalTicks / (double)TEST_RUNS) / Stopwatch.Frequency * 1000;
                double minMs = minTicks / (double)Stopwatch.Frequency * 1000;
                double maxMs = maxTicks / (double)Stopwatch.Frequency * 1000;
                
                long totalAllocatedBytes = finalMemory - initialMemory;
                long avgAllocatedBytes = totalAllocatedBytes / TEST_RUNS;
                
                int gen0Collections = finalGen0 - initialGen0;
                int gen1Collections = finalGen1 - initialGen1;
                int gen2Collections = finalGen2 - initialGen2;

                // 結果を出力
                BTLogger.Info($"\n=== {testName} Performance Results ===");
                BTLogger.Info($"Content Size: {content.Length:N0} characters");
                BTLogger.Info($"Iterations: {TEST_RUNS}");
                BTLogger.Info($"\nExecution Time:");
                BTLogger.Info($"  Average: {avgMs:F3} ms");
                BTLogger.Info($"  Min: {minMs:F3} ms");
                BTLogger.Info($"  Max: {maxMs:F3} ms");
                BTLogger.Info($"\nMemory Allocation:");
                BTLogger.Info($"  Total: {totalAllocatedBytes:N0} bytes");
                BTLogger.Info($"  Average per parse: {avgAllocatedBytes:N0} bytes");
                BTLogger.Info($"  Average per character: {avgAllocatedBytes / (double)content.Length:F2} bytes");
                BTLogger.Info($"\nGarbage Collections:");
                BTLogger.Info($"  Gen0: {gen0Collections}");
                BTLogger.Info($"  Gen1: {gen1Collections}");
                BTLogger.Info($"  Gen2: {gen2Collections}");
                
                // パフォーマンス基準をテスト
                Assert.That(avgMs, Is.LessThan(GetExpectedMaxTime(content.Length)), 
                    $"Average parsing time {avgMs:F3}ms exceeds expected maximum for {testName}");
            }
            finally
            {
                // ログレベルとカテゴリフィルターを元に戻す
                BTLogger.SetLogLevel(originalLogLevel);
                BTLogger.SetCategoryFilter(LogCategory.Parser, originalParserCategory);
                BTLogger.SetCategoryFilter(LogCategory.System, originalSystemCategory);
            }
        }

        double GetExpectedMaxTime(int contentLength)
        {
            // 文字数に基づく期待される最大時間（ミリ秒）
            if (contentLength < 500) return 1.0;  // 小さいファイル
            if (contentLength < 2000) return 5.0; // 中程度のファイル
            return 20.0; // 大きいファイル
        }

        [Test][Description("異なるトークン化手法の性能比較テスト（現在の実装とその他手法の比較）")]
        [Category("Heavy")]
        public void CompareTokenizationMethods()
        {
            var parser = new BTParser();
            var content = largeBTContent;
            
            BTLogger.Info("\n=== Tokenization Method Comparison ===");
            
            // 現在の実装をテスト
            MeasureTokenizationOnly("Current Implementation", parser, content);
            
            // ここに代替実装のテストを追加可能
        }

        void MeasureTokenizationOnly(string methodName, BTParser parser, string content)
        {
            // リフレクションを使ってprivateメソッドにアクセス
            var tokenizeMethod = typeof(BTParser).GetMethod("Tokenize", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (tokenizeMethod == null)
            {
                BTLogger.Warning("Tokenize method not found via reflection");
                return;
            }

            // ウォームアップ
            for (int i = 0; i < WARMUP_RUNS; i++)
            {
                tokenizeMethod.Invoke(parser, new object[] { content });
            }

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();

            long initialMemory = System.GC.GetTotalMemory(false);
            var stopwatch = new Stopwatch();
            
            stopwatch.Start();
            for (int i = 0; i < TEST_RUNS; i++)
            {
                var tokens = tokenizeMethod.Invoke(parser, new object[] { content });
            }
            stopwatch.Stop();
            
            long finalMemory = System.GC.GetTotalMemory(false);
            
            double avgMs = stopwatch.ElapsedMilliseconds / (double)TEST_RUNS;
            long avgBytes = (finalMemory - initialMemory) / TEST_RUNS;
            
            BTLogger.Info($"\n{methodName}:");
            BTLogger.Info($"  Average time: {avgMs:F3} ms");
            BTLogger.Info($"  Average allocation: {avgBytes:N0} bytes");
        }
    }
#endif
}
