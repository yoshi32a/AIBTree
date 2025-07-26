using UnityEngine;
using ArcBT.Core;
using ArcBT.Samples.RPG.Components;

namespace ArcBT.Samples.RPG.Conditions
{
    /// <summary>マナ量をチェックする条件</summary>
    [BTScript("HasMana")]
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
            if (ownerComponent == null)
            {
                return BTNodeResult.Failure;
            }

            var mana = ownerComponent.GetComponent<Mana>();
            if (mana == null)
            {
                Debug.LogWarning("⚠️ HasMana: Manaコンポーネントが見つかりません");
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
                Debug.Log($"🔵 HasMana: 十分なマナあり ({mana.CurrentMana}/{mana.MaxMana} >= {minMana})");
                return BTNodeResult.Success;
            }
            else
            {
                Debug.Log($"🔴 HasMana: マナ不足 ({mana.CurrentMana}/{mana.MaxMana} < {minMana})");
                return BTNodeResult.Failure;
            }
        }
    }
}