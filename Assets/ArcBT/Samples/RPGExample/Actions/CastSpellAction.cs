using System;
using ArcBT.Core;
using ArcBT.Samples.RPG.Components;
using UnityEngine;

namespace ArcBT.Samples.RPG.Actions
{
    /// <summary>魔法を詠唱するアクション</summary>
    [BTNode("CastSpell", NodeType.Action)]
    public class CastSpellAction : BTActionNode    {
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
                    damage = Convert.ToInt32(value);
                    break;
                case "mana_cost":
                    manaCost = Convert.ToInt32(value);
                    break;
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            if (ownerComponent == null)
            {
                Debug.LogError("❌ CastSpell: Owner component is null");
                return BTNodeResult.Failure;
            }

            var mana = ownerComponent.GetComponent<Mana>();
            if (mana == null)
            {
                Debug.LogWarning("⚠️ CastSpell: Manaコンポーネントが見つかりません");
                return BTNodeResult.Failure;
            }

            // マナチェック
            if (!mana.HasEnoughMana(manaCost))
            {
                Debug.Log($"🔴 CastSpell: マナ不足で '{spellName}' を使用できません ({mana.CurrentMana} < {manaCost})");
                return BTNodeResult.Failure;
            }

            // ターゲット取得
            GameObject target = null;
            if (blackBoard != null)
            {
                target = blackBoard.GetValue<GameObject>("nearest_enemy");
            }

            if (target == null)
            {
                Debug.Log("❓ CastSpell: 魔法のターゲットが見つかりません");
                return BTNodeResult.Failure;
            }

            // マナ消費
            mana.ConsumeMana(manaCost);

            // ターゲットにダメージ
            var targetHealth = target.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
                Debug.Log($"✨ CastSpell: '{spellName}' で '{target.name}' に {damage} ダメージ！ (マナ消費: {manaCost})");
                
                // BlackBoardに魔法使用履歴を記録
                if (blackBoard != null)
                {
                    blackBoard.SetValue("last_spell_used", spellName);
                    blackBoard.SetValue("last_spell_time", Time.time);
                }
                
                return BTNodeResult.Success;
            }
            else
            {
                Debug.LogWarning($"⚠️ CastSpell: ターゲット '{target.name}' にHealthコンポーネントがありません");
                return BTNodeResult.Failure;
            }
        }
    }
}
