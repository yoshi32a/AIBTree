using UnityEngine;
using ArcBT.Core;

namespace ArcBT.Conditions
{
    /// <summary>BlackBoardに共有された敵情報があるかチェックする条件</summary>
    [BTScript("HasSharedEnemyInfo")]
    [BTNode("HasSharedEnemyInfo", NodeType.Condition)]
    public class HasSharedEnemyInfoCondition : BTConditionNode
    {
        string blackBoardKey = "has_enemy_info";

        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "bb_key":
                    blackBoardKey = value.ToString();
                    break;
            }
        }

        protected override BTNodeResult CheckCondition()
        {
            if (blackBoard == null)
            {
                Debug.LogError("HasSharedEnemyInfo: BlackBoard is null");
                return BTNodeResult.Failure;
            }

            // BlackBoardから敵情報の有無をチェック
            var hasEnemyInfo = blackBoard.GetValue<bool>(blackBoardKey, false);
            var enemyTarget = blackBoard.GetValue<GameObject>("enemy_target");

            // 敵ターゲットが有効かチェック
            var isValidTarget = enemyTarget != null && enemyTarget.activeInHierarchy;

            var result = hasEnemyInfo && isValidTarget;

            if (result)
            {
                var enemyName = enemyTarget != null ? enemyTarget.name : "null";
                Debug.Log($"HasSharedEnemyInfo: Enemy info available - Target: '{enemyName}'");
            }
            else
            {
                Debug.Log("HasSharedEnemyInfo: No valid enemy info in BlackBoard");
            }

            return result ? BTNodeResult.Success : BTNodeResult.Failure;
        }
    }
}