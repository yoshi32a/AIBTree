using UnityEngine;
using BehaviourTree.Core;

namespace BehaviourTree.Actions
{
    /// <summary>BlackBoardから敵位置を取得して移動するアクション</summary>
    public class MoveToEnemyAction : BTActionNode
    {
        float speed = 3.5f;
        float tolerance = 1.0f;
        Vector3 targetPosition;
        
        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "speed":
                    speed = System.Convert.ToSingle(value);
                    break;
                case "tolerance":
                    tolerance = System.Convert.ToSingle(value);
                    break;
            }
        }
        
        protected override BTNodeResult ExecuteAction()
        {
            if (ownerComponent == null || blackBoard == null)
            {
                Debug.LogError("MoveToEnemy: Owner or BlackBoard is null");
                return BTNodeResult.Failure;
            }
            
            // BlackBoardから敵の位置情報を取得
            GameObject enemyTarget = blackBoard.GetValue<GameObject>("enemy_target");
            if (enemyTarget == null)
            {
                Debug.Log("MoveToEnemy: No enemy target in BlackBoard");
                return BTNodeResult.Failure;
            }
            
            // 敵が生きているかチェック
            if (enemyTarget == null || !enemyTarget.activeInHierarchy)
            {
                blackBoard.SetValue("has_enemy_info", false);
                blackBoard.SetValue("enemy_target", (GameObject)null);
                Debug.Log("MoveToEnemy: Enemy target is destroyed or inactive");
                return BTNodeResult.Failure;
            }
            
            // リアルタイムで敵の位置を更新
            targetPosition = enemyTarget.transform.position;
            blackBoard.SetValue("enemy_location", targetPosition);
            
            float distance = Vector3.Distance(transform.position, targetPosition);
            
            // 目標に到達したかチェック
            if (distance <= tolerance)
            {
                Debug.Log($"MoveToEnemy: Reached enemy '{enemyTarget.name}'");
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
            
            Debug.Log($"MoveToEnemy: Moving to '{enemyTarget.name}' - Distance: {distance:F1}");
            
            return BTNodeResult.Running;
        }
        
        void OnDrawGizmosSelected()
        {
            if (ownerComponent != null && blackBoard != null)
            {
                GameObject enemy = blackBoard.GetValue<GameObject>("enemy_target");
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
