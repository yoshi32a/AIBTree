using NUnit.Framework;
using UnityEngine;
using ArcBT.TagSystem;
using System.Linq;

namespace ArcBT.Tests
{
    public class HierarchicalSearchTests
    {
        GameObject player;
        GameObject enemy;
        GameObject boss;
        GameObject minion;
        GameObject item;
        GameObject weapon;

        GameplayTagComponent playerComponent;
        GameplayTagComponent enemyComponent;
        GameplayTagComponent bossComponent;
        GameplayTagComponent minionComponent;
        GameplayTagComponent itemComponent;
        GameplayTagComponent weaponComponent;

        [SetUp]
        public void Setup()
        {
            // 階層構造のテストオブジェクト作成
            player = new GameObject("Player");
            enemy = new GameObject("Enemy");
            boss = new GameObject("Boss");
            minion = new GameObject("Minion");
            item = new GameObject("Item");
            weapon = new GameObject("Weapon");

            playerComponent = player.AddComponent<GameplayTagComponent>();
            enemyComponent = enemy.AddComponent<GameplayTagComponent>();
            bossComponent = boss.AddComponent<GameplayTagComponent>();
            minionComponent = minion.AddComponent<GameplayTagComponent>();
            itemComponent = item.AddComponent<GameplayTagComponent>();
            weaponComponent = weapon.AddComponent<GameplayTagComponent>();

            // 階層的なタグ構造を設定
            playerComponent.AddTag(new GameplayTag("Character.Player"));
            enemyComponent.AddTag(new GameplayTag("Character.Enemy"));
            bossComponent.AddTag(new GameplayTag("Character.Enemy.Boss"));
            minionComponent.AddTag(new GameplayTag("Character.Enemy.Minion"));
            itemComponent.AddTag(new GameplayTag("Object.Item"));
            weaponComponent.AddTag(new GameplayTag("Object.Item.Weapon"));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(boss);
            Object.DestroyImmediate(minion);
            Object.DestroyImmediate(item);
            Object.DestroyImmediate(weapon);
        }

        [Test]
        public void HierarchicalSearch_Character_FindsAllCharacters()
        {
            // Act
            var found = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("Character"));

            // Assert
            Assert.AreEqual(4, found.Length);
            Assert.IsTrue(found.Contains(player));
            Assert.IsTrue(found.Contains(enemy));
            Assert.IsTrue(found.Contains(boss));
            Assert.IsTrue(found.Contains(minion));
        }

        [Test]
        public void HierarchicalSearch_CharacterEnemy_FindsAllEnemies()
        {
            // Act
            var found = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("Character.Enemy"));

