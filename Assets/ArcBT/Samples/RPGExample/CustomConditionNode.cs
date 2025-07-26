using System.Collections.Generic;
using ArcBT.Core;
using UnityEngine;

namespace ArcBT.Samples.RPG
{
    public class CustomConditionNode : BTConditionNode
    {
        Dictionary<string, string> properties = new Dictionary<string, string>();
        ExampleAI aiController;

        public override void SetProperty(string key, string value)
        {
            properties[key] = value;
        }

        public string GetProperty(string key)
        {
            return properties.GetValueOrDefault(key, "");
        }

        protected override BTNodeResult CheckCondition()
        {
            // AIコントローラーを取得
            if (aiController == null)
            {
                var runner = Object.FindFirstObjectByType<ArcBT.Core.BehaviourTreeRunner>();
                if (runner != null)
                {
                    aiController = runner.GetComponent<ExampleAI>();
                }
            }

            var script = GetProperty("script");

            if (string.IsNullOrEmpty(script))
            {
                Debug.LogWarning($"No script specified for condition node: {Name}");
                return BTNodeResult.Failure;
            }

            bool result = ExecuteConditionScript(script);
            return result ? BTNodeResult.Success : BTNodeResult.Failure;
        }

        bool ExecuteConditionScript(string scriptName)
        {
            if (aiController == null)
            {
                Debug.LogError("ExampleAI controller not found");
                return false;
            }

            switch (scriptName)
            {
                case "HealthCheck":
                    return CheckHealth();
                case "EnemyCheck":
                    return CheckEnemy();
                case "HasTarget":
                    return aiController.Target != null;
                case "IsInitialized":
                    return true; // 簡単な初期化チェック
                default:
                    Debug.LogWarning($"Unknown condition script: {scriptName}");
                    return false;
            }
        }

        bool CheckHealth()
        {
            var minHealth = float.Parse(GetProperty("min_health") ?? "0");
            return aiController.CheckHealth(minHealth);
        }

        bool CheckEnemy()
        {
            var detectionRange = float.Parse(GetProperty("detection_range") ?? "5.0");
            return aiController.DetectEnemy(detectionRange);
        }
    }
}