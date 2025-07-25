using BehaviourTree.Core;

namespace BehaviourTree.Nodes
{
    public class SequenceNode : CompositeNode
    {
        public override BTNodeResult Execute()
        {
            while (currentChildIndex < Children.Count)
            {
                var result = Children[currentChildIndex].Execute();
                
                switch (result)
                {
                    case BTNodeResult.Failure:
                        currentChildIndex = 0;
                        return BTNodeResult.Failure;
                    
                    case BTNodeResult.Running:
                        return BTNodeResult.Running;
                    
                    case BTNodeResult.Success:
                        currentChildIndex++;
                        break;
                }
            }
            
            currentChildIndex = 0;
            return BTNodeResult.Success;
        }
    }
}