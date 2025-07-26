using ArcBT.Core;

namespace ArcBT.Nodes
{
    public abstract class ActionNode : BTNode
    {
        protected abstract BTNodeResult OnExecute();

        public override BTNodeResult Execute()
        {
            return OnExecute();
        }
    }
}