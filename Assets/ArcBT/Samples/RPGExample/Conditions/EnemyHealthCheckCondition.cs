using ArcBT.Core;
using ArcBT.Logger;
using ArcBT.Samples.RPG.Interfaces;
using UnityEngine;

namespace ArcBT.Samples.RPG.Conditions
{
    [BTNode("EnemyHealthCheck")]
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
                    if (float.TryParse(value, out var h)) minHealthPercent = h;
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
                var enemyInfo = blackBoard.GetValue<GameObject>("target_enemy");
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
                // インターフェースを使用してHealthコンポーネントを取得（リフレクション排除）
                var health = targetEnemy.GetComponent<IHealth>();
                if (health != null)
                {
                    var healthPercent = (health.CurrentHealth / health.MaxHealth) * 100f;
                    var result = healthPercent >= minHealthPercent;

                    BTLogger.LogCondition($"敵の体力: {healthPercent:F1}% (最低要求: {minHealthPercent}%)", Name);
                    return result ? BTNodeResult.Success : BTNodeResult.Failure;
                }
                else
                {
                    BTLogger.Log(LogLevel.Warning, LogCategory.Condition,
                        "Enemy does not implement IHealth interface.", Name);
                }
            }

            BTLogger.LogCondition("敵またはHealthコンポーネントが見つかりません", Name);
            return BTNodeResult.Failure;
        }
    }
}