            // Assert
            Assert.AreEqual(3, found.Length);
            Assert.IsTrue(found.Contains(enemy));
            Assert.IsTrue(found.Contains(boss));
            Assert.IsTrue(found.Contains(minion));
            Assert.IsFalse(found.Contains(player)); // プレイヤーは含まれない
        }

        [Test]
        public void HierarchicalSearch_CharacterPlayer_FindsOnlyPlayer()
        {
            // Act
            var found = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("Character.Player"));

            // Assert
            Assert.AreEqual(1, found.Length);
            Assert.IsTrue(found.Contains(player));
        }

        [Test]
        public void HierarchicalSearch_Object_FindsAllObjects()
        {
            // Act
            var found = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("Object"));

            // Assert
            Assert.AreEqual(2, found.Length);
            Assert.IsTrue(found.Contains(item));
            Assert.IsTrue(found.Contains(weapon));
        }

        [Test]
        public void HierarchicalSearch_ObjectItem_FindsAllItems()
        {
            // Act
            var found = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("Object.Item"));

            // Assert
            Assert.AreEqual(2, found.Length);
            Assert.IsTrue(found.Contains(item));
            Assert.IsTrue(found.Contains(weapon));
        }

        [Test]
        public void HierarchicalSearch_LeafNode_FindsOnlyItself()
        {
            // Act
            var foundBoss = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("Character.Enemy.Boss"));
            var foundWeapon = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("Object.Item.Weapon"));

            // Assert
            Assert.AreEqual(1, foundBoss.Length);
            Assert.IsTrue(foundBoss.Contains(boss));

            Assert.AreEqual(1, foundWeapon.Length);
            Assert.IsTrue(foundWeapon.Contains(weapon));
        }

        [Test]
        public void HierarchicalSearch_NonExistentTag_ReturnsEmpty()
        {
            // Act
            var found = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("NonExistent.Tag"));

            // Assert
            Assert.AreEqual(0, found.Length);
        }

        [Test]
        public void HierarchicalSearch_DeepHierarchy_WorksCorrectly()
        {
            // Arrange - 深い階層のタグを追加
            var deepObject = new GameObject("DeepObject");
            var deepComponent = deepObject.AddComponent<GameplayTagComponent>();
            deepComponent.AddTag(new GameplayTag("Level1.Level2.Level3.Level4.DeepTag"));

            // Act
            var foundLevel1 = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("Level1"));
            var foundLevel2 = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("Level1.Level2"));
            var foundLevel3 = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("Level1.Level2.Level3"));
            var foundLevel4 = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("Level1.Level2.Level3.Level4"));

            // Assert
            Assert.AreEqual(1, foundLevel1.Length);
            Assert.AreEqual(1, foundLevel2.Length);
            Assert.AreEqual(1, foundLevel3.Length);
            Assert.AreEqual(1, foundLevel4.Length);
            Assert.IsTrue(foundLevel1.Contains(deepObject));

            // Cleanup
            Object.DestroyImmediate(deepObject);
        }

        [Test]
        public void HierarchicalSearch_MultipleTagsOnSameObject_WorksCorrectly()
        {
            // Arrange - 複数の階層タグを追加
            var multiTagObject = new GameObject("MultiTagObject");
            var multiTagComponent = multiTagObject.AddComponent<GameplayTagComponent>();
            multiTagComponent.AddTag(new GameplayTag("Character.Enemy.Elite"));
            multiTagComponent.AddTag(new GameplayTag("State.Combat"));
            multiTagComponent.AddTag(new GameplayTag("Ability.Magic"));

            // Act
            var foundCharacter = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("Character"));
            var foundEnemy = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("Character.Enemy"));
            var foundState = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("State"));
            var foundAbility = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("Ability"));

            // Assert
            Assert.IsTrue(foundCharacter.Contains(multiTagObject));
            Assert.IsTrue(foundEnemy.Contains(multiTagObject));
            Assert.IsTrue(foundState.Contains(multiTagObject));
            Assert.IsTrue(foundAbility.Contains(multiTagObject));

            // Cleanup
            Object.DestroyImmediate(multiTagObject);
        }

        [Test]
        public void HierarchicalSearch_CachePerformance_WorksCorrectly()
        {
            // Act - 同じ検索を複数回実行（キャッシュテスト）
            var found1 = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("Character.Enemy"));
            var found2 = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("Character.Enemy"));
            var found3 = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("Character.Enemy"));

            // Assert - 結果は同じであるべき
            Assert.AreEqual(found1.Length, found2.Length);
            Assert.AreEqual(found2.Length, found3.Length);
            
            for (int i = 0; i < found1.Length; i++)
            {
                Assert.AreEqual(found1[i], found2[i]);
                Assert.AreEqual(found2[i], found3[i]);
            }
        }

        [Test]
        public void HierarchicalSearch_AfterTagChange_UpdatesCorrectly()
        {
            // Arrange - 最初の検索
            var initialFound = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("Character.Enemy"));
            Assert.AreEqual(3, initialFound.Length);

            // Act - タグ変更
            bossComponent.RemoveTag(new GameplayTag("Character.Enemy.Boss"));
            bossComponent.AddTag(new GameplayTag("Character.Player.Elite"));

            var afterChangeFound = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("Character.Enemy"));
            var playerFound = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("Character.Player"));

            // Assert
            Assert.AreEqual(2, afterChangeFound.Length); // Bossが除外される
            Assert.IsFalse(afterChangeFound.Contains(boss));
            Assert.IsTrue(playerFound.Contains(boss)); // Bossがプレイヤー側に移動
        }

        [Test]
        public void HierarchicalSearch_MixedWithRegularSearch_ConsistentResults()
        {
            // Act
            var hierarchicalEnemies = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("Character.Enemy"));
            
            // 通常検索で同等の結果を取得
            var exactEnemies = GameplayTagManager.FindGameObjectsWithTag(new GameplayTag("Character.Enemy"));
            var bosses = GameplayTagManager.FindGameObjectsWithTag(new GameplayTag("Character.Enemy.Boss"));
            var minions = GameplayTagManager.FindGameObjectsWithTag(new GameplayTag("Character.Enemy.Minion"));

            // Assert - 階層検索の結果は通常検索の合計と一致するべき
            var regularSearchTotal = exactEnemies.Length + bosses.Length + minions.Length;
            Assert.AreEqual(regularSearchTotal, hierarchicalEnemies.Length);
        }

        [Test]
        public void HierarchicalSearch_EmptyTag_HandlesGracefully()
        {
            // Act & Assert
            var emptyTagResult = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag(""));
            Assert.AreEqual(0, emptyTagResult.Length);

            var nullTagResult = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag());
            Assert.AreEqual(0, nullTagResult.Length);
        }

        [Test]
        public void HierarchicalSearch_CaseSensitivity_WorksCorrectly()
        {
            // Act
            var lowerCase = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("character.enemy"));
            var upperCase = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("CHARACTER.ENEMY"));
            var mixedCase = GameplayTagManager.FindGameObjectsWithTagHierarchy(new GameplayTag("Character.Enemy"));

            // Assert - タグは大文字小文字を区別するべき
            Assert.AreEqual(0, lowerCase.Length);
            Assert.AreEqual(0, upperCase.Length);
            Assert.AreEqual(3, mixedCase.Length); // 正しいケースのみマッチ
        }
    }
}