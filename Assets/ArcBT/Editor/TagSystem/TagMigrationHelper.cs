using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ArcBT.TagSystem;

namespace AIBTree.Editor
{
    /// <summary>
    /// TagSystem支援用のエディタメニュー
    /// </summary>
    public static class TagMigrationHelper
    {
        [MenuItem("ArcBT/Tag System/Create Default Tag Asset")]
        public static void CreateDefaultTagAsset()
        {
            var asset = ScriptableObject.CreateInstance<GameplayTagAsset>();
            var path = AssetDatabase.GenerateUniqueAssetPath("Assets/ArcBT/Runtime/Core/TagSystem/DefaultGameplayTags.asset");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
            
            // サンプルタグを追加
            AddSampleTagsToAsset(asset);
        }

        /// <summary>
        /// アセットにサンプルタグを追加
        /// </summary>
        public static void AddSampleTagsToAsset(GameplayTagAsset asset)
        {
            // Character タグ
            asset.AddCategory("Character");
            asset.AddTagToCategory("Character", "Character.Player", "プレイヤーキャラクター");
            asset.AddTagToCategory("Character", "Character.Enemy", "敵キャラクター");
            asset.AddTagToCategory("Character", "Character.Enemy.Boss", "ボスキャラクター");
            asset.AddTagToCategory("Character", "Character.Enemy.Minion", "雑魚敵");
            asset.AddTagToCategory("Character", "Character.NPC", "NPC");
            asset.AddTagToCategory("Character", "Character.NPC.Friendly", "友好的なNPC");
            asset.AddTagToCategory("Character", "Character.NPC.Neutral", "中立NPC");

            // Object タグ
            asset.AddCategory("Object");
            asset.AddTagToCategory("Object", "Object.Item", "アイテム");
            asset.AddTagToCategory("Object", "Object.Item.Weapon", "武器");
            asset.AddTagToCategory("Object", "Object.Item.Consumable", "消耗品");
            asset.AddTagToCategory("Object", "Object.Interactable", "インタラクト可能");
            asset.AddTagToCategory("Object", "Object.Collectible", "収集可能");
            asset.AddTagToCategory("Object", "Object.Destructible", "破壊可能");

            // State タグ
            asset.AddCategory("State");
            asset.AddTagToCategory("State", "State.Combat", "戦闘中");
            asset.AddTagToCategory("State", "State.Combat.Attacking", "攻撃中");
            asset.AddTagToCategory("State", "State.Combat.Defending", "防御中");
            asset.AddTagToCategory("State", "State.Movement.Running", "走行中");
            asset.AddTagToCategory("State", "State.Movement.Walking", "歩行中");
            asset.AddTagToCategory("State", "State.Status.Stunned", "スタン状態");
            asset.AddTagToCategory("State", "State.Status.Invincible", "無敵状態");

            // Ability タグ
            asset.AddCategory("Ability");
            asset.AddTagToCategory("Ability", "Ability.Attack", "攻撃アビリティ");
            asset.AddTagToCategory("Ability", "Ability.Attack.Melee", "近接攻撃");
            asset.AddTagToCategory("Ability", "Ability.Attack.Ranged", "遠距離攻撃");
            asset.AddTagToCategory("Ability", "Ability.Magic", "魔法アビリティ");
            asset.AddTagToCategory("Ability", "Ability.Magic.Fire", "炎魔法");
            asset.AddTagToCategory("Ability", "Ability.Magic.Ice", "氷魔法");
            asset.AddTagToCategory("Ability", "Ability.Magic.Heal", "回復魔法");

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }
    }
}
