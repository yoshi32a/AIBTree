namespace ArcBT.Core
{
    /// <summary>Selectorノード：いずれかの子ノードが成功するまで実行</summary>
    [System.Serializable]
    [BTNode("Selector")]
    public class BTSelectorNode : BTCompositeNode
    {
        public override BTNodeResult Execute()
        {
            for (var i = currentChildIndex; i < Children.Count; i++)
            {
                var result = Children[i].Execute();

                switch (result)
                {
                    case BTNodeResult.Success:
                        Reset();
                        return BTNodeResult.Success;

                    case BTNodeResult.Running:
                        currentChildIndex = i;
                        return BTNodeResult.Running;

                    case BTNodeResult.Failure:
                        currentChildIndex = i + 1;
                        continue;
                }
            }

            Reset();
            return BTNodeResult.Failure;
        }
    }
}
