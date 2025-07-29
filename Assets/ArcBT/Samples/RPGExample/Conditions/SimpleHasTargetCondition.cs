using ArcBT.Core;
using ArcBT.Logger;
using UnityEngine;

namespace ArcBT.Samples.RPG.Conditions
{
    /// <summary>
    /// ExampleAI用のシンプルなターゲット存在確認条件
    /// </summary>
    [BTNode("SimpleHasTarget")]
    public class SimpleHasTargetCondition : BTConditionNode
    {
        ExampleAI aiController;

        public override void SetProperty(string key, string value)
        {
            // この条件にはパラメータなし
        }

        public override void Initialize(MonoBehaviour owner, BlackBoard blackBoard)
        {
            base.Initialize(owner, blackBoard);
            aiController = owner.GetComponent<ExampleAI>();
            
            if (aiController == null)
            {
                BTLogger.LogError(LogCategory.System, "SimpleHasTargetCondition requires ExampleAI component");
            }
        }

        protected override BTNodeResult CheckCondition()
        {
            if (aiController == null)
            {
                BTLogger.LogError(LogCategory.System, "ExampleAI controller not found");
                return BTNodeResult.Failure;
            }

            bool hasTarget = aiController.Target != null;
            BTLogger.Log(Microsoft.Extensions.Logging.LogLevel.Debug, LogCategory.System, $"Simple has target: {hasTarget}");
            return hasTarget ? BTNodeResult.Success : BTNodeResult.Failure;
        }
    }
}