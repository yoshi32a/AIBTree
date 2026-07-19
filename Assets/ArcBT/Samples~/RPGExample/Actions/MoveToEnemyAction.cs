using System;
using ArcBT.Core;
using ArcBT.Logger;
using UnityEngine;

namespace ArcBT.Samples.RPG.Actions
{
    /// <summary>BlackBoardから敵位置を取得して移動するアクション</summary>
    [BTNode("MoveToEnemy")]
    public class MoveToEnemyAction : BTActionNode
    {
        float speed = 15.0f;
        float tolerance = 1.0f;
        Vector3 targetPosition;

        // ログ最適化用
        float lastLoggedDistance = -1f;
        float lastLogTime = 0f;

        public override void SetProperty(string key, string value)
        {
            switch (key.ToLowerInvariant())
            {
                case "speed":
                    speed = Convert.ToSingle(value);
                    break;
                case "tolerance":
                    tolerance = Convert.ToSingle(value);
                    break;
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            if (ownerComponent == null || blackBoard == null)
            {
                BTLogger.LogSystemError("Movement", "MoveToEnemy: Owner or BlackBoard is null");
                return BTNodeResult.Failure;
            }

            // BlackBoardから敵の位置情報を取得
            GameObject enemyTarget = blackBoard.GetValue<GameObject>("enemy_target");
            if (enemyTarget == null)
            {
                BTLogger.LogMovement("MoveToEnemy: No enemy target in BlackBoard", Name, ownerComponent);
                return BTNodeResult.Failure;
            }

            // 敵が生きているかチェック
            if (enemyTarget == null || !enemyTarget.activeInHierarchy)
            {
                blackBoard.SetValue("has_enemy_info", false);
                blackBoard.SetValue<GameObject>("enemy_target", null);
                BTLogger.LogMovement("MoveToEnemy: Enemy target is destroyed or inactive", Name, ownerComponent);
                return BTNodeResult.Failure;
            }

            // リアルタイムで敵の位置を更新
            targetPosition = enemyTarget.transform.position;
            blackBoard.SetValue("enemy_location", targetPosition);

            float distance = Vector3.Distance(transform.position, targetPosition);

            // 目標に到達したかチェック
            if (distance <= tolerance)
            {
                BTLogger.LogMovement($"MoveToEnemy: Reached enemy '{enemyTarget.name}'", Name, ownerComponent);
                return BTNodeResult.Success;
            }

            // 敵に向かって移動
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            // 敵の方向を向く
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }

            // スマートログ: 距離に大きな変化があった場合か、3秒間隔でのみログ出力
            bool shouldLog = lastLoggedDistance < 0 || // 初回
                             Mathf.Abs(distance - lastLoggedDistance) > 0.5f || // 0.5m以上の変化
                             Time.time - lastLogTime > 3f; // 3秒間隔

            if (shouldLog)
            {
                BTLogger.LogMovement($"🏃 MoveToEnemy: '{enemyTarget.name}' へ移動中 (距離: {distance:F1}m)", Name, ownerComponent);
                lastLoggedDistance = distance;
                lastLogTime = Time.time;
            }

            return BTNodeResult.Running;
        }

        void OnDrawGizmosSelected()
        {
            if (ownerComponent != null && blackBoard != null)
            {
                var enemy = blackBoard.GetValue<GameObject>("enemy_target");
                if (enemy != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(transform.position, enemy.transform.position);
                    Gizmos.DrawWireSphere(enemy.transform.position, tolerance);
                }
            }
        }
    }
}