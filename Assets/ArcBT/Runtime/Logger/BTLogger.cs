using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ZLogger;

namespace ArcBT.Logger
{
    /// <summary>
    /// ArcBTパッケージのログシステム
    /// ユーザーアプリケーションのLoggerFactoryに統合されることを前提とした設計
    /// </summary>
    public static class BTLogger
    {
        static ILogger instance = NullLogger.Instance;
        static bool testModeEnabled = false;
        static bool suppressLogsInTest = false;

        /// <summary>
        /// 現在設定されているILoggerインスタンス
        /// </summary>
        public static ILogger Instance => instance;

        /// <summary>
        /// ArcBTパッケージのロガーを設定します
        /// アプリケーション起動時に一度だけ呼び出してください
        /// </summary>
        /// <param name="loggerFactory">ユーザーアプリケーションのLoggerFactory</param>
        public static void Configure(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));
                
            instance = loggerFactory.CreateLogger("ArcBT");
        }
        
        /// <summary>
        /// 直接ILoggerを設定（高度な使用例）
        /// </summary>
        public static void Configure(ILogger logger)
        {
            instance = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// 設定状況の確認
        /// </summary>
        public static bool IsConfigured => instance != NullLogger.Instance;

        /// <summary>
        /// テストモードを有効化（ログ抑制機能付き）
        /// </summary>
        public static void EnableTestMode(bool suppressLogs = false)
        {
            testModeEnabled = true;
            suppressLogsInTest = suppressLogs;
        }

        /// <summary>
        /// テストモードを無効化
        /// </summary>
        public static void DisableTestMode()
        {
            testModeEnabled = false;
            suppressLogsInTest = false;
        }

        /// <summary>
        /// テストモード状態の確認
        /// </summary>
        public static bool IsTestMode()
        {
            return testModeEnabled;
        }

        /// <summary>
        /// 基本ログ出力メソッド（内部使用）
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        static void LogInternal(Microsoft.Extensions.Logging.LogLevel msLogLevel, LogCategory category, string message, string nodeName = "", UnityEngine.Object context = null)
        {
            // テストモード時のログ抑制
            if (suppressLogsInTest) return;

            // カテゴリ別タグ
            var categoryTag = GetCategoryTag(category);
            var nodeInfo = !string.IsNullOrEmpty(nodeName) ? $"[{nodeName}]" : "";
            
            // ZLoggerを使用した高性能ログ出力（ユーザー設定のフィルタリングも効く）
            switch (msLogLevel)
            {
                case Microsoft.Extensions.Logging.LogLevel.Error:
                    instance.ZLogError($"{categoryTag}{nodeInfo}: {message}");
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Warning:
                    instance.ZLogWarning($"{categoryTag}{nodeInfo}: {message}");
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Information:
                    instance.ZLogInformation($"{categoryTag}{nodeInfo}: {message}");
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Debug:
                    instance.ZLogDebug($"{categoryTag}{nodeInfo}: {message}");
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Trace:
                    instance.ZLogTrace($"{categoryTag}{nodeInfo}: {message}");
                    break;
            }
        }

        /// <summary>
        /// カテゴリ別タグの取得
        /// </summary>
        static string GetCategoryTag(LogCategory category)
        {
            return category switch
            {
                LogCategory.Combat => "[ATK]",
                LogCategory.Movement => "[MOV]",
                LogCategory.Condition => "[CHK]",
                LogCategory.BlackBoard => "[BBD]",
                LogCategory.Parser => "[PRS]",
                LogCategory.System => "[SYS]",
                LogCategory.Debug => "[DBG]",
                _ => "[UNK]"
            };
        }

        // カテゴリ別便利メソッド（ZLoggerネイティブ機能を活用）
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void LogCombat(string message, string nodeName = "", UnityEngine.Object context = null)
        {
            LogInternal(Microsoft.Extensions.Logging.LogLevel.Information, LogCategory.Combat, message, nodeName, context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void LogMovement(string message, string nodeName = "", UnityEngine.Object context = null)
        {
            LogInternal(Microsoft.Extensions.Logging.LogLevel.Information, LogCategory.Movement, message, nodeName, context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void LogCondition(string message, string nodeName = "", UnityEngine.Object context = null)
        {
            LogInternal(Microsoft.Extensions.Logging.LogLevel.Debug, LogCategory.Condition, message, nodeName, context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void LogBlackBoard(string message, string nodeName = "", UnityEngine.Object context = null)
        {
            LogInternal(Microsoft.Extensions.Logging.LogLevel.Debug, LogCategory.BlackBoard, message, nodeName, context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void LogSystem(string message, string nodeName = "", UnityEngine.Object context = null)
        {
            LogInternal(Microsoft.Extensions.Logging.LogLevel.Information, LogCategory.System, message, nodeName, context);
        }

        // エラーログは本番環境でも表示する
        public static void LogError(LogCategory category, string message, string nodeName = "", UnityEngine.Object context = null)
        {
            LogInternal(Microsoft.Extensions.Logging.LogLevel.Error, category, message, nodeName, context);
        }

        // Debug.Logから移行用の便利メソッド
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void Info(string message, UnityEngine.Object context = null)
        {
            LogInternal(Microsoft.Extensions.Logging.LogLevel.Information, LogCategory.System, message, "", context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void Warning(string message, UnityEngine.Object context = null)
        {
            LogInternal(Microsoft.Extensions.Logging.LogLevel.Warning, LogCategory.System, message, "", context);
        }

        public static void Error(string message, UnityEngine.Object context = null)
        {
            LogInternal(Microsoft.Extensions.Logging.LogLevel.Error, LogCategory.System, message, "", context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void Debug(string message, UnityEngine.Object context = null)
        {
            LogInternal(Microsoft.Extensions.Logging.LogLevel.Debug, LogCategory.Debug, message, "", context);
        }

        // 構造化ログ出力（ZLogger推奨）
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void LogStructured<T>(Microsoft.Extensions.Logging.LogLevel level, LogCategory category, string template, T value, string nodeName = "")
        {
            if (suppressLogsInTest) return;

            var categoryTag = GetCategoryTag(category);
            var nodeInfo = !string.IsNullOrEmpty(nodeName) ? $"[{nodeName}]" : "";
            
            switch (level)
            {
                case Microsoft.Extensions.Logging.LogLevel.Error:
                    instance.ZLogError($"{categoryTag}{nodeInfo}: {template}", value);
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Warning:
                    instance.ZLogWarning($"{categoryTag}{nodeInfo}: {template}", value);
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Information:
                    instance.ZLogInformation($"{categoryTag}{nodeInfo}: {template}", value);
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Debug:
                    instance.ZLogDebug($"{categoryTag}{nodeInfo}: {template}", value);
                    break;
                case Microsoft.Extensions.Logging.LogLevel.Trace:
                    instance.ZLogTrace($"{categoryTag}{nodeInfo}: {template}", value);
                    break;
            }
        }

        // 基本Logメソッド（Microsoft.Extensions.Logging.LogLevel使用）
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void Log(Microsoft.Extensions.Logging.LogLevel level, LogCategory category, string message, string nodeName = "", UnityEngine.Object context = null)
        {
            LogInternal(level, category, message, nodeName, context);
        }

        // パフォーマンス測定用ログ
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void LogPerformance(string operation, float elapsedMs, string nodeName = "")
        {
            LogInternal(Microsoft.Extensions.Logging.LogLevel.Debug, LogCategory.System, $"{operation} completed in {elapsedMs:F2}ms", nodeName);
        }

        // Phase 6.4: レガシーAPI削除完了
        // SetLogLevel, GetCurrentLogLevel, SetCategoryFilter, IsCategoryEnabled, ResetToDefaults, ClearHistoryを削除
        // ユーザーはLoggerFactory設定でフィルタリングを制御

        /// <summary>
        /// 後方互換性：最近のログ取得（Phase 6.3: 完全削除）
        /// </summary>
        public static string[] GetRecentLogs(int count = 10)
        {
            // Phase 6.3: LogEntry完全削除のため、空文字列配列を返す
            // ZLoggerプロバイダーが履歴を管理
            return new string[0];
        }

        /// <summary>
        /// 後方互換性：カテゴリ別ログ取得（Phase 6.3: 完全削除）
        /// </summary>
        public static string[] GetLogsByCategory(LogCategory category, int count = 10)
        {
            // Phase 6.3: LogEntry完全削除のため、空文字列配列を返す
            // ZLoggerプロバイダーが履歴を管理
            return new string[0];
        }

        /// <summary>
        /// 後方互換性：フォーマットログ（Combat）- ZLoggerネイティブ最適化
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void LogCombatFormat(string format, object arg1, string nodeName = "", UnityEngine.Object context = null)
        {
            if (suppressLogsInTest) return;

            var categoryTag = GetCategoryTag(LogCategory.Combat);
            var nodeInfo = !string.IsNullOrEmpty(nodeName) ? $"[{nodeName}]" : "";
            
            // ZLoggerネイティブフォーマット（ゼロアロケーション）
            instance.ZLogInformation($"{categoryTag}{nodeInfo}: {format}", arg1);
        }

        /// <summary>
        /// 後方互換性：フォーマットログ（Movement）- ZLoggerネイティブ最適化
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("BT_LOGGING_ENABLED")]
        public static void LogMovementFormat(string format, object arg1, string nodeName = "", UnityEngine.Object context = null)
        {
            if (suppressLogsInTest) return;

            var categoryTag = GetCategoryTag(LogCategory.Movement);
            var nodeInfo = !string.IsNullOrEmpty(nodeName) ? $"[{nodeName}]" : "";
            
            // ZLoggerネイティブフォーマット（ゼロアロケーション）
            instance.ZLogInformation($"{categoryTag}{nodeInfo}: {format}", arg1);
        }

        /// <summary>
        /// 後方互換性：リソース解放（新実装では何もしない）
        /// </summary>
        public static void Dispose()
        {
            // ユーザーのLoggerFactoryを使用しているため、何もしない
            // 後方互換性のためにメソッドのみ残す
        }
    }
}
