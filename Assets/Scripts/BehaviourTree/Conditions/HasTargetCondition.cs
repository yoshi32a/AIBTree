using UnityEngine;
using BehaviourTree.Core;

namespace BehaviourTree.Conditions
{
    /// <summary>
    /// ターゲットの存在をチェックする条件
    /// </summary>
    public class HasTargetCondition : BTConditionNode
    {
        protected override BTNodeResult CheckCondition()
        {
            if (ownerComponent == null || blackBoard == null)
            {
                return BTNodeResult.Failure;
            }
            
            // BlackBoardからターゲット情報を取得
            GameObject target = blackBoard.GetValue<GameObject>("current_target");
            
            if (target != null && target.activeInHierarchy)
            {
                // ターゲットが有効な場合、距離も記録
                float distance = Vector3.Distance(transform.position, target.transform.position);
                blackBoard.SetValue("target_distance", distance);
                return BTNodeResult.Success;
            }
            
            // ターゲット情報をクリア
            blackBoard.SetValue("current_target", (GameObject)null);
            blackBoard.RemoveValue("target_distance");
            
            return BTNodeResult.Failure;
        }
    }
}