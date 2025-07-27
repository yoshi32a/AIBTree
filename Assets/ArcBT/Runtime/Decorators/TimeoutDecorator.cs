using ArcBT.Core;
using ArcBT.Logger;
using UnityEngine;

namespace ArcBT.Decorators
{
    /// <summary>
    /// 子ノードの実行に制限時間を設けるデコレーター
    /// </summary>
    [BTNode("Timeout")]
    public class TimeoutDecorator : BTDecoratorNode
    {
        float timeoutDuration = 5f; // タイムアウト時間（秒）
        float startTime = 0f;
        bool isRunning = false;
        bool returnSuccessOnTimeout = false; // タイムアウト時にSuccessを返すか（デフォルトはFailure）

        public override void SetProperty(string key, string value)
        {
            switch (key.ToLower())
            {
                case "timeout":
                case "duration":
                    if (float.TryParse(value, out var timeout))
                    {
                        timeoutDuration = Mathf.Max(0.1f, timeout); // 最小0.1秒
                    }
                    break;
                case "success_on_timeout":
                case "return_success":
                    if (bool.TryParse(value, out var returnSuccess))
                    {
                        returnSuccessOnTimeout = returnSuccess;
                    }
                    break;
            }
        }

        public override void Reset()
        {
            base.Reset();
            isRunning = false;
            startTime = 0f;
        }

        protected override BTNodeResult DecorateExecution(BTNode child)
        {
            // 初回実行時に開始時刻を記録
            if (!isRunning)
            {
                isRunning = true;
                startTime = Time.time;
                BTLogger.LogSystem($"Timeout '{Name}': Starting execution with {timeoutDuration}s timeout", Name, ownerComponent);
            }

            // タイムアウトチェック
            var elapsedTime = Time.time - startTime;
            if (elapsedTime >= timeoutDuration)
            {
                // タイムアウト発生
                BTLogger.LogSystem($"Timeout '{Name}': Timed out after {elapsedTime:F2}s", Name, ownerComponent);
                
                // 子ノードに条件失敗を通知
                child.OnConditionFailed();
                
                isRunning = false;
                return returnSuccessOnTimeout ? BTNodeResult.Success : BTNodeResult.Failure;
            }

            // 子ノードを実行
            var result = child.Execute();

            switch (result)
            {
                case BTNodeResult.Running:
                    // まだ実行中の場合は継続
                    return BTNodeResult.Running;

                case BTNodeResult.Success:
                case BTNodeResult.Failure:
                    // 子ノードが完了した場合
                    BTLogger.LogSystem($"Timeout '{Name}': Child completed with {result} after {elapsedTime:F2}s", Name, ownerComponent);
                    isRunning = false;
                    return result;

                default:
                    isRunning = false;
                    return BTNodeResult.Failure;
            }
        }

        public override void OnConditionFailed()
        {
            base.OnConditionFailed();
            // 条件失敗時は実行状態をリセット
            isRunning = false;
        }

        /// <summary>実行中かどうか</summary>
        public bool IsRunning() => isRunning;

        /// <summary>経過時間を取得</summary>
        public float GetElapsedTime()
        {
            if (!isRunning) return 0f;
            return Time.time - startTime;
        }

        /// <summary>残り時間を取得</summary>
        public float GetRemainingTime()
        {
            if (!isRunning) return timeoutDuration;
            return Mathf.Max(0f, timeoutDuration - GetElapsedTime());
        }

        /// <summary>タイムアウト進行率を取得（0.0-1.0）</summary>
        public float GetTimeoutProgress()
        {
            if (!isRunning) return 0f;
            return Mathf.Clamp01(GetElapsedTime() / timeoutDuration);
        }

        /// <summary>タイムアウト期間を取得</summary>
        public float GetTimeoutDuration() => timeoutDuration;
    }
}