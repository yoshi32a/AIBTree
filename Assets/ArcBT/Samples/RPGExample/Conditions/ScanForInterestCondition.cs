using UnityEngine;
using ArcBT.Core;
using System.Collections.Generic;

namespace ArcBT.Samples.RPG.Conditions
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

            // 興味のあるオブジェクトを検索（タグベースに変更）
            Collider[] allObjects = Physics.OverlapSphere(transform.position, scanRadius);
            List<Collider> interestingObjects = new List<Collider>();
            
            foreach (var obj in allObjects)
            {
                // デバッグ: 検出されたオブジェクトの詳細をログ出力
                string objName = obj.gameObject.name;
                string objTag = obj.tag;
                float distance = Vector3.Distance(transform.position, obj.transform.position);
                Debug.Log($"ScanForInterest: Detected '{objName}' with tag '{objTag}' at distance {distance:F1}");
                
                // タグベースの安全な検査
                if (HasTag(obj, "Interactable") || HasTag(obj, "Item") || HasTag(obj, "Treasure"))
                {
                    interestingObjects.Add(obj);
                    Debug.Log($"ScanForInterest: Added '{objName}' as interesting object");
                }
            }
            
            Debug.Log($"ScanForInterest: Found {allObjects.Length} total objects, {interestingObjects.Count} interesting objects in radius {scanRadius}");

            if (interestingObjects.Count > 0)
            {
                // 最近調査したオブジェクトのリストを取得
                var recentlyInvestigated = blackBoard.GetValue<System.Collections.Generic.HashSet<string>>("recently_investigated");
                if (recentlyInvestigated == null)
                {
                    recentlyInvestigated = new System.Collections.Generic.HashSet<string>();
                    blackBoard.SetValue("recently_investigated", recentlyInvestigated);
                }

                // 最も近い未調査オブジェクトを選択
                GameObject nearestObject = null;
                float nearestDistance = float.MaxValue;

                foreach (var obj in interestingObjects)
                {
                    float distance = Vector3.Distance(transform.position, obj.transform.position);
                    string objKey = $"{obj.gameObject.name}_{obj.transform.position}";
                    
                    // 最近調査していないオブジェクトを優先
                    if (!recentlyInvestigated.Contains(objKey) && distance < nearestDistance)
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

        bool HasTag(Component component, string tagName)
        {
            // タグの存在チェックと安全な比較
            if (string.IsNullOrEmpty(tagName)) return false;
            if (component == null || component.gameObject == null) return false;
            
            try
            {
                return component.CompareTag(tagName);
            }
            catch (UnityEngine.UnityException)
            {
                // Unity固有の例外（タグ未定義など）をキャッチ
                Debug.LogWarning($"Tag '{tagName}' is not defined in Unity Tag Manager.");
                return false;
            }
            catch (System.Exception ex)
            {
                // その他の例外
                Debug.LogWarning($"Unexpected error checking tag '{tagName}': {ex.Message}");
                return false;
            }
        }
    }
}