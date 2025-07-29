using ArcBT.Core;
using ArcBT.Logger;
using ArcBT.Samples.RPG.Components;
using UnityEngine;

namespace ArcBT.Samples.RPG.Actions
{
    /// <summary>アイテムを使用するアクション</summary>
    [BTNode("UseItem")]
    public class UseItemAction : BTActionNode    {
        string itemType = "healing_potion";

        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "item_type":
                    itemType = value;
                    break;
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            if (ownerComponent == null || blackBoard == null)
            {
                return BTNodeResult.Failure;
            }

            // インベントリから指定アイテムを取得
            var inventory = ownerComponent.GetComponent<Inventory>();
            if (inventory == null)
            {
                BTLogger.LogSystemError("System", "UseItem: No Inventory component found");
                return BTNodeResult.Failure;
            }

            if (!inventory.HasItem(itemType))
            {
                BTLogger.LogSystem(Name, $"No {itemType} available");
                return BTNodeResult.Failure;
            }

            // アイテムを使用
            bool used = inventory.UseItem(itemType);
            if (!used)
            {
                BTLogger.LogSystem(Name, $"Failed to use {itemType}");
                return BTNodeResult.Failure;
            }

            // アイテム効果を適用
            ApplyItemEffect(itemType);

            // BlackBoardに使用情報を記録
            blackBoard.SetValue("last_used_item", itemType);
            blackBoard.SetValue("item_use_time", Time.time);

            BTLogger.LogSystem(Name, $"Successfully used {itemType}");
            return BTNodeResult.Success;
        }

        void ApplyItemEffect(string item)
        {
            switch (item)
            {
                case "healing_potion":
                    var health = ownerComponent.GetComponent<Health>();
                    if (health != null)
                    {
                        health.Heal(25);
                        blackBoard.SetValue("health_restored", 25);
                    }

                    break;
                case "mana_potion":
                    int currentMana = blackBoard.GetValue<int>("current_mana", 0);
                    blackBoard.SetValue("current_mana", currentMana + 30);
                    blackBoard.SetValue("mana_restored", 30);
                    break;
            }
        }
    }
}
