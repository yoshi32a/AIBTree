using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArcBT.TagSystem
{
    /// <summary>
    /// ゲーム全体のタグシステムを管理するシングルトン
    /// </summary>
    public class GameplayTagManager : MonoBehaviour
    {
        static GameplayTagManager instance;

        [SerializeField] List<GameplayTagAsset> tagAssets = new();

        // タグコンポーネントのキャッシュ（高速アクセス用）
        readonly Dictionary<GameObject, GameplayTagComponent> componentCache = new();

        // タグ別オブジェクトキャッシュ（検索高速化）
        readonly Dictionary<GameplayTag, HashSet<GameObject>> taggedObjectsCache = new();

        // 階層検索キャッシュ（親タグ → 子オブジェクト群）
        readonly Dictionary<GameplayTag, HashSet<GameObject>> hierarchyCache = new();

        // タグ階層構造キャッシュ（親 → 子タグリスト）
        readonly Dictionary<GameplayTag, HashSet<GameplayTag>> tagHierarchy = new();

        // アクティブコンポーネントリスト（FindObjectsByType回避）
        readonly HashSet<GameplayTagComponent> activeComponents = new();

        // オブジェクト → タグリスト逆引きキャッシュ（高速削除用）
        readonly Dictionary<GameObject, HashSet<GameplayTag>> objectToTagsCache = new();

        // イベントリスナー管理（メモリリーク防止）
        readonly Dictionary<GameplayTagComponent, Action<GameplayTagContainer>> listenerMap = new();

        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static GameplayTagManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<GameplayTagManager>();
                    if (instance == null)
                    {
                        var go = new GameObject("GameplayTagManager");
                        instance = go.AddComponent<GameplayTagManager>();
                        DontDestroyOnLoad(go);
                    }
                }

                return instance;
            }
        }

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 指定されたタグを持つGameObjectを検索（プール版）
        /// </summary>
        /// <param name="tag">検索するタグ</param>
        /// <returns>プールされた結果配列（using文で自動返却）</returns>
        public static PooledGameObjectArray FindGameObjectsWithTag(GameplayTag tag)
        {
            return Instance.FindObjectsWithTagPooled(tag);
        }

        /// <summary>
        /// 階層検索：指定されたタグとその子タグを持つすべてのGameObjectを検索（プール版）
        /// 例: "Character.Enemy" → "Character.Enemy", "Character.Enemy.Boss", "Character.Enemy.Minion"
        /// </summary>
        public static PooledGameObjectArray FindGameObjectsWithTagHierarchy(GameplayTag parentTag)
        {
            return Instance.FindObjectsWithTagHierarchyPooled(parentTag);
        }

        /// <summary>
        /// 指定されたタグを持つ最初のGameObjectを検索
        /// </summary>
        public static GameObject FindGameObjectWithTag(GameplayTag tag)
        {
            using var objects = FindGameObjectsWithTag(tag);
            return objects.Count > 0 ? objects[0] : null;
        }

        /// <summary>
        /// 指定されたタグコンテナのいずれかのタグを持つGameObjectを検索（プール版）
        /// </summary>
        /// <param name="tags">検索するタグコンテナ</param>
        /// <returns>プールされた結果配列（using文で自動返却）</returns>
        public static PooledGameObjectArray FindGameObjectsWithAnyTags(GameplayTagContainer tags)
        {
            return Instance.FindObjectsWithAnyTagsPooled(tags);
        }

        /// <summary>
        /// 指定されたタグコンテナのすべてのタグを持つGameObjectを検索（プール版）
        /// </summary>
        /// <param name="tags">検索するタグコンテナ</param>
        /// <returns>プールされた結果配列（using文で自動返却）</returns>
        public static PooledGameObjectArray FindGameObjectsWithAllTags(GameplayTagContainer tags)
        {
            return Instance.FindObjectsWithAllTagsPooled(tags);
        }

        /// <summary>
        /// GameObjectのGameplayTagComponentを取得（キャッシュ付き）
        /// </summary>
        public static GameplayTagComponent GetTagComponent(GameObject go)
        {
            if (go == null) return null;

            if (Instance.componentCache.TryGetValue(go, out var cached))
            {
                if (cached != null) return cached;
                Instance.componentCache.Remove(go);
            }

            var component = go.GetComponent<GameplayTagComponent>();
            if (component != null)
            {
                Instance.componentCache[go] = component;
            }

            return component;
        }

        /// <summary>
        /// GameObjectがタグを持っているかチェック
        /// </summary>
        public static bool HasTag(GameObject go, GameplayTag tag)
        {
            var component = GetTagComponent(go);
            return component != null && component.HasTag(tag);
        }

        /// <summary>
        /// すべてのタグ定義を取得
        /// </summary>
        public List<GameplayTag> GetAllDefinedTags()
        {
            var allTags = new List<GameplayTag>();
            foreach (var asset in tagAssets)
            {
                if (asset != null)
                {
                    allTags.AddRange(asset.GetAllTags());
                }
            }

            return allTags.Distinct().ToList();
        }


        PooledGameObjectArray FindObjectsWithTagPooled(GameplayTag tag)
        {
            var pooled = GameObjectArrayPool.Get();

            if (!tag.IsValid)
            {
                return pooled; // Count = 0
            }

            // キャッシュから高速検索
            if (taggedObjectsCache.TryGetValue(tag, out var cached))
            {
                int count = 0;

                // 無効なオブジェクトを除去しつつ、プール配列に直接コピー
                foreach (var obj in cached)
                {
                    if (obj != null)
                    {
                        if (count >= pooled.Objects.Length)
                        {
                            // 配列サイズが足りない場合は新しいプールを取得
                            pooled.Dispose();
                            pooled = GameObjectArrayPool.Get(cached.Count);
                        }

                        pooled.Objects[count] = obj;
                        count++;
                    }
                }

                pooled.Count = count;

                // キャッシュからnullを削除（必要時のみ）
                if (count != cached.Count)
                {
                    cached.RemoveWhere(go => go == null);
                }

                return pooled;
            }

            // キャッシュミス時：activeComponentsを使用（FindObjectsByType回避）
            var results = new HashSet<GameObject>();

            foreach (var comp in activeComponents)
            {
                if (comp != null && comp.gameObject != null && comp.HasTag(tag))
                {
                    results.Add(comp.gameObject);
                }
            }

            // キャッシュ保存
            taggedObjectsCache[tag] = results;

            // 結果をプール配列にコピー
            if (results.Count > pooled.Objects.Length)
            {
                pooled.Dispose();
                pooled = GameObjectArrayPool.Get(results.Count);
            }

            int resultCount = 0;
            foreach (var obj in results)
            {
                pooled.Objects[resultCount] = obj;
                resultCount++;
            }

            pooled.Count = resultCount;
            return pooled;
        }


        /// <summary>
        /// 階層検索の実装（高速化版）
        /// </summary>
        PooledGameObjectArray FindObjectsWithTagHierarchyPooled(GameplayTag parentTag)
        {
            var pooled = GameObjectArrayPool.Get();

            if (!parentTag.IsValid)
            {
                return pooled; // Count = 0
            }

            // 階層キャッシュから高速検索
            if (hierarchyCache.TryGetValue(parentTag, out var cachedHierarchy))
            {
                int count = 0;

                // 無効なオブジェクトを除去しつつ、プール配列に直接コピー
                foreach (var obj in cachedHierarchy)
                {
                    if (obj != null)
                    {
                        if (count >= pooled.Objects.Length)
                        {
                            // 配列サイズが足りない場合は新しいプールを取得
                            pooled.Dispose();
                            pooled = GameObjectArrayPool.Get(cachedHierarchy.Count);
                        }

                        pooled.Objects[count] = obj;
                        count++;
                    }
                }

                pooled.Count = count;

                // キャッシュからnullを削除（必要時のみ）
                if (count != cachedHierarchy.Count)
                {
                    cachedHierarchy.RemoveWhere(go => go == null);
                }

                return pooled;
            }

            // 階層検索実行
            var results = new HashSet<GameObject>();

            // 1. 完全一致のオブジェクト
            if (taggedObjectsCache.TryGetValue(parentTag, out var exactMatches))
            {
                results.UnionWith(exactMatches);
            }

            // 2. 子タグのオブジェクト（効率的な階層検索）
            if (tagHierarchy.TryGetValue(parentTag, out var childTags))
            {
                foreach (var childTag in childTags)
                {
                    if (taggedObjectsCache.TryGetValue(childTag, out var childObjects))
                    {
                        results.UnionWith(childObjects);
                    }
                }
            }
            else
            {
                // 階層構造キャッシュがない場合は構築
                BuildTagHierarchyFor(parentTag);

                // 再帰的に検索
                foreach (var kvp in taggedObjectsCache)
                {
                    if (kvp.Key.MatchesTag(parentTag))
                    {
                        results.UnionWith(kvp.Value);
                    }
                }
            }

            // 階層キャッシュ保存
            hierarchyCache[parentTag] = results;

            // 結果をプール配列にコピー
            if (results.Count > pooled.Objects.Length)
            {
                pooled.Dispose();
                pooled = GameObjectArrayPool.Get(results.Count);
            }

            int resultCount = 0;
            foreach (var obj in results)
            {
                pooled.Objects[resultCount] = obj;
                resultCount++;
            }

            pooled.Count = resultCount;
            return pooled;
        }

        GameObject[] FindObjectsWithTagHierarchy(GameplayTag parentTag)
        {
            if (!parentTag.IsValid) return Array.Empty<GameObject>();

            // 階層キャッシュから高速検索
            if (hierarchyCache.TryGetValue(parentTag, out var cachedHierarchy))
            {
                cachedHierarchy.RemoveWhere(go => go == null);
                return cachedHierarchy.ToArray();
            }

            // 階層検索実行
            var results = new HashSet<GameObject>();

            // 1. 完全一致のオブジェクト
            if (taggedObjectsCache.TryGetValue(parentTag, out var exactMatches))
            {
                results.UnionWith(exactMatches);
            }

            // 2. 子タグのオブジェクト（効率的な階層検索）
            if (tagHierarchy.TryGetValue(parentTag, out var childTags))
            {
                foreach (var childTag in childTags)
                {
                    if (taggedObjectsCache.TryGetValue(childTag, out var childObjects))
                    {
                        results.UnionWith(childObjects);
                    }
                }
            }
            else
            {
                // 階層構造キャッシュがない場合は構築
                BuildTagHierarchyFor(parentTag);

                // 再帰的に検索
                foreach (var kvp in taggedObjectsCache)
                {
                    if (kvp.Key.MatchesTag(parentTag))
                    {
                        results.UnionWith(kvp.Value);
                    }
                }
            }

            // 階層キャッシュに保存
            hierarchyCache[parentTag] = results;
            return results.ToArray();
        }

        /// <summary>
        /// 特定の親タグの階層構造を構築
        /// </summary>
        void BuildTagHierarchyFor(GameplayTag parentTag)
        {
            var childTags = new HashSet<GameplayTag>();

            foreach (var tag in taggedObjectsCache.Keys)
            {
                if (tag.MatchesTag(parentTag) && tag != parentTag)
                {
                    childTags.Add(tag);
                }
            }

            tagHierarchy[parentTag] = childTags;
        }


        PooledGameObjectArray FindObjectsWithAnyTagsPooled(GameplayTagContainer tags)
        {
            var pooled = GameObjectArrayPool.Get();
            var uniqueObjects = GameObjectHashSetPool.Get();

            // 各タグで最適化済みの単一検索を実行（プール版使用、0アロケーション）
            for (int index = 0; index < tags.Count; index++)
            {
                using var singleResults = FindObjectsWithTagPooled(tags.Tags[index]);
                foreach (var go in singleResults)
                {
                    uniqueObjects.Add(go);
                }
            }

            // 結果が配列サイズを超える場合は新しいプールを取得
            if (uniqueObjects.Count > pooled.Objects.Length)
            {
                pooled.Dispose();
                pooled = GameObjectArrayPool.Get(uniqueObjects.Count);
            }

            // 結果をプール配列にコピー
            int resultCount = 0;
            foreach (var obj in uniqueObjects)
            {
                pooled.Objects[resultCount] = obj;
                resultCount++;
            }

            pooled.Count = resultCount;

            // HashSetをプールに返却
            GameObjectHashSetPool.Return(uniqueObjects);

            return pooled;
        }


        PooledGameObjectArray FindObjectsWithAllTagsPooled(GameplayTagContainer tags)
        {
            var pooled = GameObjectArrayPool.Get();

            if (tags.Count == 0)
            {
                return pooled; // Count = 0
            }

            // 最初のタグで候補を取得（プール版使用）
            using var candidates = FindObjectsWithTagPooled(tags.Tags.First());
            var validObjects = GameObjectListPool.Get();

            // 候補の中から全てのタグを持つオブジェクトを検索
            for (int i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                var component = GetTagComponent(candidate);
                if (component != null && component.HasAllTags(tags))
                {
                    validObjects.Add(candidate);
                }
            }

            // 結果が配列サイズを超える場合は新しいプールを取得
            if (validObjects.Count > pooled.Objects.Length)
            {
                pooled.Dispose();
                pooled = GameObjectArrayPool.Get(validObjects.Count);
            }

            // 結果をプール配列にコピー
            for (int i = 0; i < validObjects.Count; i++)
            {
                pooled.Objects[i] = validObjects[i];
            }

            pooled.Count = validObjects.Count;

            // Listをプールに返却
            GameObjectListPool.Return(validObjects);

            return pooled;
        }

        /// <summary>
        /// タグコンポーネントが追加されたときの通知（高速化版）
        /// </summary>
        public void RegisterTagComponent(GameplayTagComponent component)
        {
            if (component == null) return;

            var gameObj = component.gameObject;

            // 重複チェック（O(1)）
            if (!componentCache.TryAdd(gameObj, component)) return;

            // 高速登録
            activeComponents.Add(component);

            // イベントリスナー登録（メモリリーク防止）
            Action<GameplayTagContainer> listener = _ => OnTagsChangedFast(component);
            listenerMap[component] = listener;
            component.OnTagsChanged += listener;

            // タグキャッシュ更新（遅延実行で最適化）
            UpdateTagCacheForComponent(component);
        }

        /// <summary>
        /// タグコンポーネントが削除されたときの通知（高速化版）
        /// </summary>
        public void UnregisterTagComponent(GameplayTagComponent component)
        {
            if (component == null) return;

            var gameObj = component.gameObject;

            // 高速削除
            componentCache.Remove(gameObj);
            activeComponents.Remove(component);

            // イベントリスナー削除（メモリリーク防止）
            if (listenerMap.TryGetValue(component, out var listener))
            {
                component.OnTagsChanged -= listener;
                listenerMap.Remove(component);
            }

            // タグキャッシュから削除
            RemoveFromTagCache(component);
        }

        /// <summary>
        /// タグ変更時の高速処理
        /// </summary>
        void OnTagsChangedFast(GameplayTagComponent component)
        {
            // 部分的キャッシュ更新（全削除ではなく）
            UpdateTagCacheForComponent(component);
        }

        /// <summary>
        /// 特定コンポーネントのタグキャッシュ更新（高速化版）
        /// </summary>
        void UpdateTagCacheForComponent(GameplayTagComponent component)
        {
            if (component == null || component.gameObject == null) return;

            var gameObj = component.gameObject;

            // 既存キャッシュから高速削除
            RemoveFromTagCacheFast(gameObj);

            // 新しいタグでキャッシュ追加
            var affectedHierarchies = new HashSet<GameplayTag>();
            var newTags = new HashSet<GameplayTag>();

            foreach (var tag in component.OwnedTags.Tags)
            {
                newTags.Add(tag);

                if (!taggedObjectsCache.TryGetValue(tag, out var set))
                {
                    set = new HashSet<GameObject>();
                    taggedObjectsCache[tag] = set;
                }

                set.Add(gameObj);

                // 階層キャッシュ無効化が必要な親タグを特定
                InvalidateHierarchyCacheFor(tag, affectedHierarchies);
            }

            // 逆引きキャッシュ更新
            objectToTagsCache[gameObj] = newTags;

            // 影響のある階層キャッシュを無効化
            foreach (var parentTag in affectedHierarchies)
            {
                hierarchyCache.Remove(parentTag);
                tagHierarchy.Remove(parentTag);
            }
        }

        /// <summary>
        /// 指定されたタグに影響する階層キャッシュを特定
        /// </summary>
        void InvalidateHierarchyCacheFor(GameplayTag tag, HashSet<GameplayTag> affectedHierarchies)
        {
            // 親タグたちを特定して階層キャッシュを無効化
            var currentTag = tag;
            while (currentTag.IsValid)
            {
                var parentTag = currentTag.GetParentTag();
                if (parentTag.IsValid)
                {
                    affectedHierarchies.Add(parentTag);
                    currentTag = parentTag;
                }
                else
                {
                    break;
                }
            }

            // 既存の階層キャッシュで影響のあるものを特定
            foreach (var kvp in hierarchyCache.ToArray()) // ToArrayでコレクション変更を回避
            {
                if (tag.MatchesTag(kvp.Key))
                {
                    affectedHierarchies.Add(kvp.Key);
                }
            }
        }

        /// <summary>
        /// タグキャッシュからコンポーネントを削除（高速化版）
        /// </summary>
        void RemoveFromTagCache(GameplayTagComponent component)
        {
            if (component?.gameObject == null) return;
            RemoveFromTagCacheFast(component.gameObject);
        }

        /// <summary>
        /// GameObjectを高速削除（逆引きキャッシュ使用）
        /// </summary>
        void RemoveFromTagCacheFast(GameObject gameObj)
        {
            if (gameObj == null) return;

            // 逆引きキャッシュから該当タグを取得
            if (objectToTagsCache.TryGetValue(gameObj, out var tagsToRemove))
            {
                var affectedHierarchies = new HashSet<GameplayTag>();
                
                // 該当タグのみから削除（O(k) k=タグ数）
                foreach (var tag in tagsToRemove)
                {
                    if (taggedObjectsCache.TryGetValue(tag, out var tagCache))
                    {
                        tagCache.Remove(gameObj);
                        // 空になったキャッシュは削除
                        if (tagCache.Count == 0)
                        {
                            taggedObjectsCache.Remove(tag);
                        }
                    }
                    
                    // 削除されるタグの階層キャッシュ無効化が必要な親タグを特定
                    InvalidateHierarchyCacheFor(tag, affectedHierarchies);
                }

                // 逆引きキャッシュからも削除
                objectToTagsCache.Remove(gameObj);
                
                // 影響のある階層キャッシュを無効化
                foreach (var parentTag in affectedHierarchies)
                {
                    hierarchyCache.Remove(parentTag);
                    tagHierarchy.Remove(parentTag);
                }
            }
            else
            {
                // 逆引きキャッシュにない場合は従来の方法（フォールバック）
                foreach (var cache in taggedObjectsCache.Values)
                {
                    cache.Remove(gameObj);
                }
                
                // 階層キャッシュを完全にクリア（安全策）
                hierarchyCache.Clear();
                tagHierarchy.Clear();
            }
        }


        /// <summary>
        /// テスト用：すべてのキャッシュをクリア
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        internal static void ClearAllCachesForTesting()
        {
            if (instance != null)
            {
                instance.componentCache.Clear();
                instance.taggedObjectsCache.Clear();
                instance.hierarchyCache.Clear();
                instance.tagHierarchy.Clear();
                instance.activeComponents.Clear();
                instance.objectToTagsCache.Clear();
                instance.listenerMap.Clear();
            }
        }

        void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
