using UnityEngine;
using BehaviourTree.Core;

namespace BehaviourTree.Actions
{
    /// <summary>少量のマナを回復するアクション</summary>
    public class RestoreSmallManaAction : BTActionNode
    {
        int manaGain = 10;
        
        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "mana_gain":
                    manaGain = System.Convert.ToInt32(value);
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
                
                Debug.Log($"RestoreSmallMana: Restored {actualGain} mana ({currentMana} -> {newMana})");
                return BTNodeResult.Success;
            }
            else
            {
                Debug.Log("RestoreSmallMana: Mana already at maximum");
                return BTNodeResult.Failure;
            }
        }
    }
}
