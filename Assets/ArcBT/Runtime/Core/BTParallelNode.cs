namespace ArcBT.Core
{
    /// <summary>Parallelノード：全ての子ノードを並行実行</summary>
    [System.Serializable]
    [BTNode("Parallel")]
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
            var successCount = 0;
            var failureCount = 0;
            var runningCount = 0;

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

            switch (FailurePolicy)
            {
                // 失敗条件チェック
                case ParallelPolicy.RequireOne when failureCount > 0:
                case ParallelPolicy.RequireAll when failureCount >= Children.Count:
                    return BTNodeResult.Failure;
            }

            switch (SuccessPolicy)
            {
                // 成功条件チェック
                case ParallelPolicy.RequireOne when successCount > 0:
                case ParallelPolicy.RequireAll when successCount >= Children.Count:
                    return BTNodeResult.Success;
                default:
                    return BTNodeResult.Running;
            }
        }
    }
}
