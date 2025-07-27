using UnityEngine;

namespace ArcBT.TagSystem
{
    /// <summary>
    /// GameObjectにGameplayTagを付与するコンポーネント
    /// </summary>
    public class GameplayTagComponent : MonoBehaviour
    {
        [SerializeField] GameplayTagContainer ownedTags = new();

        [SerializeField] GameplayTagContainer blockingTags = new();

        // タグ変更時のイベント（高速C#イベント）
        public event System.Action<GameplayTagContainer> OnTagsChanged;

        /// <summary>
        /// 所有しているタグ
        /// </summary>
        public GameplayTagContainer OwnedTags => ownedTags;

        /// <summary>
        /// ブロックしているタグ（このオブジェクトが持つことができないタグ）
        /// </summary>
        public GameplayTagContainer BlockingTags => blockingTags;

        /// <summary>
        /// タグを追加
        /// </summary>
        public bool AddTag(GameplayTag tag)
        {
            if (!tag.IsValid || blockingTags.HasTag(tag))
                return false;

            if (!ownedTags.HasTagExact(tag))
            {
                ownedTags.AddTag(tag);
                OnTagsChanged?.Invoke(ownedTags);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 複数のタグを追加
        /// </summary>
        public void AddTags(GameplayTagContainer container)
        {
            bool changed = false;
            foreach (var tag in container.Tags)
            {
                if (AddTag(tag))
                    changed = true;
            }

            if (changed)
            {
                OnTagsChanged?.Invoke(ownedTags);
            }
        }

        /// <summary>
        /// タグを削除
        /// </summary>
        public bool RemoveTag(GameplayTag tag)
        {
            if (ownedTags.RemoveTag(tag))
            {
                OnTagsChanged?.Invoke(ownedTags);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 複数のタグを削除
        /// </summary>
        public void RemoveTags(GameplayTagContainer container)
        {
            ownedTags.RemoveTags(container);
            OnTagsChanged?.Invoke(ownedTags);
        }

        /// <summary>
        /// すべてのタグをクリア
        /// </summary>
        public void ClearAllTags()
        {
            ownedTags.Clear();
            OnTagsChanged?.Invoke(ownedTags);
        }

        /// <summary>
        /// 指定されたタグを持っているかチェック
        /// </summary>
        public bool HasTag(GameplayTag tag)
        {
            return ownedTags.HasTag(tag);
        }

        /// <summary>
        /// 指定されたタグと完全一致するものを持っているかチェック
        /// </summary>
        public bool HasTagExact(GameplayTag tag)
        {
            return ownedTags.HasTagExact(tag);
        }

        /// <summary>
        /// 指定されたコンテナのいずれかのタグを持っているかチェック
        /// </summary>
        public bool HasAnyTags(GameplayTagContainer container)
        {
            return ownedTags.HasAnyTags(container);
        }

        /// <summary>
        /// 指定されたコンテナのすべてのタグを持っているかチェック
        /// </summary>
        public bool HasAllTags(GameplayTagContainer container)
        {
            return ownedTags.HasAllTags(container);
        }

        /// <summary>
        /// ブロックタグを追加
        /// </summary>
        public void AddBlockingTag(GameplayTag tag)
        {
            if (tag.IsValid)
            {
                blockingTags.AddTag(tag);
                // ブロックされたタグを現在のタグから削除
                RemoveTag(tag);
            }
        }

        /// <summary>
        /// ブロックタグを削除
        /// </summary>
        public void RemoveBlockingTag(GameplayTag tag)
        {
            blockingTags.RemoveTag(tag);
        }

        /// <summary>
        /// タグをブロックしているかチェック
        /// </summary>
        public bool IsTagBlocked(GameplayTag tag)
        {
            return blockingTags.HasTag(tag);
        }

        void OnEnable()
        {
            // アクティブになった時に登録（非アクティブGameObjectでも対応）
            GameplayTagManager.Instance.RegisterTagComponent(this);
        }

        void OnDisable()
        {
            // 非アクティブになった時に登録解除
            if (GameplayTagManager.Instance != null)
            {
                GameplayTagManager.Instance.UnregisterTagComponent(this);
            }
        }

        void OnValidate()
        {
            // エディタで変更された場合もイベントを発火
            OnTagsChanged?.Invoke(ownedTags);
        }
    }
}
