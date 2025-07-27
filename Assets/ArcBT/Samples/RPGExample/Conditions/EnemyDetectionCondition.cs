using ArcBT.Core;
using UnityEngine;

namespace ArcBT.Samples.RPG.Conditions
{
    /// <summary>
    /// ExampleAI用の敵検出条件
    /// </summary>
    [BTNode("EnemyDetection")]
    public class EnemyDetectionCondition : BTConditionNode
    {
        float detectionRange = 5.0f;
        ExampleAI aiController;

        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "detection_range":
                    if (float.TryParse(value, out var rangeValue))
                        detectionRange = rangeValue;
                    break;
            }
        }

        public override void Initialize(MonoBehaviour owner, BlackBoard blackBoard)
        {
            base.Initialize(owner, blackBoard);
            aiController = owner.GetComponent<ExampleAI>();
            
            if (aiController == null)
            {
                BTLogger.LogError(LogCategory.System, "EnemyDetectionCondition requires ExampleAI component");
            }
        }

        protected override BTNodeResult CheckCondition()
        {
            if (aiController == null)
            {
                BTLogger.LogError(LogCategory.System, "ExampleAI controller not found");
                return BTNodeResult.Failure;
            }

            bool result = aiController.DetectEnemy(detectionRange);
            BTLogger.Log(LogLevel.Debug, LogCategory.System, $"Enemy detection in range {detectionRange}: {result}");
            return result ? BTNodeResult.Success : BTNodeResult.Failure;
        }
    }
}