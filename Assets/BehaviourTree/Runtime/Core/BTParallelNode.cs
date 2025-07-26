namespace BehaviourTree.Core
{
    /// <summary>Parallelノード：全ての子ノードを並行実行</summary>
    [System.Serializable]
    public class BTParallelNode : BTCompositeNode
    {
        public enum ParallelPolicy
        {
            RequireAll, // 全て成功が必要
            RequireOne // 一つ成功すれば十分
        }

        public ParallelPolicy SuccessPolicy { get; set; } = ParallelPolicy.RequireAll;
        public ParallelPolicy FailurePolicy { get; set; } = ParallelPolicy.RequireOne;

        public override BTNodeResult Execute()
        {
            int successCount = 0;
            int failureCount = 0;
            int runningCount = 0;

            foreach (var child in Children)
            {
                var result = child.Execute();

                switch (result)
                {
                    case BTNodeResult.Success:
                        successCount++;
                        break;
                    case BTNodeResult.Failure:
                        failureCount++;
                        break;
                    case BTNodeResult.Running:
                        runningCount++;
                        break;
                }
            }

            // 失敗条件チェック
            if (FailurePolicy == ParallelPolicy.RequireOne && failureCount > 0)
            {
                return BTNodeResult.Failure;
            }

            if (FailurePolicy == ParallelPolicy.RequireAll && failureCount >= Children.Count)
            {
                return BTNodeResult.Failure;
            }

            // 成功条件チェック
            if (SuccessPolicy == ParallelPolicy.RequireOne && successCount > 0)
            {
                return BTNodeResult.Success;
            }

            if (SuccessPolicy == ParallelPolicy.RequireAll && successCount >= Children.Count)
            {
                return BTNodeResult.Success;
            }

            return BTNodeResult.Running;
        }
    }
}