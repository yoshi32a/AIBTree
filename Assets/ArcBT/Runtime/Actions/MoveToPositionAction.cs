using System;
using ArcBT.Core;
using UnityEngine;

namespace ArcBT.Actions
{
    [Serializable]
    [BTScript("MoveToPosition")]
    [BTNode("MoveToPosition", NodeType.Action)]
    public class MoveToPositionAction : BTActionNode
    {
        [SerializeField] string target;
        [SerializeField] float speed = 12.0f;
        [SerializeField] float tolerance = 0.5f;

        Vector3 targetPosition;
        bool hasValidTarget = false;

        public override void Initialize(MonoBehaviour owner, BlackBoard sharedBlackBoard = null)
        {
            base.Initialize(owner, sharedBlackBoard);

            // ターゲット位置を取得
            if (!string.IsNullOrEmpty(target))
            {
                var targetObj = GameObject.Find(target);
                if (targetObj != null)
                {
                    targetPosition = targetObj.transform.position;
                    hasValidTarget = true;

                    // BlackBoardにターゲット情報を保存
                    if (blackBoard != null)
                    {
                        blackBoard.SetValue($"{Name}_target_position", targetPosition);
                        blackBoard.SetValue($"{Name}_target_name", target);
                    }

                    Debug.Log($"MoveToPosition: Target found '{target}' at {targetPosition}");
                }
                else
                {
                    Debug.LogWarning($"MoveToPosition: Target '{target}' not found!");
                    hasValidTarget = false;
                }
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            Debug.Log($"=== MoveToPositionAction '{Name}' EXECUTING ===");

            if (!hasValidTarget)
            {
                Debug.LogError($"MoveToPosition '{Name}': No valid target '{target}' - trying to find it again");

                // ターゲットを再検索
                if (!string.IsNullOrEmpty(target))
                {
                    var targetObj = GameObject.Find(target);
                    if (targetObj != null)
                    {
                        targetPosition = targetObj.transform.position;
                        hasValidTarget = true;
                        Debug.Log($"MoveToPosition '{Name}': Found target '{target}' at {targetPosition}");
                    }
                    else
                    {
                        Debug.LogError($"MoveToPosition '{Name}': Target '{target}' still not found!");
                        return BTNodeResult.Failure;
                    }
                }
                else
                {
                    Debug.LogError($"MoveToPosition '{Name}': No target name specified!");
                    return BTNodeResult.Failure;
                }
            }

            // 現在位置とターゲット位置の距離をチェック
            var currentPos = transform.position;
            var distance = Vector3.Distance(currentPos, targetPosition);

            Debug.Log($"MoveToPosition '{Name}': Current pos = {currentPos}, Target pos = {targetPosition}");
            Debug.Log($"MoveToPosition '{Name}': Distance = {distance:F2}, Tolerance = {tolerance}");

            if (distance <= tolerance)
            {
                Debug.Log($"MoveToPosition '{Name}': ✓ REACHED target '{target}' (Distance: {distance:F2} <= {tolerance})");
                return BTNodeResult.Success;
            }

            // ターゲットに向かって移動
            var direction = (targetPosition - currentPos).normalized;
            var newPosition = currentPos + direction * speed * Time.deltaTime;
            transform.position = newPosition;

            // BlackBoardに現在の移動状態を記録
            if (blackBoard != null)
            {
                blackBoard.SetValue($"{Name}_current_distance", distance);
                blackBoard.SetValue($"{Name}_is_moving", true);
            }

            Debug.Log($"MoveToPosition '{Name}': → Moving to '{target}' (Distance: {distance:F2}, Speed: {speed})");
            return BTNodeResult.Running;
        }

        protected override void OnConditionFailed()
        {
            Debug.Log($"MoveToPosition '{Name}': 🚨 Condition failed - stopping movement");

            // BlackBoardに停止状態を記録
            if (blackBoard != null)
            {
                blackBoard.SetValue($"{Name}_is_moving", false);
                blackBoard.SetValue($"{Name}_stopped_reason", "condition_failed");
            }
        }

        public override void SetProperty(string propertyName, string value)
        {
            switch (propertyName.ToLower())
            {
                case "target":
                    target = value;
                    break;
                case "speed":
                    if (float.TryParse(value, out var speedValue))
                    {
                        speed = speedValue;
                    }

                    break;
                case "tolerance":
                    if (float.TryParse(value, out var toleranceValue))
                    {
                        tolerance = toleranceValue;
                    }

                    break;
                default:
                    base.SetProperty(propertyName, value);
                    break;
            }
        }
    }
}
