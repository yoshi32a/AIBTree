using ArcBT.TagSystem;
using NUnit.Framework;
using UnityEngine;

namespace ArcBT.Tests
{
    public class GameplayTagSystemTests
    {
        [Test][Description("GameplayTagコンストラクタで有効なタグが正しく作成されることを確認")]
        public void GameplayTag_Constructor_CreatesValidTag()
        {
            var tag = new GameplayTag("Character.Player");
            
            Assert.IsTrue(tag.IsValid);
            Assert.AreEqual("Character.Player", tag.TagName);
        }

        [Test][Description("GameplayTagのMatchesTagメソッドで階層マッチングが正しく動作することを確認")]
        public void GameplayTag_MatchesTag_WorksCorrectly()
        {
            var parentTag = new GameplayTag("Character");
            var childTag = new GameplayTag("Character.Player");
            var unrelatedTag = new GameplayTag("Object.Item");

            Assert.IsTrue(childTag.MatchesTag(parentTag));
            Assert.IsTrue(parentTag.MatchesTag(parentTag));
            Assert.IsFalse(parentTag.MatchesTag(childTag));
            Assert.IsFalse(childTag.MatchesTag(unrelatedTag));
        }

        [Test][Description("GameplayTagのGetParentTagメソッドで正しい親タグが取得できることを確認")]
        public void GameplayTag_GetParentTag_ReturnsCorrectParent()
        {
            var tag = new GameplayTag("Character.Enemy.Boss");
            var parent = tag.GetParentTag();
            
            Assert.AreEqual("Character.Enemy", parent.TagName);
        }

        [Test][Description("GameplayTagのGetDepthメソッドでタグの階層の深さが正しく取得できることを確認")]
        public void GameplayTag_GetDepth_ReturnsCorrectDepth()
        {
            var tag1 = new GameplayTag("Character");
            var tag2 = new GameplayTag("Character.Player");
            var tag3 = new GameplayTag("Character.Enemy.Boss");

            Assert.AreEqual(1, tag1.GetDepth());
            Assert.AreEqual(2, tag2.GetDepth());
            Assert.AreEqual(3, tag3.GetDepth());
        }

        [Test][Description("GameplayTagContainerのAddTagメソッドでタグが正しく追加されることを確認")]
        public void GameplayTagContainer_AddTag_WorksCorrectly()
        {
            var container = new GameplayTagContainer();
            var tag = new GameplayTag("Character.Player");

            container.AddTag(tag);

            Assert.AreEqual(1, container.Count);
            Assert.IsTrue(container.HasTag(tag));
        }

        [Test][Description("GameplayTagContainerのHasTagメソッドで階層チェックが正しく動作することを確認")]
        public void GameplayTagContainer_HasTag_ChecksHierarchy()
        {
            var container = new GameplayTagContainer();
            container.AddTag(new GameplayTag("Character.Player"));

            Assert.IsTrue(container.HasTag(new GameplayTag("Character")));
            Assert.IsTrue(container.HasTag(new GameplayTag("Character.Player")));
            Assert.IsFalse(container.HasTag(new GameplayTag("Character.Enemy")));
        }

        [Test][Description("GameplayTagContainerのHasAllTagsメソッドで必要なタグをすべて持っているか正しく判定できることを確認")]
        public void GameplayTagContainer_HasAllTags_WorksCorrectly()
        {
            var container = new GameplayTagContainer();
            container.AddTag(new GameplayTag("Character.Player"));
            container.AddTag(new GameplayTag("State.Combat"));

            var requiredTags = new GameplayTagContainer(
                new GameplayTag("Character"),
                new GameplayTag("State.Combat")
            );

            Assert.IsTrue(container.HasAllTags(requiredTags));
        }

        [Test][Description("GameplayTagContainerのGetExpandedContainerメソッドで親タグを含む展開されたコンテナが取得できることを確認")]
        public void GameplayTagContainer_GetExpandedContainer_IncludesParents()
        {
            var container = new GameplayTagContainer();
            container.AddTag(new GameplayTag("Character.Enemy.Boss"));

            var expanded = container.GetExpandedContainer();

            Assert.IsTrue(expanded.HasTag(new GameplayTag("Character")));
            Assert.IsTrue(expanded.HasTag(new GameplayTag("Character.Enemy")));
            Assert.IsTrue(expanded.HasTag(new GameplayTag("Character.Enemy.Boss")));
        }

        [Test][Description("GameplayTagComponentのAddTag実行時にタグ変更イベントが正しく発火されることを確認")]
        public void GameplayTagComponent_AddTag_FiresEvent()
        {
            var go = new GameObject("TestObject");
            var component = go.AddComponent<GameplayTagComponent>();
            
            bool eventFired = false;
            component.OnTagsChanged += (tags) => eventFired = true;

            component.AddTag(new GameplayTag("Character.Player"));

            Assert.IsTrue(eventFired);
            Assert.IsTrue(component.HasTag(new GameplayTag("Character.Player")));

            Object.DestroyImmediate(go);
        }

        [Test][Description("GameplayTagComponentのブロッキングタグ機能で特定タグの追加が防げることを確認")]
        public void GameplayTagComponent_BlockingTags_PreventAddition()
        {
            var go = new GameObject("TestObject");
            var component = go.AddComponent<GameplayTagComponent>();

            var blockingTag = new GameplayTag("State.Dead");
            component.AddBlockingTag(blockingTag);

            bool added = component.AddTag(blockingTag);

            Assert.IsFalse(added);
            Assert.IsFalse(component.HasTag(blockingTag));

            Object.DestroyImmediate(go);
        }

        [Test][Description("GameplayTagManagerのFindGameObjectsWithTagメソッドで指定タグを持つオブジェクトが正しく検索できることを確認")]
        public void GameplayTagManager_FindGameObjectsWithTag_FindsCorrectObjects()
        {
            var go1 = new GameObject("Player");
            var go2 = new GameObject("Enemy");
            var component1 = go1.AddComponent<GameplayTagComponent>();
            var component2 = go2.AddComponent<GameplayTagComponent>();

            component1.AddTag(new GameplayTag("Character.Player"));
            component2.AddTag(new GameplayTag("Character.Enemy"));

            var foundCharacters = GameplayTagManager.FindGameObjectsWithTag(new GameplayTag("Character"));
            var foundPlayers = GameplayTagManager.FindGameObjectsWithTag(new GameplayTag("Character.Player"));

            Assert.AreEqual(2, foundCharacters.Length);
            Assert.AreEqual(1, foundPlayers.Length);
            Assert.AreEqual(go1, foundPlayers[0]);

            Object.DestroyImmediate(go1);
            Object.DestroyImmediate(go2);
        }
    }
}
