using System;
using ArcBT.Core;
using ArcBT.Samples.RPG.Components;
using UnityEngine;

namespace ArcBT.Samples.RPG.Actions
{
    /// <summary>汎用攻撃アクション</summary>
    [BTNode("Attack", NodeType.Action)]
    public class AttackAction : BTActionNode    {
        int damage = 25;
        float attackRange = 2.0f;
        float cooldown = 1.0f;
        float lastAttackTime = 0f;

        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "damage":
                    damage = Convert.ToInt32(value);
                    break;
                case "attack_range":
                    attackRange = Convert.ToSingle(value);
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

            // ターゲットを取得
            GameObject target = blackBoard.GetValue<GameObject>("current_target");
            if (target == null)
            {
                target = blackBoard.GetValue<GameObject>("enemy_target");
            }

            if (target == null || !target.activeInHierarchy)
            {
                BTLogger.LogCombat("No valid target found");
                return BTNodeResult.Failure;
            }

            // 攻撃範囲チェック
            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance > attackRange)
            {
                BTLogger.LogCombat($"Target out of range ({distance:F1} > {attackRange})");
                return BTNodeResult.Failure;
            }

            // クールダウンチェック
            if (Time.time - lastAttackTime < cooldown)
            {
                return BTNodeResult.Running;
            }

            // 攻撃実行
            Health targetHealth = target.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
                lastAttackTime = Time.time;

                // ターゲットの方向を向く
                Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
                if (directionToTarget != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(directionToTarget);
                }

                // BlackBoardに攻撃情報を記録
                blackBoard.SetValue("last_attack_time", Time.time);
                blackBoard.SetValue("last_attack_damage", damage);
                blackBoard.SetValue("target_remaining_health", targetHealth.CurrentHealth);

                BTLogger.LogCombat($"Attacked '{target.name}' for {damage} damage. Target health: {targetHealth.CurrentHealth}");

                // ターゲットが死んだ場合
                if (targetHealth.CurrentHealth <= 0)
                {
                    blackBoard.SetValue("target_defeated", true);
                    blackBoard.SetValue("current_target", (GameObject)null);
                    BTLogger.LogCombat($"Target '{target.name}' defeated");
                }

                return BTNodeResult.Success;
            }
            else
            {
                BTLogger.Log(LogLevel.Warning, LogCategory.Combat, $"Target '{target.name}' has no Health component", "Attack");
                return BTNodeResult.Failure;
            }
        }

        public override void Reset()
        {
            base.Reset();
            lastAttackTime = 0f;
        }

        void OnDrawGizmosSelected()
        {
            if (ownerComponent != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, attackRange);
            }
        }
    }
}
