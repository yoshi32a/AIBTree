using UnityEngine;
using ArcBT.Core;

namespace ArcBT.Conditions
{
    [BTScript("ScanForInterest")]
    public class ScanForInterestCondition : BTConditionNode
    {
        float scanRange = 5f;
        string interestTag = "Item";

        public override void SetProperty(string key, string value)
        {
            switch (key.ToLower())
            {
                case "range":
                case "scan_range":
                    if (float.TryParse(value, out var r)) scanRange = r;
                    break;
                case "interest":
                case "interest_tag":
                    interestTag = value;
                    break;
            }
        }

        protected override BTNodeResult CheckCondition()
        {
            var interestObjects = GameObject.FindGameObjectsWithTag(interestTag);
            
            foreach (var obj in interestObjects)
            {
                var distance = Vector3.Distance(transform.position, obj.transform.position);
                if (distance <= scanRange)
                {
                    // BlackBoardに興味のあるオブジェクト情報を記録
                    if (blackBoard != null)
                    {
                        blackBoard.SetValue("interest_object", obj);
                        blackBoard.SetValue("interest_distance", distance);
                    }

                    BTLogger.LogCondition($"興味のあるオブジェクト発見: {obj.name} (距離: {distance:F1})", Name);
                    return BTNodeResult.Success;
                }
            }

            BTLogger.LogCondition($"興味のあるオブジェクト未発見 (範囲: {scanRange})", Name);
            return BTNodeResult.Failure;
        }
    }
}