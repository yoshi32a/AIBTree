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
                        // 成功した子ノードのみリセットし、インデックスを先頭に戻す
                        Children[i].Reset();
                        currentChildIndex = 0;
                        return BTNodeResult.Success;

                    case BTNodeResult.Running:
                        currentChildIndex = i;
                        return BTNodeResult.Running;

                    case BTNodeResult.Failure:
                        currentChildIndex = i + 1;
                        continue;
                }
            }

            // 全子ノードが失敗した場合、インデックスを先頭に戻す
            currentChildIndex = 0;
            return BTNodeResult.Failure;
        }
    }
}
