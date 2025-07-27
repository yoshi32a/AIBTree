using System;
using ArcBT.Core;
using UnityEngine;

namespace ArcBT.Actions
{
    /// <summary>環境をスキャンするアクション</summary>
    [BTNode("ScanEnvironment")]
    public class ScanEnvironmentAction : BTActionNode    {
        float scanInterval = 2.0f;
        float scanRadius = 15.0f;
        float lastScanTime = 0f;

        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "scan_interval":
                    scanInterval = Convert.ToSingle(value);
                    break;
                case "scan_radius":
                    scanRadius = Convert.ToSingle(value);
                    break;
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            if (ownerComponent == null || blackBoard == null)
            {
                return BTNodeResult.Failure;
            }

            // スキャン間隔チェック
            if (Time.time - lastScanTime < scanInterval)
            {
                return BTNodeResult.Running;
            }

            lastScanTime = Time.time;

            // 環境をスキャン
            var enemyCount = 0;
            var itemCount = 0;
            var interactableCount = 0;

            // 敵を検索
            var enemies = Physics.OverlapSphere(transform.position, scanRadius, LayerMask.GetMask("Enemy"));
            enemyCount = enemies.Length;

            if (enemyCount > 0)
            {
                // 最も近い敵を記録
                GameObject nearestEnemy = null;
                var nearestDistance = float.MaxValue;

                foreach (var enemy in enemies)
                {
                    var distance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestEnemy = enemy.gameObject;
                    }
                }

                if (nearestEnemy != null)
                {
                    blackBoard.SetValue("scanned_enemy", nearestEnemy);
                    blackBoard.SetValue("scanned_enemy_distance", nearestDistance);
                }
            }

            // アイテムを検索
            var items = Physics.OverlapSphere(transform.position, scanRadius, LayerMask.GetMask("Item"));
            itemCount = items.Length;

            if (itemCount > 0)
            {
                blackBoard.SetValue("scanned_items", items.Length);
                blackBoard.SetValue("nearest_item", items[0].gameObject);
            }

            // インタラクト可能オブジェクトを検索
            var interactables = Physics.OverlapSphere(transform.position, scanRadius, LayerMask.GetMask("Interactable"));
            interactableCount = interactables.Length;

            // スキャン結果をBlackBoardに記録
            blackBoard.SetValue("environment_scan_time", Time.time);
            blackBoard.SetValue("enemies_detected", enemyCount);
            blackBoard.SetValue("items_detected", itemCount);
            blackBoard.SetValue("interactables_detected", interactableCount);
            blackBoard.SetValue("scan_radius_used", scanRadius);

            // 環境の脅威レベルを計算
            var threatLevel = "safe";
            if (enemyCount > 3)
                threatLevel = "high";
            else if (enemyCount > 1)
                threatLevel = "medium";
            else if (enemyCount > 0)
                threatLevel = "low";

            blackBoard.SetValue("threat_level", threatLevel);

            Debug.Log($"EnvironmentScan: Enemies:{enemyCount}, Items:{itemCount}, Threat:{threatLevel}");

            return BTNodeResult.Success;
        }

        public override void Reset()
        {
            base.Reset();
            lastScanTime = 0f;
        }

        void OnDrawGizmosSelected()
        {
            if (ownerComponent != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, scanRadius);
            }
        }
    }
}
