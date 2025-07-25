namespace BehaviourTree.Core
{
    /// <summary>Sequenceノード：全ての子ノードが成功するまで実行</summary>
    [System.Serializable]
    public class BTSequenceNode : BTCompositeNode
    {
        public override BTNodeResult Execute()
        {
            for (int i = currentChildIndex; i < Children.Count; i++)
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
                        Reset();
                        return BTNodeResult.Failure;
                }
            }

            Reset();
            return BTNodeResult.Success;
        }
    }
}