using System;
using ArcBT.Core;

namespace ArcBT.Nodes
{
    /// <summary>
    /// 非推奨: BTSequenceNodeを使用してください
    /// Legacy sequence node class. Use BTSequenceNode instead.
    /// </summary>
    [Obsolete("SequenceNodeは非推奨です。BTSequenceNodeを使用してください。", false)]
    public class SequenceNode : BTCompositeNode
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