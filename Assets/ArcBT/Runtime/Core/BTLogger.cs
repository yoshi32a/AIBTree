using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace ArcBT.Core
{
    public enum LogCategory
    {
        Combat,      // 戦闘関連
        Movement,    // 移動関連
        Condition,   // 条件チェック
        BlackBoard,  // BlackBoard操作
        Parser,      // BTファイル解析
        System,      // システム全般
        Debug        // デバッグ用
    }

    public enum LogLevel
    {
        Error = 0,   // エラー（常に表示）
        Warning = 1, // 警告
        Info = 2,    // 情報
        Debug = 3,   // デバッグ
        Trace = 4    // 詳細トレース
    }

    public struct LogEntry
    {
        public DateTime timestamp;
        public LogLevel level;
        public LogCategory category;
        public string message;
        public string nodeName;
        public UnityEngine.Object context;

        public LogEntry(LogLevel level, LogCategory category, string message, string nodeName = "", UnityEngine.Object context = null)
        {
            this.timestamp = DateTime.Now;
            this.level = level;
            this.category = category;
            this.message = message;
            this.nodeName = nodeName;
            this.context = context;
        }
    }

    public static class BTLogger
    {
        static LogLevel currentLogLevel = LogLevel.Info;
        static readonly Dictionary<LogCategory, bool> categoryFilters = new Dictionary<LogCategory, bool>
        {
            { LogCategory.Combat, true },
            { LogCategory.Movement, true },
            { LogCategory.Condition, true },
            { LogCategory.BlackBoard, true },
            { LogCategory.Parser, false },  // パーサーログはデフォルト無効
            { LogCategory.System, true },
            { LogCategory.Debug, false }    // デバッグログはデフォルト無効
        };

        static readonly Queue<LogEntry> logHistory = new Queue<LogEntry>();
        const int MAX_LOG_HISTORY = 100;

        // カテゴリ別タグ（3文字）
        static readonly Dictionary<LogCategory, string> categoryTags = new Dictionary<LogCategory, string>
        {
            { LogCategory.Combat, "[ATK]" },     // Attack
            { LogCategory.Movement, "[MOV]" },   // Move
            { LogCategory.Condition, "[CHK]" },  // Check
            { LogCategory.BlackBoard, "[BBD]" }, // BlackBoard
            { LogCategory.Parser, "[PRS]" },     // Parse
            { LogCategory.System, "[SYS]" },     // System
            { LogCategory.Debug, "[DBG]" }       // Debug
        };

        // レベル別タグ（3文字）
        static readonly Dictionary<LogLevel, string> levelTags = new Dictionary<LogLevel, string>
        {
            { LogLevel.Error, "[ERR]" },   // Error
            { LogLevel.Warning, "[WRN]" }, // Warning
            { LogLevel.Info, "[INF]" },    // Info
            { LogLevel.Debug, "[DBG]" },   // Debug
            { LogLevel.Trace, "[TRC]" }    // Trace
        };

        public static void SetLogLevel(LogLevel level)
        {
            currentLogLevel = level;
        }

        public static LogLevel GetCurrentLogLevel()
        {
            return currentLogLevel;
        }

        public static void SetCategoryFilter(LogCategory category, bool enabled)
        {
            categoryFilters[category] = enabled;
        }

        public static bool IsCategoryEnabled(LogCategory category)
        {
            return categoryFilters.GetValueOrDefault(category, true);
        }

        public static void ResetToDefaults()
        {
            currentLogLevel = LogLevel.Info;
            categoryFilters[LogCategory.Combat] = true;
            categoryFilters[LogCategory.Movement] = true;
            categoryFilters[LogCategory.Condition] = true;
            categoryFilters[LogCategory.BlackBoard] = true;
            categoryFilters[LogCategory.Parser] = false;
            categoryFilters[LogCategory.System] = true;
            categoryFilters[LogCategory.Debug] = false;
        }

        public static void EnableTestMode()
        {
            currentLogLevel = LogLevel.Info;
            categoryFilters[LogCategory.Parser] = true;  // テスト時はParserログを有効化
            categoryFilters[LogCategory.Debug] = true;   // テスト時はDebugログも有効化
        }

        public static void ClearHistory()
        {
            logHistory.Clear();
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void Log(LogLevel level, LogCategory category, string message, string nodeName = "", UnityEngine.Object context = null)
        {
            // レベルフィルター
            if (level > currentLogLevel) return;
            
            // カテゴリフィルター
            if (!categoryFilters.GetValueOrDefault(category, true)) return;

            var entry = new LogEntry(level, category, message, nodeName, context);
            
            // 履歴に追加
            logHistory.Enqueue(entry);
            if (logHistory.Count > MAX_LOG_HISTORY)
            {
                logHistory.Dequeue();
            }

            // フォーマットしてUnityコンソールに出力
            var formattedMessage = FormatLogMessage(entry);
            
            switch (level)
            {
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(formattedMessage, context);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(formattedMessage, context);
                    break;
                default:
                    UnityEngine.Debug.Log(formattedMessage, context);
                    break;
            }
        }

        static string FormatLogMessage(LogEntry entry)
        {
            var categoryTag = categoryTags.GetValueOrDefault(entry.category, "[UNKNOWN]");
            var levelTag = levelTags.GetValueOrDefault(entry.level, "[UNKNOWN]");
            var nodeInfo = !string.IsNullOrEmpty(entry.nodeName) ? $"[{entry.nodeName}]" : "";
            
            return $"{levelTag}{categoryTag}{nodeInfo}: {entry.message}";
        }

        // 便利メソッド
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void LogCombat(string message, string nodeName = "", UnityEngine.Object context = null)
        {
            Log(LogLevel.Info, LogCategory.Combat, message, nodeName, context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void LogMovement(string message, string nodeName = "", UnityEngine.Object context = null)
        {
            Log(LogLevel.Info, LogCategory.Movement, message, nodeName, context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void LogCondition(string message, string nodeName = "", UnityEngine.Object context = null)
        {
            Log(LogLevel.Debug, LogCategory.Condition, message, nodeName, context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void LogBlackBoard(string message, string nodeName = "", UnityEngine.Object context = null)
        {
            Log(LogLevel.Debug, LogCategory.BlackBoard, message, nodeName, context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void LogSystem(string message, string nodeName = "", UnityEngine.Object context = null)
        {
            Log(LogLevel.Info, LogCategory.System, message, nodeName, context);
        }

        // エラーログは本番環境でも表示する
        public static void LogError(LogCategory category, string message, string nodeName = "", UnityEngine.Object context = null)
        {
            Log(LogLevel.Error, category, message, nodeName, context);
        }

        // Debug.Logから移行用の便利メソッド
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void Info(string message, UnityEngine.Object context = null)
        {
            Log(LogLevel.Info, LogCategory.System, message, "", context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void Warning(string message, UnityEngine.Object context = null)
        {
            Log(LogLevel.Warning, LogCategory.System, message, "", context);
        }

        public static void Error(string message, UnityEngine.Object context = null)
        {
            Log(LogLevel.Error, LogCategory.System, message, "", context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void Debug(string message, UnityEngine.Object context = null)
        {
            Log(LogLevel.Debug, LogCategory.Debug, message, "", context);
        }

        // パフォーマンス最適化：ログ生成コストを削減
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void LogFormat(LogLevel level, LogCategory category, string format, params object[] args)
        {
            if (level > currentLogLevel || !categoryFilters.GetValueOrDefault(category, true)) return;
            Log(level, category, string.Format(format, args));
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void LogCombatFormat(string format, params object[] args)
        {
            LogFormat(LogLevel.Info, LogCategory.Combat, format, args);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void LogMovementFormat(string format, params object[] args)
        {
            LogFormat(LogLevel.Info, LogCategory.Movement, format, args);
        }

        // 履歴取得
        public static LogEntry[] GetRecentLogs(int count = 10)
        {
            return logHistory.TakeLast(count).ToArray();
        }

        public static LogEntry[] GetLogsByCategory(LogCategory category, int count = 10)
        {
            return logHistory.Where(log => log.category == category).TakeLast(count).ToArray();
        }
    }
}