using System;
using ArcBT.Core;

namespace ArcBT.Nodes
{
    /// <summary>
    /// 非推奨: BTConditionNodeを使用してください
    /// Legacy condition node class. Use BTConditionNode instead.
    /// </summary>
    [Obsolete("ConditionNodeは非推奨です。BTConditionNodeを使用してください。", false)]
    public abstract class ConditionNode : BTNode
    {
        protected abstract bool CheckCondition();

        public override BTNodeResult Execute()
        {
            return CheckCondition() ? BTNodeResult.Success : BTNodeResult.Failure;
        }
    }
}