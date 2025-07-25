using UnityEngine;
using BehaviourTree.Core;

namespace BehaviourTree.Conditions
{
    /// <summary>
    /// 攻撃範囲内に敵がいるかチェックする条件
    /// </summary>
    public class EnemyInRangeCondition : BTConditionNode
    {
        float attackRange = 5.0f;
        
        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "attack_range":
                    attackRange = System.Convert.ToSingle(value);
                    break;
            }
        }
        
        protected override BTNodeResult CheckCondition()
        {
            // 指定範囲内の敵を検索
            Collider[] enemies = Physics.OverlapSphere(transform.position, attackRange, LayerMask.GetMask("Enemy"));
            
            bool hasEnemyInRange = enemies.Length > 0;
            
            if (blackBoard != null && hasEnemyInRange)
            {
                blackBoard.SetValue("nearest_enemy", enemies[0].gameObject);
                blackBoard.SetValue("enemy_distance", Vector3.Distance(transform.position, enemies[0].transform.position));
            }
            
            return hasEnemyInRange ? BTNodeResult.Success : BTNodeResult.Failure;
        }
    }
}