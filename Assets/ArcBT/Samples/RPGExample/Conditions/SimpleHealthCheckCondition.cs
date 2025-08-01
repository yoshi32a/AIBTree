using ArcBT.Core;
using ArcBT.Logger;
using UnityEngine;

namespace ArcBT.Samples.RPG.Conditions
{
    /// <summary>
    /// ExampleAI用のシンプルな体力チェック条件
    /// </summary>
    [BTNode("SimpleHealthCheck")]
    public class SimpleHealthCheckCondition : BTConditionNode
    {
        float minHealth = 0;
        ExampleAI aiController;

        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "min_health":
                    if (float.TryParse(value, out var healthValue))
                        minHealth = healthValue;
                    break;
            }
        }

        public override void Initialize(MonoBehaviour owner, BlackBoard blackBoard)
        {
            base.Initialize(owner, blackBoard);
            aiController = owner.GetComponent<ExampleAI>();
            
            if (aiController == null)
            {
                BTLogger.LogSystemError("System", "SimpleHealthCheckCondition requires ExampleAI component");
            }
        }

        protected override BTNodeResult CheckCondition()
        {
            if (aiController == null)
            {
                BTLogger.LogSystemError("System", "ExampleAI controller not found");
                return BTNodeResult.Failure;
            }

            bool result = aiController.CheckHealth(minHealth);
            BTLogger.LogSystem($"Simple health check: {aiController.Health} >= {minHealth} = {result}");
            return result ? BTNodeResult.Success : BTNodeResult.Failure;
        }
    }
}