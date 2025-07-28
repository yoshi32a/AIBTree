using NUnit.Framework;
using UnityEngine;
using ArcBT.TagSystem;

namespace ArcBT.Tests
{
    public class UnityTagCompatibilityTests
    {
        GameObject testObject;
        GameplayTagComponent tagComponent;

        [SetUp]
        public void Setup()
        {
            testObject = new GameObject("TestObject");
            tagComponent = testObject.AddComponent<GameplayTagComponent>();
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null) Object.DestroyImmediate(testObject);
        }

        [Test][Description("GameObjectでのGameplayTag比較機能が正常に動作することを確認（階層マッチング含む）")]
        public void CompareGameplayTag_GameObject_WorksCorrectly()
        {
            // Arrange
            var testTag = new GameplayTag("Character.Player");
            tagComponent.AddTag(testTag);

            // Act & Assert
            Assert.IsTrue(testObject.CompareGameplayTag(testTag));
            Assert.IsTrue(testObject.CompareGameplayTag("Character.Player"));
            Assert.IsTrue(testObject.CompareGameplayTag("Character")); // 階層マッチング

            Assert.IsFalse(testObject.CompareGameplayTag("Character.Enemy"));
            Assert.IsFalse(testObject.CompareGameplayTag("Object.Item"));
        }

        [Test][Description("ComponentでのGameplayTag比較機能が正常に動作することを確認（階層マッチング含む）")]
        public void CompareGameplayTag_Component_WorksCorrectly()
        {
            // Arrange
            var testTag = new GameplayTag("Character.Enemy");
            tagComponent.AddTag(testTag);

            // Act & Assert
            Assert.IsTrue(tagComponent.CompareGameplayTag(testTag));
            Assert.IsTrue(tagComponent.CompareGameplayTag("Character.Enemy"));
            Assert.IsTrue(tagComponent.CompareGameplayTag("Character")); // 階層マッチング

            Assert.IsFalse(tagComponent.CompareGameplayTag("Character.Player"));
            Assert.IsFalse(tagComponent.CompareGameplayTag("State.Combat"));
        }

        [Test][Description("TagComponentがアタッチされていないGameObjectでのタグ比較がfalseを返すことを確認")]
        public void CompareGameplayTag_WithoutTagComponent_ReturnsFalse()
        {
            // Arrange
            var objectWithoutComponent = new GameObject("NoTagComponent");

            // Act & Assert
            Assert.IsFalse(objectWithoutComponent.CompareGameplayTag("Character.Player"));
            Assert.IsFalse(objectWithoutComponent.CompareGameplayTag(new GameplayTag("Character.Player")));

            // Cleanup
            Object.DestroyImmediate(objectWithoutComponent);
        }

        [Test][Description("null入力や空文字列入力に対して適切にエラーハンドリングされることを確認")]
        public void CompareGameplayTag_NullInputs_HandleGracefully()
        {
            // Arrange
            GameObject nullObject = null;
            Component nullComponent = null;

            // Act & Assert
            Assert.IsFalse(nullObject.CompareGameplayTag("Character.Player"));
            Assert.IsFalse(nullComponent.CompareGameplayTag("Character.Player"));
            Assert.IsFalse(tagComponent.CompareGameplayTag(""));
            Assert.IsFalse(tagComponent.CompareGameplayTag(new GameplayTag("")));
        }

        [Test][Description("GameplayTagを使用したGameObject検索が正しいオブジェクトを返すことを確認")]
        public void FindWithGameplayTag_FindsCorrectObject()
        {
            // Arrange
            var testTag = new GameplayTag("Character.Player");
            tagComponent.AddTag(testTag);

            // Act
            var found1 = UnityTagCompatibility.FindWithGameplayTag(testTag);
            var found2 = UnityTagCompatibility.FindWithGameplayTag("Character.Player");

            // Assert
            Assert.AreEqual(testObject, found1);
            Assert.AreEqual(testObject, found2);
        }

        [Test][Description("存在しないGameplayTagでの検索がnullを返すことを確認")]
        public void FindWithGameplayTag_ReturnsNullForNonExistent()
        {
            // Act
            var found1 = UnityTagCompatibility.FindWithGameplayTag("NonExistent.Tag");
            var found2 = UnityTagCompatibility.FindWithGameplayTag(new GameplayTag("NonExistent.Tag"));

            // Assert
            Assert.IsNull(found1);
            Assert.IsNull(found2);
        }

        [Test][Description("GameplayTagを使用した複数GameObject検索が正しいオブジェクト群を返すことを確認")]
        public void FindGameObjectsWithGameplayTag_FindsCorrectObjects()
        {
            // Arrange
            var testTag = new GameplayTag("Character.Enemy");
            tagComponent.AddTag(testTag);

            var secondObject = new GameObject("SecondObject");
            var secondComponent = secondObject.AddComponent<GameplayTagComponent>();
            secondComponent.AddTag(testTag);

            // Act
            var found1 = UnityTagCompatibility.FindGameObjectsWithGameplayTag(testTag);
            var found2 = UnityTagCompatibility.FindGameObjectsWithGameplayTag("Character.Enemy");

            // Assert
            Assert.AreEqual(2, found1.Length);
            Assert.AreEqual(2, found2.Length);
            Assert.IsTrue(System.Array.Exists(found1, obj => obj == testObject));
            Assert.IsTrue(System.Array.Exists(found1, obj => obj == secondObject));

            // Cleanup
            Object.DestroyImmediate(secondObject);
        }

