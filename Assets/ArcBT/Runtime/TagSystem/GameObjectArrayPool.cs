using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcBT.TagSystem
{
    /// <summary>
    /// GameObject配列のオブジェクトプール
    /// </summary>
    internal static class GameObjectArrayPool
    {
        static readonly Queue<GameObject[]> arrayPool = new();
        static readonly Queue<PooledGameObjectArray> wrapperPool = new();

        const int DefaultArraySize = 100;
        const int MaxPoolSize = 50;

        /// <summary>
        /// プールから配列を取得（または新規作成）
        /// </summary>
        public static PooledGameObjectArray Get(int minSize = DefaultArraySize)
        {
            // 配列を取得
            GameObject[] array;
            if (arrayPool.Count > 0 && arrayPool.Peek().Length >= minSize)
            {
                array = arrayPool.Dequeue();
            }
            else
            {
                array = new GameObject[Math.Max(minSize, DefaultArraySize)];
            }

            // ラッパーを取得
            PooledGameObjectArray wrapper;
            if (wrapperPool.Count > 0)
            {
                wrapper = wrapperPool.Dequeue();
                wrapper.Objects = array;
                wrapper.Count = 0;
                wrapper.isDisposed = false;
            }
            else
            {
                wrapper = new PooledGameObjectArray(array, 0);
            }

            return wrapper;
        }

        /// <summary>
        /// プールに返却
        /// </summary>
        public static void Return(PooledGameObjectArray wrapper)
        {
            if (wrapper == null || wrapper.Objects == null) return;

            // 配列をクリア（参照を残さない）
            var array = wrapper.Objects;
            for (int i = 0; i < wrapper.Count; i++)
            {
                array[i] = null;
            }

            // プールサイズ制限
            if (arrayPool.Count < MaxPoolSize)
            {
                arrayPool.Enqueue(array);
            }

            if (wrapperPool.Count < MaxPoolSize)
            {
                wrapper.Objects = null;
                wrapper.Count = 0;
                wrapperPool.Enqueue(wrapper);
            }
        }

        /// <summary>
        /// プール統計情報（デバッグ用）
        /// </summary>
        public static string GetPoolStats()
        {
            return $"ArrayPool: {arrayPool.Count}, WrapperPool: {wrapperPool.Count}";
        }
    }
}
