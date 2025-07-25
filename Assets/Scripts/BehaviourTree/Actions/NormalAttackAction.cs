using UnityEngine;
using BehaviourTree.Core;
using Components;

namespace BehaviourTree.Actions
{
    /// <summary>通常攻撃アクション</summary>
    public class NormalAttackAction : BTActionNode
    {
        int damage = 15;
        float cooldown = 1.0f;
        float lastAttackTime = 0f;

        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "damage":
                    damage = System.Convert.ToInt32(value);
                    break;
                case "cooldown":
                    cooldown = System.Convert.ToSingle(value);
                    break;
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            if (ownerComponent == null || blackBoard == null)
            {
                return BTNodeResult.Failure;
            }

            // ターゲットを取得
            GameObject target = blackBoard.GetValue<GameObject>("nearest_enemy");
            if (target == null)
            {
                target = blackBoard.GetValue<GameObject>("current_target");
            }

            if (target == null || !target.activeInHierarchy)
            {
                Debug.Log("NormalAttack: No valid target found");
                return BTNodeResult.Failure;
            }

            // クールダウンチェック
            if (Time.time - lastAttackTime < cooldown)
            {
                return BTNodeResult.Running;
            }

            // 通常攻撃実行
            Health targetHealth = target.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
                lastAttackTime = Time.time;

                // BlackBoardに攻撃情報を記録
                blackBoard.SetValue("last_normal_attack_time", Time.time);
                blackBoard.SetValue("normal_attack_count", blackBoard.GetValue<int>("normal_attack_count", 0) + 1);

                Debug.Log($"NormalAttack: Normal attack on '{target.name}' for {damage} damage");
                return BTNodeResult.Success;
            }
            else
            {
                Debug.LogWarning($"NormalAttack: Target '{target.name}' has no Health component");
                return BTNodeResult.Failure;
            }
        }

        public override void Reset()
        {
            base.Reset();
            lastAttackTime = 0f;
        }
    }
}