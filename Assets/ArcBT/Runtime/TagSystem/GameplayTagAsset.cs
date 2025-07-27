using System.Collections.Generic;
using UnityEngine;

namespace ArcBT.TagSystem
{
    /// <summary>
    /// プロジェクト全体で使用するタグを定義するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "GameplayTagAsset", menuName = "ArcBT/Tag System/Gameplay Tag Asset")]
    public class GameplayTagAsset : ScriptableObject
    {
        [SerializeField] List<TagCategory> categories = new List<TagCategory>();

        /// <summary>
        /// すべてのカテゴリを取得
        /// </summary>
        public List<TagCategory> Categories => categories;

        /// <summary>
        /// すべての定義されたタグを取得
        /// </summary>
        public List<GameplayTag> GetAllTags()
        {
            var allTags = new List<GameplayTag>();

            foreach (var category in categories)
            {
                foreach (var tagDef in category.tags)
                {
                    if (!string.IsNullOrEmpty(tagDef.tagName))
                    {
                        allTags.Add(new GameplayTag(tagDef.tagName));
                    }
                }
            }

            return allTags;
        }

        /// <summary>
        /// タグ名からタグ定義を取得
        /// </summary>
        public TagDefinition GetTagDefinition(string tagName)
        {
            foreach (var category in categories)
            {
                foreach (var tagDef in category.tags)
                {
                    if (tagDef.tagName == tagName)
                    {
                        return tagDef;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// タグが定義されているかチェック
        /// </summary>
        public bool IsTagDefined(string tagName)
        {
            return GetTagDefinition(tagName) != null;
        }

#if UNITY_EDITOR
        /// <summary>
        /// エディタ用：新しいカテゴリを追加
        /// </summary>
        public void AddCategory(string categoryName)
        {
            categories.Add(new TagCategory { categoryName = categoryName });
        }

        /// <summary>
        /// エディタ用：カテゴリにタグを追加
        /// </summary>
        public void AddTagToCategory(string categoryName, string tagName, string description = "")
        {
            var category = categories.Find(c => c.categoryName == categoryName);
            if (category != null)
            {
                category.tags.Add(new TagDefinition
                {
                    tagName = tagName,
                    description = description
                });
            }
        }
#endif
    }
}
