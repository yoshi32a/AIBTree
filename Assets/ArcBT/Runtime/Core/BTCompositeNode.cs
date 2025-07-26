namespace ArcBT.Core
{
    /// <summary>複合ノードのベースクラス</summary>
    [System.Serializable]
    public abstract class BTCompositeNode : BTNode
    {
        protected int currentChildIndex = 0;

        public override void Reset()
        {
            base.Reset();
            currentChildIndex = 0;
        }

        /// <summary>動的条件チェックを設定（ConditionとActionを関連付け）</summary>
        public void SetupDynamicConditionChecking()
        {
            for (var i = 0; i < Children.Count - 1; i++)
            {
                if (Children[i] is BTConditionNode condition && Children[i + 1] is BTActionNode action)
                {
                    action.AddWatchedCondition(condition);
                }
            }
        }
    }
}