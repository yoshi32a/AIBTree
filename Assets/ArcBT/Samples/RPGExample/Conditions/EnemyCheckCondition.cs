using System;
using ArcBT.Core;
using ArcBT.Logger;
using ArcBT.TagSystem;
using UnityEngine;

namespace ArcBT.Samples.RPG.Conditions
{
    /// <summary>敵検出条件</summary>
    [Serializable]
    [BTNode("EnemyCheck")]
    public class EnemyCheckCondition : BTConditionNode
    {
        [SerializeField] float detectionRange = 5.0f;

        public override void Initialize(MonoBehaviour owner, BlackBoard sharedBlackBoard = null)
        {
            base.Initialize(owner, sharedBlackBoard);
        }

        protected override BTNodeResult CheckCondition()
        {
            BTLogger.LogCondition($"=== EnemyCheckCondition '{Name}' EXECUTING ===", Name, ownerComponent);

            if (detectionRange <= 0)
            {
                BTLogger.LogError(LogCategory.System, $"EnemyCheck '{Name}': Invalid detection range: {detectionRange}", Name, ownerComponent);
                return BTNodeResult.Failure;
            }

            // シンプルな敵検出: "Enemy"タグのオブジェクトを検索
            var enemies = GameplayTagManager.FindGameObjectsWithTag("Character.Enemy");

            foreach (var enemy in enemies)
            {
                var distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= detectionRange)
                {
                    BTLogger.LogCondition($"EnemyCheck '{Name}': Enemy detected at distance {distance:F2} ✓", Name, ownerComponent);

                    // 検出した敵の情報を保存（他のノードで使用可能）
                    if (ownerComponent is BehaviourTreeRunner runner)
                    {
                        // TODO: 検出した敵の情報をBlackboardに保存
                    }

                    return BTNodeResult.Success;
                }
            }

            BTLogger.LogCondition($"EnemyCheck '{Name}': No enemies in range {detectionRange} ✗", Name, ownerComponent);
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
}
