using ArcBT.Core;
using ArcBT.Logger;
using ArcBT.Samples.RPG.Interfaces;
using ArcBT.TagSystem;
using UnityEngine;

namespace ArcBT.Samples.RPG.Actions
{
    [BTNode("NormalAttack")]
    public class NormalAttackAction : BTActionNode
    {
        float damage = 10f;
        float range = 2f;

        public override void SetProperty(string key, string value)
        {
            switch (key.ToLower())
            {
                case "damage":
                    if (float.TryParse(value, out var d)) damage = d;
                    break;
                case "range":
                    if (float.TryParse(value, out var r)) range = r;
                    break;
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            var enemies = GameplayTagManager.FindGameObjectsWithTag("Character.Enemy");
            GameObject nearestEnemy = null;
            var nearestDistance = float.MaxValue;

            foreach (var enemy in enemies)
            {
                var distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= range && distance < nearestDistance)
                {
                    nearestEnemy = enemy;
                    nearestDistance = distance;
                }
            }

            if (nearestEnemy != null)
            {
                BTLogger.LogCombat($"通常攻撃実行: ダメージ{damage}", Name);

                // ダメージ処理の実装（インターフェース使用でリフレクション排除）
                var health = nearestEnemy.GetComponent<IHealth>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                }
                else
                {
                    // 後方互換性のためのフォールバック（将来的に削除予定）
                    BTLogger.Log(Microsoft.Extensions.Logging.LogLevel.Warning, LogCategory.Combat,
                        "Enemy does not implement IHealth interface. Consider updating Health component.", Name);
                }

                return BTNodeResult.Success;
            }

            BTLogger.LogCombat("攻撃範囲内に敵がいません", Name);
            return BTNodeResult.Failure;
        }
    }
}
