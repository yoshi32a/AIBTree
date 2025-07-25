using UnityEngine;
using BehaviourTree.Core;

namespace BehaviourTree.Actions
{
    /// <summary>
    /// BlackBoardの敵情報を使用して攻撃するアクション
    /// </summary>
    public class AttackTargetAction : BTActionNode
    {
        float damage = 25.0f;
        float attackRange = 2.0f;
        float attackCooldown = 1.0f;
        float lastAttackTime = 0f;
        
        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "damage":
                    damage = System.Convert.ToSingle(value);
                    break;
                case "attack_range":
                    attackRange = System.Convert.ToSingle(value);
                    break;
                case "cooldown":
                    attackCooldown = System.Convert.ToSingle(value);
                    break;
            }
        }
        
        protected override BTNodeResult ExecuteAction()
        {
            if (ownerComponent == null || blackBoard == null)
            {
                Debug.LogError("AttackTarget: Owner or BlackBoard is null");
                return BTNodeResult.Failure;
            }
            
            // BlackBoardから敵ターゲットを取得
            GameObject enemyTarget = blackBoard.GetValue<GameObject>("enemy_target");
            if (enemyTarget == null)
            {
                Debug.Log("AttackTarget: No enemy target in BlackBoard");
                return BTNodeResult.Failure;
            }
            
            // 敵が生きているかチェック
            if (!enemyTarget.activeInHierarchy)
            {
                blackBoard.SetValue("has_enemy_info", false);
                blackBoard.SetValue("enemy_target", (GameObject)null);
                Debug.Log("AttackTarget: Enemy target is destroyed");
                return BTNodeResult.Failure;
            }
            
            // 攻撃範囲内かチェック
            float distance = Vector3.Distance(transform.position, enemyTarget.transform.position);
            if (distance > attackRange)
            {
                Debug.Log($"AttackTarget: Enemy '{enemyTarget.name}' out of range ({distance:F1} > {attackRange})");
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
                Debug.LogWarning($"AttackTarget: Enemy '{enemyTarget.name}' has no Health component");
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
            
            Debug.Log($"AttackTarget: Attacked '{enemyTarget.name}' for {damage} damage. Enemy health: {enemyHealth.CurrentHealth}");
            
            // 敵が死んだらBlackBoardをクリア
            if (enemyHealth.CurrentHealth <= 0)
            {
                blackBoard.SetValue("has_enemy_info", false);
                blackBoard.SetValue("enemy_target", (GameObject)null);
                Debug.Log($"AttackTarget: Enemy '{enemyTarget.name}' destroyed");
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