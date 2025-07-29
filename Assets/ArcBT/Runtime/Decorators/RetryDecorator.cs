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
            BTLogger.LogSystem(Name, $"Execute() called - isWaitingForRetry={isWaitingForRetry}, currentRetries={currentRetries}");
            
            // リトライ準備状態の場合
            if (isWaitingForRetry)
            {
                BTLogger.LogSystem(Name, $"In waiting state, retryDelay={retryDelay}");
                
                // リトライ限界チェック - リトライ開始前に判定
                if (currentRetries > maxRetries)
                {
                    BTLogger.LogSystem(Name, $"Max retries ({maxRetries}) exceeded, giving up");
                    isWaitingForRetry = false;
                    currentRetries = 0;
                    BTLogger.LogSystem(Name, "Returning FAILURE");
                    return BTNodeResult.Failure;
                }
                
                // 遅延がある場合は時間チェック
                if (retryDelay > 0f && Time.time - lastRetryTime < retryDelay)
                {
                    BTLogger.LogSystem(Name, "Still waiting for delay, returning Running");
                    return BTNodeResult.Running;
                }
                
                // 遅延終了またはdelay=0の場合、リトライ実行準備
                isWaitingForRetry = false;
                child.Reset();
                BTLogger.LogSystem(Name, $"Starting retry attempt {currentRetries + 1}/{maxRetries + 1}");
            }

            var result = child.Execute();
            BTLogger.LogSystem(Name, $"Child returned {result}");

            switch (result)
            {
                case BTNodeResult.Running:
                    BTLogger.LogSystem(Name, "Child running, returning Running");
                    return BTNodeResult.Running;

                case BTNodeResult.Success:
                    currentRetries = 0;
                    BTLogger.LogSystem(Name, "Child succeeded, returning Success");
                    return BTNodeResult.Success;

                case BTNodeResult.Failure:
                    currentRetries++;
                    BTLogger.LogSystem(Name, $"Attempt {currentRetries} failed");
                    
                    // 失敗時は常にリトライ準備（限界チェックは次のExecute()で行う）
                    isWaitingForRetry = true;
                    lastRetryTime = Time.time;
                    
                    BTLogger.LogSystem(Name, $"Ready for immediate retry {currentRetries + 1}/{maxRetries + 1}");
                    BTLogger.LogSystem(Name, "Returning RUNNING");
                    
                    return BTNodeResult.Running;

                default:
                    BTLogger.LogSystem(Name, $"Unexpected result {result}, returning Failure");
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