using System;
using UnityEngine;

namespace ArcBT.TagSystem
{
    /// <summary>
    /// 階層的なゲームプレイタグを表すクラス
    /// 例: "Character.Enemy.Boss" のような階層構造を持つ
    /// </summary>
    [Serializable]
    public struct GameplayTag : IEquatable<GameplayTag>
    {
        [SerializeField]
        string tagName;

        /// <summary>
        /// タグの完全な名前（例: "Character.Enemy.Boss"）
        /// </summary>
        public string TagName => tagName ?? string.Empty;

        /// <summary>
        /// タグが有効かどうか
        /// </summary>
        public bool IsValid => !string.IsNullOrEmpty(tagName);

        /// <summary>
        /// 新しいGameplayTagを作成
        /// </summary>
        public GameplayTag(string name)
        {
            tagName = name ?? string.Empty;
        }

        /// <summary>
        /// このタグが指定されたタグと一致するか、その子孫かを判定
        /// </summary>
        public bool MatchesTag(GameplayTag parentTag)
        {
            if (!IsValid || !parentTag.IsValid)
                return false;

            // 完全一致
            if (tagName == parentTag.tagName)
                return true;

            // 親子関係チェック（0アロケーション）
            ReadOnlySpan<char> thisSpan = tagName.AsSpan();
            ReadOnlySpan<char> parentSpan = parentTag.tagName.AsSpan();
            
            // 親タグより短い場合は階層関係ではない
            if (thisSpan.Length <= parentSpan.Length)
                return false;
                
            // 親タグ部分が一致し、その後に"."が続くかチェック
            return thisSpan.StartsWith(parentSpan) && thisSpan[parentSpan.Length] == '.';
        }

        /// <summary>
        /// このタグが指定されたタグの完全な一致かを判定
        /// </summary>
        public bool MatchesTagExact(GameplayTag other)
        {
            return tagName == other.tagName;
        }

        /// <summary>
        /// 指定されたタグコンテナ内のいずれかのタグとマッチするか
        /// </summary>
        public bool MatchesAny(GameplayTagContainer container)
        {
            return container.HasTag(this);
        }

        /// <summary>
        /// 指定されたタグコンテナ内のすべてのタグとマッチするか
        /// </summary>
        public bool MatchesAll(GameplayTagContainer container)
        {
            return container.HasAllTags(new GameplayTagContainer(this));
        }

        /// <summary>
        /// 親タグを取得（例: "Character.Enemy.Boss" → "Character.Enemy"）
        /// </summary>
        public GameplayTag GetParentTag()
        {
            if (!IsValid)
                return new GameplayTag();

            int lastDot = tagName.LastIndexOf('.');
            if (lastDot <= 0)
                return new GameplayTag();

            return new GameplayTag(tagName.Substring(0, lastDot));
        }

        /// <summary>
        /// タグの深さを取得（例: "Character.Enemy.Boss" → 3）
        /// </summary>
        public int GetDepth()
        {
            if (!IsValid)
                return 0;

            return tagName.Split('.').Length;
        }

        public override string ToString()
        {
            return tagName ?? "None";
        }

        public override int GetHashCode()
        {
            return tagName?.GetHashCode() ?? 0;
        }

        public override bool Equals(object obj)
        {
            return obj is GameplayTag other && Equals(other);
        }

        public bool Equals(GameplayTag other)
        {
            return tagName == other.tagName;
        }

        public static bool operator ==(GameplayTag left, GameplayTag right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GameplayTag left, GameplayTag right)
        {
            return !left.Equals(right);
        }

        public static implicit operator string(GameplayTag tag)
        {
            return tag.TagName;
        }

        public static implicit operator GameplayTag(string tagName)
        {
            return new GameplayTag(tagName);
        }
    }
}
