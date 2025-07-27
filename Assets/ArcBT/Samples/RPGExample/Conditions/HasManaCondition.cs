using System;
using ArcBT.Core;
using ArcBT.Logger;
using ArcBT.Samples.RPG.Components;
using UnityEngine;

namespace ArcBT.Samples.RPG.Conditions
{
    /// <summary>マナ量をチェックする条件</summary>
    [BTNode("HasMana")]
    public class HasManaCondition : BTConditionNode
    {
        int minMana = 50;

        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "min_mana":
                    minMana = Convert.ToInt32(value);
                    break;
            }
        }

        protected override BTNodeResult CheckCondition()
        {
            if (ownerComponent == null)
            {
                return BTNodeResult.Failure;
            }

            var mana = ownerComponent.GetComponent<Mana>();
            if (mana == null)
            {
                BTLogger.LogError(LogCategory.System, "⚠️ HasMana: Manaコンポーネントが見つかりません", Name, ownerComponent);
                return BTNodeResult.Failure;
            }

            bool hasEnoughMana = mana.CurrentMana >= minMana;

            if (blackBoard != null)
            {
                blackBoard.SetValue("current_mana", mana.CurrentMana);
                blackBoard.SetValue("max_mana", mana.MaxMana);
                blackBoard.SetValue("has_sufficient_mana", hasEnoughMana);
            }

            if (hasEnoughMana)
            {
                BTLogger.LogCondition($"🔵 HasMana: 十分なマナあり ({mana.CurrentMana}/{mana.MaxMana} >= {minMana})", Name, ownerComponent);
                return BTNodeResult.Success;
            }
            else
            {
                BTLogger.LogCondition($"🔴 HasMana: マナ不足 ({mana.CurrentMana}/{mana.MaxMana} < {minMana})", Name, ownerComponent);
                return BTNodeResult.Failure;
            }
        }
    }
}