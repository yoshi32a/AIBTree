namespace ArcBT.Core
{
    /// <summary>Sequenceノード：全ての子ノードが成功するまで実行</summary>
    [System.Serializable]
    [BTNode("Sequence")]
    public class BTSequenceNode : BTCompositeNode
    {
        public override BTNodeResult Execute()
        {
            for (var i = currentChildIndex; i < Children.Count; i++)
            {
                var result = Children[i].Execute();

                switch (result)
                {
                    case BTNodeResult.Success:
                        currentChildIndex = i + 1;
                        continue;

                    case BTNodeResult.Running:
                        currentChildIndex = i;
                        return BTNodeResult.Running;

                    case BTNodeResult.Failure:
                        // 現在実行中だった子ノードのみリセットし、インデックスを先頭に戻す
                        Children[i].Reset();
                        currentChildIndex = 0;
                        return BTNodeResult.Failure;
                }
            }

            // 全子ノードが成功した場合、インデックスを先頭に戻す
            currentChildIndex = 0;
            return BTNodeResult.Success;
        }
    }
}