using System.Collections.Generic;
using ArcBT.Core;
using ArcBT.Logger;
using UnityEngine;

namespace ArcBT.Actions
{
    /// <summary>
    /// BlackBoardに値を設定するアクション
    /// key: value 形式で型自動判定
    /// </summary>
    [BTNode("SetBlackBoard")]
    public class SetBlackBoardAction : BTActionNode
    {
        Dictionary<string, string> assignments = new Dictionary<string, string>();

        public override void SetProperty(string key, string value)
        {
            // 任意のkey名での直接代入
            // 例: move_speed: 10.5 → assignments["move_speed"] = "10.5"
            assignments[key] = value;
        }

        protected override BTNodeResult ExecuteAction()
        {
            if (blackBoard == null)
            {
                BTLogger.LogSystemError(this, "BlackBoard is null");
                return BTNodeResult.Failure;
            }

            if (assignments.Count == 0)
            {
                BTLogger.LogSystemError(this, "No assignments specified");
                return BTNodeResult.Failure;
            }

            try
            {
                foreach (var assignment in assignments)
                {
                    string key = assignment.Key;
                    string value = assignment.Value;
                    
                    if (!SetBlackBoardValue(key, value))
                    {
                        return BTNodeResult.Failure;
                    }
                }

                return BTNodeResult.Success;
            }
            catch (System.Exception e)
            {
                BTLogger.LogSystemError(this, $"Error: {e.Message}");
                return BTNodeResult.Failure;
            }
        }

        /// <summary>
        /// 値の型を自動判定してBlackBoardに設定
        /// </summary>
        bool SetBlackBoardValue(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                BTLogger.LogSystemError(this, "Key is empty");
                return false;
            }

            // 型の自動判定と設定
            // int
            if (int.TryParse(value, out var intValue))
            {
                blackBoard.SetValue(key, intValue);
                return true;
            }
            
            // float
            if (float.TryParse(value, out var floatValue))
            {
                blackBoard.SetValue(key, floatValue);
                return true;
            }
            
            // bool
            if (bool.TryParse(value, out var boolValue))
            {
                blackBoard.SetValue(key, boolValue);
                return true;
            }
            
            // Vector3 "(x,y,z)" 形式
            if (TryParseVector3(value, out var vectorValue))
            {
                blackBoard.SetValue(key, vectorValue);
                return true;
            }
            
            // string（デフォルト）
            blackBoard.SetValue(key, value);
            return true;
        }


        /// <summary>
        /// Vector3パース: (x,y,z) 形式
        /// </summary>
        bool TryParseVector3(string value, out Vector3 result)
        {
            result = Vector3.zero;
            
            if (string.IsNullOrEmpty(value))
                return false;
                
            value = value.Trim();
            
            // (x,y,z) 形式をチェック
            if (value.StartsWith("(") && value.EndsWith(")"))
            {
                string inner = value.Substring(1, value.Length - 2);
                var parts = inner.Split(',');
                
                if (parts.Length == 3 &&
                    float.TryParse(parts[0].Trim(), out var x) &&
                    float.TryParse(parts[1].Trim(), out var y) &&
                    float.TryParse(parts[2].Trim(), out var z))
                {
                    result = new Vector3(x, y, z);
                    return true;
                }
            }
            
            return false;
        }

    }
}
