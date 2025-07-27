using ArcBT.Core;
using ArcBT.Logger;
using UnityEngine;

namespace ArcBT.Actions
{
    /// <summary>
    /// デバッグ用のログ出力アクション
    /// </summary>
    [BTNode("Log")]
    public class LogAction : BTActionNode
    {
        string message = "Log message";
        string logLevel = "Info";
        bool includeBlackBoardInfo = false;
        string blackBoardKey = "";

        public override void SetProperty(string key, string value)
        {
            switch (key.ToLower())
            {
                case "message":
                    message = value;
                    break;
                case "level":
                    logLevel = value;
                    break;
                case "include_blackboard":
                    if (bool.TryParse(value, out var include))
                        includeBlackBoardInfo = include;
                    break;
                case "blackboard_key":
                    blackBoardKey = value;
                    break;
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            var logMessage = message;

            // BlackBoard情報を含める場合
            if (includeBlackBoardInfo && blackBoard != null)
            {
                if (!string.IsNullOrEmpty(blackBoardKey))
                {
                    var bbValue = blackBoard.GetValueAsString(blackBoardKey);
                    logMessage = $"{message} [{blackBoardKey}={bbValue}]";
                }
                else
                {
                    logMessage = $"{message} [BlackBoard Keys: {blackBoard.GetAllKeys().Length}]";
                }
            }

            // ログレベルに応じて出力
            switch (logLevel.ToLower())
            {
                case "error":
                    BTLogger.Error(logMessage);
                    break;
                case "warning":
                case "warn":
                    BTLogger.Warning(logMessage);
                    break;
                case "debug":
                    BTLogger.Info($"[DEBUG] {logMessage}");
                    break;
                default:
                    BTLogger.Info(logMessage);
                    break;
            }

            return BTNodeResult.Success;
        }
    }
}