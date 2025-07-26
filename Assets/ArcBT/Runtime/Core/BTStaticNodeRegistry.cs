using System;
using System.Collections.Generic;

namespace ArcBT.Core
{
    /// <summary>
    /// リフレクションを使わない静的ノードレジストリ
    /// ビルド時に自動生成される想定
    /// </summary>
    public static class BTStaticNodeRegistry
    {
        // アクション生成関数の登録（実行時に追加可能）
        static readonly Dictionary<string, Func<BTActionNode>> actionCreators = new()
        {
            // コアノードのみここで登録
            // RPGサンプルは RPGNodeRegistration.cs で動的に登録される
            ["MoveToPosition"] = () => new Actions.MoveToPositionAction(),
            ["Wait"] = () => new Actions.WaitAction(),
            ["ScanEnvironment"] = () => new Actions.ScanEnvironmentAction(),
            ["SearchForEnemy"] = () => new Actions.SearchForEnemyAction(),
            ["NormalAttack"] = () => new Actions.NormalAttackAction(),
            ["Interact"] = () => new Actions.InteractAction(),
            ["EnvironmentScan"] = () => new Actions.EnvironmentScanAction(),
        };
        
        // 条件生成関数の登録（実行時に追加可能）
        static readonly Dictionary<string, Func<BTConditionNode>> conditionCreators = new()
        {
            ["HasSharedEnemyInfo"] = () => new Conditions.HasSharedEnemyInfoCondition(),
            ["HasTarget"] = () => new Conditions.HasTargetCondition(),
            ["EnemyHealthCheck"] = () => new Conditions.EnemyHealthCheckCondition(),
            ["ScanForInterest"] = () => new Conditions.ScanForInterestCondition(),
        };
        
        /// <summary>アクションを作成（リフレクション不使用）</summary>
        public static BTActionNode CreateAction(string scriptName)
        {
            if (actionCreators.TryGetValue(scriptName, out var creator))
            {
                return creator();
            }
            
            // フォールバック: BTNodeRegistryを使用（RPGサンプル等のため）
            var node = BTNodeRegistry.CreateAction(scriptName);
            if (node != null)
            {
                BTLogger.Log(LogLevel.Debug, LogCategory.System, 
                    $"Created action '{scriptName}' via BTNodeRegistry (reflection fallback)");
                return node;
            }
            
            BTLogger.LogError(LogCategory.System, $"Unknown action script: {scriptName}");
            return null;
        }
        
        /// <summary>条件を作成（リフレクション不使用）</summary>
        public static BTConditionNode CreateCondition(string scriptName)
        {
            if (conditionCreators.TryGetValue(scriptName, out var creator))
            {
                return creator();
            }
            
            // フォールバック: BTNodeRegistryを使用（RPGサンプル等のため）
            var node = BTNodeRegistry.CreateCondition(scriptName);
            if (node != null)
            {
                BTLogger.Log(LogLevel.Debug, LogCategory.System, 
                    $"Created condition '{scriptName}' via BTNodeRegistry (reflection fallback)");
                return node;
            }
            
            BTLogger.LogError(LogCategory.System, $"Unknown condition script: {scriptName}");
            return null;
        }
        
        /// <summary>アクションを動的に登録</summary>
        public static void RegisterAction(string scriptName, Func<BTActionNode> creator)
        {
            if (actionCreators.ContainsKey(scriptName))
            {
                BTLogger.Log(LogLevel.Warning, LogCategory.System, 
                    $"Action '{scriptName}' is already registered. Overwriting.");
            }
            actionCreators[scriptName] = creator;
        }
        
        /// <summary>条件を動的に登録</summary>
        public static void RegisterCondition(string scriptName, Func<BTConditionNode> creator)
        {
            if (conditionCreators.ContainsKey(scriptName))
            {
                BTLogger.Log(LogLevel.Warning, LogCategory.System, 
                    $"Condition '{scriptName}' is already registered. Overwriting.");
            }
            conditionCreators[scriptName] = creator;
        }
        
        /// <summary>登録されているアクション名</summary>
        public static IEnumerable<string> GetActionNames() => actionCreators.Keys;
        
        /// <summary>登録されている条件名</summary>
        public static IEnumerable<string> GetConditionNames() => conditionCreators.Keys;
    }
}