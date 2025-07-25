using UnityEngine;
using BehaviourTree.Core;
using Components;

namespace BehaviourTree.Conditions
{
    /// <summary>体力チェック条件</summary>
    [System.Serializable]
    public class HealthCheckCondition : BTConditionNode
    {
        [SerializeField] int minHealth = 50;

        Health healthComponent;

        public override void Initialize(MonoBehaviour owner, BlackBoard sharedBlackBoard = null)
        {
            base.Initialize(owner, sharedBlackBoard);
            healthComponent = GetComponent<Health>();

            if (healthComponent == null)
            {
                Debug.LogError($"HealthCheck: No Health component found on {gameObject.name}");
            }
        }

        protected override BTNodeResult CheckCondition()
        {
            Debug.Log($"=== HealthCheckCondition '{Name}' EXECUTING ===");

            if (healthComponent == null)
            {
                Debug.LogError($"HealthCheck '{Name}': Health component is null - trying to find it again");
                healthComponent = GetComponent<Health>();
                if (healthComponent == null)
                {
                    Debug.LogError($"HealthCheck '{Name}': Still no Health component found!");
                    return BTNodeResult.Failure;
                }
            }

            var currentHealth = healthComponent.CurrentHealth;
            var healthOK = currentHealth >= minHealth;

            // BlackBoardに健康状態を記録
            if (blackBoard != null)
            {
                blackBoard.SetValue("current_health", currentHealth);
                blackBoard.SetValue("health_status", healthOK ? "healthy" : "low_health");
                blackBoard.SetValue("min_health_threshold", minHealth);
            }

            Debug.Log($"HealthCheck '{Name}': Current health = {currentHealth}, Required = {minHealth}");
            Debug.Log($"HealthCheck '{Name}': Result = {(healthOK ? "SUCCESS ✓" : "FAILURE ✗")}");

            return healthOK ? BTNodeResult.Success : BTNodeResult.Failure;
        }

        public override void SetProperty(string propertyName, string value)
        {
            switch (propertyName.ToLower())
            {
                case "min_health":
                case "minhealth":
                    if (int.TryParse(value, out var healthValue))
                    {
                        minHealth = healthValue;
                    }

                    break;
                default:
                    base.SetProperty(propertyName, value);
                    break;
            }
        }
    }
}