using ArcBT.Core;
using ArcBT.Logger;
using UnityEngine;

namespace ArcBT.Conditions
{
    /// <summary>BlackBoardに共有された敵情報があるかチェックする条件</summary>
    [BTNode("HasSharedEnemyInfo")]
    public class HasSharedEnemyInfoCondition : BTConditionNode
    {
        string blackBoardKey = "has_enemy_info";

        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "bb_key":
                    blackBoardKey = value;
                    break;
            }
        }

        protected override BTNodeResult CheckCondition()
        {
            if (blackBoard == null)
            {
                BTLogger.LogSystemError(this, "BlackBoard is null");
                return BTNodeResult.Failure;
            }

            // BlackBoardから敵情報の有無をチェック
            var hasEnemyInfo = blackBoard.GetValue<bool>(blackBoardKey);
            var enemyTarget = blackBoard.GetValue<GameObject>("enemy_target");

            // 敵ターゲットが有効かチェック
            var isValidTarget = enemyTarget != null && enemyTarget.activeInHierarchy;

            var result = hasEnemyInfo && isValidTarget;

            if (result)
            {
                var enemyName = enemyTarget != null ? enemyTarget.name : "null";
                BTLogger.LogSystem(this, $"Enemy info available - Target: '{enemyName}'");
            }
            else
            {
                BTLogger.LogSystem(this, "No valid enemy info in BlackBoard");
            }

            return result ? BTNodeResult.Success : BTNodeResult.Failure;
        }
    }
}
