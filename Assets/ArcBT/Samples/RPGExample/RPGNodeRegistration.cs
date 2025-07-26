using System.Collections.Generic;
using ArcBT.Core;
using ArcBT.Samples.RPG.Actions;
using ArcBT.Samples.RPG.Conditions;
using UnityEngine;

namespace ArcBT.Samples.RPG
{
    /// <summary>
    /// RPGサンプルのノードを静的レジストリに登録
    /// </summary>
    [DefaultExecutionOrder(-1000)] // 他のスクリプトより先に実行
    public static class RPGNodeRegistration
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RegisterNodes()
        {
            // RPGアクションを登録
            BTStaticNodeRegistry.RegisterAction("RandomWander", () => new RandomWanderAction());
            BTStaticNodeRegistry.RegisterAction("AttackEnemy", () => new AttackEnemyAction());
            BTStaticNodeRegistry.RegisterAction("AttackTarget", () => new AttackTargetAction());
            BTStaticNodeRegistry.RegisterAction("CastSpell", () => new CastSpellAction());
            BTStaticNodeRegistry.RegisterAction("FleeToSafety", () => new FleeToSafetyAction());
            BTStaticNodeRegistry.RegisterAction("MoveToEnemy", () => new MoveToEnemyAction());
            BTStaticNodeRegistry.RegisterAction("MoveToTarget", () => new MoveToTargetAction());
            BTStaticNodeRegistry.RegisterAction("UseItem", () => new UseItemAction());
            BTStaticNodeRegistry.RegisterAction("InitializeResources", () => new InitializeResourcesAction());
            BTStaticNodeRegistry.RegisterAction("RestoreSmallMana", () => new RestoreSmallManaAction());
            BTStaticNodeRegistry.RegisterAction("SearchForEnemy", () => new SearchForEnemyAction());
            BTStaticNodeRegistry.RegisterAction("Attack", () => new AttackAction());
            
            // RPG条件を登録
            BTStaticNodeRegistry.RegisterCondition("HealthCheck", () => new HealthCheckCondition());
            BTStaticNodeRegistry.RegisterCondition("EnemyCheck", () => new EnemyCheckCondition());
            BTStaticNodeRegistry.RegisterCondition("HasMana", () => new HasManaCondition());
            BTStaticNodeRegistry.RegisterCondition("EnemyInRange", () => new EnemyInRangeCondition());
            BTStaticNodeRegistry.RegisterCondition("IsInitialized", () => new IsInitializedCondition());
            BTStaticNodeRegistry.RegisterCondition("CheckManaResource", () => new CheckManaResourceCondition());
            BTStaticNodeRegistry.RegisterCondition("HasItem", () => new HasItemCondition());
            BTStaticNodeRegistry.RegisterCondition("HasTarget", () => new HasTargetCondition());
            BTStaticNodeRegistry.RegisterCondition("EnemyHealthCheck", () => new EnemyHealthCheckCondition());
            BTStaticNodeRegistry.RegisterCondition("ScanForInterest", () => new ScanForInterestCondition());
            
            BTLogger.LogSystem($"RPGサンプルノードを登録しました");
        }
    }
}