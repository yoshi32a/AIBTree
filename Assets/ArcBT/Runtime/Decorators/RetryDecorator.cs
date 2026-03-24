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
        bool needsFrameBreak = false; // delay=0時に最低1フレーム待機するためのフラグ

        public override void SetProperty(string key, string value)
        {
            switch (key.ToLowerInvariant())
            {
                case "max_retries":
                case "retries":
                    if (TryParseInt(value, out var retries))
                    {
                        maxRetries = Mathf.Max(0, retries);
                    }
                    break;
                case "delay":
                case "retry_delay":
                    if (TryParseFloat(value, out var delay))
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
            needsFrameBreak = false;
        }

        protected override BTNodeResult DecorateExecution(BTNode child)
        {
            // リトライ準備状態の場合
            if (isWaitingForRetry)
            {
                // リトライ限界チェック - リトライ開始前に判定
                if (currentRetries >= maxRetries)
                {
                    BTLogger.LogSystem(this, $"All {maxRetries} retries exhausted, returning Failure");
                    isWaitingForRetry = false;
                    currentRetries = 0;
                    needsFrameBreak = false;
                    return BTNodeResult.Failure;
                }

                // delay=0の場合でも最低1フレーム待機して無限ループを防止
                if (needsFrameBreak)
                {
                    needsFrameBreak = false;
                    return BTNodeResult.Running;
                }

                // 遅延がある場合は時間チェック
                if (retryDelay > 0f && Time.time - lastRetryTime < retryDelay)
                {
                    return BTNodeResult.Running;
                }

                // 遅延終了、リトライ実行準備
                isWaitingForRetry = false;
                child.Reset();
                BTLogger.LogSystem(this, $"Retry attempt {currentRetries}/{maxRetries}");
            }

            var result = child.Execute();

            switch (result)
            {
                case BTNodeResult.Running:
                    return BTNodeResult.Running;

                case BTNodeResult.Success:
                    if (currentRetries > 0)
                    {
                        BTLogger.LogSystem(this, $"Succeeded after {currentRetries} retries");
                    }
                    currentRetries = 0;
                    return BTNodeResult.Success;

                case BTNodeResult.Failure:
                    currentRetries++;

                    // 失敗時は常にリトライ準備（限界チェックは次のExecute()で行う）
                    isWaitingForRetry = true;
                    lastRetryTime = Time.time;

                    // delay=0の場合、次のExecute()で1フレーム分待機させて無限ループを防止
                    if (retryDelay <= 0f)
                    {
                        needsFrameBreak = true;
                    }

                    return BTNodeResult.Running;

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
            needsFrameBreak = false;
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
