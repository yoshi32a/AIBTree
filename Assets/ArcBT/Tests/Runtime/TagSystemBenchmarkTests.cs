using NUnit.Framework;
using UnityEngine;
using ArcBT.TagSystem;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace ArcBT.Tests
{
    public class TagSystemBenchmarkTests
    {
        List<GameObject> testObjects;
        List<GameplayTagComponent> components;
        const int BENCHMARK_OBJECT_COUNT = 500;
        const int BENCHMARK_SEARCH_COUNT = 100;

        [SetUp]
        public void Setup()
        {
            testObjects = new List<GameObject>();
            components = new List<GameplayTagComponent>();
            CreateBenchmarkObjects();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in testObjects)
            {
                if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            }
            testObjects.Clear();
            components.Clear();
        }

        void CreateBenchmarkObjects()
        {
            for (int i = 0; i < BENCHMARK_OBJECT_COUNT; i++)
            {
                var obj = new GameObject($"BenchmarkObject_{i}");
                var component = obj.AddComponent<GameplayTagComponent>();
                
                // 様々なタグパターンを設定
                switch (i % 4)
                {
                    case 0:
                        component.AddTag(new GameplayTag("Character.Player"));
                        break;
                    case 1:
                        component.AddTag(new GameplayTag("Character.Enemy"));
                        break;
                    case 2:
                        component.AddTag(new GameplayTag("Character.Enemy.Boss"));
                        break;
                    case 3:
                        component.AddTag(new GameplayTag("Object.Item"));
                        break;
                }

                testObjects.Add(obj);
                components.Add(component);
            }
        }

        struct BenchmarkResult
        {
            public long TimeMs;
            public long AllocatedBytes;
            public int SearchCount;
            public string Description;

            public float TimePerSearch => (float)TimeMs / SearchCount;
            public float AllocPerSearch => (float)AllocatedBytes / SearchCount;
        }

        BenchmarkResult MeasureMultipleTagSearch(string description)
        {
            var searchTags = new GameplayTagContainer(
                new GameplayTag("Character.Player"),
                new GameplayTag("Character.Enemy")
            );

            // アロケーション測定開始
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var startMemory = GC.GetTotalMemory(false);

            // 時間測定開始
            var stopwatch = Stopwatch.StartNew();
            
            // ベンチマーク実行（プール版）
            for (int i = 0; i < BENCHMARK_SEARCH_COUNT; i++)
            {
                using var foundAny = GameplayTagManager.FindGameObjectsWithAnyTags(searchTags);
                using var foundAll = GameplayTagManager.FindGameObjectsWithAllTags(searchTags);
                // usingで自動的にプールに返却される
            }
            
            stopwatch.Stop();

            // アロケーション測定終了
            var endMemory = GC.GetTotalMemory(false);
            var allocatedBytes = endMemory - startMemory;

            return new BenchmarkResult
            {
                TimeMs = stopwatch.ElapsedMilliseconds,
                AllocatedBytes = allocatedBytes,
                SearchCount = BENCHMARK_SEARCH_COUNT * 2, // FindAny + FindAll
                Description = description
            };
        }

        void PrintBenchmarkResult(BenchmarkResult result)
        {
            UnityEngine.Debug.Log($"=== {result.Description} ===");
            UnityEngine.Debug.Log($"総時間: {result.TimeMs}ms");
            UnityEngine.Debug.Log($"総アロケーション: {result.AllocatedBytes} bytes ({result.AllocatedBytes / 1024.0:F1} KB)");
            UnityEngine.Debug.Log($"検索回数: {result.SearchCount}回");
            UnityEngine.Debug.Log($"1検索あたり時間: {result.TimePerSearch:F2}ms");
            UnityEngine.Debug.Log($"1検索あたりアロケーション: {result.AllocPerSearch:F1} bytes");
            UnityEngine.Debug.Log("");
        }

        void CompareBenchmarkResults(BenchmarkResult before, BenchmarkResult after)
        {
            var timeImprovement = ((float)before.TimeMs - after.TimeMs) / before.TimeMs * 100;
            var allocImprovement = ((float)before.AllocatedBytes - after.AllocatedBytes) / before.AllocatedBytes * 100;

            UnityEngine.Debug.Log("=== 改善結果比較 ===");
            UnityEngine.Debug.Log($"時間: {before.TimeMs}ms → {after.TimeMs}ms ({timeImprovement:+F1}% 改善)");
            UnityEngine.Debug.Log($"アロケーション: {before.AllocatedBytes / 1024.0:F1}KB → {after.AllocatedBytes / 1024.0:F1}KB ({allocImprovement:+F1}% 改善)");
            UnityEngine.Debug.Log($"1検索時間: {before.TimePerSearch:F2}ms → {after.TimePerSearch:F2}ms");
            UnityEngine.Debug.Log($"1検索アロケーション: {before.AllocPerSearch:F1}bytes → {after.AllocPerSearch:F1}bytes");
        }

        [Test]
        public void Benchmark_AllTagSearchTypes_Combined()
        {
            UnityEngine.Debug.Log("=== 統合ベンチマーク開始 ===");
            
            // 1. 複数タグ検索ベンチマーク
            var multipleResult = MeasureMultipleTagSearch("複数タグ検索");
            PrintBenchmarkResult(multipleResult);
            
            // 2. 単一タグ検索ベンチマーク
            var singleResult = MeasureSingleTagSearch("単一タグ検索");
            PrintBenchmarkResult(singleResult);
            
            // 3. 階層検索ベンチマーク
            var hierarchicalResult = MeasureHierarchicalSearch("階層検索");
            PrintBenchmarkResult(hierarchicalResult);
            
            // 全体サマリー
            UnityEngine.Debug.Log("=== 全体サマリー ===");
            UnityEngine.Debug.Log($"複数タグ: {multipleResult.TimePerSearch:F2}ms/検索, {multipleResult.AllocPerSearch:F1}bytes/検索");
            UnityEngine.Debug.Log($"単一タグ: {singleResult.TimePerSearch:F2}ms/検索, {singleResult.AllocPerSearch:F1}bytes/検索");
            UnityEngine.Debug.Log($"階層検索: {hierarchicalResult.TimePerSearch:F2}ms/検索, {hierarchicalResult.AllocPerSearch:F1}bytes/検索");
            
            // ベースライン記録
            UnityEngine.Debug.Log($"[BASELINE] Multiple: {multipleResult.TimeMs}ms, {multipleResult.AllocatedBytes}bytes");
            UnityEngine.Debug.Log($"[BASELINE] Single: {singleResult.TimeMs}ms, {singleResult.AllocatedBytes}bytes");
            UnityEngine.Debug.Log($"[BASELINE] Hierarchical: {hierarchicalResult.TimeMs}ms, {hierarchicalResult.AllocatedBytes}bytes");

            // 基本的なAssert
            Assert.IsTrue(multipleResult.TimeMs > 0, "複数タグ検索の時間測定が正常に動作すること");
            Assert.IsTrue(singleResult.TimeMs >= 0, "単一タグ検索の時間測定が正常に動作すること");
            Assert.IsTrue(hierarchicalResult.TimeMs >= 0, "階層検索の時間測定が正常に動作すること");
        }

        // 改善版のテスト用（実装後に有効化）
        /*
        [Test]
        public void Benchmark_ImprovedImplementation_MultipleTagSearch()
        {
            // 改善前の測定
            var beforeResult = MeasureMultipleTagSearch("改善前");
            
            // TODO: ここで改善された実装に切り替える
            // 例: GameplayTagManager.EnableOptimizedSearch(true);
            
            // 改善後の測定
            var afterResult = MeasureMultipleTagSearch("改善後");
            
            // 結果表示と比較
            PrintBenchmarkResult(beforeResult);
            PrintBenchmarkResult(afterResult);
            CompareBenchmarkResults(beforeResult, afterResult);
            
            // 改善の検証
            Assert.Less(afterResult.TimeMs, beforeResult.TimeMs, "時間が改善されていること");
            Assert.Less(afterResult.AllocatedBytes, beforeResult.AllocatedBytes, "アロケーションが改善されていること");
        }
        */

        BenchmarkResult MeasureSingleTagSearch(string description)
        {
            var searchTag = new GameplayTag("Character.Player");

            // アロケーション測定
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var startMemory = GC.GetTotalMemory(false);

            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < BENCHMARK_SEARCH_COUNT; i++)
            {
                using var found = GameplayTagManager.FindGameObjectsWithTag(searchTag);
            }
            
            stopwatch.Stop();

            var endMemory = GC.GetTotalMemory(false);
            var allocatedBytes = endMemory - startMemory;

            return new BenchmarkResult
            {
                TimeMs = stopwatch.ElapsedMilliseconds,
                AllocatedBytes = allocatedBytes,
                SearchCount = BENCHMARK_SEARCH_COUNT,
                Description = description
            };
        }

        BenchmarkResult MeasureHierarchicalSearch(string description)
        {
            var searchTag = new GameplayTag("Character");

            // アロケーション測定
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var startMemory = GC.GetTotalMemory(false);

            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < BENCHMARK_SEARCH_COUNT; i++)
            {
                using var found = GameplayTagManager.FindGameObjectsWithTagHierarchy(searchTag);
            }
            
            stopwatch.Stop();

            var endMemory = GC.GetTotalMemory(false);
            var allocatedBytes = endMemory - startMemory;

            return new BenchmarkResult
            {
                TimeMs = stopwatch.ElapsedMilliseconds,
                AllocatedBytes = allocatedBytes,
                SearchCount = BENCHMARK_SEARCH_COUNT,
                Description = description
            };
        }
    }
}