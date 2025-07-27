using ArcBT.Core;
using ArcBT.Logger;
using ArcBT.Samples.RPG.Components;
using UnityEngine;

namespace ArcBT.Samples.RPG.Actions
{
    /// <summary>リソースを初期化するアクション</summary>
    [BTNode("InitializeResources")]
    public class InitializeResourcesAction : BTActionNode    {
        protected override BTNodeResult ExecuteAction()
        {
            if (ownerComponent == null || blackBoard == null)
            {
                return BTNodeResult.Failure;
            }

            // リソースの初期化
            InitializeHealth();
            InitializeMana();
            InitializeInventory();
            InitializeOtherResources();

            // 初期化完了をBlackBoardに記録
            blackBoard.SetValue("resources_initialized", true);
            blackBoard.SetValue("initialization_time", Time.time);

            BTLogger.LogSystem("InitializeResources: All resources initialized", Name, ownerComponent);
            return BTNodeResult.Success;
        }

        void InitializeHealth()
        {
            var health = ownerComponent.GetComponent<Health>();
            if (health != null)
            {
                blackBoard.SetValue("max_health", health.MaxHealth);
                blackBoard.SetValue("current_health", health.CurrentHealth);
            }
            else
            {
                // Healthコンポーネントがない場合のデフォルト値
                blackBoard.SetValue("max_health", 100);
                blackBoard.SetValue("current_health", 100);
            }
        }

        void InitializeMana()
        {
            // マナリソースの初期化
            int maxMana = 100;
            int currentMana = maxMana;

            blackBoard.SetValue("max_mana", maxMana);
            blackBoard.SetValue("current_mana", currentMana);
            blackBoard.SetValue("mana_regeneration_rate", 5); // 毎秒5マナ回復
        }

        void InitializeInventory()
        {
            var inventory = ownerComponent.GetComponent<Inventory>();
            if (inventory != null)
            {
                // インベントリが存在する場合の初期化
                blackBoard.SetValue("has_inventory", true);
                blackBoard.SetValue("inventory_capacity", 20);
            }
            else
            {
                // インベントリがない場合
                blackBoard.SetValue("has_inventory", false);
            }
        }

        void InitializeOtherResources()
        {
            // その他のリソースの初期化
            blackBoard.SetValue("stamina", 100);
            blackBoard.SetValue("max_stamina", 100);
            blackBoard.SetValue("experience", 0);
            blackBoard.SetValue("level", 1);

            // AIの状態初期化
            blackBoard.SetValue("is_alert", false);
            blackBoard.SetValue("is_in_combat", false);
            blackBoard.SetValue("is_patrolling", false);
            blackBoard.SetValue("current_state", "idle");

            // 位置情報の初期化
            blackBoard.SetValue("spawn_position", transform.position);
            blackBoard.SetValue("last_known_player_position", Vector3.zero);

            // カウンター類の初期化
            blackBoard.SetValue("kill_count", 0);
            blackBoard.SetValue("death_count", 0);
            blackBoard.SetValue("items_collected", 0);
        }
    }
}
