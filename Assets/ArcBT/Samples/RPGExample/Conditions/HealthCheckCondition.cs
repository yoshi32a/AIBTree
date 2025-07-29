using System;
using ArcBT.Core;
using ArcBT.Logger;
using ArcBT.Samples.RPG.Components;
using UnityEngine;

namespace ArcBT.Samples.RPG.Conditions
{
    /// <summary>体力チェック条件</summary>
    [Serializable]
    [BTNode("HealthCheck")]
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
                BTLogger.LogSystemError("System", $"HealthCheck: No Health component found on {gameObject.name}");
            }
        }

        protected override BTNodeResult CheckCondition()
        {
            // 安全地帯にいる間は緊急時判定をスキップ（低い閾値の場合のみ）
            if (blackBoard != null && minHealth <= 30)
            {
                float safetyTimer = blackBoard.GetValue<float>("safety_timer", 0f);
                if (Time.time < safetyTimer)
                {
                    BTLogger.LogSystem(Name, "In safety period - skipping emergency check");
                    return BTNodeResult.Failure; // 緊急時チェックをスキップ
                }
            }

            // スマートログ: 10回に1回だけ詳細ログを出力
            if (Time.frameCount % 10 == 0)
            {
                BTLogger.LogCondition($"=== HealthCheckCondition '{Name}' EXECUTING ===", Name, ownerComponent);
            }

            if (healthComponent == null)
            {
                BTLogger.LogSystemError("System", $"HealthCheck '{Name}': Health component is null - trying to find it again");
                healthComponent = GetComponent<Health>();
                if (healthComponent == null)
                {
                    BTLogger.LogSystemError("System", $"HealthCheck '{Name}': Still no Health component found!");
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

            // 結果に変化があった場合のみログ出力
            if (Time.frameCount % 10 == 0 || !healthOK)
            {
                BTLogger.LogCondition($"HealthCheck '{Name}': Current health = {currentHealth}, Required = {minHealth}", Name, ownerComponent);
                BTLogger.LogCondition($"HealthCheck '{Name}': Result = {(healthOK ? "SUCCESS ✓" : "FAILURE ✗")}", Name, ownerComponent);
            }

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