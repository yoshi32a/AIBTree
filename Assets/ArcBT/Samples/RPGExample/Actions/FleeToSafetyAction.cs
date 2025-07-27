using System;
using ArcBT.Core;
using ArcBT.Logger;
using ArcBT.TagSystem;
using UnityEngine;

namespace ArcBT.Samples.RPG.Actions
{
    /// <summary>安全な場所に逃走するアクション</summary>
    [BTNode("FleeToSafety")]
    public class FleeToSafetyAction : BTActionNode    {
        float minDistance = 20.0f;
        float speedMultiplier = 1.5f;
        Vector3 fleeTarget;
        bool hasFleeTarget = false;
        
        MovementController movementController;

        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "min_distance":
                    minDistance = Convert.ToSingle(value);
                    break;
                case "speed_multiplier":
                    speedMultiplier = Convert.ToSingle(value);
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

            // 既存の逃走先をBlackBoardから取得
            if (!hasFleeTarget)
            {
                var existingTarget = blackBoard.GetValue<Vector3>("flee_target", Vector3.zero);
                if (existingTarget != Vector3.zero)
                {
                    fleeTarget = existingTarget;
                    hasFleeTarget = true;
                    BTLogger.LogMovement($"FleeToSafety: Using existing flee target: {fleeTarget}", Name, ownerComponent);
                }
            }
            
            // 新しい逃走先を決定
            if (!hasFleeTarget)
            {
                GameObject threat = blackBoard.GetValue<GameObject>("enemy_target");
                if (threat == null)
                {
                    threat = blackBoard.GetValue<GameObject>("current_target");
                }

                if (threat != null)
                {
                    // 脅威から離れる方向を計算
                    Vector3 fleeDirection = (transform.position - threat.transform.position).normalized;
                    fleeTarget = transform.position + fleeDirection * minDistance;

                    // 安全地点を探す（Safe Zoneタグのオブジェクトがあれば）
                    using var safeZones = GameplayTagManager.FindGameObjectsWithTag("Object.SafeZone");
                    if (safeZones.Count > 0)
                    {
                        // 最も近い安全地点を選択
                        GameObject nearestSafeZone = null;
                        float nearestDistance = float.MaxValue;

                        foreach (var zone in safeZones)
                        {
                            float zoneDistance = Vector3.Distance(transform.position, zone.transform.position);
                            if (zoneDistance < nearestDistance)
                            {
                                nearestDistance = zoneDistance;
                                nearestSafeZone = zone;
                            }
                        }

                        if (nearestSafeZone != null)
                        {
                            fleeTarget = nearestSafeZone.transform.position;
                        }
                    }

                    hasFleeTarget = true;
                    blackBoard.SetValue("flee_target", fleeTarget);
                }
                else
                {
                    BTLogger.LogError(LogCategory.Movement, "FleeToSafety: No threat detected", Name, ownerComponent);
                    return BTNodeResult.Failure;
                }
            }

            // MovementControllerで移動中でない場合、新しい目標を設定
            if (!movementController.IsMoving)
            {
                float moveSpeed = 22.0f * speedMultiplier;
                movementController.SetTarget(fleeTarget, moveSpeed);
                movementController.OnTargetReached = () => {
                    // 安全地点に到達 - 一定時間安全状態を維持
                    blackBoard.SetValue("is_safe", true);
                    blackBoard.SetValue("flee_completed", true);
                    blackBoard.SetValue("safety_timer", Time.time + 10.0f); // 10秒間安全状態
                    blackBoard.SetValue("last_flee_time", Time.time);
                    
                    BTLogger.LogMovement("FleeToSafety: Reached safety - Safe for 10 seconds", Name, ownerComponent);
                };
            }

            // 移動完了チェック
            if (blackBoard.GetValue<bool>("flee_completed", false))
            {
                blackBoard.SetValue("flee_completed", false); // リセット
                return BTNodeResult.Success;
            }

            blackBoard.SetValue("is_fleeing", true);
            blackBoard.SetValue("flee_distance_remaining", movementController.DistanceToTarget);

            return BTNodeResult.Running;
        }

        public override void Reset()
        {
            base.Reset();
            hasFleeTarget = false;
            blackBoard?.SetValue("is_fleeing", false);
        }
    }
}
