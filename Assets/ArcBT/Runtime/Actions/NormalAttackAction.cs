using UnityEngine;
using ArcBT.Core;

namespace ArcBT.Actions
{
    [BTScript("NormalAttack")]
    public class NormalAttackAction : BTActionNode
    {
        float damage = 10f;
        float range = 2f;

        public override void SetProperty(string key, string value)
        {
            switch (key.ToLower())
            {
                case "damage":
                    if (float.TryParse(value, out float d)) damage = d;
                    break;
                case "range":
                    if (float.TryParse(value, out float r)) range = r;
                    break;
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            GameObject nearestEnemy = null;
            float nearestDistance = float.MaxValue;

            foreach (var enemy in enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= range && distance < nearestDistance)
                {
                    nearestEnemy = enemy;
                    nearestDistance = distance;
                }
            }

            if (nearestEnemy != null)
            {
                BTLogger.LogCombat($"通常攻撃実行: ダメージ{damage}", Name);
                
                // ダメージ処理の実装（リフレクションを使用）
                var healthComponent = nearestEnemy.GetComponent("Health");
                if (healthComponent != null)
                {
                    var takeDamageMethod = healthComponent.GetType().GetMethod("TakeDamage");
                    if (takeDamageMethod != null)
                    {
                        takeDamageMethod.Invoke(healthComponent, new object[] { damage });
                    }
                }

                return BTNodeResult.Success;
            }

            BTLogger.LogCombat("攻撃範囲内に敵がいません", Name);
            return BTNodeResult.Failure;
        }
    }
}