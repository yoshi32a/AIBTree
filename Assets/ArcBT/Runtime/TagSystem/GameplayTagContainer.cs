using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArcBT.TagSystem
{
    /// <summary>
    /// 複数のGameplayTagを管理するコンテナ
    /// </summary>
    [Serializable]
    public class GameplayTagContainer
    {
        [SerializeField] List<GameplayTag> tags;

        /// <summary>
        /// タグの数
        /// </summary>
        public int Count => tags.Count;

        /// <summary>
        /// すべてのタグを取得
        /// </summary>
        public IReadOnlyList<GameplayTag> Tags => tags;

        /// <summary>
        /// 空のコンテナを作成
        /// </summary>
        public GameplayTagContainer()
        {
            tags = new List<GameplayTag>();
        }

        /// <summary>
        /// 単一のタグでコンテナを作成
        /// </summary>
        public GameplayTagContainer(GameplayTag tag)
        {
            tags = new List<GameplayTag> { tag };
        }

        /// <summary>
        /// 複数のタグでコンテナを作成
        /// </summary>
        public GameplayTagContainer(params GameplayTag[] tagsArray)
        {
            tags = new List<GameplayTag>(tagsArray);
        }

        /// <summary>
        /// 別のコンテナからコピーして作成
        /// </summary>
        public GameplayTagContainer(GameplayTagContainer other)
        {
            tags = new List<GameplayTag>(other.tags);
        }

        /// <summary>
        /// タグを追加
        /// </summary>
        public void AddTag(GameplayTag tag)
        {
            if (tag.IsValid && !tags.Contains(tag))
            {
                tags.Add(tag);
            }
        }

        /// <summary>
        /// 複数のタグを追加
        /// </summary>
        public void AddTags(GameplayTagContainer container)
        {
            foreach (var tag in container.tags)
            {
                AddTag(tag);
            }
        }

        /// <summary>
        /// タグを削除
        /// </summary>
        public bool RemoveTag(GameplayTag tag)
        {
            return tags.Remove(tag);
        }

        /// <summary>
        /// 複数のタグを削除
        /// </summary>
        public void RemoveTags(GameplayTagContainer container)
        {
            foreach (var tag in container.tags)
            {
                RemoveTag(tag);
            }
        }

        /// <summary>
        /// すべてのタグをクリア
        /// </summary>
        public void Clear()
        {
            tags.Clear();
        }

        /// <summary>
        /// 指定されたタグ（またはその親）を含むかチェック
        /// </summary>
        public bool HasTag(GameplayTag tag)
        {
            if (!tag.IsValid)
                return false;

            foreach (var containerTag in tags)
            {
                if (containerTag.MatchesTag(tag) || tag.MatchesTag(containerTag))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 指定されたタグと完全一致するものを含むかチェック
        /// </summary>
        public bool HasTagExact(GameplayTag tag)
        {
            return tags.Contains(tag);
        }

        /// <summary>
        /// 指定されたコンテナのいずれかのタグを含むかチェック
        /// </summary>
        public bool HasAnyTags(GameplayTagContainer container)
        {
            foreach (var tag in container.tags)
            {
                if (HasTag(tag))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 指定されたコンテナのすべてのタグを含むかチェック
        /// </summary>
        public bool HasAllTags(GameplayTagContainer container)
        {
            foreach (var tag in container.tags)
            {
                if (!HasTag(tag))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 指定されたコンテナのタグを一つも含まないかチェック
        /// </summary>
        public bool HasNoTags(GameplayTagContainer container)
        {
            return !HasAnyTags(container);
        }

        /// <summary>
        /// このコンテナと別のコンテナの交差を取得
        /// </summary>
        public GameplayTagContainer GetIntersection(GameplayTagContainer other)
        {
            var result = new GameplayTagContainer();
            foreach (var tag in tags)
            {
                if (other.HasTag(tag))
                {
                    result.AddTag(tag);
                }
            }

            return result;
        }

        /// <summary>
        /// このコンテナと別のコンテナの和集合を取得
        /// </summary>
        public GameplayTagContainer GetUnion(GameplayTagContainer other)
        {
            var result = new GameplayTagContainer(this);
            result.AddTags(other);
            return result;
        }

        /// <summary>
        /// 親タグを含む展開されたコンテナを取得
        /// 例: "Character.Enemy.Boss" → ["Character", "Character.Enemy", "Character.Enemy.Boss"]
        /// </summary>
        public GameplayTagContainer GetExpandedContainer()
        {
            var result = new GameplayTagContainer();
            var addedTags = new HashSet<string>();

            foreach (var tag in tags)
            {
                var current = tag;
                while (current.IsValid)
                {
                    if (addedTags.Add(current.TagName))
                    {
                        result.AddTag(current);
                    }

                    current = current.GetParentTag();
                }
            }

            return result;
        }

        public override string ToString()
        {
            return tags.Count == 0 ? "Empty" : string.Join(", ", tags.Select(t => t.ToString()));
        }
    }
}
