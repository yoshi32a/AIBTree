using UnityEngine;
using ArcBT.Core;

namespace ArcBT.Conditions
{
    [BTScript("EnemyHealthCheck")]
    public class EnemyHealthCheckCondition : BTConditionNode
    {
        float minHealthPercent = 50f;
        string targetTag = "Enemy";

        public override void SetProperty(string key, string value)
        {
            switch (key.ToLower())
            {
                case "min_health":
                case "min_health_percent":
                    if (float.TryParse(value, out float h)) minHealthPercent = h;
                    break;
                case "target_tag":
                    targetTag = value;
                    break;
            }
        }

        protected override BTNodeResult CheckCondition()
        {
            // BlackBoardから敵情報を取得
            GameObject targetEnemy = null;
            
            if (blackBoard != null)
            {
                var enemyInfo = blackBoard.GetValue<GameObject>("target_enemy", null);
                if (enemyInfo != null)
                {
                    targetEnemy = enemyInfo;
                }
            }

            // BlackBoardに情報がない場合はシーンから検索
            if (targetEnemy == null)
            {
                var enemies = GameObject.FindGameObjectsWithTag(targetTag);
                if (enemies.Length > 0)
                {
                    targetEnemy = enemies[0]; // 最初の敵を選択
                }
            }

            if (targetEnemy != null)
            {
                // リフレクションを使用してHealthコンポーネントを取得
                var healthComponent = targetEnemy.GetComponent("Health");
                if (healthComponent != null)
                {
                    var type = healthComponent.GetType();
                    var currentHealthProp = type.GetProperty("CurrentHealth");
                    var maxHealthProp = type.GetProperty("MaxHealth");
                    
                    if (currentHealthProp != null && maxHealthProp != null)
                    {
                        float currentHealth = (float)currentHealthProp.GetValue(healthComponent);
                        float maxHealth = (float)maxHealthProp.GetValue(healthComponent);
                        float healthPercent = (currentHealth / maxHealth) * 100f;
                        bool result = healthPercent >= minHealthPercent;
                        
                        BTLogger.LogCondition($"敵の体力: {healthPercent:F1}% (最低要求: {minHealthPercent}%)", Name);
                        return result ? BTNodeResult.Success : BTNodeResult.Failure;
                    }
                }
            }

            BTLogger.LogCondition("敵またはHealthコンポーネントが見つかりません", Name);
            return BTNodeResult.Failure;
        }
    }
}