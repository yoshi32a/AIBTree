using System;
using ArcBT.Core;
using ArcBT.Logger;
using ArcBT.Samples.RPG.Components;
using ArcBT.TagSystem;
using UnityEngine;

namespace ArcBT.Samples.RPG.Actions
{
    /// <summary>敵を攻撃するアクション</summary>
    [Serializable]
    [BTNode("AttackEnemy")]
    public class AttackEnemyAction : BTActionNode
    {
        [SerializeField] int damage = 25;
        [SerializeField] float attackRange = 2.0f;
        [SerializeField] float cooldown = 1.0f;
        [SerializeField] string attackType = "melee";

        float lastAttackTime = 0f;

        public override void Initialize(MonoBehaviour owner, BlackBoard sharedBlackBoard = null)
        {
            base.Initialize(owner, sharedBlackBoard);
        }

        protected override BTNodeResult ExecuteAction()
        {
            BTLogger.LogCombat(this, $"=== AttackEnemyAction '{Name}' EXECUTING ===");

            // クールダウンチェック
            if (Time.time - lastAttackTime < cooldown)
            {
                var remainingCooldown = cooldown - (Time.time - lastAttackTime);
                BTLogger.LogCombat(this, $"AttackEnemy '{Name}': On cooldown ({remainingCooldown:F1}s remaining)");
                return BTNodeResult.Running;
            }

            // 攻撃範囲内の敵を検索
            var enemies = GameplayTagManager.FindGameObjectsWithTag("Character.Enemy");
            GameObject targetEnemy = null;
            var closestDistance = float.MaxValue;

            foreach (var enemy in enemies)
            {
                var distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= attackRange && distance < closestDistance)
                {
                    targetEnemy = enemy;
                    closestDistance = distance;
                }
            }

            if (targetEnemy == null)
            {
                BTLogger.LogCombat(this, $"AttackEnemy '{Name}': No enemies in attack range {attackRange} ✗");
                return BTNodeResult.Failure;
            }

            // 攻撃実行
            BTLogger.LogCombat(this, $"AttackEnemy '{Name}': Attacking {targetEnemy.name} with {damage} {attackType} damage ⚔️");

            // 敵にダメージを与える
            var enemyHealth = targetEnemy.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                BTLogger.LogCombat(this, $"AttackEnemy '{Name}': Dealt {damage} damage to {targetEnemy.name}");
            }
            else
            {
                BTLogger.LogCombat(this, $"AttackEnemy '{Name}': Enemy has no Health component - attack missed");
            }

            lastAttackTime = Time.time;
            return BTNodeResult.Success;
        }

        public override void SetProperty(string propertyName, string value)
        {
            switch (propertyName.ToLower())
            {
                case "damage":
                    if (int.TryParse(value, out var dmg))
                    {
                        damage = dmg;
                    }

                    break;
                case "attack_range":
                case "attackrange":
                case "range":
                    if (float.TryParse(value, out var range))
                    {
                        attackRange = range;
                    }

                    break;
                case "cooldown":
                    if (float.TryParse(value, out var cd))
                    {
                        cooldown = cd;
                    }

                    break;
                case "attack_type":
                case "attacktype":
                    attackType = value;
                    break;
                default:
                    base.SetProperty(propertyName, value);
                    break;
            }
        }
    }
}
