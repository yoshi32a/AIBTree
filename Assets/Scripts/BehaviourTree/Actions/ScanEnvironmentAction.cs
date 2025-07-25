using UnityEngine;
using BehaviourTree.Core;

namespace BehaviourTree.Actions
{
    /// <summary>環境をスキャンして敵情報をBlackBoardに保存するアクション</summary>
    public class ScanEnvironmentAction : BTActionNode
    {
        float scanRadius = 10.0f;
        
        public override void SetProperty(string key, string value)
        {
            switch (key)
            {
                case "scan_radius":
                    scanRadius = System.Convert.ToSingle(value);
                    break;
            }
        }
        
        protected override BTNodeResult ExecuteAction()
        {
            if (ownerComponent == null || blackBoard == null)
            {
                Debug.LogError("ScanEnvironment: Owner or BlackBoard is null");
                return BTNodeResult.Failure;
            }
            
            // 周囲の敵を検索
            Collider[] enemies = Physics.OverlapSphere(transform.position, scanRadius, LayerMask.GetMask("Default"));
            GameObject closestEnemy = null;
            float closestDistance = float.MaxValue;
            
            foreach (var collider in enemies)
            {
                if (collider.CompareTag("Enemy"))
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = collider.gameObject;
                    }
                }
            }
            
            // BlackBoardに敵情報を保存
            if (closestEnemy != null)
            {
                blackBoard.SetValue("enemy_target", closestEnemy);
                blackBoard.SetValue("enemy_location", closestEnemy.transform.position);
                blackBoard.SetValue("enemy_distance", closestDistance);
                blackBoard.SetValue("has_enemy_info", true);
                
                Debug.Log($"ScanEnvironment: Enemy found '{closestEnemy.name}' at distance {closestDistance:F1}");
                
                return BTNodeResult.Success;
            }
            else
            {
                blackBoard.SetValue("has_enemy_info", false);
                blackBoard.SetValue("enemy_target", (GameObject)null);
                
                Debug.Log("ScanEnvironment: No enemies found in scan radius");
                
                return BTNodeResult.Failure;
            }
        }
        
        void OnDrawGizmosSelected()
        {
            if (ownerComponent != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, scanRadius);
            }
        }
    }
}
