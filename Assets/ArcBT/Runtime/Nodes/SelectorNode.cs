using System;
using ArcBT.Core;

namespace ArcBT.Nodes
{
    /// <summary>
    /// 非推奨: BTSelectorNodeを使用してください
    /// Legacy selector node class. Use BTSelectorNode instead.
    /// </summary>
    [Obsolete("SelectorNodeは非推奨です。BTSelectorNodeを使用してください。", false)]
    public class SelectorNode : BTCompositeNode
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