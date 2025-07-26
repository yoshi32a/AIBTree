using System;
using ArcBT.Core;
using UnityEngine;

namespace ArcBT.Samples.RPG.Actions
{
    /// <summary>ターゲットに移動するアクション</summary>
    [BTNode("MoveToTarget", NodeType.Action)]
    public class MoveToTargetAction : BTActionNode    {
        string moveType = "normal";
        float speed = 20.0f; // 高速化（検証時間短縮）
        float tolerance = 1.0f;
        
        MovementController movementController;

        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "move_type":
                    moveType = value;
                    break;
                case "speed":
                    speed = Convert.ToSingle(value);
                    break;
                case "tolerance":
                    tolerance = Convert.ToSingle(value);
                    break;
            }
        }

        public override void Initialize(MonoBehaviour owner, BlackBoard sharedBlackBoard = null)
        {
            base.Initialize(owner, sharedBlackBoard);
            
            // MovementControllerを取得または追加
            movementController = owner.GetComponent<MovementController>();
            if (movementController == null)
            {
                movementController = owner.gameObject.AddComponent<MovementController>();
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            if (ownerComponent == null || blackBoard == null || movementController == null)
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

            // MovementControllerで移動中でない場合、新しい目標を設定
            if (!movementController.IsMoving)
            {
                movementController.SetTarget(targetPosition, speed);
                movementController.OnTargetReached = () => {
                    Debug.Log($"MoveToTarget: Reached target ({moveType})");
                };
            }

            // 移動完了チェック
            if (blackBoard.GetValue<bool>("move_completed", false))
            {
                blackBoard.SetValue("move_completed", false); // リセット
                return BTNodeResult.Success;
            }

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
