using BehaviourTree.Core;

namespace BehaviourTree.Nodes
{
    public abstract class CompositeNode : BTNode
    {
        protected int currentChildIndex = 0;

        public override void Reset()
        {
            currentChildIndex = 0;
            base.Reset();
        }
    }
}