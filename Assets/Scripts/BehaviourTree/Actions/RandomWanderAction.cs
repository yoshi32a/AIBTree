using UnityEngine;
using BehaviourTree.Core;

namespace BehaviourTree.Actions
{
    /// <summary>ランダムに徘徊するアクション</summary>
    public class RandomWanderAction : BTActionNode
    {
        float wanderRadius = 10.0f;
        float speed = 2.0f;
        float tolerance = 0.5f;
        Vector3 wanderTarget;
        Vector3 initialPosition;
        bool hasTarget = false;
        
        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "wander_radius":
                    wanderRadius = System.Convert.ToSingle(value);
                    break;
                case "speed":
                    speed = System.Convert.ToSingle(value);
                    break;
                case "tolerance":
                    tolerance = System.Convert.ToSingle(value);
                    break;
            }
        }
        
        public override void Initialize(MonoBehaviour owner, BlackBoard sharedBlackBoard = null)
        {
            base.Initialize(owner, sharedBlackBoard);
            if (owner != null)
            {
                initialPosition = owner.transform.position;
            }
        }
        
        protected override BTNodeResult ExecuteAction()
        {
            if (ownerComponent == null)
            {
                Debug.LogError("RandomWander: Owner is null");
                return BTNodeResult.Failure;
            }
            
            // 新しいランダムターゲットを設定
            if (!hasTarget)
            {
                SetNewWanderTarget();
                hasTarget = true;
            }
            
            // ターゲットに向かって移動
            float distance = Vector3.Distance(transform.position, wanderTarget);
            
            if (distance <= tolerance)
            {
                // ターゲットに到達したら新しいターゲットを設定
                hasTarget = false;
                
                Debug.Log("RandomWander: Reached wander target, setting new target");
                
                return BTNodeResult.Success;
            }
            
            // 移動処理
            Vector3 direction = (wanderTarget - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
            
            // 移動方向を向く
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
            
            Debug.Log($"RandomWander: Moving to target - Distance: {distance:F1}");
            
            return BTNodeResult.Running;
        }
        
        void SetNewWanderTarget()
        {
            // 初期位置を中心とした円内でランダムな点を選択
            Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
            wanderTarget = initialPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
            
            Debug.Log($"RandomWander: New target set at {wanderTarget}");
        }
        
        public override void Reset()
        {
            base.Reset();
            hasTarget = false;
        }
        
        void OnDrawGizmosSelected()
        {
            if (ownerComponent != null)
            {
                // 徘徊範囲を描画
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(initialPosition, wanderRadius);
                
                // 現在のターゲットを描画
                if (hasTarget)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(transform.position, wanderTarget);
                    Gizmos.DrawWireSphere(wanderTarget, tolerance);
                }
            }
        }
    }
}
