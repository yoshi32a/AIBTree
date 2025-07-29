using System;
using ArcBT.Core;
using ArcBT.Logger;
using UnityEngine;

namespace ArcBT.Actions
{
    /// <summary>指定条件が満たされるまで待機するアクション</summary>
    [Serializable]
    [BTNode("WaitUntil")]
    public class WaitUntilAction : BTActionNode
    {
        [SerializeField] string conditionExpression = ""; // "key" == "value" 形式
        [SerializeField] float timeoutDuration = -1f; // -1 = タイムアウトなし
        
        float waitStartTime = -1f;

        public override void Initialize(MonoBehaviour owner, BlackBoard sharedBlackBoard = null)
        {
            base.Initialize(owner, sharedBlackBoard);
            waitStartTime = -1f;
        }

        protected override BTNodeResult ExecuteAction()
        {
            if (string.IsNullOrEmpty(conditionExpression))
            {
                BTLogger.LogSystemError(this, "conditionが設定されていません");
                return BTNodeResult.Failure;
            }

            if (waitStartTime < 0)
            {
                waitStartTime = Time.time;
                BTLogger.LogSystem(this, $"条件待機開始: {conditionExpression}");
                
                if (timeoutDuration > 0)
                {
                    BTLogger.LogSystem(this, $"タイムアウト: {timeoutDuration}秒");
                }
            }

            // タイムアウトチェック
            if (timeoutDuration > 0)
            {
                var elapsed = Time.time - waitStartTime;
                if (elapsed >= timeoutDuration)
                {
                    BTLogger.LogSystemError(this, $"タイムアウト: {elapsed:F1}秒経過");
                    waitStartTime = -1f;
                    return BTNodeResult.Failure;
                }
            }

            // 条件式を解析・評価
            if (EvaluateConditionExpression(conditionExpression))
            {
                var waitTime = Time.time - waitStartTime;
                BTLogger.LogSystem(this, $"条件達成: {conditionExpression} ({waitTime:F1}秒待機)");
                waitStartTime = -1f;
                return BTNodeResult.Success;
            }
            
            BTLogger.LogSystem(this, $"条件未達成: {conditionExpression}");
            return BTNodeResult.Running;
        }
        
        /// <summary>条件式を評価する</summary>
        bool EvaluateConditionExpression(string expression)
        {
            if (blackBoard == null)
            {
                return false;
            }
            
            // "key" == "value" 形式をパース
            var parts = expression.Split(new string[] { " == " }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                BTLogger.LogSystemError(this, $"無効な条件式形式: {expression}");
                return false;
            }

            var key = parts[0].Trim().Trim('"');
            var expectedVal = parts[1].Trim().Trim('"');
            
            if (!blackBoard.HasKey(key))
            {
                return false;
            }
            
            var currentValue = blackBoard.GetValueAsString(key);
            return string.Equals(currentValue, expectedVal, StringComparison.OrdinalIgnoreCase);
        }

        public override void SetProperty(string propertyName, string value)
        {
            switch (propertyName.ToLower())
            {
                case "condition":
                    conditionExpression = value;
                    break;
                case "timeout":
                case "timeoutduration":
                    if (float.TryParse(value, out var timeout))
                    {
                        timeoutDuration = timeout;
                    }
                    break;
                default:
                    base.SetProperty(propertyName, value);
                    break;
            }
        }
    }
}