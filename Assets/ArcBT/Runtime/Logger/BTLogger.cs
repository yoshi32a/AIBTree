using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using UnityEngine;
using ZLogger;

namespace ArcBT.Logger
{
    /// <summary>
    /// ArcBT統合ログシステム - ZLoggerMessage Source Generator最適化版
    /// </summary>
    public static partial class BTLogger
    {
        /// <summary>
        /// 単一グローバルロガーインスタンス（内部使用）
        /// </summary>
        static Microsoft.Extensions.Logging.ILogger globalLogger = NullLogger.Instance;

        /// <summary>
        /// ArcBTロガーを設定（アプリケーション起動時に一度だけ実行）
        /// </summary>
        public static void Configure(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));
                
            globalLogger = loggerFactory.CreateLogger("ArcBT");
        }

        /// <summary>
        /// 直接ILoggerを設定（高度な使用例）
        /// </summary>
        public static void Configure(Microsoft.Extensions.Logging.ILogger logger)
        {
            globalLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 設定状況の確認
        /// </summary>
        public static bool IsConfigured => globalLogger != NullLogger.Instance;

        // Combat関連ログ - ZLoggerMessage Source Generator自動最適化
        [ZLoggerMessage(LogLevel.Information, "[ATK][{nodeName}] Attack {targetName} with {damage} damage")]
        static partial void LogAttackInternal(Microsoft.Extensions.Logging.ILogger logger, string nodeName, string targetName, int damage);

        [ZLoggerMessage(LogLevel.Information, "[ATK][{nodeName}] Health {currentHealth}/{maxHealth} after taking {damage} damage")]
        static partial void LogHealthChangeInternal(Microsoft.Extensions.Logging.ILogger logger, string nodeName, int currentHealth, int maxHealth, int damage);

        [ZLoggerMessage(LogLevel.Debug, "[ATK][{nodeName}] Calculating damage: base={baseDamage}, multiplier={multiplier}, final={finalDamage}")]
        static partial void LogDamageCalculationInternal(Microsoft.Extensions.Logging.ILogger logger, string nodeName, int baseDamage, float multiplier, int finalDamage);

        // Movement関連ログ - ZLoggerMessage Source Generator自動最適化
        [ZLoggerMessage(LogLevel.Information, "[MOV][{nodeName}] Moving from {fromPos} to {toPos} at speed {speed:F1}")]
        static partial void LogMovementInternal(Microsoft.Extensions.Logging.ILogger logger, string nodeName, Vector3 fromPos, Vector3 toPos, float speed);

        [ZLoggerMessage(LogLevel.Debug, "[MOV][{nodeName}] Reached destination {position} in {elapsedTime:F2}s")]
        static partial void LogDestinationReachedInternal(Microsoft.Extensions.Logging.ILogger logger, string nodeName, Vector3 position, float elapsedTime);

        [ZLoggerMessage(LogLevel.Debug, "[MOV][{nodeName}] Movement progress: {currentDistance:F1}/{totalDistance:F1} ({progress:P1})")]
        static partial void LogMovementProgressInternal(Microsoft.Extensions.Logging.ILogger logger, string nodeName, float currentDistance, float totalDistance, float progress);

        // BlackBoard関連ログ - ZLoggerMessage Source Generator自動最適化（ジェネリック回避）
        [ZLoggerMessage(LogLevel.Debug, "[BBD][{nodeName}] Set {key} = {value} (type: {valueType})")]
        static partial void LogBlackBoardSetInternal(Microsoft.Extensions.Logging.ILogger logger, string nodeName, string key, string value, string valueType);

        [ZLoggerMessage(LogLevel.Debug, "[BBD][{nodeName}] Get {key} returned {value}")]
        static partial void LogBlackBoardGetInternal(Microsoft.Extensions.Logging.ILogger logger, string nodeName, string key, string value);

        [ZLoggerMessage(LogLevel.Debug, "[BBD][{nodeName}] Shared data updated: {sharedKey} with {itemCount} items")]
        static partial void LogSharedDataUpdateInternal(Microsoft.Extensions.Logging.ILogger logger, string nodeName, string sharedKey, int itemCount);

        // Condition関連ログ - ZLoggerMessage Source Generator自動最適化
        [ZLoggerMessage(LogLevel.Debug, "[CHK][{nodeName}] Condition {conditionName} evaluated to {result}")]
        static partial void LogConditionResultInternal(Microsoft.Extensions.Logging.ILogger logger, string nodeName, string conditionName, bool result);

        [ZLoggerMessage(LogLevel.Debug, "[CHK][{nodeName}] Health check: {currentHealth}/{maxHealth} (threshold: {threshold})")]
        static partial void LogHealthCheckInternal(Microsoft.Extensions.Logging.ILogger logger, string nodeName, int currentHealth, int maxHealth, int threshold);

        // Parser関連ログ - ZLoggerMessage Source Generator自動最適化
        [ZLoggerMessage(LogLevel.Error, "[PRS] Failed to parse node {nodeType} at line {lineNumber}: {errorMessage}")]
        static partial void LogParseErrorInternal(Microsoft.Extensions.Logging.ILogger logger, string nodeType, int lineNumber, string errorMessage);

        [ZLoggerMessage(LogLevel.Information, "[PRS] Successfully parsed tree '{treeName}' with {nodeCount} nodes")]
        static partial void LogParseSuccessInternal(Microsoft.Extensions.Logging.ILogger logger, string treeName, int nodeCount);

        [ZLoggerMessage(LogLevel.Debug, "[PRS] Parsing token: {tokenType} = '{tokenValue}' at position {position}")]
        static partial void LogTokenParsingInternal(Microsoft.Extensions.Logging.ILogger logger, string tokenType, string tokenValue, int position);

        // System関連ログ - ZLoggerMessage Source Generator自動最適化
        [ZLoggerMessage(LogLevel.Information, "[SYS] {message}")]
        static partial void LogSystemInternal(Microsoft.Extensions.Logging.ILogger logger, string message);

        [ZLoggerMessage(LogLevel.Information, "[SYS][{nodeName}] {message}")]
        static partial void LogSystemInternal2(Microsoft.Extensions.Logging.ILogger logger, string nodeName, string message);

        [ZLoggerMessage(LogLevel.Error, "[SYS][{nodeName}] Error: {errorMessage}")]
        static partial void LogSystemErrorInternal(Microsoft.Extensions.Logging.ILogger logger, string nodeName, string errorMessage);

        [ZLoggerMessage(LogLevel.Debug, "[SYS][{nodeName}] Performance: {operation} completed in {elapsedMs:F2}ms")]
        static partial void LogPerformanceInternal(Microsoft.Extensions.Logging.ILogger logger, string nodeName, string operation, float elapsedMs);

        // ==========================================
        // 公開API - ロガーインスタンス不要（オプション3B）
        // ==========================================

        // Combat関連公開API
        public static void LogAttack(string nodeName, string targetName, int damage)
            => LogAttackInternal(globalLogger, nodeName, targetName, damage);

        public static void LogHealthChange(string nodeName, int currentHealth, int maxHealth, int damage)
            => LogHealthChangeInternal(globalLogger, nodeName, currentHealth, maxHealth, damage);

        public static void LogDamageCalculation(string nodeName, int baseDamage, float multiplier, int finalDamage)
            => LogDamageCalculationInternal(globalLogger, nodeName, baseDamage, multiplier, finalDamage);

        // Movement関連公開API
        public static void LogMovement(string nodeName, Vector3 fromPos, Vector3 toPos, float speed)
            => LogMovementInternal(globalLogger, nodeName, fromPos, toPos, speed);

        public static void LogDestinationReached(string nodeName, Vector3 position, float elapsedTime)
            => LogDestinationReachedInternal(globalLogger, nodeName, position, elapsedTime);

        public static void LogMovementProgress(string nodeName, float currentDistance, float totalDistance, float progress)
            => LogMovementProgressInternal(globalLogger, nodeName, currentDistance, totalDistance, progress);

        // BlackBoard関連公開API
        public static void LogBlackBoardSet(string nodeName, string key, string value, string valueType)
            => LogBlackBoardSetInternal(globalLogger, nodeName, key, value, valueType);

        public static void LogBlackBoardGet(string nodeName, string key, string value)
            => LogBlackBoardGetInternal(globalLogger, nodeName, key, value);

        // 旧API互換性のためのオーバーロード
        public static void LogBlackBoard(string message, string nodeName = "", object context = null)
            => LogSystemInternal2(globalLogger, nodeName, $"[BBD] {message}");

        public static void LogSharedDataUpdate(string nodeName, string sharedKey, int itemCount)
            => LogSharedDataUpdateInternal(globalLogger, nodeName, sharedKey, itemCount);

        // Condition関連公開API
        public static void LogConditionResult(string nodeName, string conditionName, bool result)
            => LogConditionResultInternal(globalLogger, nodeName, conditionName, result);

        public static void LogHealthCheck(string nodeName, int currentHealth, int maxHealth, int threshold)
            => LogHealthCheckInternal(globalLogger, nodeName, currentHealth, maxHealth, threshold);

        // Parser関連公開API
        public static void LogParseError(string nodeType, int lineNumber, string errorMessage)
            => LogParseErrorInternal(globalLogger, nodeType, lineNumber, errorMessage);

        public static void LogParseSuccess(string treeName, int nodeCount)
            => LogParseSuccessInternal(globalLogger, treeName, nodeCount);

        public static void LogTokenParsing(string tokenType, string tokenValue, int position)
            => LogTokenParsingInternal(globalLogger, tokenType, tokenValue, position);

        public static void LogSystem(string message)
            => LogSystemInternal(globalLogger, message);

        // System関連公開API
        public static void LogSystem(string nodeName, string message)
            => LogSystemInternal2(globalLogger, nodeName, message);

        public static void LogSystemError(string nodeName, string errorMessage)
            => LogSystemErrorInternal(globalLogger, nodeName, errorMessage);

        public static void LogPerformance(string nodeName, string operation, float elapsedMs)
            => LogPerformanceInternal(globalLogger, nodeName, operation, elapsedMs);

        // Debug.Logから移行用の便利メソッド（後方互換性）
        public static void Info(string message, string nodeName = "System")
            => LogSystemInternal2(globalLogger, nodeName, message);

        public static void Error(string message, string nodeName = "System")
            => LogSystemErrorInternal(globalLogger, nodeName, message);

        public static void Debug(string message, string nodeName = "System")
            => LogSystemInternal2(globalLogger, nodeName, $"[DEBUG] {message}");

        public static void Warning(string message, string nodeName = "System")
            => LogSystemInternal2(globalLogger, nodeName, $"[WARNING] {message}");

        // 旧API互換性メソッド（LogCategory削除済み）
        public static void LogCombat(string message, string nodeName = "", object context = null)
            => LogSystemInternal2(globalLogger, nodeName, $"[ATK] {message}");

        public static void LogMovement(string message, string nodeName = "", object context = null)
            => LogSystemInternal2(globalLogger, nodeName, $"[MOV] {message}");

        public static void LogCondition(string message, string nodeName = "", object context = null)
            => LogConditionResultInternal(globalLogger, nodeName, "condition", message.Contains("true") || message.Contains("Success"));

        /// <summary>
        /// 後方互換性：レガシーAPI削除のため何もしない
        /// </summary>
        public static void Dispose()
        {
            // ユーザーのLoggerFactoryを使用しているため、何もしない
        }
    }

}
