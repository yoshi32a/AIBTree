using System;
using System.Collections.Generic;
using ArcBT.Core;
using ArcBT.Logger;
using ArcBT.TagSystem;
using UnityEngine;

namespace ArcBT.Samples.RPG.Conditions
{
    /// <summary>興味のあるオブジェクトをスキャンする条件</summary>
    [BTNode("ScanForInterest")]
    public class ScanForInterestCondition : BTConditionNode
    {
        float scanRadius = 12.0f;

        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "scan_radius":
                    scanRadius = Convert.ToSingle(value);
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
                string objTag = GetGameplayTagsString(obj.gameObject);
                float distance = Vector3.Distance(transform.position, obj.transform.position);
                BTLogger.LogCondition($"ScanForInterest: Detected '{objName}' with tags '{objTag}' at distance {distance:F1}", Name, ownerComponent);
                
                // タグベースの安全な検査
                if (HasTag(obj, "Interactable") || HasTag(obj, "Item") || HasTag(obj, "Treasure"))
                {
                    interestingObjects.Add(obj);
                    BTLogger.LogCondition($"ScanForInterest: Added '{objName}' as interesting object", Name, ownerComponent);
                }
            }
            
            BTLogger.LogCondition($"ScanForInterest: Found {allObjects.Length} total objects, {interestingObjects.Count} interesting objects in radius {scanRadius}", Name, ownerComponent);

            if (interestingObjects.Count > 0)
            {
                // 最近調査したオブジェクトのリストを取得
                var recentlyInvestigated = blackBoard.GetValue<HashSet<string>>("recently_investigated");
                if (recentlyInvestigated == null)
                {
                    recentlyInvestigated = new HashSet<string>();
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
                    blackBoard.SetValue("interest_type", GetGameplayTagsString(nearestObject));

                    BTLogger.LogCondition($"ScanForInterest: Found {nearestObject.name} at distance {nearestDistance:F1}", Name, ownerComponent);
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
                return component.CompareGameplayTag(tagName);
            }
            catch (UnityException)
            {
                // Unity固有の例外（タグ未定義など）をキャッチ
                BTLogger.LogSystemError("System", $"Tag '{tagName}' is not defined in Unity Tag Manager.");
                return false;
            }
            catch (Exception ex)
            {
                // その他の例外
                BTLogger.LogSystemError("System", $"Unexpected error checking tag '{tagName}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// GameObjectのGameplayTagを文字列として取得
        /// </summary>
        string GetGameplayTagsString(GameObject gameObject)
        {
            var tagComponent = gameObject.GetComponent<GameplayTagComponent>();
            if (tagComponent != null && tagComponent.OwnedTags.Tags.Count > 0)
            {
                return string.Join(", ", tagComponent.OwnedTags.Tags);
            }
            return "None";
        }
    }
}