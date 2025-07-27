using System;
using System.Collections.Generic;
using ArcBT.Core;
using ArcBT.Logger;
using ArcBT.Samples.RPG.Components;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ArcBT.Samples.RPG.Actions
{
    /// <summary>オブジェクトとの相互作用アクション</summary>
    [BTNode("Interact")]
    public class InteractAction : BTActionNode
    {
        string interactionType = "examine";
        float interactionRange = 2.0f;

        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "interaction_type":
                    interactionType = value;
                    break;
                case "interaction_range":
                    interactionRange = Convert.ToSingle(value);
                    break;
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            if (ownerComponent == null || blackBoard == null)
            {
                return BTNodeResult.Failure;
            }

            // インタラクト対象を取得
            GameObject target = blackBoard.GetValue<GameObject>("interest_target");
            if (target == null)
            {
                target = blackBoard.GetValue<GameObject>("interact_target");
            }

            if (target == null || !target.activeInHierarchy)
            {
                BTLogger.LogSystem("Interact: No interaction target found", Name, ownerComponent);
                return BTNodeResult.Failure;
            }

            // 範囲チェック
            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance > interactionRange)
            {
                BTLogger.LogSystem($"Interact: Target out of range ({distance:F1} > {interactionRange})", Name, ownerComponent);
                return BTNodeResult.Failure;
            }

            // インタラクションタイプに応じた処理
            bool success = false;
            switch (interactionType)
            {
                case "examine":
                    success = ExamineObject(target);
                    break;
                case "collect":
                    success = CollectObject(target);
                    break;
                case "activate":
                    success = ActivateObject(target);
                    break;
                case "open":
                    success = OpenObject(target);
                    break;
                default:
                    success = DefaultInteraction(target);
                    break;
            }

            if (success)
            {
                // インタラクション成功をBlackBoardに記録
                blackBoard.SetValue("last_interaction_type", interactionType);
                blackBoard.SetValue("last_interaction_target", target.name);
                blackBoard.SetValue("last_interaction_time", Time.time);

                // 調査済みオブジェクトとして記録（重複調査を防ぐため）
                var recentlyInvestigated = blackBoard.GetValue<HashSet<string>>("recently_investigated");
                if (recentlyInvestigated == null)
                {
                    recentlyInvestigated = new HashSet<string>();
                    blackBoard.SetValue("recently_investigated", recentlyInvestigated);
                }
                string objKey = $"{target.name}_{target.transform.position}";
                recentlyInvestigated.Add(objKey);

                BTLogger.LogSystem($"Interact: Successfully {interactionType}ed '{target.name}'", Name, ownerComponent);
                return BTNodeResult.Success;
            }
            else
            {
                BTLogger.LogSystem($"Interact: Failed to {interactionType} '{target.name}'", Name, ownerComponent);
                return BTNodeResult.Failure;
            }
        }

        bool ExamineObject(GameObject target)
        {
            // オブジェクトを調査
            blackBoard.SetValue("examined_object", target.name);
            blackBoard.SetValue("object_type", target.tag);
            blackBoard.SetValue("object_position", target.transform.position);
            return true;
        }

        bool CollectObject(GameObject target)
        {
            // アイテムを収集
            var inventory = ownerComponent.GetComponent<Inventory>();
            if (inventory != null)
            {
                var item = target.GetComponent<InventoryItem>();
                if (item != null)
                {
                    inventory.AddItem(item.itemType, 1);
                    Object.Destroy(target);
                    blackBoard.SetValue("collected_item", item.itemType);
                    return true;
                }
            }

            return false;
        }

        bool ActivateObject(GameObject target)
        {
            // オブジェクトを起動
            var activatable = target.GetComponent<IActivatable>();
            if (activatable != null)
            {
                activatable.Activate();
                blackBoard.SetValue("activated_object", target.name);
                return true;
            }

            return false;
        }

        bool OpenObject(GameObject target)
        {
            // オブジェクトを開く
            blackBoard.SetValue("opened_object", target.name);
            // 宝箱やドアなどの処理をここに追加
            return true;
        }

        bool DefaultInteraction(GameObject target)
        {
            // デフォルトインタラクション
            blackBoard.SetValue("interacted_with", target.name);
            return true;
        }
    }

    // インタラクト可能オブジェクト用インターフェース
    public interface IActivatable
    {
        void Activate();
    }
}