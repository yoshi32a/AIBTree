using ArcBT.Core;
using ArcBT.Logger;
using UnityEngine;

namespace ArcBT.Decorators
{
    /// <summary>
    /// 子ノードが失敗した場合に指定回数リトライするデコレーター
    /// </summary>
    [BTNode("Retry")]
    public class RetryDecorator : BTDecoratorNode
    {
        int maxRetries = 3;
        int currentRetries = 0;
        float retryDelay = 0f; // リトライ間の遅延（秒）
        float lastRetryTime = 0f;
        bool isWaitingForRetry = false;

        public override void SetProperty(string key, string value)
        {
            switch (key.ToLower())
            {
                case "max_retries":
                case "retries":
                    if (int.TryParse(value, out var retries))
                    {
                        maxRetries = Mathf.Max(0, retries);
                    }
                    break;
                case "delay":
                case "retry_delay":
                    if (float.TryParse(value, out var delay))
                    {
                        retryDelay = Mathf.Max(0f, delay);
                    }
                    break;
            }
        }

        public override void Reset()
        {
            base.Reset();
            currentRetries = 0;
            isWaitingForRetry = false;
            lastRetryTime = 0f;
        }

        protected override BTNodeResult DecorateExecution(BTNode child)
        {
            // リトライ遅延中の場合
            if (isWaitingForRetry)
            {
                if (Time.time - lastRetryTime < retryDelay)
                {
                    return BTNodeResult.Running;
                }
                
                // 遅延終了、リトライ実行
                isWaitingForRetry = false;
                child.Reset(); // 子ノードをリセット
                BTLogger.LogSystem($"Retry '{Name}': Starting retry attempt {currentRetries + 1}/{maxRetries + 1}", Name, ownerComponent);
            }

            var result = child.Execute();

            switch (result)
            {
                case BTNodeResult.Running:
                    // 実行中の場合はそのまま継続
                    return BTNodeResult.Running;

                case BTNodeResult.Success:
                    // 成功した場合はリトライカウンターをリセットして成功を返す
                    currentRetries = 0;
                    BTLogger.LogSystem($"Retry '{Name}': Child succeeded", Name, ownerComponent);
                    return BTNodeResult.Success;

                case BTNodeResult.Failure:
                    currentRetries++;
                    BTLogger.LogSystem($"Retry '{Name}': Attempt {currentRetries} failed", Name, ownerComponent);
                    
                    // 最大リトライ回数に達した場合
                    if (currentRetries > maxRetries)
                    {
                        BTLogger.LogSystem($"Retry '{Name}': All {maxRetries + 1} attempts failed, giving up", Name, ownerComponent);
                        currentRetries = 0; // 次回のためにリセット
                        return BTNodeResult.Failure;
                    }
                    
                    // まだリトライ可能な場合
                    if (retryDelay > 0f)
                    {
                        // 遅延がある場合は待機状態に入る
                        isWaitingForRetry = true;
                        lastRetryTime = Time.time;
                        BTLogger.LogSystem($"Retry '{Name}': Waiting {retryDelay}s before retry {currentRetries + 1}", Name, ownerComponent);
                        return BTNodeResult.Running;
                    }
                    else
                    {
                        // 遅延がない場合は即座にリトライ
                        child.Reset();
                        BTLogger.LogSystem($"Retry '{Name}': Immediate retry attempt {currentRetries + 1}/{maxRetries + 1}", Name, ownerComponent);
                        return BTNodeResult.Running;
                    }

                default:
                    return BTNodeResult.Failure;
            }
        }

        public override void OnConditionFailed()
        {
            base.OnConditionFailed();
            // 条件失敗時はリトライ状態をリセット
            currentRetries = 0;
            isWaitingForRetry = false;
        }

        /// <summary>現在のリトライ回数を取得</summary>
        public int GetCurrentRetries() => currentRetries;

        /// <summary>最大リトライ回数を取得</summary>
        public int GetMaxRetries() => maxRetries;

        /// <summary>リトライ待機中かどうか</summary>
        public bool IsWaitingForRetry() => isWaitingForRetry;

        /// <summary>次のリトライまでの残り時間</summary>
        public float GetTimeUntilNextRetry()
        {
            if (!isWaitingForRetry) return 0f;
            return Mathf.Max(0f, retryDelay - (Time.time - lastRetryTime));
        }
    }
}