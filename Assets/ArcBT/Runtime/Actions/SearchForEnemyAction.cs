using ArcBT.Core;
using ArcBT.Logger;
using ArcBT.TagSystem;
using UnityEngine;

namespace ArcBT.Actions
{
    [BTNode("SearchForEnemy")]
    public class SearchForEnemyAction : BTActionNode
    {
        float searchRange = 10f;
        GameplayTag enemyTag = "Character.Enemy";

        public override void SetProperty(string key, string value)
        {
            switch (key.ToLower())
            {
                case "range":
                case "search_range":
                    if (float.TryParse(value, out var r)) searchRange = r;
                    break;
                case "tag":
                case "enemy_tag":
                    enemyTag = new GameplayTag(value);
                    break;
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            var enemies = GameplayTagManager.FindGameObjectsWithTag(enemyTag);
            GameObject nearestEnemy = null;
            var nearestDistance = float.MaxValue;

            foreach (var enemy in enemies)
            {
                var distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= searchRange && distance < nearestDistance)
                {
                    nearestEnemy = enemy;
                    nearestDistance = distance;
                }
            }

            if (nearestEnemy != null)
            {
                // BlackBoardに敵情報を記録
                if (blackBoard != null)
                {
                    blackBoard.SetValue("target_enemy", nearestEnemy);
                    blackBoard.SetValue("enemy_distance", nearestDistance);
                    blackBoard.SetValue("last_search_time", Time.time);
                }

                BTLogger.LogCombat($"敵を発見: {nearestEnemy.name} (距離: {nearestDistance:F1})", Name);
                return BTNodeResult.Success;
            }

            BTLogger.LogCombat($"敵が見つかりません (検索範囲: {searchRange})", Name);
            return BTNodeResult.Failure;
        }
    }
}
