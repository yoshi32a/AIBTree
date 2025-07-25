using UnityEngine;
using BehaviourTree.Core;
using Components;

namespace BehaviourTree.Actions
{
    /// <summary>魔法を詠唱するアクション</summary>
    public class CastSpellAction : BTActionNode
    {
        string spellName = "fireball";
        int damage = 40;
        int manaCost = 50;
        
        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "spell_name":
                    spellName = value;
                    break;
                case "damage":
                    damage = System.Convert.ToInt32(value);
                    break;
                case "mana_cost":
                    manaCost = System.Convert.ToInt32(value);
                    break;
            }
        }
        
        protected override BTNodeResult ExecuteAction()
        {
            if (ownerComponent == null || blackBoard == null)
            {
                return BTNodeResult.Failure;
            }
            
            // マナチェック
            int currentMana = blackBoard.GetValue<int>("current_mana", 0);
            if (currentMana < manaCost)
            {
                Debug.Log($"CastSpell: Not enough mana ({currentMana} < {manaCost})");
                return BTNodeResult.Failure;
            }
            
            // ターゲット取得
            GameObject target = blackBoard.GetValue<GameObject>("nearest_enemy");
            if (target == null)
            {
                Debug.Log("CastSpell: No target found");
                return BTNodeResult.Failure;
            }
            
            // 魔法詠唱
            var targetHealth = target.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
                Debug.Log($"CastSpell: Cast {spellName} for {damage} damage");
            }
            
            // マナ消費
            blackBoard.SetValue("current_mana", currentMana - manaCost);
            
            return BTNodeResult.Success;
        }
    }
}