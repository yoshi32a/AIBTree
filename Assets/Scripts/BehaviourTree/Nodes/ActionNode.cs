using BehaviourTree.Core;

namespace BehaviourTree.Nodes
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