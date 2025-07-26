using UnityEngine;

namespace ArcBT.Core
{
    /// <summary>毎フレーム更新される滑らかな移動コントローラー</summary>
    public class MovementController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float defaultSpeed = 12.0f;
        public float rotationSpeed = 15.0f;
        public float tolerance = 1.0f;
        
        [Header("Current State")]
        public Vector3 targetPosition = Vector3.zero;
        public float currentSpeed = 12.0f;
        public bool isMoving = false;
        public bool hasTarget = false;
        
        // 移動完了コールバック
        public System.Action OnTargetReached;
        
        BlackBoard blackBoard;
        
        void Start()
        {
            // BehaviourTreeRunnerのBlackBoardを取得
            var btRunner = GetComponent<BehaviourTreeRunner>();
            if (btRunner != null)
            {
                blackBoard = btRunner.BlackBoard;
            }
        }
        
        void Update()
        {
            if (hasTarget && isMoving)
            {
                ProcessMovement();
            }
        }
        
        /// <summary>移動目標を設定</summary>
        public void SetTarget(Vector3 target, float speed = 0f)
        {
            targetPosition = target;
            currentSpeed = speed > 0 ? speed : defaultSpeed;
            hasTarget = true;
            isMoving = true;
            
            if (blackBoard != null)
            {
                blackBoard.SetValue("movement_target", target);
                blackBoard.SetValue("is_moving", true);
            }
        }
        
        /// <summary>移動を停止</summary>
        public void StopMovement()
        {
            isMoving = false;
            hasTarget = false;
            
            if (blackBoard != null)
            {
                blackBoard.SetValue("is_moving", false);
            }
        }
        
        /// <summary>毎フレーム移動処理</summary>
        void ProcessMovement()
        {
            float distance = Vector3.Distance(transform.position, targetPosition);
            
            // 目標に到達チェック
            if (distance <= tolerance)
            {
                transform.position = targetPosition;
                StopMovement();
                OnTargetReached?.Invoke();
                
                if (blackBoard != null)
                {
                    blackBoard.SetValue("move_completed", true);
                    blackBoard.SetValue("arrived_at_target", true);
                }
                return;
            }
            
            // 滑らかな移動処理
            Vector3 direction = (targetPosition - transform.position).normalized;
            Vector3 moveStep = direction * currentSpeed * Time.deltaTime;
            
            // 目標地点を超えないように制限
            if (moveStep.magnitude > distance)
            {
                transform.position = targetPosition;
            }
            else
            {
                transform.position += moveStep;
            }
            
            // 滑らかな回転
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            
            // BlackBoardに移動状態を更新
            if (blackBoard != null)
            {
                blackBoard.SetValue("move_distance_remaining", distance);
                blackBoard.SetValue("move_direction", direction);
            }
        }
        
        /// <summary>現在移動中かどうか</summary>
        public bool IsMoving => hasTarget && isMoving;
        
        /// <summary>目標までの距離</summary>
        public float DistanceToTarget => hasTarget ? Vector3.Distance(transform.position, targetPosition) : 0f;
    }
}