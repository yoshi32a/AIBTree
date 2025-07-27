using ArcBT.Core;
using ArcBT.Logger;
using UnityEngine;

namespace ArcBT.Conditions
{
    /// <summary>
    /// 確率ベースのランダム条件ノード
    /// </summary>
    [BTNode("Random")]
    public class RandomCondition : BTConditionNode
    {
        float probability = 0.5f; // 成功確率（0.0-1.0）
        int seed = -1; // -1の場合はシード未設定
        bool useBlackBoardProbability = false;
        string probabilityKey = "";

        public override void SetProperty(string key, string value)
        {
            switch (key.ToLower())
            {
                case "probability":
                case "chance":
                    if (float.TryParse(value, out var prob))
                    {
                        probability = Mathf.Clamp01(prob);
                    }
                    break;
                case "seed":
                    if (int.TryParse(value, out var s))
                    {
                        seed = s;
                    }
                    break;
                case "probability_key":
                case "bb_key":
                    probabilityKey = value;
                    useBlackBoardProbability = !string.IsNullOrEmpty(value);
                    break;
            }
        }

        public override void Initialize(MonoBehaviour owner, BlackBoard sharedBlackBoard = null)
        {
            base.Initialize(owner, sharedBlackBoard);

            // シードが設定されている場合、固定シードを使用
            if (seed >= 0)
            {
                Random.InitState(seed);
            }
        }

        protected override BTNodeResult CheckCondition()
        {
            var currentProbability = probability;

            // BlackBoardから確率を取得する場合
            if (useBlackBoardProbability && blackBoard != null && !string.IsNullOrEmpty(probabilityKey))
            {
                if (blackBoard.HasKey(probabilityKey))
                {
                    var bbProb = blackBoard.GetValue<float>(probabilityKey, probability);
                    currentProbability = Mathf.Clamp01(bbProb);
                    BTLogger.LogCondition($"Random: Using BlackBoard probability '{probabilityKey}' = {currentProbability}", Name, ownerComponent);
                }
                else
                {
                    BTLogger.LogCondition($"Random: BlackBoard key '{probabilityKey}' not found, using default probability {probability}", Name, ownerComponent);
                }
            }

            // ランダム判定
            var randomValue = Random.value;
            var success = randomValue <= currentProbability;

            BTLogger.LogCondition($"Random: {randomValue:F3} <= {currentProbability:F3} = {success}", Name, ownerComponent);

            // 結果をBlackBoardに記録（デバッグ用）
            if (blackBoard != null)
            {
                blackBoard.SetValue($"{Name}_last_random_value", randomValue);
                blackBoard.SetValue($"{Name}_last_probability", currentProbability);
                blackBoard.SetValue($"{Name}_last_result", success);
            }

            return success ? BTNodeResult.Success : BTNodeResult.Failure;
        }
    }
}