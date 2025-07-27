using ArcBT.Core;
using ArcBT.Logger;

namespace ArcBT.Decorators
{
    /// <summary>
    /// 子ノードの実行結果を反転するデコレーター
    /// Success → Failure, Failure → Success, Running → Running
    /// </summary>
    [BTNode("Inverter")]
    public class InverterDecorator : BTDecoratorNode
    {
        protected override BTNodeResult DecorateExecution(BTNode child)
        {
            var result = child.Execute();

            // 結果を反転（Runningはそのまま）
            var invertedResult = result switch
            {
                BTNodeResult.Success => BTNodeResult.Failure,
                BTNodeResult.Failure => BTNodeResult.Success,
                BTNodeResult.Running => BTNodeResult.Running,
                _ => BTNodeResult.Failure
            };

            BTLogger.LogSystem($"Inverter '{Name}': {result} → {invertedResult}", Name, ownerComponent);

            return invertedResult;
        }
    }
}