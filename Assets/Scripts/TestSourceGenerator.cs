using UnityEngine;
using ArcBT.Core;
using ArcBT.Logger;

/// <summary>
/// Source Generator の動作テスト用スクリプト
/// Play モードで実行すると、自動生成された登録コードが動作しているか確認できます
/// </summary>
public class TestSourceGenerator : MonoBehaviour
{
    void Start()
    {
        BTLogger.Info("=== Testing Source Generator ===");
        
        // MoveToPosition アクションを作成（自動登録されているはず）
        var moveAction = BTStaticNodeRegistry.CreateNode("Action", "MoveToPosition");
        if (moveAction != null)
        {
            BTLogger.Info($"✓ MoveToPosition created successfully: {moveAction.GetType().Name}");
        }
        else
        {
            BTLogger.Error("✗ MoveToPosition not found in registry!");
        }
        
        // Wait アクションを作成
        var waitAction = BTStaticNodeRegistry.CreateNode("Action", "Wait");
        if (waitAction != null)
        {
            BTLogger.Info($"✓ Wait created successfully: {waitAction.GetType().Name}");
        }
        else
        {
            BTLogger.Error("✗ Wait not found in registry!");
        }
        
        // HasSharedEnemyInfo 条件を作成
        var condition = BTStaticNodeRegistry.CreateNode("Condition", "HasSharedEnemyInfo");
        if (condition != null)
        {
            BTLogger.Info($"✓ HasSharedEnemyInfo created successfully: {condition.GetType().Name}");
        }
        else
        {
            BTLogger.Error("✗ HasSharedEnemyInfo not found in registry!");
        }
        
        // RPGサンプルのノード（自動登録されているはず）
        var attackAction = BTStaticNodeRegistry.CreateNode("Action", "AttackEnemy");
        if (attackAction != null)
        {
            BTLogger.Info($"✓ AttackEnemy created successfully: {attackAction.GetType().Name}");
        }
        else
        {
            BTLogger.Error("✗ AttackEnemy not found in registry!");
        }
        
        var healthCheck = BTStaticNodeRegistry.CreateNode("Condition", "HealthCheck");
        if (healthCheck != null)
        {
            BTLogger.Info($"✓ HealthCheck created successfully: {healthCheck.GetType().Name}");
        }
        else
        {
            BTLogger.Error("✗ HealthCheck not found in registry!");
        }
        
        BTLogger.Info("=== Test Complete ===");
    }
}
