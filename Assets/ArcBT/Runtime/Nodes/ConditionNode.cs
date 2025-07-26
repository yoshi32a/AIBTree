using ArcBT.Core;

namespace ArcBT.Nodes
{
    public abstract class ConditionNode : BTNode
    {
        protected abstract bool CheckCondition();

        public override BTNodeResult Execute()
        {
            return CheckCondition() ? BTNodeResult.Success : BTNodeResult.Failure;
        }
    }
}