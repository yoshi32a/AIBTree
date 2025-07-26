using System;
using System.Collections.Generic;

namespace ArcBT.Core
{
    // Action用ベースクラス
    [Serializable]
    public abstract class BTActionNode : BTNode
    {
        // このActionが依存する条件ノード（動的チェック用）
        protected List<BTConditionNode> watchedConditions = new();

        // Action実行中かどうか
        protected bool isExecuting = false;

        public override BTNodeResult Execute()
        {
            // Action開始時に実行中フラグを設定
            isExecuting = true;

            // 依存する条件を動的にチェック
            if (watchedConditions.Count > 0)
            {
                foreach (var condition in watchedConditions)
                {
                    var conditionResult = condition.Execute();
                    if (conditionResult == BTNodeResult.Failure)
                    {
                        isExecuting = false;
                        OnConditionFailed();
                        return BTNodeResult.Failure;
                    }
                }
            }

            var result = ExecuteAction();

            // Action完了時に実行中フラグをクリア
            if (result != BTNodeResult.Running)
            {
                isExecuting = false;
            }

            return result;
        }

        protected abstract BTNodeResult ExecuteAction();

        /// <summary>動的にチェックする条件を追加</summary>
        public void AddWatchedCondition(BTConditionNode condition)
        {
            if (!watchedConditions.Contains(condition))
            {
                watchedConditions.Add(condition);
            }
        }

        /// <summary>条件が失敗した時に呼ばれる（オーバーライド可能）</summary>
        protected virtual void OnConditionFailed()
        {
            // デフォルトでは何もしない
        }

        /// <summary>現在実行中かどうか</summary>
        public bool IsExecuting => isExecuting;

        public override void Reset()
        {
            base.Reset();
            isExecuting = false;
        }
    }
}
