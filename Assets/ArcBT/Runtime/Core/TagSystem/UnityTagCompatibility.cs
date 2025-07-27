using UnityEngine;

namespace ArcBT.TagSystem
{
    /// <summary>
    /// UnityのTag APIとの互換性を提供する拡張メソッド
    /// 既存コードの移行を簡単にするためのヘルパー
    /// </summary>
    public static class UnityTagCompatibility
    {
        /// <summary>
        /// GameObject.CompareTagの代替
        /// GameObjectがGameplayTagComponentを持っていて指定されたタグを持つかチェック
        /// </summary>
        public static bool CompareGameplayTag(this GameObject gameObject, GameplayTag tag)
        {
            return GameplayTagManager.HasTag(gameObject, tag);
        }

        /// <summary>
        /// GameObject.CompareTagの代替（文字列版）
        /// GameObjectがGameplayTagComponentを持っていて指定されたタグを持つかチェック
        /// </summary>
        public static bool CompareGameplayTag(this GameObject gameObject, string tagName)
        {
            return GameplayTagManager.HasTag(gameObject, tagName);
        }

        /// <summary>
        /// Component.CompareTagの代替
        /// ComponentのGameObjectがGameplayTagComponentを持っていて指定されたタグを持つかチェック
        /// </summary>
        public static bool CompareGameplayTag(this Component component, GameplayTag tag)
        {
            if (component == null || component.gameObject == null) return false;
            return GameplayTagManager.HasTag(component.gameObject, tag);
        }

        /// <summary>
        /// GameObject.FindWithTagの代替
        /// 指定されたタグを持つ最初のGameObjectを検索
        /// </summary>
        public static GameObject FindWithGameplayTag(GameplayTag tag)
        {
            return GameplayTagManager.FindGameObjectWithTag(tag);
        }

        /// <summary>
        /// GameObject.FindWithTagの代替（文字列版）
        /// 指定されたタグを持つ最初のGameObjectを検索
        /// </summary>
        public static GameObject FindWithGameplayTag(string tagName)
        {
            return GameplayTagManager.FindGameObjectWithTag(new GameplayTag(tagName));
        }

        /// <summary>
        /// GameObject.FindGameObjectsWithTagの代替
        /// 指定されたタグを持つすべてのGameObjectを検索
        /// </summary>
        public static GameObject[] FindGameObjectsWithGameplayTag(GameplayTag tag)
        {
            using var pooled = GameplayTagManager.FindGameObjectsWithTag(tag);
            var result = new GameObject[pooled.Count];
            for (int i = 0; i < pooled.Count; i++)
            {
                result[i] = pooled[i];
            }
            return result;
        }

        /// <summary>
        /// GameObject.FindGameObjectsWithTagの代替（文字列版）
        /// 指定されたタグを持つすべてのGameObjectを検索
        /// </summary>
        public static GameObject[] FindGameObjectsWithGameplayTag(string tagName)
        {
            using var pooled = GameplayTagManager.FindGameObjectsWithTag(new GameplayTag(tagName));
            var result = new GameObject[pooled.Count];
            for (int i = 0; i < pooled.Count; i++)
            {
                result[i] = pooled[i];
            }
            return result;
        }

        /// <summary>
        /// Unityタグをgameplay tagに変換してコンポーネントに追加
        /// 移行時の便利メソッド
        /// </summary>
        public static void ConvertUnityTagToGameplayTag(this GameObject gameObject, string unityTag)
        {
            var tagComponent = gameObject.GetComponent<GameplayTagComponent>();
            if (tagComponent == null)
            {
                tagComponent = gameObject.AddComponent<GameplayTagComponent>();
            }

            // Unityタグをゲームプレイタグの形式に変換
            var convertedTag = ConvertUnityTagName(unityTag);
            tagComponent.AddTag(new GameplayTag(convertedTag));
        }

        /// <summary>
        /// Unityタグ名をGameplayTag形式に変換
        /// 例: "Player" → "Character.Player"
        /// </summary>
        static string ConvertUnityTagName(string unityTag)
        {
            switch (unityTag.ToLower())
            {
                case "player":
                    return "Character.Player";
                case "enemy":
                    return "Character.Enemy";
                case "untagged":
                    return "";
                default:
                    // デフォルトでは Object カテゴリに配置
                    return $"Object.{unityTag}";
            }
        }
    }
}
