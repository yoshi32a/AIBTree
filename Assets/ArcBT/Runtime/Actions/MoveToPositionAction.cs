using System;
using ArcBT.Core;
using ArcBT.Logger;
using UnityEngine;

namespace ArcBT.Actions
{
    [Serializable]
    [BTNode("MoveToPosition")]
    public class MoveToPositionAction : BTActionNode
    {
        [SerializeField] string target;
        [SerializeField] float speed = 12.0f;
        [SerializeField] float tolerance = 0.5f;

        Vector3 targetPosition;
        bool hasValidTarget;

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

                    BTLogger.LogMovement($"MoveToPosition: Target found '{target}' at {targetPosition}", Name, ownerComponent);
                }
                else
                {
                    BTLogger.LogError(LogCategory.Movement, $"MoveToPosition: Target '{target}' not found!", Name, ownerComponent);
                    hasValidTarget = false;
                }
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            BTLogger.LogMovement($"=== MoveToPositionAction '{Name}' EXECUTING ===", Name, ownerComponent);

            if (!hasValidTarget)
            {
                BTLogger.LogError(LogCategory.Movement, $"MoveToPosition '{Name}': No valid target '{target}' - trying to find it again", Name, ownerComponent);

                // ターゲットを再検索
                if (!string.IsNullOrEmpty(target))
                {
                    var targetObj = GameObject.Find(target);
                    if (targetObj != null)
                    {
                        targetPosition = targetObj.transform.position;
                        hasValidTarget = true;
                        BTLogger.LogMovement($"MoveToPosition '{Name}': Found target '{target}' at {targetPosition}", Name, ownerComponent);
                    }
                    else
                    {
                        BTLogger.LogError(LogCategory.Movement, $"MoveToPosition '{Name}': Target '{target}' still not found!", Name, ownerComponent);
                        return BTNodeResult.Failure;
                    }
                }
                else
                {
                    BTLogger.LogError(LogCategory.Movement, $"MoveToPosition '{Name}': No target name specified!", Name, ownerComponent);
                    return BTNodeResult.Failure;
                }
            }

            // 現在位置とターゲット位置の距離をチェック
            var currentPos = transform.position;
            var distance = Vector3.Distance(currentPos, targetPosition);

            BTLogger.Log(LogLevel.Debug, LogCategory.Movement, $"MoveToPosition '{Name}': Current pos = {currentPos}, Target pos = {targetPosition}", Name,
                ownerComponent);
            BTLogger.Log(LogLevel.Debug, LogCategory.Movement, $"MoveToPosition '{Name}': Distance = {distance:F2}, Tolerance = {tolerance}", Name,
                ownerComponent);

            if (distance <= tolerance)
            {
                BTLogger.LogMovement($"MoveToPosition '{Name}': ✓ REACHED target '{target}' (Distance: {distance:F2} <= {tolerance})", Name, ownerComponent);
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

            BTLogger.Log(LogLevel.Debug, LogCategory.Movement, $"MoveToPosition '{Name}': → Moving to '{target}' (Distance: {distance:F2}, Speed: {speed})",
                Name, ownerComponent);
            return BTNodeResult.Running;
        }

        public override void OnConditionFailed()
        {
            BTLogger.LogMovement($"MoveToPosition '{Name}': 🚨 Condition failed - stopping movement", Name, ownerComponent);

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
