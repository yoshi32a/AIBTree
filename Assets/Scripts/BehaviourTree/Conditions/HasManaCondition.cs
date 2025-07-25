using UnityEngine;
using BehaviourTree.Core;

namespace BehaviourTree.Conditions
{
    /// <summary>マナ量をチェックする条件</summary>
    public class HasManaCondition : BTConditionNode
    {
        int minMana = 50;

        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "min_mana":
                    minMana = System.Convert.ToInt32(value);
                    break;
            }
        }

        protected override BTNodeResult CheckCondition()
        {
            if (ownerComponent == null || blackBoard == null)
            {
                return BTNodeResult.Failure;
            }

            // BlackBoardから現在のマナ量を取得
            int currentMana = blackBoard.GetValue<int>("current_mana", 0);

            bool hasEnoughMana = currentMana >= minMana;

            if (hasEnoughMana)
            {
                blackBoard.SetValue("mana_sufficient", true);
                return BTNodeResult.Success;
            }
            else
            {
                blackBoard.SetValue("mana_sufficient", false);
                Debug.Log($"HasMana: Insufficient mana ({currentMana} < {minMana})");
                return BTNodeResult.Failure;
            }
        }
    }
}