using ArcBT.Core;
using UnityEngine;

namespace ArcBT.Samples.RPG.Conditions
{
    /// <summary>初期化状態をチェックする条件</summary>
    [BTNode("IsInitialized")]
    public class IsInitializedCondition : BTConditionNode
    {
        protected override BTNodeResult CheckCondition()
        {
            if (ownerComponent == null || blackBoard == null)
            {
                return BTNodeResult.Failure;
            }

            // BlackBoardから初期化状態を確認
            bool isInitialized = blackBoard.GetValue<bool>("is_initialized", false);

            if (!isInitialized)
            {
                // 初期化を試行
                blackBoard.SetValue("is_initialized", true);
                blackBoard.SetValue("initialization_time", Time.time);
                Debug.Log("IsInitialized: System initialized");
                return BTNodeResult.Success;
            }

            return BTNodeResult.Success;
        }
    }
}