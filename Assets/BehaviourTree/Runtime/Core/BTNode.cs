using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree.Core
{
    [System.Serializable]
    public abstract class BTNode
    {
        public string Name { get; set; }
        public BTNode Parent { get; set; }
        public List<BTNode> Children { get; set; }

        // Unity関連の参照（実行時に設定）
        protected Transform transform;
        protected GameObject gameObject;
        protected MonoBehaviour ownerComponent;
        protected BlackBoard blackBoard;

        protected BTNode()
        {
            Children = new List<BTNode>();
        }

        public abstract BTNodeResult Execute();

        public virtual void Initialize(MonoBehaviour owner, BlackBoard sharedBlackBoard = null)
        {
            ownerComponent = owner;
            blackBoard = sharedBlackBoard;
            if (owner != null)
            {
                this.transform = owner.transform;
                this.gameObject = owner.gameObject;
            }
        }

        public virtual void SetProperty(string propertyName, string value)
        {
            // デフォルト実装：何もしない
        }

        // Unity関連のヘルパーメソッド
        protected T GetComponent<T>() where T : Component
        {
            return ownerComponent != null ? ownerComponent.GetComponent<T>() : null;
        }

        public virtual void AddChild(BTNode child)
        {
            child.Parent = this;
            Children.Add(child);

            // 子ノードにも同じownerとblackBoardを設定
            if (ownerComponent != null)
            {
                child.Initialize(ownerComponent, blackBoard);
            }
        }

        public virtual void RemoveChild(BTNode child)
        {
            child.Parent = null;
            Children.Remove(child);
        }

        public virtual void Reset()
        {
            foreach (var child in Children)
            {
                child.Reset();
            }
        }
    }
}