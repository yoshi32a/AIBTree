using System.Collections.Generic;
using BehaviourTree.Core;
using BehaviourTree.Examples;
using UnityEngine;

namespace BehaviourTree.Nodes
{
    public class CustomActionNode : ActionNode
    {
        Dictionary<string, string> properties = new Dictionary<string, string>();
        ExampleAI aiController;
        
        public void SetProperty(string key, string value)
        {
            properties[key] = value;
        }
        
        public string GetProperty(string key)
        {
            return properties.GetValueOrDefault(key, "");
        }
        
        protected override BTNodeResult OnExecute()
        {
            // AIコントローラーを取得
            if (aiController == null)
            {
                var runner = Object.FindFirstObjectByType<BehaviourTreeRunner>();
                if (runner != null)
                {
                    aiController = runner.GetComponent<ExampleAI>();
                }
            }

            var script = GetProperty("script");
            
            if (string.IsNullOrEmpty(script))
            {
                Debug.LogWarning($"No script specified for action node: {Name}");
                return BTNodeResult.Failure;
            }
            
            // スクリプト実行ロジック
            return ExecuteScript(script);
        }

        BTNodeResult ExecuteScript(string scriptName)
        {
            if (aiController == null)
            {
                Debug.LogError("ExampleAI controller not found");
                return BTNodeResult.Failure;
            }

            switch (scriptName)
            {
                case "MoveToPosition":
                    return ExecuteMoveToPosition();
                case "Wait":
                    return ExecuteWait();
                case "Attack":
                case "AttackEnemy":
                    return ExecuteAttack();
                default:
                    Debug.LogWarning($"Unknown script: {scriptName}");
                    return BTNodeResult.Failure;
            }
        }

        BTNodeResult ExecuteMoveToPosition()
        {
            var target = GetProperty("target");
            var speed = float.Parse(GetProperty("speed") ?? "3.5");
            var tolerance = float.Parse(GetProperty("tolerance") ?? "0.5");
            
            if (string.IsNullOrEmpty(target))
            {
                Debug.LogWarning("No target specified for move action");
                return BTNodeResult.Failure;
            }

            aiController.MoveToPosition(target, speed, tolerance);
            return BTNodeResult.Success;
        }

        BTNodeResult ExecuteWait()
        {
            var duration = float.Parse(GetProperty("duration") ?? "1.0");
            aiController.Wait(duration);
            return BTNodeResult.Success;
        }

        BTNodeResult ExecuteAttack()
        {
            var damage = int.Parse(GetProperty("damage") ?? "25");
            var attackRange = float.Parse(GetProperty("attack_range") ?? "2.0");
            
            aiController.AttackEnemy(damage, attackRange);
            return BTNodeResult.Success;
        }
    }
}