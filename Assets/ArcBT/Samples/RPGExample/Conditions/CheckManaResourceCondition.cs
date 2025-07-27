using System;
using ArcBT.Core;
using ArcBT.Logger;

namespace ArcBT.Samples.RPG.Conditions
{
    /// <summary>マナリソースをチェックする条件</summary>
    [BTNode("CheckManaResource")]
    public class CheckManaResourceCondition : BTConditionNode
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
            if (ownerComponent == null || blackBoard == null)
            {
                return BTNodeResult.Failure;
            }

            // BlackBoardから現在のマナリソースを取得
            int currentMana = blackBoard.GetValue<int>("current_mana", 0);
            int maxMana = blackBoard.GetValue<int>("max_mana", 100);

            bool hasEnoughMana = currentMana >= minMana;

            // マナリソースの詳細情報をBlackBoardに記録
            blackBoard.SetValue("mana_percentage", (float)currentMana / maxMana * 100);
            blackBoard.SetValue("mana_available", hasEnoughMana);

            if (hasEnoughMana)
            {
                BTLogger.LogCondition($"CheckManaResource: Sufficient mana ({currentMana}/{maxMana})", Name, ownerComponent);
                return BTNodeResult.Success;
            }
            else
            {
                BTLogger.LogCondition($"CheckManaResource: Insufficient mana ({currentMana} < {minMana})", Name, ownerComponent);
                return BTNodeResult.Failure;
            }
        }
    }
}