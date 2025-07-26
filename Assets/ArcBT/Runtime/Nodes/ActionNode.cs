using System;
using ArcBT.Core;

namespace ArcBT.Nodes
{
    /// <summary>
    /// 非推奨: BTActionNodeを使用してください
    /// Legacy action node class. Use BTActionNode instead.
    /// </summary>
    [Obsolete("ActionNodeは非推奨です。BTActionNodeを使用してください。", false)]
    public abstract class ActionNode : BTNode
    {
        protected abstract BTNodeResult OnExecute();

        public override BTNodeResult Execute()
        {
            return OnExecute();
        }
    }
}