using ArcBT.TagSystem;
using UnityEngine;

namespace ArcBT.Samples.TagSystem
{
    /// <summary>
    /// GameplayTagシステムの使用例を示すサンプルクラス
    /// </summary>
    public class TagSystemExample : MonoBehaviour
    {
        [SerializeField] GameplayTagAsset tagAsset;

        void Start()
        {
            DemonstrateTagUsage();
            DemonstrateTagSearch();
            DemonstrateTagHierarchy();
        }

        /// <summary>
        /// 基本的なタグの使用方法を示す
        /// </summary>
        void DemonstrateTagUsage()
        {
            Debug.Log("=== GameplayTag 基本使用例 ===");

            // タグの作成
            var playerTag = new GameplayTag("Character.Player");
            var enemyTag = new GameplayTag("Character.Enemy");

            Debug.Log($"プレイヤータグ: {playerTag}");
            Debug.Log($"敵タグ: {enemyTag}");

            // タグコンテナの使用
            var playerTags = new GameplayTagContainer();
            playerTags.AddTag(playerTag);
            playerTags.AddTag(new GameplayTag("State.Combat"));

            Debug.Log($"プレイヤータグコンテナ: {playerTags}");

            // タグの階層チェック
            Debug.Log($"プレイヤータグは 'Character' タグにマッチ: {playerTag.MatchesTag(new GameplayTag("Character"))}");
        }

        /// <summary>
        /// タグを使ったGameObject検索を示す
        /// </summary>
        void DemonstrateTagSearch()
        {
            Debug.Log("=== GameplayTag 検索例 ===");

            // シーン内のキャラクターを検索
            var characters = GameplayTagManager.FindGameObjectsWithTag(new GameplayTag("Character"));
            Debug.Log($"見つかったキャラクター数: {characters.Length}");

            foreach (var character in characters)
            {
                Debug.Log($"キャラクター: {character.name}");
            }

            // 敵のみを検索
            using var enemies = GameplayTagManager.FindGameObjectsWithTag(new GameplayTag("Character.Enemy"));
            Debug.Log($"見つかった敵の数: {enemies.Count}");

            // 複数タグでの検索（プール版）
            var combatTags = new GameplayTagContainer(
                new GameplayTag("Character"),
                new GameplayTag("State.Combat")
            );
            using var combatCharacters = GameplayTagManager.FindGameObjectsWithAllTags(combatTags);
            Debug.Log($"戦闘中のキャラクター数: {combatCharacters.Count}");
        }

        /// <summary>
        /// タグの階層構造と継承を示す
        /// </summary>
        void DemonstrateTagHierarchy()
        {
            Debug.Log("=== GameplayTag 階層構造例 ===");

            var bossTag = new GameplayTag("Character.Enemy.Boss");
            
            Debug.Log($"ボスタグ: {bossTag}");
            Debug.Log($"ボスタグの深さ: {bossTag.GetDepth()}");
            Debug.Log($"ボスタグの親: {bossTag.GetParentTag()}");

            // 階層マッチング
            Debug.Log($"ボスは 'Character' にマッチ: {bossTag.MatchesTag(new GameplayTag("Character"))}");
            Debug.Log($"ボスは 'Character.Enemy' にマッチ: {bossTag.MatchesTag(new GameplayTag("Character.Enemy"))}");
            Debug.Log($"ボスは 'Character.Player' にマッチ: {bossTag.MatchesTag(new GameplayTag("Character.Player"))}");

            // 展開されたタグコンテナ
            var bossContainer = new GameplayTagContainer(bossTag);
            var expandedContainer = bossContainer.GetExpandedContainer();
            Debug.Log($"展開されたボスタグ: {expandedContainer}");
        }

        /// <summary>
        /// 実行時にタグを動的に変更する例
        /// </summary>
        [ContextMenu("Toggle Combat State")]
        public void ToggleCombatState()
        {
            var tagComponent = GetComponent<GameplayTagComponent>();
            if (tagComponent == null)
            {
                tagComponent = gameObject.AddComponent<GameplayTagComponent>();
            }

            var combatTag = new GameplayTag("State.Combat");
            
            if (tagComponent.HasTag(combatTag))
            {
                tagComponent.RemoveTag(combatTag);
                Debug.Log($"{gameObject.name}: 戦闘状態を解除");
            }
            else
            {
                tagComponent.AddTag(combatTag);
                Debug.Log($"{gameObject.name}: 戦闘状態に移行");
            }
        }
    }
}
