using System;
using ArcBT.Core;
using ArcBT.Logger;
using ArcBT.Samples.RPG.Components;
using UnityEngine;

namespace ArcBT.Samples.RPG.Actions
{
    /// <summary>敵を検索するアクション</summary>
    [BTNode("SearchForEnemy")]
    public class SearchForEnemyAction : BTActionNode
    {
        float searchRadius = 15.0f;
        float searchDuration = 5.0f;
        float searchStartTime = 0f;
        bool isSearching = false;

        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "search_radius":
                    searchRadius = Convert.ToSingle(value);
                    break;
                case "search_duration":
                    searchDuration = Convert.ToSingle(value);
                    break;
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            if (ownerComponent == null || blackBoard == null)
            {
                return BTNodeResult.Failure;
            }

            // 検索開始
            if (!isSearching)
            {
                isSearching = true;
                searchStartTime = Time.time;
                blackBoard.SetValue("is_searching", true);
                BTLogger.LogSystem("SearchForEnemy: Started enemy search", Name, ownerComponent);
            }

            // 敵を検索
            Collider[] enemies = Physics.OverlapSphere(transform.position, searchRadius, LayerMask.GetMask("Enemy"));

            if (enemies.Length > 0)
            {
                // 敵が見つかった場合
                GameObject nearestEnemy = null;
                float nearestDistance = float.MaxValue;

                foreach (var enemy in enemies)
                {
                    if (enemy.GetComponent<Health>()?.IsAlive == true)
                    {
                        float distance = Vector3.Distance(transform.position, enemy.transform.position);
                        if (distance < nearestDistance)
                        {
                            nearestDistance = distance;
                            nearestEnemy = enemy.gameObject;
                        }
                    }
                }

                if (nearestEnemy != null)
                {
                    // 敵をBlackBoardに記録
                    blackBoard.SetValue("found_enemy", nearestEnemy);
                    blackBoard.SetValue("enemy_distance", nearestDistance);
                    blackBoard.SetValue("enemy_last_seen_position", nearestEnemy.transform.position);
                    blackBoard.SetValue("enemy_search_success", true);

                    // アラート状態に設定
                    blackBoard.SetValue("is_alert", true);
                    blackBoard.SetValue("alert_reason", "enemy_found");

                    BTLogger.LogSystem($"SearchForEnemy: Found enemy '{nearestEnemy.name}' at distance {nearestDistance:F1}", Name, ownerComponent);

                    // 検索完了
                    isSearching = false;
                    blackBoard.SetValue("is_searching", false);
                    return BTNodeResult.Success;
                }
            }

            // 検索時間チェック
            if (Time.time - searchStartTime >= searchDuration)
            {
                // 検索時間終了、敵が見つからなかった
                blackBoard.SetValue("enemy_search_success", false);
                blackBoard.SetValue("is_searching", false);
                blackBoard.SetValue("last_search_time", Time.time);

                BTLogger.LogSystem("SearchForEnemy: Search completed, no enemies found", Name, ownerComponent);

                isSearching = false;
                return BTNodeResult.Failure;
            }

            // 検索中の視覚的フィードバック
            float searchProgress = (Time.time - searchStartTime) / searchDuration;
            blackBoard.SetValue("search_progress", searchProgress);

            // 検索中は少しずつ回転して周囲を見回す
            transform.Rotate(0, 30 * Time.deltaTime, 0);

            return BTNodeResult.Running;
        }

        public override void Reset()
        {
            base.Reset();
            isSearching = false;
            blackBoard?.SetValue("is_searching", false);
        }

        void OnDrawGizmosSelected()
        {
            if (ownerComponent != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, searchRadius);

                if (isSearching)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireCube(transform.position + Vector3.up * 2, Vector3.one * 0.5f);
                }
            }
        }
    }
}
