using ArcBT.Core;
using ArcBT.Logger;
using UnityEngine;

namespace ArcBT.Conditions
{
    /// <summary>
    /// 比較演算子の列挙型
    /// </summary>
    public enum ComparisonOperator
    {
        Equal,
        NotEqual,
        Greater,
        Less,
        GreaterEqual,
        LessEqual
    }

    /// <summary>
    /// BlackBoard値を比較する条件ノード
    /// condition: key >= value 形式
    /// </summary>
    [BTNode("CompareBlackBoard")]
    public class CompareBlackBoardCondition : BTConditionNode
    {
        string conditionExpression = "";
        
        // パース済みの値（内部使用）
        string key1 = "";
        string key2 = "";
        string value2 = "";
        ComparisonOperator compareType = ComparisonOperator.Equal;
        bool useSecondKey = false;

        public override void SetProperty(string key, string value)
        {
            if (key.ToLower() == "condition")
            {
                conditionExpression = value;
                ParseConditionExpression(conditionExpression);
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

        bool CompareValues(object value1, object value2, ComparisonOperator operation)
        {
            if (value1 == null || value2 == null)
            {
                return operation == ComparisonOperator.Equal ? (value1 == value2) : (value1 != value2);
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
                ComparisonOperator.Equal => str1 == str2,
                ComparisonOperator.NotEqual => str1 != str2,
                ComparisonOperator.Greater => string.Compare(str1, str2) > 0,
                ComparisonOperator.Less => string.Compare(str1, str2) < 0,
                ComparisonOperator.GreaterEqual => string.Compare(str1, str2) >= 0,
                ComparisonOperator.LessEqual => string.Compare(str1, str2) <= 0,
                _ => false
            };
        }

        bool TryCompareNumeric(object value1, object value2, ComparisonOperator operation, out bool result)
        {
            result = false;

            // float変換を試行
            if (TryConvertToFloat(value1, out var float1) && TryConvertToFloat(value2, out var float2))
            {
                result = operation switch
                {
                    ComparisonOperator.Equal => Mathf.Approximately(float1, float2),
                    ComparisonOperator.NotEqual => !Mathf.Approximately(float1, float2),
                    ComparisonOperator.Greater => float1 > float2,
                    ComparisonOperator.Less => float1 < float2,
                    ComparisonOperator.GreaterEqual => float1 >= float2,
                    ComparisonOperator.LessEqual => float1 <= float2,
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

        /// <summary>
        /// 条件式をパースして比較要素を抽出
        /// 例: "current_health >= 50" → key1="current_health", compareType="greater_equal", value2="50"
        /// 例: "player_level == max_level" → key1="player_level", compareType="equal", key2="max_level"
        /// </summary>
        void ParseConditionExpression(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                BTLogger.LogError(LogCategory.Condition, "CompareBlackBoard: Empty condition expression", Name, ownerComponent);
                return;
            }

            expression = expression.Trim();

            // 演算子を検出（順序重要：長い演算子から先にチェック）
            var operatorMappings = new[]
            {
                (">=", ComparisonOperator.GreaterEqual),
                ("<=", ComparisonOperator.LessEqual),
                ("==", ComparisonOperator.Equal),
                ("!=", ComparisonOperator.NotEqual),
                (">", ComparisonOperator.Greater),
                ("<", ComparisonOperator.Less)
            };

            foreach (var (op, operatorEnum) in operatorMappings)
            {
                int opIndex = expression.IndexOf(op);
                
                if (opIndex > 0)
                {
                    // 左辺（key1）
                    key1 = expression.Substring(0, opIndex).Trim();
                    
                    // 右辺
                    string rightSide = expression.Substring(opIndex + op.Length).Trim();
                    
                    // 比較演算子
                    compareType = operatorEnum;
                    
                    // 右辺がBlackBoardキーか値かを判定
                    // 数値または quoted string の場合は値、それ以外はキー
                    if (IsLiteralValue(rightSide))
                    {
                        // 引用符で囲まれた文字列の場合は引用符を除去
                        if (rightSide.StartsWith("\"") && rightSide.EndsWith("\""))
                        {
                            value2 = rightSide.Substring(1, rightSide.Length - 2);
                        }
                        else
                        {
                            value2 = rightSide;
                        }
                        useSecondKey = false;
                    }
                    else
                    {
                        key2 = rightSide;
                        useSecondKey = true;
                    }

                    BTLogger.Log(Microsoft.Extensions.Logging.LogLevel.Debug, LogCategory.Condition, 
                        $"CompareBlackBoard '{Name}': Parsed '{expression}' → key1='{key1}', op='{compareType}', " +
                        $"{(useSecondKey ? $"key2='{key2}'" : $"value='{value2}'")}", 
                        Name, ownerComponent);
                    return;
                }
            }

            BTLogger.LogError(LogCategory.Condition, 
                $"CompareBlackBoard '{Name}': No valid operator found in expression '{expression}'", 
                Name, ownerComponent);
        }

        /// <summary>
        /// 右辺が文字通りの値（数値やquoted string）かどうか判定
        /// </summary>
        bool IsLiteralValue(string value)
        {
            // 数値かどうかチェック
            if (float.TryParse(value, out _) || int.TryParse(value, out _))
                return true;
                
            // bool値かどうかチェック
            if (bool.TryParse(value, out _))
                return true;
                
            // quoted stringかどうかチェック
            if (value.StartsWith("\"") && value.EndsWith("\""))
                return true;
                
            // それ以外はBlackBoardキーとして扱う
            return false;
        }
    }
}