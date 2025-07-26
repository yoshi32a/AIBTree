using UnityEngine;
using BehaviourTree.Core;
using BehaviourTree.Components;

namespace BehaviourTree.Conditions
{
    /// <summary>アイテム所持状態をチェックする条件</summary>
    [System.Serializable]
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
                Debug.LogWarning($"HasItem '{Name}': No Inventory component found on {gameObject.name}");
            }
        }

        protected override BTNodeResult CheckCondition()
        {
            Debug.Log($"=== HasItemCondition '{Name}' EXECUTING ===");

            if (inventoryComponent == null)
            {
                Debug.LogWarning($"HasItem '{Name}': No inventory component - assuming no items");
                return BTNodeResult.Failure;
            }

            if (string.IsNullOrEmpty(itemType))
            {
                Debug.LogError($"HasItem '{Name}': No item type specified");
                return BTNodeResult.Failure;
            }

            var hasItem = inventoryComponent.HasItem(itemType);

            Debug.Log($"HasItem '{Name}': Checking for '{itemType}' = {(hasItem ? "FOUND ✓" : "NOT FOUND ✗")}");

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