using NUnit.Framework;
using UnityEngine;
using ArcBT.TagSystem;
using System.Linq;

namespace ArcBT.Tests
{
    public class GameplayTagManagerTests
    {
        GameplayTagManager manager;
        GameObject testObject1;
        GameObject testObject2;
        GameplayTagComponent component1;
        GameplayTagComponent component2;

        [SetUp]
        public void Setup()
        {
            // テストオブジェクト作成
            testObject1 = new GameObject("TestObject1");
            testObject2 = new GameObject("TestObject2");
            
            component1 = testObject1.AddComponent<GameplayTagComponent>();
            component2 = testObject2.AddComponent<GameplayTagComponent>();

            // GameplayTagManagerインスタンス取得
            manager = GameplayTagManager.Instance;
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject1 != null) Object.DestroyImmediate(testObject1);
            if (testObject2 != null) Object.DestroyImmediate(testObject2);
        }

        [Test]
        public void GameplayTagManager_Singleton_WorksCorrectly()
        {
            // Arrange & Act
            var instance1 = GameplayTagManager.Instance;
            var instance2 = GameplayTagManager.Instance;

            // Assert
            Assert.IsNotNull(instance1);
            Assert.AreSame(instance1, instance2);
        }

        [Test]
        public void FindGameObjectsWithTag_FindsCorrectObjects()
        {
            // Arrange
            var playerTag = new GameplayTag("Character.Player");
            var enemyTag = new GameplayTag("Character.Enemy");
            
            component1.AddTag(playerTag);
            component2.AddTag(enemyTag);

            // Act
            var foundPlayers = GameplayTagManager.FindGameObjectsWithTag(playerTag);
            var foundEnemies = GameplayTagManager.FindGameObjectsWithTag(enemyTag);

            // Assert
            Assert.AreEqual(1, foundPlayers.Length);
            Assert.AreEqual(testObject1, foundPlayers[0]);
            Assert.AreEqual(1, foundEnemies.Length);
            Assert.AreEqual(testObject2, foundEnemies[0]);
        }

        [Test]
        public void FindGameObjectsWithTag_EmptyForInvalidTag()
        {
            // Arrange
            var invalidTag = new GameplayTag("NonExistent.Tag");

            // Act
            var found = GameplayTagManager.FindGameObjectsWithTag(invalidTag);

            // Assert
            Assert.AreEqual(0, found.Length);
        }

        [Test]
        public void FindGameObjectWithTag_ReturnsFirstObject()
        {
            // Arrange
            var playerTag = new GameplayTag("Character.Player");
            component1.AddTag(playerTag);
            component2.AddTag(playerTag); // 両方に同じタグ

            // Act
            var found = GameplayTagManager.FindGameObjectWithTag(playerTag);

            // Assert
            Assert.IsNotNull(found);
            Assert.IsTrue(found == testObject1 || found == testObject2);
        }

        [Test]
        public void FindGameObjectsWithTagHierarchy_FindsParentAndChildren()
        {
            // Arrange
            var parentTag = new GameplayTag("Character.Enemy");
            var childTag = new GameplayTag("Character.Enemy.Boss");
            
            component1.AddTag(parentTag);
            component2.AddTag(childTag);

            // Act
            var found = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("Character.Enemy"));

