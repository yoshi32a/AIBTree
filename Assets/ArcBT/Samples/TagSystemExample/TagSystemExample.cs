using ArcBT.TagSystem;
using ArcBT.Logger;
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
            BTLogger.LogSystem("=== GameplayTag 基本使用例 ===", nameof(TagSystemExample));

            // タグの作成
            var playerTag = new GameplayTag("Character.Player");
            var enemyTag = new GameplayTag("Character.Enemy");

            BTLogger.LogSystem($"プレイヤータグ: {playerTag}", nameof(TagSystemExample));
            BTLogger.LogSystem($"敵タグ: {enemyTag}", nameof(TagSystemExample));

            // タグコンテナの使用
            var playerTags = new GameplayTagContainer();
            playerTags.AddTag(playerTag);
            playerTags.AddTag(new GameplayTag("State.Combat"));

            BTLogger.LogSystem($"プレイヤータグコンテナ: {playerTags}", nameof(TagSystemExample));

            // タグの階層チェック
            BTLogger.LogSystem($"プレイヤータグは 'Character' タグにマッチ: {playerTag.MatchesTag(new GameplayTag("Character"))}", nameof(TagSystemExample));
        }

        /// <summary>
        /// タグを使ったGameObject検索を示す
        /// </summary>
        void DemonstrateTagSearch()
        {
            BTLogger.LogSystem("=== GameplayTag 検索例 ===", nameof(TagSystemExample));

            // シーン内のキャラクターを検索
            var characters = GameplayTagManager.FindGameObjectsWithTag(new GameplayTag("Character"));
            BTLogger.LogSystem($"見つかったキャラクター数: {characters.Length}", nameof(TagSystemExample));

            foreach (var character in characters)
            {
                BTLogger.LogSystem($"キャラクター: {character.name}", nameof(TagSystemExample));
            }

            // 敵のみを検索
            using var enemies = GameplayTagManager.FindGameObjectsWithTag(new GameplayTag("Character.Enemy"));
            BTLogger.LogSystem($"見つかった敵の数: {enemies.Count}", nameof(TagSystemExample));

            // 複数タグでの検索（プール版）
            var combatTags = new GameplayTagContainer(
                new GameplayTag("Character"),
                new GameplayTag("State.Combat")
            );
            using var combatCharacters = GameplayTagManager.FindGameObjectsWithAllTags(combatTags);
            BTLogger.LogSystem($"戦闘中のキャラクター数: {combatCharacters.Count}", nameof(TagSystemExample));
        }

        /// <summary>
        /// タグの階層構造と継承を示す
        /// </summary>
        void DemonstrateTagHierarchy()
        {
            BTLogger.LogSystem("=== GameplayTag 階層構造例 ===", nameof(TagSystemExample));

            var bossTag = new GameplayTag("Character.Enemy.Boss");
            
            BTLogger.LogSystem($"ボスタグ: {bossTag}", nameof(TagSystemExample));
            BTLogger.LogSystem($"ボスタグの深さ: {bossTag.GetDepth()}", nameof(TagSystemExample));
            BTLogger.LogSystem($"ボスタグの親: {bossTag.GetParentTag()}", nameof(TagSystemExample));

            // 階層マッチング
            BTLogger.LogSystem($"ボスは 'Character' にマッチ: {bossTag.MatchesTag(new GameplayTag("Character"))}", nameof(TagSystemExample));
            BTLogger.LogSystem($"ボスは 'Character.Enemy' にマッチ: {bossTag.MatchesTag(new GameplayTag("Character.Enemy"))}", nameof(TagSystemExample));
            BTLogger.LogSystem($"ボスは 'Character.Player' にマッチ: {bossTag.MatchesTag(new GameplayTag("Character.Player"))}", nameof(TagSystemExample));

            // 展開されたタグコンテナ
            var bossContainer = new GameplayTagContainer(bossTag);
            var expandedContainer = bossContainer.GetExpandedContainer();
            BTLogger.LogSystem($"展開されたボスタグ: {expandedContainer}", nameof(TagSystemExample));
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
                BTLogger.LogSystem($"{gameObject.name}: 戦闘状態を解除", nameof(TagSystemExample));
            }
            else
            {
                tagComponent.AddTag(combatTag);
                BTLogger.LogSystem($"{gameObject.name}: 戦闘状態に移行", nameof(TagSystemExample));
            }
        }
    }
}
