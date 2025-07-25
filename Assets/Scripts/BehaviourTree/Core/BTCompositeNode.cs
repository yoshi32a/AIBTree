using System.Collections.Generic;

namespace BehaviourTree.Core
{
    /// <summary>
    /// 複合ノードのベースクラス
    /// </summary>
    [System.Serializable]
    public abstract class BTCompositeNode : BTNode
    {
        protected int currentChildIndex = 0;
        
        public override void Reset()
        {
            base.Reset();
            currentChildIndex = 0;
        }
        
        /// <summary>
        /// 動的条件チェックを設定（ConditionとActionを関連付け）
        /// </summary>
        public void SetupDynamicConditionChecking()
        {
            for (int i = 0; i < Children.Count - 1; i++)
            {
                if (Children[i] is BTConditionNode condition && Children[i + 1] is BTActionNode action)
                {
                    action.AddWatchedCondition(condition);
                }
            }
        }
    }
    
    /// <summary>
    /// Sequenceノード：全ての子ノードが成功するまで実行
    /// </summary>
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
    
    /// <summary>
    /// Selectorノード：いずれかの子ノードが成功するまで実行
    /// </summary>
    [System.Serializable]
    public class BTSelectorNode : BTCompositeNode
    {
        public override BTNodeResult Execute()
        {
            for (int i = currentChildIndex; i < Children.Count; i++)
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
    
    /// <summary>
    /// Parallelノード：全ての子ノードを並行実行
    /// </summary>
    [System.Serializable]
    public class BTParallelNode : BTCompositeNode
    {
        public enum ParallelPolicy
        {
            RequireAll,    // 全て成功が必要
            RequireOne     // 一つ成功すれば十分
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