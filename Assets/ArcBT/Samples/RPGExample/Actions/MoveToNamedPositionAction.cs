using ArcBT.Core;
using ArcBT.Logger;
using UnityEngine;

namespace ArcBT.Samples.RPG.Actions
{
    /// <summary>
    /// ExampleAI用の名前指定位置への移動アクション
    /// </summary>
    [BTNode("MoveToNamedPosition")]
    public class MoveToNamedPositionAction : BTActionNode
    {
        string target;
        float speed = 3.5f;
        float tolerance = 0.5f;
        ExampleAI aiController;

        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "target":
                    target = value;
                    break;
                case "speed":
                    if (float.TryParse(value, out var speedValue))
                        speed = speedValue;
                    break;
                case "tolerance":
                    if (float.TryParse(value, out var toleranceValue))
                        tolerance = toleranceValue;
                    break;
            }
        }

        public override void Initialize(MonoBehaviour owner, BlackBoard blackBoard)
        {
            base.Initialize(owner, blackBoard);
            aiController = owner.GetComponent<ExampleAI>();

            if (aiController == null)
            {
                BTLogger.LogSystemError("Movement", "MoveToNamedPositionAction requires ExampleAI component");
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            if (aiController == null)
            {
                BTLogger.LogSystemError("Movement", "ExampleAI controller not found");
                return BTNodeResult.Failure;
            }

            if (string.IsNullOrEmpty(target))
            {
                BTLogger.LogSystemError("Movement", "No target specified for move action");
                return BTNodeResult.Failure;
            }

            bool moveSucceeded = aiController.MoveToPosition(target, speed, tolerance);
            BTLogger.LogMovement($"Moving to {target} at speed {speed}");
            return moveSucceeded ? BTNodeResult.Success : BTNodeResult.Failure;
        }
    }
}