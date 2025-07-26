using System;
using System.Collections.Generic;
using ArcBT.Actions;
using ArcBT.Conditions;
using ArcBT.Samples.RPG;
using ArcBT.Samples.RPG.Actions;
using ArcBT.Samples.RPG.Conditions;

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
            // コアアクション
            ["MoveToPosition"] = () => new Actions.MoveToPositionAction(),
            ["Wait"] = () => new Actions.WaitAction(),
            ["ScanEnvironment"] = () => new Actions.ScanEnvironmentAction(),
            ["SearchForEnemy"] = () => new Actions.SearchForEnemyAction(),
            ["NormalAttack"] = () => new Actions.NormalAttackAction(),
            ["Interact"] = () => new Actions.InteractAction(),
            ["EnvironmentScan"] = () => new Actions.EnvironmentScanAction(),
            
            // RPGサンプルアクション
            ["RandomWander"] = () => new Samples.RPG.RandomWanderAction(),
            ["AttackEnemy"] = () => new Samples.RPG.Actions.AttackEnemyAction(),
            ["AttackTarget"] = () => new Samples.RPG.Actions.AttackTargetAction(),
            ["CastSpell"] = () => new Samples.RPG.Actions.CastSpellAction(),
            ["FleeToSafety"] = () => new Samples.RPG.Actions.FleeToSafetyAction(),
            ["MoveToEnemy"] = () => new Samples.RPG.Actions.MoveToEnemyAction(),
            ["UseItem"] = () => new Samples.RPG.Actions.UseItemAction(),
            ["InitializeResources"] = () => new Samples.RPG.Actions.InitializeResourcesAction(),
        };
        
        // 条件生成関数の静的登録
        static readonly Dictionary<string, Func<BTConditionNode>> conditionCreators = new()
        {
            // コア条件
            ["HasSharedEnemyInfo"] = () => new Conditions.HasSharedEnemyInfoCondition(),
            ["HasTarget"] = () => new Conditions.HasTargetCondition(),
            ["EnemyHealthCheck"] = () => new Conditions.EnemyHealthCheckCondition(),
            ["ScanForInterest"] = () => new Conditions.ScanForInterestCondition(),
            
            // RPGサンプル条件
            ["HealthCheck"] = () => new Samples.RPG.Conditions.HealthCheckCondition(),
            ["EnemyCheck"] = () => new Samples.RPG.Conditions.EnemyCheckCondition(),
            ["HasMana"] = () => new Samples.RPG.Conditions.HasManaCondition(),
            ["EnemyInRange"] = () => new Samples.RPG.Conditions.EnemyInRangeCondition(),
            ["IsInitialized"] = () => new Samples.RPG.Conditions.IsInitializedCondition(),
            ["CheckManaResource"] = () => new Samples.RPG.Conditions.CheckManaResourceCondition(),
            ["HasItem"] = () => new Samples.RPG.Conditions.HasItemCondition(),
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