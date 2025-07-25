using UnityEngine;
using BehaviourTree.Core;

[System.Serializable]
public class EnemyCheckCondition : BTConditionNode
{
    [SerializeField] float detectionRange = 5.0f;
    
    public override void Initialize(MonoBehaviour owner, BlackBoard sharedBlackBoard = null)
    {
        base.Initialize(owner, sharedBlackBoard);
    }
    
    protected override BTNodeResult CheckCondition()
    {
        Debug.Log($"=== EnemyCheckCondition '{Name}' EXECUTING ===");
        
        if (detectionRange <= 0)
        {
            Debug.LogError($"EnemyCheck '{Name}': Invalid detection range: {detectionRange}");
            return BTNodeResult.Failure;
        }
        
        // シンプルな敵検出: "Enemy"タグのオブジェクトを検索
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        foreach (var enemy in enemies)
        {
            var distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance <= detectionRange)
            {
                Debug.Log($"EnemyCheck '{Name}': Enemy detected at distance {distance:F2} ✓");
                
                // 検出した敵の情報を保存（他のノードで使用可能）
                if (ownerComponent is BehaviourTreeRunner runner)
                {
                    // TODO: 検出した敵の情報をBlackboardに保存
                }
                
                return BTNodeResult.Success;
            }
        }
        
        Debug.Log($"EnemyCheck '{Name}': No enemies in range {detectionRange} ✗");
        return BTNodeResult.Failure;
    }
    
    public override void SetProperty(string propertyName, string value)
    {
        switch (propertyName.ToLower())
        {
            case "detection_range":
            case "detectionrange":
                if (float.TryParse(value, out var range))
                {
                    detectionRange = range;
                }

                break;
            default:
                base.SetProperty(propertyName, value);
                break;
        }
    }
}
