using System;
using ArcBT.Core;
using ArcBT.Logger;
using ArcBT.Samples.RPG.Components;
using UnityEngine;

namespace ArcBT.Samples.RPG.Actions
{
    /// <summary>通常攻撃アクション</summary>
    [BTNode("NormalAttack")]
    public class NormalAttackAction : BTActionNode
    {
        int damage = 15;
        float cooldown = 1.0f;
        float lastAttackTime = 0f;

        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "damage":
                    damage = Convert.ToInt32(value);
                    break;
                case "cooldown":
                    cooldown = Convert.ToSingle(value);
                    break;
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            if (ownerComponent == null || blackBoard == null)
            {
                return BTNodeResult.Failure;
            }

            // 現在のアクション状態をBlackBoardに記録
            blackBoard.SetValue("current_action", "NormalAttack");

            // ターゲットを取得
            GameObject target = blackBoard.GetValue<GameObject>("nearest_enemy");
            if (target == null)
            {
                target = blackBoard.GetValue<GameObject>("current_target");
            }

            if (target == null || !target.activeInHierarchy)
            {
                blackBoard.SetValue("current_action", "Idle");
                BTLogger.LogCombat("NormalAttack: No valid target found", Name, ownerComponent);
                return BTNodeResult.Failure;
            }

            // ターゲット情報をBlackBoardに更新
            blackBoard.SetValue("current_target", target);
            blackBoard.SetValue("is_in_combat", true);

            // クールダウンチェック
            if (Time.time - lastAttackTime < cooldown)
            {
                blackBoard.SetValue("attack_cooldown_remaining", cooldown - (Time.time - lastAttackTime));
                return BTNodeResult.Running;
            }

            // 通常攻撃実行
            Health targetHealth = target.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
                lastAttackTime = Time.time;

                // BlackBoardに攻撃情報を記録
                blackBoard.SetValue("last_normal_attack_time", Time.time);
                blackBoard.SetValue("normal_attack_count", blackBoard.GetValue<int>("normal_attack_count", 0) + 1);
                blackBoard.SetValue("last_damage_dealt", damage);
                blackBoard.SetValue("attack_cooldown_remaining", 0f);

                BTLogger.LogCombat($"NormalAttack: Normal attack on '{target.name}' for {damage} damage", Name, ownerComponent);
                return BTNodeResult.Success;
            }
            else
            {
                blackBoard.SetValue("current_action", "Idle");
                BTLogger.LogError(LogCategory.Combat, $"NormalAttack: Target '{target.name}' has no Health component", Name, ownerComponent);
                return BTNodeResult.Failure;
            }
        }

        public override void Reset()
        {
            base.Reset();
            lastAttackTime = 0f;
        }
    }
}