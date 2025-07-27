using System.Collections.Generic;
using UnityEngine;

namespace ArcBT.TagSystem
{
    /// <summary>
    /// GameObject用Listのオブジェクトプール
    /// </summary>
    internal static class GameObjectListPool
    {
        static readonly Stack<List<GameObject>> pool = new Stack<List<GameObject>>();
        const int MAX_POOL_SIZE = 20;

        /// <summary>
        /// プールからListを取得
        /// </summary>
        public static List<GameObject> Get()
        {
            if (pool.Count > 0)
            {
                var list = pool.Pop();
                list.Clear(); // 使用前にクリア
                return list;
            }
            
            return new List<GameObject>();
        }

        /// <summary>
        /// Listをプールに返却
        /// </summary>
        public static void Return(List<GameObject> list)
        {
            if (list == null) return;
            
            list.Clear(); // 返却前にクリア
            
            if (pool.Count < MAX_POOL_SIZE)
            {
                pool.Push(list);
            }
        }
    }
}