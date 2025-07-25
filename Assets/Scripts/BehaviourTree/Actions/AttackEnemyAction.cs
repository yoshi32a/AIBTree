using UnityEngine;
using BehaviourTree.Core;

[System.Serializable]
public class AttackEnemyAction : BTActionNode
{
    [SerializeField] int damage = 25;
    [SerializeField] float attackRange = 2.0f;
    [SerializeField] float cooldown = 1.0f;
    [SerializeField] string attackType = "melee";

    float lastAttackTime = 0f;
    
    public override void Initialize(MonoBehaviour owner)
    {
        base.Initialize(owner);
    }
    
    protected override BTNodeResult ExecuteAction()
    {
        Debug.Log($"=== AttackEnemyAction '{Name}' EXECUTING ===");
        
        // クールダウンチェック
        if (Time.time - lastAttackTime < cooldown)
        {
            var remainingCooldown = cooldown - (Time.time - lastAttackTime);
            Debug.Log($"AttackEnemy '{Name}': On cooldown ({remainingCooldown:F1}s remaining)");
            return BTNodeResult.Running;
        }
        
        // 攻撃範囲内の敵を検索
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
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
            Debug.Log($"AttackEnemy '{Name}': No enemies in attack range {attackRange} ✗");
            return BTNodeResult.Failure;
        }
        
        // 攻撃実行
        Debug.Log($"AttackEnemy '{Name}': Attacking {targetEnemy.name} with {damage} {attackType} damage ⚔️");
        
        // 敵にダメージを与える
        var enemyHealth = targetEnemy.GetComponent<Health>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
            Debug.Log($"AttackEnemy '{Name}': Dealt {damage} damage to {targetEnemy.name}");
        }
        else
        {
            Debug.Log($"AttackEnemy '{Name}': Enemy has no Health component - attack missed");
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
