using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace ArcBT.Logger
{
    public static class BTLogger
    {
        static LogLevel currentLogLevel = LogLevel.Info;
        static bool testModeEnabled = false;
        static bool suppressLogsInTest = false;
        static ILogger logger;
        static bool isInitialized = false;

        static readonly Dictionary<LogCategory, bool> categoryFilters = new()
        {
            { LogCategory.Combat, true },
            { LogCategory.Movement, true },
            { LogCategory.Condition, true },
            { LogCategory.BlackBoard, true },
            { LogCategory.Parser, false }, // パーサーログはデフォルト無効
            { LogCategory.System, true },
            { LogCategory.Debug, false } // デバッグログはデフォルト無効
        };

        static readonly Queue<LogEntry> logHistory = new();
        const int MAX_LOG_HISTORY = 100;

        // カテゴリ別タグ（3文字）
        static readonly Dictionary<LogCategory, string> categoryTags = new()
        {
            { LogCategory.Combat, "[ATK]" }, // Attack
            { LogCategory.Movement, "[MOV]" }, // Move
            { LogCategory.Condition, "[CHK]" }, // Check
            { LogCategory.BlackBoard, "[BBD]" }, // BlackBoard
            { LogCategory.Parser, "[PRS]" }, // Parse
            { LogCategory.System, "[SYS]" }, // System
            { LogCategory.Debug, "[DBG]" } // Debug
        };

        // レベル別タグ（3文字）
        static readonly Dictionary<LogLevel, string> levelTags = new()
        {
            { LogLevel.Error, "[ERR]" }, // Error
            { LogLevel.Warning, "[WRN]" }, // Warning
            { LogLevel.Info, "[INF]" }, // Info
            { LogLevel.Debug, "[DBG]" }, // Debug
            { LogLevel.Trace, "[TRC]" } // Trace
        };

        // Microsoft.Extensions.Logging.LogLevelとBTLogger.LogLevelのマッピング
        static readonly Dictionary<LogLevel, Microsoft.Extensions.Logging.LogLevel> logLevelMapping = new()
        {
            { LogLevel.Error, Microsoft.Extensions.Logging.LogLevel.Error },
            { LogLevel.Warning, Microsoft.Extensions.Logging.LogLevel.Warning },
            { LogLevel.Info, Microsoft.Extensions.Logging.LogLevel.Information },
            { LogLevel.Debug, Microsoft.Extensions.Logging.LogLevel.Debug },
            { LogLevel.Trace, Microsoft.Extensions.Logging.LogLevel.Trace }
        };

        static void EnsureInitialized()
        {
            if (isInitialized) return;

            // ZLoggerの初期化（シンプル版）
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                
                // Unity Consoleへの出力
                builder.AddZLoggerConsole();
                
                // 開発環境でファイル出力も有効にする場合
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                try
                {
                    builder.AddZLoggerFile("Logs/aibtree.log");
                }
                catch
                {
                    // ファイルログでエラーが出てもConsoleログは継続
                }
                #endif
            });

            logger = loggerFactory.CreateLogger("ArcBT");
            isInitialized = true;
        }

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

        public static void EnableTestMode(bool suppressLogs = false)
        {
            testModeEnabled = true;
            suppressLogsInTest = suppressLogs;
            
            if (!suppressLogs)
            {
                currentLogLevel = LogLevel.Debug; // テスト時はDebugレベルまで有効化
                categoryFilters[LogCategory.Parser] = true; // テスト時はParserログを有効化
                categoryFilters[LogCategory.Debug] = true; // テスト時はDebugログも有効化
            }
        }

        public static void DisableTestMode()
        {
            testModeEnabled = false;
            suppressLogsInTest = false;
            ResetToDefaults();
        }

        public static bool IsTestMode()
        {
            return testModeEnabled;
        }

        public static void ClearHistory()
        {
            logHistory.Clear();
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void Log(LogLevel level, LogCategory category, string message, string nodeName = "", UnityEngine.Object context = null)
        {
            // テストモード時のログ抑制
            if (suppressLogsInTest) return;

            // レベルフィルター
            if (level > currentLogLevel) return;

            // カテゴリフィルター
            if (!categoryFilters.GetValueOrDefault(category, true)) return;

            EnsureInitialized();

            var entry = new LogEntry(level, category, message, nodeName, context);

            // 履歴に追加
            logHistory.Enqueue(entry);
            if (logHistory.Count > MAX_LOG_HISTORY)
            {
                logHistory.Dequeue();
            }

            // ZLoggerを使用してログ出力
            var categoryTag = categoryTags.GetValueOrDefault(category, "[UNKNOWN]");
            var levelTag = levelTags.GetValueOrDefault(level, "[UNKNOWN]");
            var nodeInfo = !string.IsNullOrEmpty(nodeName) ? $"[{nodeName}]" : "";
            var msLogLevel = logLevelMapping.GetValueOrDefault(level, Microsoft.Extensions.Logging.LogLevel.Information);

            // ZLoggerのString Interpolationを活用した高性能ログ出力
            var formattedMessage = $"{levelTag}{categoryTag}{nodeInfo}: {message}";
            
            switch (msLogLevel)
            {
                case Microsoft.Extensions.Logging.LogLevel.Error:
                    logger.ZLogError($"{formattedMessage}");
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Warning:
                    logger.ZLogWarning($"{formattedMessage}");
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Information:
                    logger.ZLogInformation($"{formattedMessage}");
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Debug:
                    logger.ZLogDebug($"{formattedMessage}");
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Trace:
                    logger.ZLogTrace($"{formattedMessage}");
                    break;
            }
        }

        static string FormatLogMessage(LogEntry entry)
        {
            var categoryTag = categoryTags.GetValueOrDefault(entry.Category, "[UNKNOWN]");
            var levelTag = levelTags.GetValueOrDefault(entry.Level, "[UNKNOWN]");
            var nodeInfo = !string.IsNullOrEmpty(entry.NodeName) ? $"[{entry.NodeName}]" : "";

            return $"{levelTag}{categoryTag}{nodeInfo}: {entry.Message}";
        }

        // 便利メソッド（ZLoggerのゼロアロケーション機能を活用）
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

        // ZLoggerの高性能フォーマットメソッド
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void LogCombatFormat(string format, object arg1, string nodeName = "", UnityEngine.Object context = null)
        {
            if (LogLevel.Info > currentLogLevel || !categoryFilters.GetValueOrDefault(LogCategory.Combat, true)) return;
            EnsureInitialized();
            
            var categoryTag = categoryTags[LogCategory.Combat];
            var levelTag = levelTags[LogLevel.Info];
            var nodeInfo = !string.IsNullOrEmpty(nodeName) ? $"[{nodeName}]" : "";
            
            logger.ZLogInformation($"{levelTag}{categoryTag}{nodeInfo}: {format}", arg1);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void LogMovementFormat(string format, object arg1, string nodeName = "", UnityEngine.Object context = null)
        {
            if (LogLevel.Info > currentLogLevel || !categoryFilters.GetValueOrDefault(LogCategory.Movement, true)) return;
            EnsureInitialized();
            
            var categoryTag = categoryTags[LogCategory.Movement];
            var levelTag = levelTags[LogLevel.Info];
            var nodeInfo = !string.IsNullOrEmpty(nodeName) ? $"[{nodeName}]" : "";
            
            logger.ZLogInformation($"{levelTag}{categoryTag}{nodeInfo}: {format}", arg1);
        }

        // 履歴取得
        public static LogEntry[] GetRecentLogs(int count = 10)
        {
            return logHistory.TakeLast(count).ToArray();
        }

        public static LogEntry[] GetLogsByCategory(LogCategory category, int count = 10)
        {
            return logHistory.Where(log => log.Category == category).TakeLast(count).ToArray();
        }

        // ZLogger専用メソッド: 構造化ログ出力
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void LogStructured(LogLevel level, LogCategory category, string template, object value, string nodeName = "")
        {
            if (level > currentLogLevel || !categoryFilters.GetValueOrDefault(category, true)) return;
            EnsureInitialized();
            
            var categoryTag = categoryTags.GetValueOrDefault(category, "[UNKNOWN]");
            var levelTag = levelTags.GetValueOrDefault(level, "[UNKNOWN]");
            var nodeInfo = !string.IsNullOrEmpty(nodeName) ? $"[{nodeName}]" : "";
            var msLogLevel = logLevelMapping.GetValueOrDefault(level, Microsoft.Extensions.Logging.LogLevel.Information);
            
            switch (msLogLevel)
            {
                case Microsoft.Extensions.Logging.LogLevel.Error:
                    logger.ZLogError($"{levelTag}{categoryTag}{nodeInfo}: {template}", value);
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Warning:
                    logger.ZLogWarning($"{levelTag}{categoryTag}{nodeInfo}: {template}", value);
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Information:
                    logger.ZLogInformation($"{levelTag}{categoryTag}{nodeInfo}: {template}", value);
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Debug:
                    logger.ZLogDebug($"{levelTag}{categoryTag}{nodeInfo}: {template}", value);
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Trace:
                    logger.ZLogTrace($"{levelTag}{categoryTag}{nodeInfo}: {template}", value);
                    break;
            }
        }

        // 高性能なパフォーマンス測定用ログ
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void LogPerformance(string operation, float elapsedMs, string nodeName = "")
        {
            if (LogLevel.Debug > currentLogLevel || !categoryFilters.GetValueOrDefault(LogCategory.System, true)) return;
            EnsureInitialized();
            
            logger.ZLogDebug($"[DBG][SYS][{nodeName}]: {operation} completed in {elapsedMs:F2}ms");
        }

        // リソース解放
        public static void Dispose()
        {
            if (isInitialized && logger is IDisposable disposable)
            {
                disposable.Dispose();
                isInitialized = false;
            }
        }
    }
}