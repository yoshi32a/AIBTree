using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcBT.Core
{
    [Serializable]
    public abstract class BTNode
    {
        public string Name { get; set; }
        public BTNode Parent { get; set; }

        /// <summary>子ノードのリスト（読み取り専用）。子ノードの追加/削除にはAddChild/RemoveChildを使用する。</summary>
        public IReadOnlyList<BTNode> Children => children;

        // 内部用の変更可能なリスト（サブクラスからAddChild/RemoveChildのオーバーライドでアクセス可能）
        protected readonly List<BTNode> children = new();

        // Unity関連の参照（実行時に設定）
        protected Transform transform;
        protected GameObject gameObject;
        protected MonoBehaviour ownerComponent;
        protected BlackBoard blackBoard;

        // GetComponent結果のキャッシュ
        Dictionary<System.Type, Component> componentCache;

        public abstract BTNodeResult Execute();

        public virtual void Initialize(MonoBehaviour owner, BlackBoard sharedBlackBoard = null)
        {
            // ownerが変わった場合はGetComponentキャッシュをクリア
            if (ownerComponent != owner)
            {
                componentCache?.Clear();
            }

            ownerComponent = owner;
            blackBoard = sharedBlackBoard;
            if (owner != null)
            {
                transform = owner.transform;
                gameObject = owner.gameObject;
            }
        }

        public virtual void SetProperty(string propertyName, string value)
        {
            // デフォルト実装：何もしない
        }

        // プロパティ値パースユーティリティ（SetProperty実装で使用）
        protected static bool TryParseFloat(string value, out float result) =>
            float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result);

        protected static bool TryParseInt(string value, out int result) =>
            int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out result);

        protected static bool TryParseBool(string value, out bool result) =>
            bool.TryParse(value, out result);

        // Unity関連のヘルパーメソッド（結果をキャッシュ）
        protected T GetComponent<T>() where T : Component
        {
            componentCache ??= new Dictionary<System.Type, Component>();
            var type = typeof(T);
            if (!componentCache.TryGetValue(type, out var cached))
            {
                cached = ownerComponent != null ? ownerComponent.GetComponent<T>() : null;
                componentCache[type] = cached;
            }
            return cached as T;
        }

        public virtual void AddChild(BTNode child)
        {
            child.Parent = this;
            children.Add(child);

            // 子ノードにも同じownerとblackBoardを設定
            if (ownerComponent != null)
            {
                child.Initialize(ownerComponent, blackBoard);
            }
        }

        public virtual void RemoveChild(BTNode child)
        {
            child.Parent = null;
            children.Remove(child);
        }

        public virtual void Reset()
        {
            foreach (var child in Children)
            {
                child.Reset();
            }
        }

        /// <summary>条件が失敗した時に呼ばれる（オーバーライド可能）</summary>
        public virtual void OnConditionFailed()
        {
            // デフォルトでは何もしない
        }
    }
}
