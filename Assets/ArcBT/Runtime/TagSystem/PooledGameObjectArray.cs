using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ArcBT.TagSystem
{
    /// <summary>
    /// プールされた GameObject 配列のラッパー
    /// using 文で自動的にプールに返却される
    /// </summary>
    public class PooledGameObjectArray : IDisposable, IEnumerable<GameObject>
    {
        /// <summary>
        /// 検索結果のGameObject配列
        /// </summary>
        public GameObject[] Objects { get; internal set; }
        
        /// <summary>
        /// 有効なオブジェクトの数
        /// </summary>
        public int Count { get; internal set; }
        
        /// <summary>
        /// 後方互換用のLengthプロパティ
        /// </summary>
        public int Length => Count;
        
        /// <summary>
        /// インデクサでアクセス可能
        /// </summary>
        public GameObject this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException($"Index {index} is out of range [0, {Count})");
                return Objects[index];
            }
        }
        
        internal bool isDisposed = false;

        internal PooledGameObjectArray(GameObject[] array, int count)
        {
            Objects = array;
            Count = count;
        }

        /// <summary>
        /// foreach で使用可能
        /// </summary>
        public IEnumerable<GameObject> GetValidObjects()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return Objects[i];
            }
        }

        /// <summary>
        /// 構造体Enumeratorを返す（0アロケーション）
        /// </summary>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// IEnumerable実装 - foreach で直接使用可能
        /// </summary>
        IEnumerator<GameObject> IEnumerable<GameObject>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// IEnumerable実装
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// 構造体Enumerator（0アロケーション）
        /// </summary>
        public struct Enumerator : IEnumerator<GameObject>
        {
            readonly PooledGameObjectArray array;
            int index;

            internal Enumerator(PooledGameObjectArray array)
            {
                this.array = array;
                this.index = -1;
            }

            public GameObject Current => array.Objects[index];

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                return ++index < array.Count;
            }

            public void Reset()
            {
                index = -1;
            }

            public void Dispose()
            {
                // 構造体なので何もしない
            }
        }

        /// <summary>
        /// 自動的にプールに返却
        /// </summary>
        public void Dispose()
        {
            if (!isDisposed)
            {
                GameObjectArrayPool.Return(this);
                isDisposed = true;
            }
        }

        /// <summary>
        /// デバッグ用の文字列表現
        /// </summary>
        public override string ToString()
        {
            return $"PooledGameObjectArray(Count: {Count})";
        }
    }
}
