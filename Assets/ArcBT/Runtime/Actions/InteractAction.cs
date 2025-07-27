using ArcBT.Core;
using ArcBT.Logger;
using UnityEngine;

namespace ArcBT.Actions
{
    [BTNode("Interact")]
    public class InteractAction : BTActionNode
    {
        string targetTag = "Interactable";
        float interactionRange = 2f;

        public override void SetProperty(string key, string value)
        {
            switch (key.ToLower())
            {
                case "target":
                case "target_tag":
                    targetTag = value;
                    break;
                case "range":
                case "interaction_range":
                    if (float.TryParse(value, out var r)) interactionRange = r;
                    break;
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            var interactables = GameObject.FindGameObjectsWithTag(targetTag);
            GameObject nearestInteractable = null;
            var nearestDistance = float.MaxValue;

            foreach (var interactable in interactables)
            {
                var distance = Vector3.Distance(transform.position, interactable.transform.position);
                if (distance <= interactionRange && distance < nearestDistance)
                {
                    nearestInteractable = interactable;
                    nearestDistance = distance;
                }
            }

            if (nearestInteractable != null)
            {
                BTLogger.LogMovement($"オブジェクト {nearestInteractable.name} と相互作用", Name);

                // 相互作用処理の実装
                var interactComponent = nearestInteractable.GetComponent<IInteractable>();
                if (interactComponent != null)
                {
                    interactComponent.OnInteract(ownerComponent.gameObject);
                }

                return BTNodeResult.Success;
            }

            BTLogger.LogMovement($"相互作用可能なオブジェクトが範囲内にありません (範囲: {interactionRange})", Name);
            return BTNodeResult.Failure;
        }
    }
}
