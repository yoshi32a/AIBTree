using UnityEngine;
using ArcBT.Core;
using ArcBT.Samples.RPG.Components;

namespace ArcBT.Samples.RPG.Conditions
{
    /// <summary>敵の体力をチェックする条件</summary>
    public class EnemyHealthCheckCondition : BTConditionNode
    {
        int minHealth = 80;

        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "min_health":
                    minHealth = System.Convert.ToInt32(value);
                    break;
            }
        }

        protected override BTNodeResult CheckCondition()
        {
            if (ownerComponent == null || blackBoard == null)
            {
                return BTNodeResult.Failure;
            }

            // BlackBoardから敵ターゲットを取得
            GameObject enemy = blackBoard.GetValue<GameObject>("enemy_target");
            if (enemy == null)
            {
                enemy = blackBoard.GetValue<GameObject>("current_target");
            }

            if (enemy == null)
            {
                return BTNodeResult.Failure;
            }

            // 敵の体力コンポーネントを取得
            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth == null)
            {
                Debug.LogWarning($"EnemyHealthCheck: Enemy '{enemy.name}' has no Health component");
                return BTNodeResult.Failure;
            }

            int currentHealth = enemyHealth.CurrentHealth;
            bool healthSufficient = currentHealth >= minHealth;

            // BlackBoardに敵の体力情報を記録
            blackBoard.SetValue("enemy_current_health", currentHealth);
            blackBoard.SetValue("enemy_health_sufficient", healthSufficient);

            return healthSufficient ? BTNodeResult.Success : BTNodeResult.Failure;
        }
    }
}