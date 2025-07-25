using UnityEngine;
using BehaviourTree.Core;

namespace BehaviourTree.Actions
{
    /// <summary>安全な場所に逃走するアクション</summary>
    public class FleeToSafetyAction : BTActionNode
    {
        float minDistance = 20.0f;
        float speedMultiplier = 1.5f;
        Vector3 fleeTarget;
        bool hasFleeTarget = false;

        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "min_distance":
                    minDistance = System.Convert.ToSingle(value);
                    break;
                case "speed_multiplier":
                    speedMultiplier = System.Convert.ToSingle(value);
                    break;
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            if (ownerComponent == null || blackBoard == null)
            {
                return BTNodeResult.Failure;
            }

            // 逃走先を決定
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
                    GameObject[] safeZones = GameObject.FindGameObjectsWithTag("SafeZone");
                    if (safeZones.Length > 0)
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
                    Debug.LogWarning("FleeToSafety: No threat detected");
                    return BTNodeResult.Failure;
                }
            }

            // 逃走先に移動
            float distance = Vector3.Distance(transform.position, fleeTarget);
            if (distance <= 1.0f)
            {
                // 安全地点に到達
                blackBoard.SetValue("is_safe", true);
                blackBoard.SetValue("flee_completed", true);
                Debug.Log("FleeToSafety: Reached safety");
                return BTNodeResult.Success;
            }

            // 移動処理
            Vector3 direction = (fleeTarget - transform.position).normalized;
            float moveSpeed = 3.5f * speedMultiplier;
            transform.position += direction * moveSpeed * Time.deltaTime;

            // 移動中は向きを更新
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }

            blackBoard.SetValue("is_fleeing", true);
            blackBoard.SetValue("flee_distance_remaining", distance);

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