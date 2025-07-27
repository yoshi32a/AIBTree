using System;
using ArcBT.Core;
using ArcBT.TagSystem;
using UnityEngine;

namespace ArcBT.Samples.RPG.Conditions
{
    /// <summary>攻撃範囲内に敵がいるかチェックする条件</summary>
    [BTNode("EnemyInRange")]
    public class EnemyInRangeCondition : BTConditionNode
    {
        float attackRange = 5.0f;

        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "attack_range":
                    attackRange = Convert.ToSingle(value);
                    break;
            }
        }

        protected override BTNodeResult CheckCondition()
        {
            if (ownerComponent == null)
            {
                return BTNodeResult.Failure;
            }

            // 指定範囲内の敵を検索（タグベース）
            Collider[] colliders = Physics.OverlapSphere(transform.position, attackRange);
            GameObject nearestEnemy = null;
            float nearestDistance = float.MaxValue;

            foreach (var collider in colliders)
            {
                if (collider.gameObject.CompareGameplayTag("Character.Enemy"))
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestEnemy = collider.gameObject;
                    }
                }
            }

            if (nearestEnemy != null && blackBoard != null)
            {
                blackBoard.SetValue("nearest_enemy", nearestEnemy);
                blackBoard.SetValue("enemy_distance", nearestDistance);
                blackBoard.SetValue("in_combat_range", true);
                return BTNodeResult.Success;
            }
            else
            {
                if (blackBoard != null)
                {
                    blackBoard.SetValue<GameObject>("nearest_enemy", null);
                    blackBoard.SetValue("in_combat_range", false);
                }
                return BTNodeResult.Failure;
            }
        }
    }
}