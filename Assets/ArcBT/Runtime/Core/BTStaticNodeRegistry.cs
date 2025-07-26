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
        // アクション生成関数の静的登録
        static readonly Dictionary<string, Func<BTActionNode>> actionCreators = new()
        {
            // 手動登録またはコード生成で追加
            ["MoveToPosition"] = () => new Actions.MoveToPositionAction(),
            ["Wait"] = () => new Actions.WaitAction(),
            ["ScanEnvironment"] = () => new Actions.ScanEnvironmentAction(),
            ["SearchForEnemy"] = () => new Actions.SearchForEnemyAction(),
            ["NormalAttack"] = () => new Actions.NormalAttackAction(),
            ["InteractAction"] = () => new Actions.InteractAction(),
            ["EnvironmentScan"] = () => new Actions.EnvironmentScanAction(),
        };
        
        // 条件生成関数の静的登録
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
            
            BTLogger.LogError(LogCategory.System, $"Unknown condition script: {scriptName}");
            return null;
        }
        
        /// <summary>登録されているアクション名</summary>
        public static IEnumerable<string> GetActionNames() => actionCreators.Keys;
        
        /// <summary>登録されている条件名</summary>
        public static IEnumerable<string> GetConditionNames() => conditionCreators.Keys;
    }
}