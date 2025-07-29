using System;
using ArcBT.Core;
using ArcBT.Logger;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ArcBT.Samples.RPG
{
    /// <summary>ランダムに徘徊するアクション</summary>
    [BTNode("RandomWander")]
    public class RandomWanderAction : BTActionNode
    {
        float wanderRadius = 10.0f;
        float speed = 25.0f; // 大幅高速化（検証時間短縮）
        float tolerance = 0.5f;
        Vector3 wanderTarget;
        Vector3 initialPosition;
        bool hasTarget = false;

        MovementController movementController;

        // スマートログ用
        float lastLoggedDistance = float.MaxValue;
        float lastLogTime = 0f;
        float logInterval = 2.0f; // 2秒間隔でログ出力

        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "wander_radius":
                    wanderRadius = Convert.ToSingle(value);
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
            if (owner != null)
            {
                initialPosition = owner.transform.position;
            }

            // MovementControllerを取得または追加
            movementController = owner.GetComponent<MovementController>();
            if (movementController == null)
            {
                movementController = owner.gameObject.AddComponent<MovementController>();
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            if (ownerComponent == null || movementController == null)
            {
                BTLogger.LogSystemError("Movement", "RandomWander: Owner or MovementController is null");
                return BTNodeResult.Failure;
            }

            // 現在のアクション状態をBlackBoardに記録
            if (blackBoard != null)
            {
                blackBoard.SetValue("current_action", "RandomWander");
                blackBoard.SetValue("is_patrolling", true);
                blackBoard.SetValue("is_in_combat", false);
            }

            // 新しいランダムターゲットを設定
            if (!hasTarget)
            {
                SetNewWanderTarget();
                hasTarget = true;
            }

            // MovementControllerで移動中でない場合、新しい目標を設定
            if (!movementController.IsMoving)
            {
                movementController.SetTarget(wanderTarget, speed);
                movementController.OnTargetReached = () =>
                {
                    // ターゲットに到達したら新しいターゲットを設定
                    hasTarget = false;
                    BTLogger.LogMovement("RandomWander: Reached wander target, setting new target", Name, ownerComponent);
                };
            }

            // ターゲットに到達したかチェック
            if (blackBoard.GetValue<bool>("move_completed", false))
            {
                blackBoard.SetValue("move_completed", false); // リセット
                return BTNodeResult.Success;
            }

            // BlackBoardに移動情報を記録
            if (blackBoard != null)
            {
                blackBoard.SetValue("wander_target", wanderTarget);
                blackBoard.SetValue("wander_distance_remaining", movementController.DistanceToTarget);
            }

            // スマートログ: 距離変化が大きいか時間間隔でログ出力
            var distance = movementController.DistanceToTarget;
            var shouldLog = false;
            if (Mathf.Abs(distance - lastLoggedDistance) > 1.0f || Time.time - lastLogTime > logInterval)
            {
                shouldLog = true;
                lastLoggedDistance = distance;
                lastLogTime = Time.time;
            }

            if (shouldLog)
            {
                BTLogger.LogMovement($"RandomWander: Moving to target - Distance: {distance:F1}", Name, ownerComponent);
            }

            return BTNodeResult.Running;
        }

        void SetNewWanderTarget()
        {
            // 初期位置を中心とした円内でランダムな点を選択
            var randomCircle = Random.insideUnitCircle * wanderRadius;
            wanderTarget = initialPosition + new Vector3(randomCircle.x, 0, randomCircle.y);

            BTLogger.LogMovement($"RandomWander: New target set at ({wanderTarget.x:F2}, {wanderTarget.y:F2}, {wanderTarget.z:F2})", Name, ownerComponent);

            // ログ状態をリセット
            lastLoggedDistance = float.MaxValue;
            lastLogTime = Time.time;
        }

        public override void Reset()
        {
            base.Reset();
            hasTarget = false;
            lastLoggedDistance = float.MaxValue;
            lastLogTime = 0f;
        }

        void OnDrawGizmosSelected()
        {
            if (ownerComponent != null)
            {
                // 徘徊範囲を描画
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(initialPosition, wanderRadius);

                // 現在のターゲットを描画
                if (hasTarget)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(transform.position, wanderTarget);
                    Gizmos.DrawWireSphere(wanderTarget, tolerance);
                }
            }
        }
    }
}