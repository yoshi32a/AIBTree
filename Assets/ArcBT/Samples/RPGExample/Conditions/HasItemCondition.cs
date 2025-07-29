using System;
using ArcBT.Core;
using ArcBT.Logger;
using ArcBT.Samples.RPG.Components;
using UnityEngine;

namespace ArcBT.Samples.RPG.Conditions
{
    /// <summary>アイテム所持状態をチェックする条件</summary>
    [Serializable]
    [BTNode("HasItem")]
    public class HasItemCondition : BTConditionNode
    {
        [SerializeField] string itemType;

        Inventory inventoryComponent;

        public override void Initialize(MonoBehaviour owner, BlackBoard sharedBlackBoard = null)
        {
            base.Initialize(owner, sharedBlackBoard);
            inventoryComponent = GetComponent<Inventory>();

            if (inventoryComponent == null)
            {
                BTLogger.LogSystemError("System", $"HasItem '{Name}': No Inventory component found on {gameObject.name}");
            }
        }

        protected override BTNodeResult CheckCondition()
        {
            BTLogger.LogCondition(this, $"=== HasItemCondition '{Name}' EXECUTING ===");

            if (inventoryComponent == null)
            {
                BTLogger.LogSystemError("System", $"HasItem '{Name}': No inventory component - assuming no items");
                return BTNodeResult.Failure;
            }

            if (string.IsNullOrEmpty(itemType))
            {
                BTLogger.LogSystemError("System", $"HasItem '{Name}': No item type specified");
                return BTNodeResult.Failure;
            }

            var hasItem = inventoryComponent.HasItem(itemType);

            BTLogger.LogCondition(this, $"HasItem '{Name}': Checking for '{itemType}' = {(hasItem ? "FOUND ✓" : "NOT FOUND ✗")}");

            return hasItem ? BTNodeResult.Success : BTNodeResult.Failure;
        }

        public override void SetProperty(string propertyName, string value)
        {
            switch (propertyName.ToLower())
            {
                case "item_type":
                case "itemtype":
                    itemType = value;
                    break;
                default:
                    base.SetProperty(propertyName, value);
                    break;
            }
        }
    }
}