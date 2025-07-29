using ArcBT.Logger;

namespace ArcBT.Core
{
    /// <summary>
    /// デコレーターノードの基底クラス
    /// 単一の子ノードを持ち、その実行を修飾する
    /// </summary>
    public abstract class BTDecoratorNode : BTNode
    {
        protected BTNode childNode;

        public override void AddChild(BTNode child)
        {
            if (childNode != null)
            {
                BTLogger.LogError(LogCategory.System, $"Decorator '{Name}' already has a child. Replacing existing child.", Name, ownerComponent);
                // 既存の子ノードをChildrenリストから削除
                Children.Remove(childNode);
            }
            
            childNode = child;
            child.Parent = this;
            
            // Childrenリストにも追加（BTNodeの基本構造との整合性のため）
            if (!Children.Contains(child))
            {
                Children.Add(child);
            }
            
            // 子ノードを初期化
            if (ownerComponent != null)
            {
                child.Initialize(ownerComponent, blackBoard);
            }
        }

        public override void RemoveChild(BTNode child)
        {
            if (childNode == child)
            {
                childNode = null;
                child.Parent = null;
                Children.Remove(child);
            }
        }

        public override BTNodeResult Execute()
        {
            if (childNode == null)
            {
                BTLogger.LogError(LogCategory.System, $"Decorator '{Name}' has no child node", Name, ownerComponent);
                return BTNodeResult.Failure;
            }

            return DecorateExecution(childNode);
        }

        /// <summary>
        /// 子ノードの実行を修飾するメソッド
        /// 派生クラスでオーバーライドして具体的な動作を実装
        /// </summary>
        /// <param name="child">実行する子ノード</param>
        /// <returns>修飾された実行結果</returns>
        protected abstract BTNodeResult DecorateExecution(BTNode child);

        public override void Reset()
        {
            base.Reset();
            childNode?.Reset();
        }

        /// <summary>子ノードを取得</summary>
        public BTNode GetChild() => childNode;

        /// <summary>子ノードが存在するかチェック</summary>
        public bool HasChild() => childNode != null;
        
        /// <summary>条件失敗時の処理</summary>
        public override void OnConditionFailed()
        {
            base.OnConditionFailed();
            childNode?.OnConditionFailed();
        }
    }
}