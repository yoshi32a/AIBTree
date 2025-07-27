using ArcBT.Core;
using UnityEngine;

namespace ArcBT.Samples.RPG.Actions
{
    /// <summary>
    /// ExampleAI用のシンプルな待機アクション
    /// </summary>
    [BTNode("WaitSimple")]
    public class WaitSimpleAction : BTActionNode
    {
        float duration = 1.0f;
        ExampleAI aiController;

        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "duration":
                    if (float.TryParse(value, out var durationValue))
                        duration = durationValue;
                    break;
            }
        }

        public override void Initialize(MonoBehaviour owner, BlackBoard blackBoard)
        {
            base.Initialize(owner, blackBoard);
            aiController = owner.GetComponent<ExampleAI>();
            
            if (aiController == null)
            {
                BTLogger.LogError(LogCategory.System, "WaitSimpleAction requires ExampleAI component");
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            if (aiController == null)
            {
                BTLogger.LogError(LogCategory.System, "ExampleAI controller not found");
                return BTNodeResult.Failure;
            }

            aiController.Wait(duration);
            BTLogger.Log(LogLevel.Debug, LogCategory.System, $"Executed wait for {duration} seconds");
            return BTNodeResult.Success;
        }
    }
}