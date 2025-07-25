using UnityEngine;
using BehaviourTree.Core;

namespace BehaviourTree.Conditions
{
    /// <summary>興味のあるオブジェクトをスキャンする条件</summary>
    public class ScanForInterestCondition : BTConditionNode
    {
        float scanRadius = 12.0f;
        
        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "scan_radius":
                    scanRadius = System.Convert.ToSingle(value);
                    break;
            }
        }
        
        protected override BTNodeResult CheckCondition()
        {
            if (ownerComponent == null || blackBoard == null)
            {
                return BTNodeResult.Failure;
            }
            
            // 興味のあるオブジェクトを検索
            Collider[] interestingObjects = Physics.OverlapSphere(transform.position, scanRadius, LayerMask.GetMask("Interactable", "Item", "Treasure"));
            
            if (interestingObjects.Length > 0)
            {
                // 最も近いオブジェクトを選択
                GameObject nearestObject = null;
                float nearestDistance = float.MaxValue;
                
                foreach (var obj in interestingObjects)
                {
                    float distance = Vector3.Distance(transform.position, obj.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestObject = obj.gameObject;
                    }
                }
                
                if (nearestObject != null)
                {
                    // BlackBoardに興味のあるオブジェクト情報を保存
                    blackBoard.SetValue("interest_target", nearestObject);
                    blackBoard.SetValue("interest_distance", nearestDistance);
                    blackBoard.SetValue("interest_type", nearestObject.tag);
                    
                    Debug.Log($"ScanForInterest: Found {nearestObject.name} at distance {nearestDistance:F1}");
                    return BTNodeResult.Success;
                }
            }
            
            // 興味のあるオブジェクトが見つからない場合
            blackBoard.SetValue("interest_target", (GameObject)null);
            blackBoard.RemoveValue("interest_distance");
            
            return BTNodeResult.Failure;
        }
    }
}
