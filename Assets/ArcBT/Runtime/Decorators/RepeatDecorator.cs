using ArcBT.Core;
using ArcBT.Logger;

namespace ArcBT.Decorators
{
    /// <summary>
    /// 子ノードを指定回数または無限に繰り返すデコレーター
    /// </summary>
    [BTNode("Repeat")]
    public class RepeatDecorator : BTDecoratorNode
    {
        int maxCount = -1; // -1は無限繰り返し
        int currentCount = 0;
        bool stopOnFailure = false; // 失敗時に繰り返しを停止するか
        bool resetChildOnRepeat = true; // 繰り返し時に子ノードをリセットするか

        public override void SetProperty(string key, string value)
        {
            switch (key.ToLower())
            {
                case "count":
                case "max_count":
                    if (int.TryParse(value, out var count))
                    {
                        maxCount = count;
                    }
                    break;
                case "stop_on_failure":
                    if (bool.TryParse(value, out var stop))
                    {
                        stopOnFailure = stop;
                    }
                    break;
                case "reset_child":
                case "reset_on_repeat":
                    if (bool.TryParse(value, out var reset))
                    {
                        resetChildOnRepeat = reset;
                    }
                    break;
            }
        }

        public override void Reset()
        {
            base.Reset();
            currentCount = 0;
        }

        protected override BTNodeResult DecorateExecution(BTNode child)
        {
            // 最大回数に達している場合
            if (maxCount >= 0 && currentCount >= maxCount)
            {
                BTLogger.LogSystem(Name,$"Repeat Reached max count {maxCount}, returning Success");
                return BTNodeResult.Success;
            }

            var result = child.Execute();

            switch (result)
            {
                case BTNodeResult.Running:
                    // 実行中の場合はそのまま継続
                    return BTNodeResult.Running;

                case BTNodeResult.Success:
                    currentCount++;
                    BTLogger.LogSystem($"Repeat '{Name}': Iteration {currentCount} completed successfully", Name);
                    
                    // 最大回数に達したかチェック
                    if (maxCount >= 0 && currentCount >= maxCount)
                    {
                        BTLogger.LogSystem($"Repeat '{Name}': All {maxCount} iterations completed", Name);
                        return BTNodeResult.Success;
                    }
                    
                    // 子ノードをリセットして次の繰り返しに備える
                    if (resetChildOnRepeat)
                    {
                        child.Reset();
                    }
                    
                    // 無限繰り返しまたはまだ回数が残っている場合は継続
                    return BTNodeResult.Running;

                case BTNodeResult.Failure:
                    currentCount++;
                    BTLogger.LogSystem($"Repeat '{Name}': Iteration {currentCount} failed", Name);
                    
                    if (stopOnFailure)
                    {
                        BTLogger.LogSystem($"Repeat '{Name}': Stopping on failure", Name);
                        return BTNodeResult.Failure;
                    }
                    
                    // 最大回数に達したかチェック
                    if (maxCount >= 0 && currentCount >= maxCount)
                    {
                        BTLogger.LogSystem($"Repeat '{Name}': All {maxCount} iterations completed (with failures)", Name);
                        return BTNodeResult.Success;
                    }
                    
                    // 子ノードをリセットして次の繰り返しに備える
                    if (resetChildOnRepeat)
                    {
                        child.Reset();
                    }
                    
                    // 繰り返し継続
                    return BTNodeResult.Running;

                default:
                    return BTNodeResult.Failure;
            }
        }

        public override void OnConditionFailed()
        {
            base.OnConditionFailed();
            // 条件失敗時はカウンターをリセット
            currentCount = 0;
        }

        /// <summary>現在の繰り返し回数を取得</summary>
        public int GetCurrentCount() => currentCount;

        /// <summary>最大繰り返し回数を取得</summary>
        public int GetMaxCount() => maxCount;

        /// <summary>無限繰り返しかどうか</summary>
        public bool IsInfinite() => maxCount < 0;
    }
}
