using UnityEngine;
using BehaviourTree.Core;

namespace BehaviourTree.Conditions
{
    /// <summary>
    /// マナリソースをチェックする条件
    /// </summary>
    public class CheckManaResourceCondition : BTConditionNode
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
            
            // BlackBoardから現在のマナリソースを取得
            int currentMana = blackBoard.GetValue<int>("current_mana", 0);
            int maxMana = blackBoard.GetValue<int>("max_mana", 100);
            
            bool hasEnoughMana = currentMana >= minMana;
            
            // マナリソースの詳細情報をBlackBoardに記録
            blackBoard.SetValue("mana_percentage", (float)currentMana / maxMana * 100);
            blackBoard.SetValue("mana_available", hasEnoughMana);
            
            if (hasEnoughMana)
            {
                Debug.Log($"CheckManaResource: Sufficient mana ({currentMana}/{maxMana})");
                return BTNodeResult.Success;
            }
            else
            {
                Debug.Log($"CheckManaResource: Insufficient mana ({currentMana} < {minMana})");
                return BTNodeResult.Failure;
            }
        }
    }
}