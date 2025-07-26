using UnityEngine;
using BehaviourTree.Core;

namespace BehaviourTree.Actions
{
    /// <summary>環境をスキャンして敵情報をBlackBoardに保存するアクション</summary>
    public class ScanEnvironmentAction : BTActionNode
    {
        float scanRadius = 10.0f;
        bool hasLoggedStart = false;

        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "scan_radius":
                    scanRadius = System.Convert.ToSingle(value);
                    break;
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            if (ownerComponent == null || blackBoard == null)
            {
                Debug.LogError("❌ ScanEnvironment: Owner or BlackBoard is null");
                return BTNodeResult.Failure;
            }

            // 初回実行時のみログ出力
            if (!hasLoggedStart)
            {
                Debug.Log($"🔍 ScanEnvironment: 環境スキャン開始 (範囲: {scanRadius}m)");
                hasLoggedStart = true;
            }

            // 周囲の敵を検索
            Collider[] enemies = Physics.OverlapSphere(transform.position, scanRadius, LayerMask.GetMask("Default"));
            GameObject closestEnemy = null;
            float closestDistance = float.MaxValue;

            foreach (var collider in enemies)
            {
                if (collider.CompareTag("Enemy"))
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = collider.gameObject;
                    }
                }
            }

            // BlackBoardに敵情報を保存（状態変化時のみログ）
            if (closestEnemy != null)
            {
                GameObject previousTarget = null;
                if (blackBoard != null)
                {
                    previousTarget = blackBoard.GetValue<GameObject>("enemy_target", null);
                    blackBoard.SetValue("enemy_target", closestEnemy);
                    blackBoard.SetValue("enemy_location", closestEnemy.transform.position);
                    blackBoard.SetValue("enemy_distance", closestDistance);
                    blackBoard.SetValue("has_enemy_info", true);
                }

                // 新しい敵が見つかった場合のみログ出力
                if (previousTarget != closestEnemy)
                {
                    Debug.Log($"🎯 ScanEnvironment: 新しい敵発見 '{closestEnemy.name}' (距離: {closestDistance:F1}m)");
                }

                return BTNodeResult.Success;
            }
            else
            {
                bool previouslyHadEnemy = false;
                if (blackBoard != null)
                {
                    previouslyHadEnemy = blackBoard.GetValue("has_enemy_info", false);
                    blackBoard.SetValue("has_enemy_info", false);
                    blackBoard.SetValue<GameObject>("enemy_target", null);
                }

                // 敵を見失った場合のみログ出力
                if (previouslyHadEnemy)
                {
                    Debug.Log("❓ ScanEnvironment: 敵を見失いました");
                }

                return BTNodeResult.Failure;
            }
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