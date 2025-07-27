using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Profiling;
using ArcBT.TagSystem;
using UnityEditor;

namespace ArcBT.Tests
{
    /// <summary>
    /// TagSystem詳細プロファイリング用MonoBehaviour
    /// </summary>
    public class TagSystemDeepProfiler : MonoBehaviour
    {
        [Header("テスト設定")]
        public int objectCount = 500;
        public int searchCycles = 100;
        public bool autoStart = false;
        
        [Header("実行制御")]
        public KeyCode profileKey = KeyCode.P;
        public KeyCode clearKey = KeyCode.C;
        
        List<GameObject> testObjects;
        List<GameplayTagComponent> components;
        GameplayTagContainer searchTags;
        
        [Header("結果表示")]
        [TextArea(10, 20)]
        public string profileResults = "";

        void Start()
        {
            if (autoStart)
            {
                SetupTestObjects();
                StartCoroutine(DelayedProfile());
            }
        }

        System.Collections.IEnumerator DelayedProfile()
        {
            yield return new WaitForSeconds(1f);
            RunDeepProfile();
        }

        void Update()
        {
            // Input Systemに対応するため、キー入力は無効化
            // GUIボタンまたはautoStartを使用してください
        }

        void SetupTestObjects()
        {
            UnityEngine.Debug.Log("=== テストオブジェクト作成開始 ===");
            
            testObjects = new List<GameObject>();
            components = new List<GameplayTagComponent>();
            
            for (int i = 0; i < objectCount; i++)
            {
                var obj = new GameObject($"ProfileTestObject_{i}");
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
            
            searchTags = new GameplayTagContainer(
                new GameplayTag("Character.Player"),
                new GameplayTag("Character.Enemy")
            );
            
            UnityEngine.Debug.Log($"=== テストオブジェクト作成完了: {objectCount}個 ===");
        }

        void ClearTestObjects()
        {
            if (testObjects != null)
            {
                foreach (var obj in testObjects)
                {
                    if (obj != null) DestroyImmediate(obj);
                }
                testObjects.Clear();
                components.Clear();
            }
            UnityEngine.Debug.Log("=== テストオブジェクト削除完了 ===");
        }

        void RunDeepProfile()
        {
            if (testObjects == null) return;
            
            UnityEngine.Debug.Log("=== 詳細プロファイリング開始 ===");
            var results = new System.Text.StringBuilder();
            results.AppendLine("=== TagSystem詳細プロファイリング結果 ===");
            results.AppendLine($"オブジェクト数: {objectCount}");
            results.AppendLine($"検索サイクル数: {searchCycles}");
            results.AppendLine($"総検索回数: {searchCycles * 2} (FindAny + FindAll)");
            results.AppendLine();

            // 1. 単体操作のプロファイリング
            ProfileSingleOperations(results);
            
            // 2. ループ処理のプロファイリング
            ProfileLoopOperations(results);
            
            // 3. メモリアロケーション詳細
            ProfileMemoryAllocations(results);
            
            // 4. プール統計
            ProfilePoolStatistics(results);

            profileResults = results.ToString();
            UnityEngine.Debug.Log(profileResults);
        }

        void ProfileSingleOperations(System.Text.StringBuilder results)
        {
            results.AppendLine("### 1. 単体操作プロファイリング ###");
            
            // 単一のHashSet作成
            Profiler.BeginSample("HashSet Creation");
            var hashSet = GameObjectHashSetPool.Get();
            Profiler.EndSample();
            GameObjectHashSetPool.Return(hashSet);
            
            // 単一のList作成
            Profiler.BeginSample("List Creation");
            var list = GameObjectListPool.Get();
            Profiler.EndSample();
            GameObjectListPool.Return(list);
            
            // 単一のArray作成
            Profiler.BeginSample("Array Creation");
            var array = GameObjectArrayPool.Get();
            Profiler.EndSample();
            array.Dispose();
            
            // 単一タグ検索
            Profiler.BeginSample("Single Tag Search");
            using var singleResult = GameplayTagManager.FindGameObjectsWithTag(new GameplayTag("Character.Player"));
            Profiler.EndSample();
            
            results.AppendLine("単体操作完了 - Unity Profilerで詳細確認");
            results.AppendLine();
        }

        void ProfileLoopOperations(System.Text.StringBuilder results)
        {
            results.AppendLine("### 2. ループ処理プロファイリング ###");
            
            const int iterations = 50;
            
            // using var の繰り返し
            Profiler.BeginSample("Repeated using var");
            for (int i = 0; i < iterations; i++)
            {
                using var result = GameplayTagManager.FindGameObjectsWithTag(new GameplayTag("Character.Player"));
            }
            Profiler.EndSample();
            
            // プール取得/返却の繰り返し
            Profiler.BeginSample("Repeated Pool Get/Return");
            for (int i = 0; i < iterations; i++)
            {
                var hashSet = GameObjectHashSetPool.Get();
                GameObjectHashSetPool.Return(hashSet);
            }
            Profiler.EndSample();
            
            // HashSet操作の繰り返し
            Profiler.BeginSample("Repeated HashSet Operations");
            for (int i = 0; i < iterations; i++)
            {
                var hashSet = GameObjectHashSetPool.Get();
                for (int j = 0; j < 10; j++)
                {
                    hashSet.Add(testObjects[j % testObjects.Count]);
                }
                GameObjectHashSetPool.Return(hashSet);
            }
            Profiler.EndSample();
            
            results.AppendLine($"ループ処理完了 ({iterations}回) - Unity Profilerで詳細確認");
            results.AppendLine();
        }

        void ProfileMemoryAllocations(System.Text.StringBuilder results)
        {
            results.AppendLine("### 3. メモリアロケーション詳細 ###");
            
            // GC強制実行
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var startMemory = GC.GetTotalMemory(false);
            
            Profiler.BeginSample("Memory Allocation Test");
            
            // 実際のテストと同じ処理
            for (int i = 0; i < 20; i++) // 検索回数を減らして詳細分析
            {
                Profiler.BeginSample("FindAny + FindAll Pair");
                using var foundAny = GameplayTagManager.FindGameObjectsWithAnyTags(searchTags);
                using var foundAll = GameplayTagManager.FindGameObjectsWithAllTags(searchTags);
                Profiler.EndSample();
            }
            
            Profiler.EndSample();
            
            var endMemory = GC.GetTotalMemory(false);
            var allocatedBytes = endMemory - startMemory;
            
            results.AppendLine($"20回検索でのアロケーション: {allocatedBytes} bytes ({allocatedBytes / 1024.0:F1} KB)");
            results.AppendLine($"1検索あたり: {(float)allocatedBytes / 40:F1} bytes");
            results.AppendLine();
        }

        void ProfilePoolStatistics(System.Text.StringBuilder results)
        {
            results.AppendLine("### 4. プール統計 ###");
            
            // プール統計を取得（デバッグ用メソッドがあれば）
            try
            {
                var arrayStats = GameObjectArrayPool.GetPoolStats();
                results.AppendLine($"Array Pool: {arrayStats}");
            }
            catch
            {
                results.AppendLine("Array Pool: 統計取得不可");
            }
            
            results.AppendLine();
            results.AppendLine("=== 詳細プロファイリング完了 ===");
            results.AppendLine("Unity Profiler Window で 'Deep Profile' を有効にして詳細を確認してください");
        }

        void OnDestroy()
        {
            ClearTestObjects();
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("TagSystem Deep Profiler", EditorGUIUtility.isProSkin ? GUI.skin.box : GUI.skin.label);
            
            if (testObjects != null)
            {
                GUILayout.Label($"Objects: {testObjects.Count}");
            }
            else
            {
                GUILayout.Label("Objects: Not Created");
            }
            
            if (GUILayout.Button("Setup Test Objects"))
            {
                SetupTestObjects();
            }
            
            if (GUILayout.Button("Run Profile Now"))
            {
                if (testObjects == null) SetupTestObjects();
                RunDeepProfile();
            }
            
            if (GUILayout.Button("Clear Objects"))
            {
                ClearTestObjects();
            }
            
            GUILayout.Space(10);
            autoStart = GUILayout.Toggle(autoStart, "Auto Start on Play");
            
            GUILayout.EndArea();
        }
    }
}
