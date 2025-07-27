using NUnit.Framework;
using UnityEngine;
using ArcBT.TagSystem;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;

namespace ArcBT.Tests
{
    public class TagCachePerformanceTests
    {
        List<GameObject> testObjects;
        List<GameplayTagComponent> components;
        const int SMALL_OBJECT_COUNT = 100;
        const int MEDIUM_OBJECT_COUNT = 500;
        const int LARGE_OBJECT_COUNT = 1000;

        [SetUp]
        public void Setup()
        {
            testObjects = new List<GameObject>();
            components = new List<GameplayTagComponent>();
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

        void CreateTestObjects(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var obj = new GameObject($"TestObject_{i}");
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

        [Test]
        public void Cache_SmallScale_SearchPerformance()
        {
            // Arrange
            CreateTestObjects(SMALL_OBJECT_COUNT);
            var searchTag = new GameplayTag("Character.Player");

            // Act & Measure
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < 100; i++)
            {
                var found = GameplayTagManager.FindGameObjectsWithTag(searchTag);
            }
            
            stopwatch.Stop();

            // Assert
            UnityEngine.Debug.Log($"Small scale search (100 objects, 100 searches): {stopwatch.ElapsedMilliseconds}ms");
            Assert.Less(stopwatch.ElapsedMilliseconds, 100, "小規模検索は100ms以内であるべき");
        }

        [Test]
        public void Cache_MediumScale_SearchPerformance()
        {
            // Arrange
            CreateTestObjects(MEDIUM_OBJECT_COUNT);
            var searchTag = new GameplayTag("Character.Enemy");

            // Act & Measure
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < 50; i++)
            {
                var found = GameplayTagManager.FindGameObjectsWithTag(searchTag);
            }
            
            stopwatch.Stop();

            // Assert
            UnityEngine.Debug.Log($"Medium scale search (500 objects, 50 searches): {stopwatch.ElapsedMilliseconds}ms");
            Assert.Less(stopwatch.ElapsedMilliseconds, 200, "中規模検索は200ms以内であるべき");
        }

        [Test]
        public void Cache_LargeScale_SearchPerformance()
        {
            // Arrange
            CreateTestObjects(LARGE_OBJECT_COUNT);
            var searchTag = new GameplayTag("Character");

            // Act & Measure - First search (cache miss)
            var stopwatch1 = Stopwatch.StartNew();
            var firstResult = GameplayTagManager.FindGameObjectsWithTag(searchTag);
            stopwatch1.Stop();

            // Act & Measure - Second search (cache hit)
            var stopwatch2 = Stopwatch.StartNew();
            var secondResult = GameplayTagManager.FindGameObjectsWithTag(searchTag);
            stopwatch2.Stop();

            // Assert
            UnityEngine.Debug.Log($"Large scale first search (1000 objects): {stopwatch1.ElapsedMilliseconds}ms");
            UnityEngine.Debug.Log($"Large scale cached search (1000 objects): {stopwatch2.ElapsedMilliseconds}ms");
            
            Assert.Less(stopwatch1.ElapsedMilliseconds, 500, "大規模初回検索は500ms以内であるべき");
            Assert.Less(stopwatch2.ElapsedMilliseconds, 50, "大規模キャッシュ検索は50ms以内であるべき");
            Assert.AreEqual(firstResult.Length, secondResult.Length, "キャッシュ結果は同じであるべき");
        }

        [Test]
        public void Cache_HierarchicalSearch_Performance()
        {
            // Arrange
            CreateTestObjects(LARGE_OBJECT_COUNT);
            var hierarchyTag = new GameplayTag("Character");

            // Act & Measure - Hierarchical search
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < 10; i++)
            {
                var found = GameplayTagManager.FindGameObjectsWithTagHierarchy(hierarchyTag);
            }
            
            stopwatch.Stop();

            // Assert
            UnityEngine.Debug.Log($"Hierarchical search performance (1000 objects, 10 searches): {stopwatch.ElapsedMilliseconds}ms");
            Assert.Less(stopwatch.ElapsedMilliseconds, 300, "階層検索は300ms以内であるべき");
        }

