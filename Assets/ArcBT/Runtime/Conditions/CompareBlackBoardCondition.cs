using ArcBT.Core;
using ArcBT.Logger;
using UnityEngine;

namespace ArcBT.Conditions
{
    /// <summary>
    /// BlackBoard値を比較する条件ノード
    /// </summary>
    [BTNode("CompareBlackBoard")]
    public class CompareBlackBoardCondition : BTConditionNode
    {
        string key1 = "";
        string key2 = "";
        string value2 = "";
        string compareType = "equal"; // equal, not_equal, greater, less, greater_equal, less_equal
        bool useSecondKey = false; // key2を使うかvalue2を使うか

        public override void SetProperty(string key, string value)
        {
            switch (key.ToLower())
            {
                case "key1":
                case "first_key":
                    key1 = value;
                    break;
                case "key2":
                case "second_key":
                    key2 = value;
                    useSecondKey = true;
                    break;
                case "value":
                case "value2":
                    value2 = value;
                    useSecondKey = false;
                    break;
                case "compare":
                case "operation":
                    compareType = value.ToLower();
                    break;
            }
        }

        protected override BTNodeResult CheckCondition()
        {
            if (blackBoard == null)
            {
                BTLogger.LogError(LogCategory.BlackBoard, "CompareBlackBoard: BlackBoard is null", Name, ownerComponent);
                return BTNodeResult.Failure;
            }

            if (string.IsNullOrEmpty(key1))
            {
                BTLogger.LogError(LogCategory.BlackBoard, "CompareBlackBoard: key1 is empty", Name, ownerComponent);
                return BTNodeResult.Failure;
            }

            if (!blackBoard.HasKey(key1))
            {
                BTLogger.LogCondition($"CompareBlackBoard: key1 '{key1}' not found in BlackBoard", Name, ownerComponent);
                return BTNodeResult.Failure;
            }

            var value1Obj = blackBoard.GetValue<object>(key1);
            object value2Obj;

            if (useSecondKey)
            {
                if (string.IsNullOrEmpty(key2))
                {
                    BTLogger.LogError(LogCategory.BlackBoard, "CompareBlackBoard: key2 is empty", Name, ownerComponent);
                    return BTNodeResult.Failure;
                }

                if (!blackBoard.HasKey(key2))
                {
                    BTLogger.LogCondition($"CompareBlackBoard: key2 '{key2}' not found in BlackBoard", Name, ownerComponent);
                    return BTNodeResult.Failure;
                }

                value2Obj = blackBoard.GetValue<object>(key2);
            }
            else
            {
                value2Obj = value2;
            }

            // 比較を実行
            var result = CompareValues(value1Obj, value2Obj, compareType);

            BTLogger.LogCondition($"CompareBlackBoard: {key1}({value1Obj}) {compareType} {(useSecondKey ? key2 : "value")}({value2Obj}) = {result}", Name, ownerComponent);

            return result ? BTNodeResult.Success : BTNodeResult.Failure;
        }

        bool CompareValues(object value1, object value2, string operation)
        {
            if (value1 == null || value2 == null)
            {
                return operation == "equal" ? (value1 == value2) : (value1 != value2);
            }

            // 数値比較を試行
            if (TryCompareNumeric(value1, value2, operation, out var numericResult))
            {
                return numericResult;
            }

            // 文字列比較
            var str1 = value1.ToString();
            var str2 = value2.ToString();

            return operation switch
            {
                "equal" => str1 == str2,
                "not_equal" => str1 != str2,
                "greater" => string.Compare(str1, str2) > 0,
                "less" => string.Compare(str1, str2) < 0,
                "greater_equal" => string.Compare(str1, str2) >= 0,
                "less_equal" => string.Compare(str1, str2) <= 0,
                _ => false
            };
        }

        bool TryCompareNumeric(object value1, object value2, string operation, out bool result)
        {
            result = false;

            // float変換を試行
            if (TryConvertToFloat(value1, out var float1) && TryConvertToFloat(value2, out var float2))
            {
                result = operation switch
                {
                    "equal" => Mathf.Approximately(float1, float2),
                    "not_equal" => !Mathf.Approximately(float1, float2),
                    "greater" => float1 > float2,
                    "less" => float1 < float2,
                    "greater_equal" => float1 >= float2,
                    "less_equal" => float1 <= float2,
                    _ => false
                };
                return true;
            }

            return false;
        }

        bool TryConvertToFloat(object value, out float result)
        {
            result = 0f;

            return value switch
            {
                float f => (result = f) == f,
                int i => (result = i) == i,
                double d => (result = (float)d) == result,
                string s => float.TryParse(s, out result),
                _ => false
            };
        }
    }
}