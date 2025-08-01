using ArcBT.Core;
using ArcBT.Logger;
using UnityEngine;

namespace ArcBT.Samples.RPG.Actions
{
    /// <summary>
    /// ExampleAI用のシンプルな攻撃アクション
    /// </summary>
    [BTNode("SimpleAttack")]
    public class SimpleAttackAction : BTActionNode
    {
        int damage = 25;
        float attackRange = 2.0f;
        ExampleAI aiController;

        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "damage":
                    if (int.TryParse(value, out var damageValue))
                        damage = damageValue;
                    break;
                case "attack_range":
                    if (float.TryParse(value, out var rangeValue))
                        attackRange = rangeValue;
                    break;
            }
        }

        public override void Initialize(MonoBehaviour owner, BlackBoard blackBoard)
        {
            base.Initialize(owner, blackBoard);
            aiController = owner.GetComponent<ExampleAI>();

            if (aiController == null)
            {
                BTLogger.LogSystemError("Combat", "SimpleAttackAction requires ExampleAI component");
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            if (aiController == null)
            {
                BTLogger.LogSystemError("Combat", "ExampleAI controller not found");
                return BTNodeResult.Failure;
            }

            bool attackSucceeded = aiController.AttackEnemy(damage, attackRange);
            BTLogger.LogCombat($"Executed attack with damage {damage}");
            return attackSucceeded ? BTNodeResult.Success : BTNodeResult.Failure;
        }
    }
}