using System;
using ArcBT.Core;
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
        static partial void LogAttackInternal(this Microsoft.Extensions.Logging.ILogger logger, string nodeName, string targetName, int damage);

        [ZLoggerMessage(LogLevel.Information, "[ATK][{nodeName}] Health {currentHealth}/{maxHealth} after taking {damage} damage")]
        static partial void LogHealthChangeInternal(this Microsoft.Extensions.Logging.ILogger logger, string nodeName, int currentHealth, int maxHealth, int damage);

        [ZLoggerMessage(LogLevel.Debug, "[ATK][{nodeName}] Calculating damage: base={baseDamage}, multiplier={multiplier}, final={finalDamage}")]
        static partial void LogDamageCalculationInternal(this Microsoft.Extensions.Logging.ILogger logger, string nodeName, int baseDamage, float multiplier, int finalDamage);

        // Movement関連ログ - ZLoggerMessage Source Generator自動最適化
        [ZLoggerMessage(LogLevel.Information, "[MOV][{nodeName}] Moving from {fromPos} to {toPos} at speed {speed:F1}")]
        static partial void LogMovementInternal(this Microsoft.Extensions.Logging.ILogger logger, string nodeName, Vector3 fromPos, Vector3 toPos, float speed);

        [ZLoggerMessage(LogLevel.Debug, "[MOV][{nodeName}] Reached destination {position} in {elapsedTime:F2}s")]
        static partial void LogDestinationReachedInternal(this Microsoft.Extensions.Logging.ILogger logger, string nodeName, Vector3 position, float elapsedTime);

        [ZLoggerMessage(LogLevel.Debug, "[MOV][{nodeName}] Movement progress: {currentDistance:F1}/{totalDistance:F1} ({progress:P1})")]
        static partial void LogMovementProgressInternal(this Microsoft.Extensions.Logging.ILogger logger, string nodeName, float currentDistance, float totalDistance, float progress);

        // BlackBoard関連ログ - ZLoggerMessage Source Generator自動最適化（ジェネリック回避）
        [ZLoggerMessage(LogLevel.Debug, "[BBD] Set {key} = {value} (type: {valueType})")]
        static partial void LogBlackBoardSetInternal(this Microsoft.Extensions.Logging.ILogger logger, string key, object value, string valueType);

        [ZLoggerMessage(LogLevel.Debug, "[BBD] Get {key} returned {value}, (type: {valueType})")]
        static partial void LogBlackBoardGetInternal(this Microsoft.Extensions.Logging.ILogger logger, string key, object value, string valueType);

        [ZLoggerMessage(LogLevel.Debug, "[BBD][{nodeName}] Shared data updated: {sharedKey} with {itemCount} items")]
        static partial void LogSharedDataUpdateInternal(this Microsoft.Extensions.Logging.ILogger logger, string nodeName, string sharedKey, int itemCount);

        // Condition関連ログ - ZLoggerMessage Source Generator自動最適化
        [ZLoggerMessage(LogLevel.Debug, "[CHK][{nodeName}] Condition {conditionName} evaluated to {result}")]
        static partial void LogConditionResultInternal(this Microsoft.Extensions.Logging.ILogger logger, string nodeName, string conditionName, bool result);

        [ZLoggerMessage(LogLevel.Debug, "[CHK][{nodeName}] Health check: {currentHealth}/{maxHealth} (threshold: {threshold})")]
        static partial void LogHealthCheckInternal(this Microsoft.Extensions.Logging.ILogger logger, string nodeName, int currentHealth, int maxHealth, int threshold);

        // Parser関連ログ - ZLoggerMessage Source Generator自動最適化
        [ZLoggerMessage(LogLevel.Error, "[PRS] Failed to parse node {nodeType} at line {lineNumber}: {errorMessage}")]
        static partial void LogParseErrorInternal(this Microsoft.Extensions.Logging.ILogger logger, string nodeType, int lineNumber, string errorMessage);

        [ZLoggerMessage(LogLevel.Information, "[PRS] Successfully parsed tree '{treeName}' with {nodeCount} nodes")]
        static partial void LogParseSuccessInternal(this Microsoft.Extensions.Logging.ILogger logger, string treeName, int nodeCount);

        [ZLoggerMessage(LogLevel.Debug, "[PRS] Parsing token: {tokenType} = '{tokenValue}' at position {position}")]
        static partial void LogTokenParsingInternal(this Microsoft.Extensions.Logging.ILogger logger, string tokenType, string tokenValue, int position);

        // System関連ログ - ZLoggerMessage Source Generator自動最適化
        [ZLoggerMessage(LogLevel.Information, "[SYS] {message}")]
        static partial void LogSystemInternal(this Microsoft.Extensions.Logging.ILogger logger, string message);

        [ZLoggerMessage(LogLevel.Information, "[SYS][{nodeName}] {message}")]
        static partial void LogSystemInternal2(this Microsoft.Extensions.Logging.ILogger logger, string nodeName, string message);

        [ZLoggerMessage(LogLevel.Error, "[SYS][{nodeName}] Error: {errorMessage}")]
        static partial void LogSystemErrorInternal(this Microsoft.Extensions.Logging.ILogger logger, string nodeName, string errorMessage);

        [ZLoggerMessage(LogLevel.Debug, "[SYS][{nodeName}] Performance: {operation} completed in {elapsedMs:F2}ms")]
        static partial void LogPerformanceInternal(this Microsoft.Extensions.Logging.ILogger logger, string nodeName, string operation, float elapsedMs);

        // ==========================================
        // 公開API - ロガーインスタンス不要
        // ==========================================

        // Combat関連公開API



        // BTNodeベースAPI（推奨）
        public static void LogHealthChange(BTNode node, int currentHealth, int maxHealth, int damage)
            => LogHealthChangeInternal(globalLogger, node?.Name ?? "Unknown", currentHealth, maxHealth, damage);


        // BTNodeベースAPI（推奨）
        public static void LogDamageCalculation(BTNode node, int baseDamage, float multiplier, int finalDamage)
            => LogDamageCalculationInternal(globalLogger, node?.Name ?? "Unknown", baseDamage, multiplier, finalDamage);

        // Movement関連公開API

        // BTNodeベースAPI（推奨）
        public static void LogMovement(BTNode node, Vector3 fromPos, Vector3 toPos, float speed)
            => LogMovementInternal(globalLogger, node?.Name ?? "Unknown", fromPos, toPos, speed);


        // BTNodeベースAPI（推奨）
        public static void LogDestinationReached(BTNode node, Vector3 position, float elapsedTime)
            => LogDestinationReachedInternal(globalLogger, node?.Name ?? "Unknown", position, elapsedTime);


        // BTNodeベースAPI（推奨）
        public static void LogMovementProgress(BTNode node, float currentDistance, float totalDistance, float progress)
            => LogMovementProgressInternal(globalLogger, node?.Name ?? "Unknown", currentDistance, totalDistance, progress);

        // BlackBoard関連公開API

        // BTNodeベースAPI（推奨）
        public static void LogBlackBoardSet(string key, object value, string valueType)
            => LogBlackBoardSetInternal(globalLogger, key, value, valueType);


        // BTNodeベースAPI（推奨）
        public static void LogBlackBoardGet(string key, object value, string valueType)
            => LogBlackBoardGetInternal(globalLogger, key, value, valueType);


        // Condition関連公開API

        // BTNodeベースAPI（推奨）
        public static void LogConditionResult(BTNode node, string conditionName, bool result)
            => LogConditionResultInternal(globalLogger, node?.Name ?? "Unknown", conditionName, result);


        // BTNodeベースAPI（推奨）
        public static void LogHealthCheck(BTNode node, int currentHealth, int maxHealth, int threshold)
            => LogHealthCheckInternal(globalLogger, node?.Name ?? "Unknown", currentHealth, maxHealth, threshold);

        // Parser関連公開API
        public static void LogParseError(string nodeType, int lineNumber, string errorMessage)
            => LogParseErrorInternal(globalLogger, nodeType, lineNumber, errorMessage);

        public static void LogParseSuccess(string treeName, int nodeCount)
            => LogParseSuccessInternal(globalLogger, treeName, nodeCount);

        public static void LogTokenParsing(string tokenType, string tokenValue, int position)
            => LogTokenParsingInternal(globalLogger, tokenType, tokenValue, position);

        public static void LogSystem(string message)
            => LogSystemInternal(globalLogger, message);

        // 特定クラス専用API（Parser、BlackBoard、BehaviourTreeRunner等で使用）
        public static void LogSystem(string context, string message)
            => LogSystemInternal2(globalLogger, context, message);

        public static void LogSystemError(string context, string errorMessage)
            => LogSystemErrorInternal(globalLogger, context, errorMessage);

        // System関連公開API（BTNodeベース）

        // BTNodeベースAPI（推奨）
        public static void LogSystem(BTNode node, string message)
            => LogSystemInternal2(globalLogger, node?.Name ?? "Unknown", message);


        // BTNodeベースAPI（推奨）
        public static void LogSystemError(BTNode node, string errorMessage)
            => LogSystemErrorInternal(globalLogger, node?.Name ?? "Unknown", errorMessage);


        // BTNodeベースAPI（推奨）
        public static void LogPerformance(BTNode node, string operation, float elapsedMs)
            => LogPerformanceInternal(globalLogger, node?.Name ?? "Unknown", operation, elapsedMs);

        // Debug.Logから移行用の便利メソッド（後方互換性）
        public static void Info(string message, string nodeName = "System")
            => LogSystemInternal2(globalLogger, nodeName, message);

        // BTNodeベースAPI（推奨）
        public static void Info(BTNode node, string message)
            => LogSystemInternal2(globalLogger, node?.Name ?? "Unknown", message);

        public static void Error(string message, string nodeName = "System")
            => LogSystemErrorInternal(globalLogger, nodeName, message);

        // BTNodeベースAPI（推奨）
        public static void Error(BTNode node, string message)
            => LogSystemErrorInternal(globalLogger, node?.Name ?? "Unknown", message);

        public static void Debug(string message, string nodeName = "System")
            => LogSystemInternal2(globalLogger, nodeName, $"[DEBUG] {message}");

        // BTNodeベースAPI（推奨）
        public static void Debug(BTNode node, string message)
            => LogSystemInternal2(globalLogger, node?.Name ?? "Unknown", $"[DEBUG] {message}");

        public static void Warning(string message, string nodeName = "System")
            => LogSystemInternal2(globalLogger, nodeName, $"[WARNING] {message}");

        // BTNodeベースAPI（推奨）
        public static void Warning(BTNode node, string message)
            => LogSystemInternal2(globalLogger, node?.Name ?? "Unknown", $"[WARNING] {message}");

        // 旧API互換性メソッド（LogCategory削除済み）
        public static void LogCombat(string message, string nodeName = "", object context = null)
            => LogSystemInternal2(globalLogger, nodeName, $"[ATK] {message}");

        // BTNodeベースAPI（推奨）
        public static void LogCombat(BTNode node, string message)
            => LogSystemInternal2(globalLogger, node?.Name ?? "Unknown", $"[ATK] {message}");

        public static void LogMovement(string message, string nodeName = "", object context = null)
            => LogSystemInternal2(globalLogger, nodeName, $"[MOV] {message}");

        // BTNodeベースAPI（推奨）
        public static void LogMovement(BTNode node, string message)
            => LogSystemInternal2(globalLogger, node?.Name ?? "Unknown", $"[MOV] {message}");

        public static void LogCondition(string message, string nodeName = "", object context = null)
            => LogConditionResultInternal(globalLogger, nodeName, "condition", message.Contains("true") || message.Contains("Success"));

        // BTNodeベースAPI（推奨）
        public static void LogCondition(BTNode node, string message)
            => LogConditionResultInternal(globalLogger, node?.Name ?? "Unknown", "condition", message.Contains("true") || message.Contains("Success"));

        /// <summary>
        /// 後方互換性：レガシーAPI削除のため何もしない
        /// </summary>
        public static void Dispose()
        {
            // ユーザーのLoggerFactoryを使用しているため、何もしない
        }
    }

}