        [Test]
        public void Cache_TagModification_Performance()
        {
            // Arrange
            CreateTestObjects(MEDIUM_OBJECT_COUNT);
            var newTag = new GameplayTag("State.Combat");

            // Act & Measure - Tag modification
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < components.Count; i += 10) // 10個おきにタグ追加
            {
                components[i].AddTag(newTag);
            }
            
            stopwatch.Stop();

            // Assert
            UnityEngine.Debug.Log($"Tag modification performance (50 modifications): {stopwatch.ElapsedMilliseconds}ms");
            Assert.Less(stopwatch.ElapsedMilliseconds, 200, "タグ変更は200ms以内であるべき");
        }

        [Test]
        public void Cache_ComponentRegistration_Performance()
        {
            // Act & Measure - Component registration
            var stopwatch = Stopwatch.StartNew();
            
            var tempObjects = new List<GameObject>();
            for (int i = 0; i < MEDIUM_OBJECT_COUNT; i++)
            {
                var obj = new GameObject($"TempObject_{i}");
                obj.AddComponent<GameplayTagComponent>(); // 自動登録される
                tempObjects.Add(obj);
            }
            
            stopwatch.Stop();

            // Assert
            UnityEngine.Debug.Log($"Component registration performance (500 registrations): {stopwatch.ElapsedMilliseconds}ms");
            Assert.Less(stopwatch.ElapsedMilliseconds, 300, "コンポーネント登録は300ms以内であるべき");

            // Cleanup
            foreach (var obj in tempObjects)
            {
                UnityEngine.Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void Cache_GetTagComponent_Performance()
        {
            // Arrange
            CreateTestObjects(LARGE_OBJECT_COUNT);

            // Act & Measure - GetTagComponent calls
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < 1000; i++)
            {
                var randomObject = testObjects[i % testObjects.Count];
                var component = GameplayTagManager.GetTagComponent(randomObject);
                Assert.IsNotNull(component);
            }
            
            stopwatch.Stop();

            // Assert
            UnityEngine.Debug.Log($"GetTagComponent performance (1000 calls): {stopwatch.ElapsedMilliseconds}ms");
            Assert.Less(stopwatch.ElapsedMilliseconds, 100, "GetTagComponent呼び出しは100ms以内であるべき");
        }

        [Test]
        public void Cache_MultipleTagSearch_Performance()
        {
            // Arrange - 現実的なシナリオ：中規模ゲーム環境
            CreateTestObjects(MEDIUM_OBJECT_COUNT); // 500オブジェクト
            var searchTags = new GameplayTagContainer(
                new GameplayTag("Character.Player"),
                new GameplayTag("Character.Enemy")
            );

            const int searchCycles = 100;
            const int searchesPerCycle = 2; // FindAny + FindAll
            const int totalSearches = searchCycles * searchesPerCycle; // 200回

            // アロケーション測定開始
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var startMemory = GC.GetTotalMemory(false);

            // Act & Measure - 大規模ゲームでの検索パターン
            var stopwatch = Stopwatch.StartNew();
            
            // 大規模ゲーム：RTSやMMOでの同時検索（プール版）
            for (int i = 0; i < searchCycles; i++)
            {
                using var foundAny = GameplayTagManager.FindGameObjectsWithAnyTags(searchTags);
                using var foundAll = GameplayTagManager.FindGameObjectsWithAllTags(searchTags);
                // usingで自動的にプールに返却される
            }
            
            stopwatch.Stop();

            // アロケーション測定終了
            var endMemory = GC.GetTotalMemory(false);
            var allocatedBytes = endMemory - startMemory;

            // Assert
            UnityEngine.Debug.Log($"Multiple tag search performance: {totalSearches} searches on {MEDIUM_OBJECT_COUNT} objects = {stopwatch.ElapsedMilliseconds}ms");
            UnityEngine.Debug.Log($"Average per search: {(float)stopwatch.ElapsedMilliseconds / totalSearches:F2}ms");
            UnityEngine.Debug.Log($"Total memory allocated: {allocatedBytes} bytes ({allocatedBytes / 1024.0:F1} KB)");
            UnityEngine.Debug.Log($"Memory per search: {(float)allocatedBytes / totalSearches:F1} bytes");
            
            Assert.Less(stopwatch.ElapsedMilliseconds, 400, $"大規模ゲームでの複数タグ検索({totalSearches}回)は400ms以内であるべき");
            Assert.Less(allocatedBytes, 1024 * 1000, $"アロケーションは1000KB以内であるべき (実際: {allocatedBytes / 1024.0:F1}KB)");
        }

        [Test]
        public void Cache_ConcurrentAccess_Performance()
        {
            // Arrange
            CreateTestObjects(LARGE_OBJECT_COUNT);
            var searchTags = new[]
            {
                new GameplayTag("Character.Player"),
                new GameplayTag("Character.Enemy"),
                new GameplayTag("Character.Enemy.Boss"),
                new GameplayTag("Object.Item")
            };

            // Act & Measure - Concurrent-like access pattern
            var stopwatch = Stopwatch.StartNew();
            
            for (int iteration = 0; iteration < 20; iteration++)
            {
                foreach (var tag in searchTags)
                {
                    var found = GameplayTagManager.FindGameObjectsWithTag(tag);
                    var hierarchyFound = GameplayTagManager.FindGameObjectsWithTagHierarchy(tag);
                }
            }
            
            stopwatch.Stop();

            // Assert
            UnityEngine.Debug.Log($"Concurrent access pattern (160 operations): {stopwatch.ElapsedMilliseconds}ms");
            Assert.Less(stopwatch.ElapsedMilliseconds, 400, "並行アクセスパターンは400ms以内であるべき");
        }

        [Test]
        public void Cache_MemoryUsage_Validation()
        {
            // Arrange
            CreateTestObjects(LARGE_OBJECT_COUNT);

            // Act - Force cache population
            var searchTags = new[]
            {
                new GameplayTag("Character"),
                new GameplayTag("Character.Player"),
                new GameplayTag("Character.Enemy"),
                new GameplayTag("Object")
            };

            foreach (var tag in searchTags)
            {
                GameplayTagManager.FindGameObjectsWithTag(tag);
                GameplayTagManager.FindGameObjectsWithTagHierarchy(tag);
            }

            // Memory measurement (間接的)
            var initialMemory = GC.GetTotalMemory(false);
            
            // Act - Additional cache operations
            for (int i = 0; i < 100; i++)
            {
                foreach (var tag in searchTags)
                {
                    GameplayTagManager.FindGameObjectsWithTag(tag);
                }
            }

            var finalMemory = GC.GetTotalMemory(false);
            var memoryIncrease = finalMemory - initialMemory;

            // Assert
            UnityEngine.Debug.Log($"Memory increase during cache operations: {memoryIncrease} bytes");
            Assert.Less(memoryIncrease, 1024 * 1024, "メモリ増加は1MB以内であるべき"); // 1MB未満
        }

        [Test]
        public void Cache_InvalidationPerformance_OnTagChange()
        {
            // Arrange
            CreateTestObjects(MEDIUM_OBJECT_COUNT);
            
            // Pre-populate cache
            GameplayTagManager.FindGameObjectsWithTag(new GameplayTag("Character.Player"));
            GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("Character"));

            // Act & Measure - Cache invalidation
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < 50; i++)
            {
                var component = components[i];
                component.AddTag(new GameplayTag("State.NewState"));
                component.RemoveTag(new GameplayTag("State.NewState"));
            }
            
            stopwatch.Stop();

            // Assert
            UnityEngine.Debug.Log($"Cache invalidation performance (100 operations): {stopwatch.ElapsedMilliseconds}ms");
            Assert.Less(stopwatch.ElapsedMilliseconds, 250, "キャッシュ無効化は250ms以内であるべき");
        }
    }
}