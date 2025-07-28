using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using ZLogger;
using ZLogger.Unity;

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

            // ZLoggerの初期化（Unity環境用）
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                builder.AddZLoggerUnityDebug(options =>
                {
                    // Unity Consoleに出力する形式を設定
                    options.EnableStructuredLogging = false;
                    options.PrefixFormatter = (writer, info) =>
                    {
                        // カスタムプレフィックスフォーマット（既存のタグ形式を維持）
                        return ZString.Utf8Format(writer, "[BT] ");
                    };
                });
                
                // 開発環境でファイル出力も有効にする場合
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                builder.AddZLoggerFile("Logs/aibtree.log", options =>
                {
                    options.EnableStructuredLogging = true;
                    options.RollingInterval = ZLogger.RollingInterval.Day;
                    options.RollingSizeLimit = 10 * 1024 * 1024; // 10MB
                });
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

            // ZLoggerを使用してログ出力（構造化ログ）
            var categoryTag = categoryTags.GetValueOrDefault(category, "[UNKNOWN]");
            var levelTag = levelTags.GetValueOrDefault(level, "[UNKNOWN]");
            var nodeInfo = !string.IsNullOrEmpty(nodeName) ? $"[{nodeName}]" : "";
            var msLogLevel = logLevelMapping.GetValueOrDefault(level, Microsoft.Extensions.Logging.LogLevel.Information);

            // ZLoggerのC# 10 String Interpolationを活用した高性能ログ出力
            logger.ZLog(msLogLevel, $"{levelTag}{categoryTag}{nodeInfo}: {message}");
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

        // ZLoggerのC# 10 String Interpolationを活用した高性能フォーマットメソッド
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void LogCombatFormat<T1>(string format, T1 arg1, string nodeName = "", UnityEngine.Object context = null)
        {
            if (LogLevel.Info > currentLogLevel || !categoryFilters.GetValueOrDefault(LogCategory.Combat, true)) return;
            EnsureInitialized();
            
            var categoryTag = categoryTags[LogCategory.Combat];
            var levelTag = levelTags[LogLevel.Info];
            var nodeInfo = !string.IsNullOrEmpty(nodeName) ? $"[{nodeName}]" : "";
            
            logger.ZLogInformation($"{levelTag}{categoryTag}{nodeInfo}: {format}", arg1);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void LogMovementFormat<T1>(string format, T1 arg1, string nodeName = "", UnityEngine.Object context = null)
        {
            if (LogLevel.Info > currentLogLevel || !categoryFilters.GetValueOrDefault(LogCategory.Movement, true)) return;
            EnsureInitialized();
            
            var categoryTag = categoryTags[LogCategory.Movement];
            var levelTag = levelTags[LogLevel.Info];
            var nodeInfo = !string.IsNullOrEmpty(nodeName) ? $"[{nodeName}]" : "";
            
            logger.ZLogInformation($"{levelTag}{categoryTag}{nodeInfo}: {format}", arg1);
        }

        // 高性能な条件付きログ（ZLoggerの恩恵を最大化）
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void LogCombatDebug<T1, T2>(string format, T1 arg1, T2 arg2, string nodeName = "")
        {
            if (LogLevel.Debug > currentLogLevel || !categoryFilters.GetValueOrDefault(LogCategory.Combat, true)) return;
            EnsureInitialized();
            
            var categoryTag = categoryTags[LogCategory.Combat];
            var levelTag = levelTags[LogLevel.Debug];
            var nodeInfo = !string.IsNullOrEmpty(nodeName) ? $"[{nodeName}]" : "";
            
            logger.ZLogDebug($"{levelTag}{categoryTag}{nodeInfo}: {format}", arg1, arg2);
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
        public static void LogStructured<T>(LogLevel level, LogCategory category, string template, T value, string nodeName = "")
        {
            if (level > currentLogLevel || !categoryFilters.GetValueOrDefault(category, true)) return;
            EnsureInitialized();
            
            var categoryTag = categoryTags.GetValueOrDefault(category, "[UNKNOWN]");
            var levelTag = levelTags.GetValueOrDefault(level, "[UNKNOWN]");
            var nodeInfo = !string.IsNullOrEmpty(nodeName) ? $"[{nodeName}]" : "";
            var msLogLevel = logLevelMapping.GetValueOrDefault(level, Microsoft.Extensions.Logging.LogLevel.Information);
            
            logger.ZLog(msLogLevel, $"{levelTag}{categoryTag}{nodeInfo}: {template}", value);
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