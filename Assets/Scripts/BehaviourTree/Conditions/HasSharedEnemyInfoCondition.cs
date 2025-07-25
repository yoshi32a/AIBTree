using UnityEngine;
using BehaviourTree.Core;

namespace BehaviourTree.Conditions
{
    /// <summary>
    /// BlackBoardに共有された敵情報があるかチェックする条件
    /// </summary>
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
            bool hasEnemyInfo = blackBoard.GetValue<bool>(blackBoardKey, false);
            GameObject enemyTarget = blackBoard.GetValue<GameObject>("enemy_target");
            
            // 敵ターゲットが有効かチェック
            bool isValidTarget = enemyTarget != null && enemyTarget.activeInHierarchy;
            
            bool result = hasEnemyInfo && isValidTarget;
            
            if (result)
            {
                string enemyName = enemyTarget != null ? enemyTarget.name : "null";
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