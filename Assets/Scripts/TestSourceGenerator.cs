using UnityEngine;
using ArcBT.Core;

/// <summary>
/// Source Generator の動作テスト用スクリプト
/// Play モードで実行すると、自動生成された登録コードが動作しているか確認できます
/// </summary>
public class TestSourceGenerator : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== Testing Source Generator ===");
        
        // MoveToPosition アクションを作成（自動登録されているはず）
        var moveAction = BTStaticNodeRegistry.CreateAction("MoveToPosition");
        if (moveAction != null)
        {
            Debug.Log($"✓ MoveToPosition created successfully: {moveAction.GetType().Name}");
        }
        else
        {
            Debug.LogError("✗ MoveToPosition not found in registry!");
        }
        
        // Wait アクションを作成
        var waitAction = BTStaticNodeRegistry.CreateAction("Wait");
        if (waitAction != null)
        {
            Debug.Log($"✓ Wait created successfully: {waitAction.GetType().Name}");
        }
        else
        {
            Debug.LogError("✗ Wait not found in registry!");
        }
        
        // HasSharedEnemyInfo 条件を作成
        var condition = BTStaticNodeRegistry.CreateCondition("HasSharedEnemyInfo");
        if (condition != null)
        {
            Debug.Log($"✓ HasSharedEnemyInfo created successfully: {condition.GetType().Name}");
        }
        else
        {
            Debug.LogError("✗ HasSharedEnemyInfo not found in registry!");
        }
        
        // RPGサンプルのノード（自動登録されているはず）
        var attackAction = BTStaticNodeRegistry.CreateAction("AttackEnemy");
        if (attackAction != null)
        {
            Debug.Log($"✓ AttackEnemy created successfully: {attackAction.GetType().Name}");
        }
        else
        {
            Debug.LogError("✗ AttackEnemy not found in registry!");
        }
        
        var healthCheck = BTStaticNodeRegistry.CreateCondition("HealthCheck");
        if (healthCheck != null)
        {
            Debug.Log($"✓ HealthCheck created successfully: {healthCheck.GetType().Name}");
        }
        else
        {
            Debug.LogError("✗ HealthCheck not found in registry!");
        }
        
        Debug.Log("=== Test Complete ===");
    }
}