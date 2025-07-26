using UnityEngine;
using ArcBT.Core;
using ArcBT.Samples.RPG.Components;

namespace ArcBT.Samples.RPG.Actions
{
    /// <summary>通常攻撃アクション</summary>
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
                    damage = System.Convert.ToInt32(value);
                    break;
                case "cooldown":
                    cooldown = System.Convert.ToSingle(value);
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
                Debug.Log("NormalAttack: No valid target found");
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

                Debug.Log($"NormalAttack: Normal attack on '{target.name}' for {damage} damage");
                return BTNodeResult.Success;
            }
            else
            {
                blackBoard.SetValue("current_action", "Idle");
                Debug.LogWarning($"NormalAttack: Target '{target.name}' has no Health component");
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