using ArcBT.Core;

namespace ArcBT.Nodes
{
    public class SelectorNode : CompositeNode
    {
        public override BTNodeResult Execute()
        {
            while (currentChildIndex < Children.Count)
            {
                var result = Children[currentChildIndex].Execute();

                switch (result)
                {
                    case BTNodeResult.Success:
                        currentChildIndex = 0;
                        return BTNodeResult.Success;

                    case BTNodeResult.Running:
                        return BTNodeResult.Running;

                    case BTNodeResult.Failure:
                        currentChildIndex++;
                        break;
                }
            }

            currentChildIndex = 0;
            return BTNodeResult.Failure;
        }
    }
}