            // Assert
            Assert.AreEqual(2, found.Length);
            Assert.IsTrue(found.Contains(testObject1));
            Assert.IsTrue(found.Contains(testObject2));
        }

        [Test]
        public void FindGameObjectsWithAnyTags_WorksCorrectly()
        {
            // Arrange
            component1.AddTag(new GameplayTag("Character.Player"));
            component2.AddTag(new GameplayTag("Character.Enemy"));

            var searchTags = new GameplayTagContainer(
                new GameplayTag("Character.Player"),
                new GameplayTag("Object.Item")
            );

            // Act
            using var found = GameplayTagManager.FindGameObjectsWithAnyTags(searchTags);

            // Assert
            Assert.AreEqual(1, found.Count);
            Assert.AreEqual(testObject1, found[0]);
        }

        [Test]
        public void FindGameObjectsWithAllTags_WorksCorrectly()
        {
            // Arrange
            component1.AddTag(new GameplayTag("Character.Player"));
            component1.AddTag(new GameplayTag("State.Combat"));
            
            component2.AddTag(new GameplayTag("Character.Player")); // Playerタグのみ

            var searchTags = new GameplayTagContainer(
                new GameplayTag("Character.Player"),
                new GameplayTag("State.Combat")
            );

            // Act
            using var found = GameplayTagManager.FindGameObjectsWithAllTags(searchTags);

            // Assert
            Assert.AreEqual(1, found.Count);
            Assert.AreEqual(testObject1, found[0]);
        }

        [Test]
        public void GetTagComponent_ReturnsCorrectComponent()
        {
            // Act
            var foundComponent1 = GameplayTagManager.GetTagComponent(testObject1);
            var foundComponent2 = GameplayTagManager.GetTagComponent(testObject2);
            var foundComponentNull = GameplayTagManager.GetTagComponent(null);

            // Assert
            Assert.AreSame(component1, foundComponent1);
            Assert.AreSame(component2, foundComponent2);
            Assert.IsNull(foundComponentNull);
        }

        [Test]
        public void HasTag_WorksCorrectly()
        {
            // Arrange
            var testTag = new GameplayTag("Character.Player");
            component1.AddTag(testTag);

            // Act & Assert
            Assert.IsTrue(GameplayTagManager.HasTag(testObject1, testTag));
            Assert.IsFalse(GameplayTagManager.HasTag(testObject2, testTag));
            Assert.IsFalse(GameplayTagManager.HasTag(null, testTag));
        }

        [Test]
        public void ComponentRegistration_WorksAutomatically()
        {
            // Arrange
            var newObject = new GameObject("NewTestObject");
            var testTag = new GameplayTag("Test.Tag");

            // Act
            var newComponent = newObject.AddComponent<GameplayTagComponent>();
            newComponent.AddTag(testTag);

            // Act - タグで検索
            var found = GameplayTagManager.FindGameObjectsWithTag(testTag);

            // Assert
            Assert.AreEqual(1, found.Length);
            Assert.AreEqual(newObject, found[0]);

            // Cleanup
            Object.DestroyImmediate(newObject);
        }

        [Test]
        public void TagCacheUpdate_WorksWhenTagsChange()
        {
            // Arrange
            var tag1 = new GameplayTag("Tag.One");
            var tag2 = new GameplayTag("Tag.Two");
            
            component1.AddTag(tag1);

            // Act - 最初の検索
            var found1 = GameplayTagManager.FindGameObjectsWithTag(tag1);
            var found2 = GameplayTagManager.FindGameObjectsWithTag(tag2);

            // Assert - 最初の状態
            Assert.AreEqual(1, found1.Length);
            Assert.AreEqual(0, found2.Length);

            // Act - タグ変更
            component1.RemoveTag(tag1);
            component1.AddTag(tag2);

            var foundAfter1 = GameplayTagManager.FindGameObjectsWithTag(tag1);
            var foundAfter2 = GameplayTagManager.FindGameObjectsWithTag(tag2);

            // Assert - 変更後の状態
            Assert.AreEqual(0, foundAfter1.Length);
            Assert.AreEqual(1, foundAfter2.Length);
            Assert.AreEqual(testObject1, foundAfter2[0]);
        }

        [Test]
        public void MultipleTagsOnSameObject_WorkCorrectly()
        {
            // Arrange
            var tag1 = new GameplayTag("Character.Player");
            var tag2 = new GameplayTag("State.Combat");
            var tag3 = new GameplayTag("Ability.Dash");

            // Act
            component1.AddTag(tag1);
            component1.AddTag(tag2);
            component1.AddTag(tag3);

            // Assert
            var found1 = GameplayTagManager.FindGameObjectsWithTag(tag1);
            var found2 = GameplayTagManager.FindGameObjectsWithTag(tag2);
            var found3 = GameplayTagManager.FindGameObjectsWithTag(tag3);

            Assert.AreEqual(1, found1.Length);
            Assert.AreEqual(1, found2.Length);
            Assert.AreEqual(1, found3.Length);
            Assert.AreEqual(testObject1, found1[0]);
            Assert.AreEqual(testObject1, found2[0]);
            Assert.AreEqual(testObject1, found3[0]);
        }

        [Test]
        public void NullAndInvalidInputs_HandleGracefully()
        {
            // Arrange
            var invalidTag = new GameplayTag("");
            var validTag = new GameplayTag("Valid.Tag");

            // Act & Assert - null GameObject
            Assert.DoesNotThrow(() => GameplayTagManager.FindGameObjectsWithTag(validTag));
            Assert.DoesNotThrow(() => GameplayTagManager.HasTag(null, validTag));

            // Act & Assert - invalid tag
            using var foundInvalid = GameplayTagManager.FindGameObjectsWithTag(invalidTag);
            Assert.AreEqual(0, foundInvalid.Count);

            // Act & Assert - null tag container
            using var found = GameplayTagManager.FindGameObjectsWithAnyTags(new GameplayTagContainer());
            Assert.DoesNotThrow(() => found.Count.ToString());
        }
    }
}