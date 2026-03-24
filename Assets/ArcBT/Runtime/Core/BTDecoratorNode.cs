using ArcBT.Logger;

namespace ArcBT.Core
{
    /// <summary>
    /// デコレーターノードの基底クラス
    /// 単一の子ノードを持ち、その実行を修飾する
    /// </summary>
    public abstract class BTDecoratorNode : BTNode
    {
        /// <summary>子ノード（Children[0]のショートカット）</summary>
        public BTNode Child => Children.Count > 0 ? Children[0] : null;

        public override void AddChild(BTNode child)
        {
            if (Child != null)
            {
                BTLogger.LogSystemError("System", $"Decorator '{Name}' already has a child. Replacing existing child.");
                // 既存の子ノードをリセットしてからchildrenリストから削除
                var oldChild = Child;
                oldChild.Reset();
                oldChild.Parent = null;
                children.Remove(oldChild);
            }

            child.Parent = this;
            children.Add(child);

            // 子ノードを初期化
            if (ownerComponent != null)
            {
                child.Initialize(ownerComponent, blackBoard);
            }
        }

        public override void RemoveChild(BTNode child)
        {
            if (Child == child)
            {
                child.Parent = null;
                children.Remove(child);
            }
        }

        public override BTNodeResult Execute()
        {
            if (Child == null)
            {
                BTLogger.LogSystemError("System", $"Decorator '{Name}' has no child node");
                return BTNodeResult.Failure;
            }

            return DecorateExecution(Child);
        }

        /// <summary>
        /// 子ノードの実行を修飾するメソッド
        /// 派生クラスでオーバーライドして具体的な動作を実装
        /// </summary>
        /// <param name="child">実行する子ノード</param>
        /// <returns>修飾された実行結果</returns>
        protected abstract BTNodeResult DecorateExecution(BTNode child);

        /// <summary>子ノードを取得</summary>
        public BTNode GetChild() => Child;

        /// <summary>子ノードが存在するかチェック</summary>
        public bool HasChild() => Child != null;

        /// <summary>条件失敗時の処理</summary>
        public override void OnConditionFailed()
        {
            base.OnConditionFailed();
            Child?.OnConditionFailed();
        }
    }
}
