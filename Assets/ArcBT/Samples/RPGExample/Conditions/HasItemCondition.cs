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
                BTLogger.LogError(LogCategory.System, $"HasItem '{Name}': No Inventory component found on {gameObject.name}", Name, ownerComponent);
            }
        }

        protected override BTNodeResult CheckCondition()
        {
            BTLogger.LogCondition($"=== HasItemCondition '{Name}' EXECUTING ===", Name, ownerComponent);

            if (inventoryComponent == null)
            {
                BTLogger.LogError(LogCategory.System, $"HasItem '{Name}': No inventory component - assuming no items", Name, ownerComponent);
                return BTNodeResult.Failure;
            }

            if (string.IsNullOrEmpty(itemType))
            {
                BTLogger.LogError(LogCategory.System, $"HasItem '{Name}': No item type specified", Name, ownerComponent);
                return BTNodeResult.Failure;
            }

            var hasItem = inventoryComponent.HasItem(itemType);

            BTLogger.LogCondition($"HasItem '{Name}': Checking for '{itemType}' = {(hasItem ? "FOUND ✓" : "NOT FOUND ✗")}", Name, ownerComponent);

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