using System;
using ArcBT.Core;
using ArcBT.Logger;
using ArcBT.TagSystem;
using UnityEngine;

namespace ArcBT.Conditions
{
    /// <summary>
    /// 対象との2D距離（地上距離）をチェックする条件ノード
    /// Y軸（高さ）を無視したXZ平面での距離判定
    /// </summary>
    [Serializable]
    [BTNode("Distance2DCheck")]
    public class Distance2DCheckCondition : BTConditionNode
    {
        [SerializeField] string targetName = ""; // 対象オブジェクト名
        [SerializeField] string targetTag = "";  // 対象タグ（オブジェクト名より優先度低）
        [SerializeField] string distanceExpression = "<= 5.0";  // 距離条件式（例: "> 3.0", "<= 10.0", "== 5.0"）
        
        // パース済みの値（内部使用）
        float distance = 5.0f;
        string comparison = "less_equal";
        [SerializeField] bool useBlackBoardTarget = false; // BlackBoardからターゲット位置を取得
        [SerializeField] string blackBoardPositionKey = "target_position"; // BlackBoard内の位置キー

        GameObject targetObject;
        Vector3 targetPosition;
        bool hasValidTarget;

        public override void Initialize(MonoBehaviour owner, BlackBoard sharedBlackBoard = null)
        {
            base.Initialize(owner, sharedBlackBoard);
            ParseDistanceExpression(distanceExpression);
            FindTarget();
        }

        protected override BTNodeResult CheckCondition()
        {
            // BlackBoardからターゲット位置を取得する場合
            if (useBlackBoardTarget && blackBoard != null)
            {
                if (blackBoard.HasKey(blackBoardPositionKey))
                {
                    targetPosition = blackBoard.GetValue<Vector3>(blackBoardPositionKey);
                    hasValidTarget = true;
                }
                else
                {
                    BTLogger.LogSystemError("Condition", $"Distance2DCheck '{Name}': BlackBoard key '{blackBoardPositionKey}' not found");
                    return BTNodeResult.Failure;
                }
            }
            // オブジェクト参照の場合、対象が見つからなければ再検索
            else if (!hasValidTarget || targetObject == null)
            {
                FindTarget();
                if (!hasValidTarget)
                {
                    return BTNodeResult.Failure;
                }
            }

            // 2D距離計算（Y軸無視）
            Vector3 currentPos = transform.position;
            Vector3 targetPos = useBlackBoardTarget ? targetPosition : targetObject.transform.position;
            
            Vector2 current2D = new Vector2(currentPos.x, currentPos.z);
            Vector2 target2D = new Vector2(targetPos.x, targetPos.z);
            float actualDistance = Vector2.Distance(current2D, target2D);

            // BlackBoardに現在の距離を保存
            if (blackBoard != null)
            {
                blackBoard.SetValue($"{Name}_current_distance_2d", actualDistance);
                blackBoard.SetValue($"{Name}_target_position", targetPos);
                blackBoard.SetValue($"{Name}_ground_distance", actualDistance); // 分かりやすい名前でも保存
            }

            // 距離比較
            bool conditionMet = comparison.ToLower() switch
            {
                "less" => actualDistance < distance,
                "less_equal" => actualDistance <= distance,
                "greater" => actualDistance > distance,
                "greater_equal" => actualDistance >= distance,
                "equal" => Mathf.Abs(actualDistance - distance) < 0.1f,
                "not_equal" => Mathf.Abs(actualDistance - distance) >= 0.1f,
                _ => false
            };

            BTLogger.LogSystem(Name, 
                $"Ground Distance={actualDistance:F2}, Target={distance:F2}, Comparison={comparison}, Result={conditionMet}");

            return conditionMet ? BTNodeResult.Success : BTNodeResult.Failure;
        }

        void FindTarget()
        {
            hasValidTarget = false;
            targetObject = null;

            // オブジェクト名で検索
            if (!string.IsNullOrEmpty(targetName))
            {
                targetObject = GameObject.Find(targetName);
                if (targetObject != null)
                {
                    hasValidTarget = true;
                    BTLogger.LogSystem(Name, $"Found target by name '{targetName}'");
                    return;
                }
            }

            // GameplayTagで検索
            if (!string.IsNullOrEmpty(targetTag))
            {
                targetObject = GameplayTagManager.FindGameObjectWithTag(new GameplayTag(targetTag));
                if (targetObject != null)
                {
                    hasValidTarget = true;
                    BTLogger.LogSystem(Name, $"Found target by GameplayTag '{targetTag}'");
                    return;
                }
            }

            BTLogger.LogSystemError("Condition", 
                $"Distance2DCheck '{Name}': No target found (name='{targetName}', tag='{targetTag}')");
        }

        /// <summary>
        /// 距離式をパースして比較演算子と値を抽出
        /// 例: "> 3.0" → comparison="greater", distance=3.0
        /// </summary>
        void ParseDistanceExpression(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                comparison = "less_equal";
                distance = 5.0f;
                return;
            }

            expression = expression.Trim();

            // 演算子を検出
            if (expression.StartsWith(">="))
            {
                comparison = "greater_equal";
                expression = expression.Substring(2).Trim();
            }
            else if (expression.StartsWith("<="))
            {
                comparison = "less_equal";
                expression = expression.Substring(2).Trim();
            }
            else if (expression.StartsWith("=="))
            {
                comparison = "equal";
                expression = expression.Substring(2).Trim();
            }
            else if (expression.StartsWith("!="))
            {
                comparison = "not_equal";
                expression = expression.Substring(2).Trim();
            }
            else if (expression.StartsWith(">"))
            {
                comparison = "greater";
                expression = expression.Substring(1).Trim();
            }
            else if (expression.StartsWith("<"))
            {
                comparison = "less";
                expression = expression.Substring(1).Trim();
            }
            else
            {
                // 演算子がない場合は <= として扱う
                comparison = "less_equal";
            }

            // 数値をパース
            if (float.TryParse(expression, out var parsedDistance))
            {
                distance = parsedDistance;
            }
            else
            {
                BTLogger.LogSystemError("Condition", 
                    $"Distance2DCheck '{Name}': Failed to parse distance expression '{expression}'");
                distance = 5.0f;
            }

            BTLogger.LogSystem(Name, 
                $"Parsed expression '{distanceExpression}' → {comparison} {distance}");
        }

        public override void SetProperty(string propertyName, string value)
        {
            switch (propertyName.ToLower())
            {
                case "target":
                case "target_name":
                    targetName = value;
                    break;
                case "target_tag":
                    targetTag = value;
                    break;
                case "distance":
                    distanceExpression = value;
                    ParseDistanceExpression(distanceExpression);
                    break;
                case "comparison":
                case "compare":
                    comparison = value;
                    break;
                case "use_blackboard_target":
                    if (bool.TryParse(value, out var useBlackBoard))
                    {
                        useBlackBoardTarget = useBlackBoard;
                    }
                    break;
                case "blackboard_position_key":
                case "bb_position_key":
                    blackBoardPositionKey = value;
                    break;
                default:
                    base.SetProperty(propertyName, value);
                    break;
            }
        }
    }
}