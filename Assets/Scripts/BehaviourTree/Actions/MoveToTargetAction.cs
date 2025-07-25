using UnityEngine;
using BehaviourTree.Core;

namespace BehaviourTree.Actions
{
    /// <summary>ターゲットに移動するアクション</summary>
    public class MoveToTargetAction : BTActionNode
    {
        string moveType = "normal";
        float speed = 2.0f;
        float tolerance = 1.0f;
        
        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "move_type":
                    moveType = value;
                    break;
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
                return BTNodeResult.Failure;
            }
            
            // 移動タイプに応じてターゲットを取得
            GameObject target = null;
            Vector3 targetPosition = Vector3.zero;
            
            switch (moveType)
            {
                case "investigate":
                    target = blackBoard.GetValue<GameObject>("interest_target");
                    break;
                case "enemy":
                    target = blackBoard.GetValue<GameObject>("enemy_target");
                    break;
                case "current":
                    target = blackBoard.GetValue<GameObject>("current_target");
                    break;
                default:
                    target = blackBoard.GetValue<GameObject>("move_target");
                    break;
            }
            
            if (target != null && target.activeInHierarchy)
            {
                targetPosition = target.transform.position;
            }
            else
            {
                // Vector3形式のターゲット位置を取得
                targetPosition = blackBoard.GetValue<Vector3>("target_position", Vector3.zero);
                if (targetPosition == Vector3.zero)
                {
                    Debug.Log($"MoveToTarget: No valid target for move type '{moveType}'");
                    return BTNodeResult.Failure;
                }
            }
            
            // 目標位置までの距離をチェック
            float distance = Vector3.Distance(transform.position, targetPosition);
            if (distance <= tolerance)
            {
                // 目標に到達
                blackBoard.SetValue("move_completed", true);
                blackBoard.SetValue("arrived_at_target", true);
                Debug.Log($"MoveToTarget: Reached target ({moveType})");
                return BTNodeResult.Success;
            }
            
            // 移動処理
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
            
            // 移動方向を向く
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
            
            // BlackBoardに移動状態を記録
            blackBoard.SetValue("is_moving", true);
            blackBoard.SetValue("move_distance_remaining", distance);
            blackBoard.SetValue("move_direction", direction);
            
            return BTNodeResult.Running;
        }
        
        public override void Reset()
        {
            base.Reset();
            blackBoard?.SetValue("is_moving", false);
        }
        
        void OnDrawGizmosSelected()
        {
            if (ownerComponent != null && blackBoard != null)
            {
                Vector3 targetPos = blackBoard.GetValue<Vector3>("target_position", Vector3.zero);
                if (targetPos != Vector3.zero)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(transform.position, targetPos);
                    Gizmos.DrawWireSphere(targetPos, tolerance);
                }
            }
        }
    }
}
