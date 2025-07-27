using System;
using ArcBT.Core;
using ArcBT.Logger;
using ArcBT.Samples.RPG.Components;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ArcBT.Samples.RPG.Actions
{
    /// <summary>BlackBoardの敵情報を使用して攻撃するアクション</summary>
    [BTNode("AttackTarget")]
    public class AttackTargetAction : BTActionNode    {
        int damage = 25;
        float attackRange = 2.0f;
        float attackCooldown = 1.0f;
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
                    attackCooldown = Convert.ToSingle(value);
                    break;
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            if (ownerComponent == null || blackBoard == null)
            {
                BTLogger.LogError(LogCategory.Combat, "AttackTarget: Owner or BlackBoard is null", Name, ownerComponent);
                return BTNodeResult.Failure;
            }

            // BlackBoardから敵ターゲットを取得
            GameObject enemyTarget = blackBoard.GetValue<GameObject>("enemy_target");
            if (enemyTarget == null)
            {
                BTLogger.LogCombat("AttackTarget: No enemy target in BlackBoard", Name, ownerComponent);
                return BTNodeResult.Failure;
            }

            // 敵が生きているかチェック
            if (!enemyTarget.activeInHierarchy)
            {
                blackBoard.SetValue("has_enemy_info", false);
                blackBoard.SetValue<GameObject>("enemy_target", null);
                BTLogger.LogCombat("AttackTarget: Enemy target is destroyed", Name, ownerComponent);
                return BTNodeResult.Failure;
            }

            // 攻撃範囲内かチェック
            float distance = Vector3.Distance(transform.position, enemyTarget.transform.position);
            if (distance > attackRange)
            {
                BTLogger.LogCombat($"AttackTarget: Enemy '{enemyTarget.name}' out of range ({distance:F1} > {attackRange})", Name, ownerComponent);
                return BTNodeResult.Failure;
            }

            // クールダウンチェック
            if (Time.time - lastAttackTime < attackCooldown)
            {
                return BTNodeResult.Running;
            }

            // 敵の体力コンポーネントを取得
            var enemyHealth = enemyTarget.GetComponent<Health>();
            if (enemyHealth == null)
            {
                BTLogger.LogError(LogCategory.Combat, $"AttackTarget: Enemy '{enemyTarget.name}' has no Health component", Name, ownerComponent);
                return BTNodeResult.Failure;
            }

            // 攻撃実行
            enemyHealth.TakeDamage(damage);
            lastAttackTime = Time.time;

            // 敵の方向を向く
            Vector3 directionToEnemy = (enemyTarget.transform.position - transform.position).normalized;
            if (directionToEnemy != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(directionToEnemy);
            }

            BTLogger.LogCombat($"AttackTarget: Attacked '{enemyTarget.name}' for {damage} damage. Enemy health: {enemyHealth.CurrentHealth}", Name, ownerComponent);

            // 敵が死んだらBlackBoardをクリアして実際にGameObjectを破壊
            if (enemyHealth.CurrentHealth <= 0)
            {
                string enemyName = enemyTarget.name;
                blackBoard.SetValue("has_enemy_info", false);
                blackBoard.SetValue<GameObject>("enemy_target", null);
                
                // GameObjectを実際に破壊
                Object.DestroyImmediate(enemyTarget);
                
                BTLogger.LogCombat($"💀 AttackTarget: 敵 '{enemyName}' を撃破しました", Name, ownerComponent);
                return BTNodeResult.Success;
            }

            return BTNodeResult.Success;
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
