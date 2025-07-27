using System.Collections.Generic;
using UnityEngine;

namespace ArcBT.TagSystem
{
    /// <summary>
    /// GameObject用HashSetのオブジェクトプール
    /// </summary>
    internal static class GameObjectHashSetPool
    {
        static readonly Stack<HashSet<GameObject>> pool = new Stack<HashSet<GameObject>>();
        const int MAX_POOL_SIZE = 20;

        /// <summary>
        /// プールからHashSetを取得
        /// </summary>
        public static HashSet<GameObject> Get()
        {
            if (pool.Count > 0)
            {
                var hashSet = pool.Pop();
                hashSet.Clear(); // 使用前にクリア
                return hashSet;
            }
            
            return new HashSet<GameObject>();
        }

        /// <summary>
        /// HashSetをプールに返却
        /// </summary>
        public static void Return(HashSet<GameObject> hashSet)
        {
            if (hashSet == null) return;
            
            hashSet.Clear(); // 返却前にクリア
            
            if (pool.Count < MAX_POOL_SIZE)
            {
                pool.Push(hashSet);
            }
        }
    }
}