        [Test][Description("Unity標準タグからGameplayTagへの変換機能が正しいタグを追加することを確認")]
        public void ConvertUnityTagToGameplayTag_AddsCorrectTag()
        {
            // Arrange
            var objectWithoutComponent = new GameObject("TestConvert");

            // Act
            objectWithoutComponent.ConvertUnityTagToGameplayTag("Player");

            // Assert
            var component = objectWithoutComponent.GetComponent<GameplayTagComponent>();
            Assert.IsNotNull(component);
            Assert.IsTrue(component.HasTag(new GameplayTag("Character.Player")));

            // Cleanup
            Object.DestroyImmediate(objectWithoutComponent);
        }

        [Test][Description("既存TagComponentを持つGameObjectでのUnityタグ変換が新しいタグを追加することを確認")]
        public void ConvertUnityTagToGameplayTag_WithExistingComponent_AddsTag()
        {
            // Arrange
            var existingTag = new GameplayTag("State.Combat");
            tagComponent.AddTag(existingTag);

            // Act
            testObject.ConvertUnityTagToGameplayTag("Enemy");

            // Assert
            Assert.IsTrue(tagComponent.HasTag(existingTag)); // 既存タグは保持
            Assert.IsTrue(tagComponent.HasTag(new GameplayTag("Character.Enemy"))); // 新しいタグが追加
        }

        [Test][Description("UnityタグからGameplayTagへのマッピングルールが正しく適用されることを確認（Player→Character.Player等）")]
        public void TagConversion_FollowsCorrectMapping()
        {
            // Arrange & Act & Assert
            var testCases = new[]
            {
                ("Player", "Character.Player"),
                ("Enemy", "Character.Enemy"),
                ("Untagged", ""),
                ("CustomTag", "Object.CustomTag")
            };

            foreach (var (unityTag, expectedGameplayTag) in testCases)
            {
                var testObj = new GameObject($"Test_{unityTag}");
                testObj.ConvertUnityTagToGameplayTag(unityTag);

                var component = testObj.GetComponent<GameplayTagComponent>();
                
                if (string.IsNullOrEmpty(expectedGameplayTag))
                {
                    Assert.AreEqual(0, component.OwnedTags.Count);
                }
                else
                {
                    Assert.IsTrue(component.HasTag(new GameplayTag(expectedGameplayTag)),
                        $"Expected tag '{expectedGameplayTag}' for Unity tag '{unityTag}'");
                }

                Object.DestroyImmediate(testObj);
            }
        }

        [Test][Description("階層的タグマッチング機能が正しく動作することを確認（Character.Enemy.Boss→Characterマッチ等）")]
        public void HierarchicalMatching_WorksCorrectly()
        {
            // Arrange
            tagComponent.AddTag(new GameplayTag("Character.Enemy.Boss"));

            // Act & Assert - 階層的マッチング
            Assert.IsTrue(testObject.CompareGameplayTag("Character"));
            Assert.IsTrue(testObject.CompareGameplayTag("Character.Enemy"));
            Assert.IsTrue(testObject.CompareGameplayTag("Character.Enemy.Boss"));

            // Act & Assert - 非マッチング
            Assert.IsFalse(testObject.CompareGameplayTag("Character.Player"));
            Assert.IsFalse(testObject.CompareGameplayTag("Character.Enemy.Minion"));
            Assert.IsFalse(testObject.CompareGameplayTag("Object"));
        }

        [Test][Description("一つのGameObjectに複数タグを設定した場合の検索機能が正しく動作することを確認")]
        public void MultipleTagsOnObject_AllWorkCorrectly()
        {
            // Arrange
            tagComponent.AddTag(new GameplayTag("Character.Player"));
            tagComponent.AddTag(new GameplayTag("State.Combat"));
            tagComponent.AddTag(new GameplayTag("Ability.Dash"));

            // Act & Assert
            Assert.IsTrue(testObject.CompareGameplayTag("Character.Player"));
            Assert.IsTrue(testObject.CompareGameplayTag("State.Combat"));
            Assert.IsTrue(testObject.CompareGameplayTag("Ability.Dash"));
            Assert.IsTrue(testObject.CompareGameplayTag("Character")); // 階層マッチング
            Assert.IsTrue(testObject.CompareGameplayTag("State")); // 階層マッチング
            Assert.IsTrue(testObject.CompareGameplayTag("Ability")); // 階層マッチング

            Assert.IsFalse(testObject.CompareGameplayTag("Character.Enemy"));
            Assert.IsFalse(testObject.CompareGameplayTag("Object.Item"));
        }

        [Test][Description("異なるタイプのComponent（BoxCollider、Rigidbody等）でのGameplayTag比較が正しく動作することを確認")]
        public void ComponentComparison_WorksWithDifferentComponentTypes()
        {
            // Arrange
            var collider = testObject.AddComponent<BoxCollider>();
            var rigidbody = testObject.AddComponent<Rigidbody>();
            tagComponent.AddTag(new GameplayTag("Character.Player"));

            // Act & Assert
            Assert.IsTrue(collider.CompareGameplayTag("Character.Player"));
            Assert.IsTrue(rigidbody.CompareGameplayTag("Character.Player"));
            Assert.IsTrue(tagComponent.CompareGameplayTag("Character.Player"));

            Assert.IsFalse(collider.CompareGameplayTag("Character.Enemy"));
            Assert.IsFalse(rigidbody.CompareGameplayTag("Character.Enemy"));
        }
    }
}
