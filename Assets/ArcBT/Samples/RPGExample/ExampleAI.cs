using ArcBT.Core;
using ArcBT.Logger;
using UnityEngine;

namespace ArcBT.Samples.RPG
{
    public class ExampleAI : MonoBehaviour
    {
        [SerializeField] BehaviourTreeRunner treeRunner;
        [SerializeField] float health = 100f;
        [SerializeField] Transform[] patrolPoints;
        [SerializeField] Transform target;

        public float Health
        {
            get => health;
            set => health = Mathf.Clamp(value, 0f, 100f);
        }

        public Transform[] PatrolPoints => patrolPoints;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        void Start()
        {
            if (treeRunner == null)
            {
                treeRunner = GetComponent<BehaviourTreeRunner>();
            }

            if (treeRunner != null)
            {
                // example_ai.btファイルを読み込み
                treeRunner.LoadBehaviourTree("example_ai.bt");
            }
        }

        // スクリプトから呼び出されるメソッド群
        public bool CheckHealth(float minHealth)
        {
            return health >= minHealth;
        }

        public bool DetectEnemy(float range)
        {
            if (target == null)
            {
                return false;
            }

            var distance = Vector3.Distance(transform.position, target.position);
            return distance <= range;
        }

        public void MoveToPosition(string targetName, float speed, float tolerance = 0.5f)
        {
            // 実際の移動ロジック（簡略化）
            var targetTransform = FindTargetByName(targetName);
            if (targetTransform != null)
            {
                var direction = (targetTransform.position - transform.position).normalized;
                transform.position += direction * speed * Time.deltaTime;

                BTLogger.LogMovement($"Moving to {targetName} at speed {speed}", gameObject.name, this);
            }
        }

        public void AttackEnemy(int damage, float attackRange)
        {
            if (target != null)
            {
                var distance = Vector3.Distance(transform.position, target.position);
                if (distance <= attackRange)
                {
                    BTLogger.LogCombat($"Attacking enemy for {damage} damage!", gameObject.name, this);
                    // 実際の攻撃ロジック
                }
            }
        }

        public void Wait(float duration)
        {
            BTLogger.LogSystem($"Waiting for {duration} seconds", gameObject.name, this);
            // 実際の待機ロジック（コルーチンなどで実装）
        }

        Transform FindTargetByName(string targetName)
        {
            // パトロールポイントから検索
            if (patrolPoints != null)
            {
                foreach (var point in patrolPoints)
                {
                    if (point.name == targetName)
                    {
                        return point;
                    }
                }
            }

            // シーン内のGameObjectから検索
            var found = GameObject.Find(targetName);
            return found?.transform;
        }

        // デバッグ用
        void OnDrawGizmosSelected()
        {
            // 体力バー
            Gizmos.color = Color.red;
            var healthBarPos = transform.position + Vector3.up * 2f;
            Gizmos.DrawWireCube(healthBarPos, new Vector3(2f, 0.2f, 0f));

            Gizmos.color = Color.green;
            var healthRatio = health / 100f;
            Gizmos.DrawCube(healthBarPos - Vector3.right * (1f - healthRatio),
                new Vector3(2f * healthRatio, 0.2f, 0f));

            // パトロールポイント
            if (patrolPoints != null)
            {
                Gizmos.color = Color.blue;
                foreach (var point in patrolPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawWireSphere(point.position, 0.5f);
                        Gizmos.DrawLine(transform.position, point.position);
                    }
                }
            }

            // ターゲット
            if (target != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(target.position, 1f);
                Gizmos.DrawLine(transform.position, target.position);
            }
        }
    }
}
