using ArcBT.Core;
using ArcBT.Logger;
using UnityEngine;

namespace ArcBT.Actions
{
    /// <summary>
    /// BlackBoardに値を設定するアクション
    /// </summary>
    [BTNode("SetBlackBoard")]
    public class SetBlackBoardAction : BTActionNode
    {
        string bbKey = "";
        string bbValue = "";
        string valueType = "string"; // string, int, float, bool, vector3

        public override void SetProperty(string key, string value)
        {
            switch (key.ToLower())
            {
                case "key":
                case "bb_key":
                    bbKey = value;
                    break;
                case "value":
                case "bb_value":
                    bbValue = value;
                    break;
                case "type":
                case "value_type":
                    valueType = value.ToLower();
                    break;
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            if (blackBoard == null)
            {
                BTLogger.LogError(LogCategory.BlackBoard, "SetBlackBoard: BlackBoard is null", Name, ownerComponent);
                return BTNodeResult.Failure;
            }

            if (string.IsNullOrEmpty(bbKey))
            {
                BTLogger.LogError(LogCategory.BlackBoard, "SetBlackBoard: Key is empty", Name, ownerComponent);
                return BTNodeResult.Failure;
            }

            // 値の型に応じて変換して設定
            try
            {
                switch (valueType)
                {
                    case "int":
                        if (int.TryParse(bbValue, out var intValue))
                        {
                            blackBoard.SetValue(bbKey, intValue);
                            BTLogger.LogBlackBoard($"Set {bbKey} = {intValue} (int)", Name, ownerComponent);
                        }
                        else
                        {
                            BTLogger.LogError(LogCategory.BlackBoard, $"Failed to parse '{bbValue}' as int", Name, ownerComponent);
                            return BTNodeResult.Failure;
                        }
                        break;

                    case "float":
                        if (float.TryParse(bbValue, out var floatValue))
                        {
                            blackBoard.SetValue(bbKey, floatValue);
                            BTLogger.LogBlackBoard($"Set {bbKey} = {floatValue} (float)", Name, ownerComponent);
                        }
                        else
                        {
                            BTLogger.LogError(LogCategory.BlackBoard, $"Failed to parse '{bbValue}' as float", Name, ownerComponent);
                            return BTNodeResult.Failure;
                        }
                        break;

                    case "bool":
                        if (bool.TryParse(bbValue, out var boolValue))
                        {
                            blackBoard.SetValue(bbKey, boolValue);
                            BTLogger.LogBlackBoard($"Set {bbKey} = {boolValue} (bool)", Name, ownerComponent);
                        }
                        else
                        {
                            BTLogger.LogError(LogCategory.BlackBoard, $"Failed to parse '{bbValue}' as bool", Name, ownerComponent);
                            return BTNodeResult.Failure;
                        }
                        break;

                    case "vector3":
                        // "x,y,z" 形式をパース
                        var parts = bbValue.Split(',');
                        if (parts.Length == 3 &&
                            float.TryParse(parts[0].Trim(), out var x) &&
                            float.TryParse(parts[1].Trim(), out var y) &&
                            float.TryParse(parts[2].Trim(), out var z))
                        {
                            var vector = new Vector3(x, y, z);
                            blackBoard.SetValue(bbKey, vector);
                            BTLogger.LogBlackBoard($"Set {bbKey} = {vector} (Vector3)", Name, ownerComponent);
                        }
                        else
                        {
                            BTLogger.LogError(LogCategory.BlackBoard, $"Failed to parse '{bbValue}' as Vector3", Name, ownerComponent);
                            return BTNodeResult.Failure;
                        }
                        break;

                    default: // string
                        blackBoard.SetValue(bbKey, bbValue);
                        BTLogger.LogBlackBoard($"Set {bbKey} = '{bbValue}' (string)", Name, ownerComponent);
                        break;
                }
            }
            catch (System.Exception e)
            {
                BTLogger.LogError(LogCategory.BlackBoard, $"SetBlackBoard error: {e.Message}", Name, ownerComponent);
                return BTNodeResult.Failure;
            }

            return BTNodeResult.Success;
        }
    }
}