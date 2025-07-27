using System;
using ArcBT.Core;
using ArcBT.Logger;
using UnityEngine;

namespace ArcBT.Samples.RPG.Actions
{
    /// <summary>少量のマナを回復するアクション</summary>
    [BTNode("RestoreSmallMana")]
    public class RestoreSmallManaAction : BTActionNode    {
        int manaGain = 10;

        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "mana_gain":
                    manaGain = Convert.ToInt32(value);
                    break;
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            if (ownerComponent == null || blackBoard == null)
            {
                return BTNodeResult.Failure;
            }

            // 現在のマナ量を取得
            int currentMana = blackBoard.GetValue<int>("current_mana", 0);
            int maxMana = blackBoard.GetValue<int>("max_mana", 100);

            // マナ回復
            int newMana = Mathf.Min(maxMana, currentMana + manaGain);
            int actualGain = newMana - currentMana;

            if (actualGain > 0)
            {
                blackBoard.SetValue("current_mana", newMana);
                blackBoard.SetValue("mana_restored", actualGain);
                blackBoard.SetValue("last_mana_restore_time", Time.time);

                BTLogger.LogSystem($"RestoreSmallMana: Restored {actualGain} mana ({currentMana} -> {newMana})", Name, ownerComponent);
                return BTNodeResult.Success;
            }
            else
            {
                BTLogger.LogSystem("RestoreSmallMana: Mana already at maximum", Name, ownerComponent);
                return BTNodeResult.Failure;
            }
        }
    }
}
