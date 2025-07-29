using ArcBT.Core;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace ArcBT.Samples.RPG
{
    /// <summary>
    /// ArcBT統合ログシステム - ZLoggerMessage Source Generator最適化版
    /// </summary>
    public static partial class SampleLogger
    {
        [ZLoggerMessage(LogLevel.Information, "[MOV][{node}] Message:{message}")]
        internal static partial void LogMovementInternal(this Microsoft.Extensions.Logging.ILogger logger, string node, string message);

        public static void LogMovement(BTNode node, string message)
        {

        }

        public static void LogCombat(BTNode node, string message)
        {

        }

        public static void EnemyAI(string message)
        {

        }
    }
}
