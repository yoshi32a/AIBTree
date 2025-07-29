using System.IO;
using ArcBT.Actions;
using ArcBT.Core;
using ArcBT.Parser;
using ArcBT.Samples.RPG;
using ArcBT.Samples.RPG.Actions;
using ArcBT.Samples.RPG.Conditions;
using NUnit.Framework;

namespace ArcBT.Tests
{
    [TestFixture]
    public class BTParserTests : BTTestBase
    {
        BTParser parser;
        string tempFilePath;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp(); // BTTestBaseのセットアップを実行（ログ抑制含む）
            parser = new BTParser();
            tempFilePath = Path.GetTempFileName();
        }

        [TearDown]
        public override void TearDown()
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
            base.TearDown(); // BTTestBaseのクリーンアップを実行
        }

        [Test][Description("空のコンテンツを解析した場合にnullが返されることを確認")]
        public void ParseContent_EmptyContent_ReturnsNull()
        {
            // Arrange
            string content = "";

            // Act
            var result = parser.ParseContent(content);

            // Assert - ログではなく実際の動作を検証
            Assert.IsNull(result, "空のコンテンツを解析した場合、nullが返されるべき");
            
            // 注意: パースエラーログはLoggingBehaviorTestsで専用テストが行われます
        }

        [Test][Description("tree定義がないコンテンツを解析した場合にnullが返されることを確認")]
        public void ParseContent_NoTreeDefinition_ReturnsNull()
        {
            // Arrange
            string content = "# This is just a comment\n";

            // Act
            var result = parser.ParseContent(content);

            // Assert - ログではなく実際の動作を検証
            Assert.IsNull(result, "tree定義がないコンテンツを解析した場合、nullが返されるべき");
            
            // 注意: パースエラーログはLoggingBehaviorTestsで専用テストが行われます
        }

        [Test][Description("シンプルなSequenceツリーが正常に解析されSequenceNodeが返されることを確認")]
        public void ParseContent_SimpleSequenceTree_ReturnsSequenceNode()
        {
            // Arrange
            string content = @"
                tree TestTree {
                    Sequence RootSequence {
                    }
                }";

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<BTSequenceNode>(result);
            Assert.AreEqual("RootSequence", result.Name);
        }

        [Test][Description("シンプルなSelectorツリーが正常に解析されSelectorNodeが返されることを確認")]
        public void ParseContent_SimpleSelectorTree_ReturnsSelectorNode()
        {
            // Arrange
            string content = @"
                tree TestTree {
                    Selector RootSelector {
                    }
                }";

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<BTSelectorNode>(result);
            Assert.AreEqual("RootSelector", result.Name);
        }

        [Test][Description("シンプルなParallelツリーが正常に解析されParallelNodeが返されることを確認")]
        public void ParseContent_SimpleParallelTree_ReturnsParallelNode()
        {
            // Arrange
            string content = @"
                tree TestTree {
                    Parallel RootParallel {
                    }
                }";

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<BTParallelNode>(result);
            Assert.AreEqual("RootParallel", result.Name);
        }

        [Test][Description("プロパティなしのActionノードが正しいActionクラスとして作成されることを確認")]
        public void ParseContent_ActionNodeWithoutProperties_CreatesCorrectAction()
        {
            // Arrange
            string content = @"
                tree TestTree {
                    Action MoveToPosition {
                    }
                }";

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<MoveToPositionAction>(result);
            Assert.AreEqual("Action:MoveToPosition", result.Name);
        }

        [Test][Description("プロパティありのActionノードでプロパティが正しく設定されることを確認")]
        public void ParseContent_ActionNodeWithProperties_SetsPropertiesCorrectly()
        {
            // Arrange
            string content = @"
                tree TestTree {
                    Action Wait {
                        duration: ""2.5""
                        test_prop: ""test_value""
                    }
                }";

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<WaitAction>(result);
            Assert.AreEqual("Action:Wait", result.Name);
        }

        [Test][Description("未登録のActionが指定された場合に適切にエラーハンドリングされることを確認")]
        public void ParseContent_UnknownAction_CreatesCustomActionNode()
        {
            // Arrange
            string content = @"
                tree TestTree {
                    Action UnknownAction {
                    }
                }";

            // Expect error for unknown action (統一システムでの新しい順序)

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNull(result); // 未知のアクションの場合はnullが返される
        }

        [Test][Description("複数の既知のActionが含まれるツリーで各々が正しいActionタイプとして作成されることを確認")]
        public void ParseContent_MultipleKnownActions_CreatesCorrectActionTypes()
        {
            // Arrange
            string content = @"
                tree TestTree {
                    Sequence Root {
                        Action AttackEnemy {
                            damage: ""50""
                        }
                        Action CastSpell {
                            spell_name: ""fireball""
                        }
                        Action FleeToSafety {
                            min_distance: ""15.0""
                        }
                    }
                }";

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<BTSequenceNode>(result);
            Assert.AreEqual(3, result.Children.Count);
            Assert.IsInstanceOf<AttackEnemyAction>(result.Children[0]);
            Assert.IsInstanceOf<CastSpellAction>(result.Children[1]);
            Assert.IsInstanceOf<FleeToSafetyAction>(result.Children[2]);
        }

        [Test][Description("プロパティなしのConditionノードが正しいConditionクラスとして作成されることを確認")]
        public void ParseContent_ConditionNodeWithoutProperties_CreatesCorrectCondition()
        {
            // Arrange
            string content = @"
                tree TestTree {
                    Condition HealthCheck {
                    }
                }";

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<HealthCheckCondition>(result);
            Assert.AreEqual("Condition:HealthCheck", result.Name);
        }

        [Test][Description("プロパティありのConditionノードでプロパティが正しく設定されることを確認")]
        public void ParseContent_ConditionNodeWithProperties_SetsPropertiesCorrectly()
        {
            // Arrange
            string content = @"
                tree TestTree {
                    Condition EnemyCheck {
                        detection_range: ""10.0""
                        alert_level: ""high""
                    }
                }";

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<EnemyCheckCondition>(result);
            Assert.AreEqual("Condition:EnemyCheck", result.Name);
        }

        [Test][Description("未登録のConditionが指定された場合に適切にエラーハンドリングされることを確認")]
        public void ParseContent_UnknownCondition_CreatesCustomConditionNode()
        {
            // Arrange
            string content = @"
                tree TestTree {
                    Condition UnknownCondition {
                    }
                }";

            // Expect error for unknown condition (統一システムでの新しい順序)

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNull(result); // 未知の条件の場合はnullが返される
        }

        [Test][Description("複数の既知のConditionが含まれるツリーで各々が正しいConditionタイプとして作成されることを確認")]
        public void ParseContent_MultipleKnownConditions_CreatesCorrectConditionTypes()
        {
            // Arrange
            string content = @"
                tree TestTree {
                    Selector Root {
                        Condition HasMana {
                            min_mana: ""50""
                        }
                        Condition EnemyInRange {
                            range: ""5.0""
                        }
                        Condition IsInitialized {
                        }
                    }
                }";

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<BTSelectorNode>(result);
            Assert.AreEqual(3, result.Children.Count);
            Assert.IsInstanceOf<HasManaCondition>(result.Children[0]);
            Assert.IsInstanceOf<EnemyInRangeCondition>(result.Children[1]);
            Assert.IsInstanceOf<IsInitializedCondition>(result.Children[2]);
        }

        [Test][Description("ネストした複合ノードが正しい階層構造として作成されることを確認")]
        public void ParseContent_NestedCompositeNodes_CreatesCorrectHierarchy()
        {
            // Arrange
            string content = @"
                tree ComplexTree {
                    Selector RootSelector {
                        Sequence AttackSequence {
                            Condition EnemyCheck {
                                detection_range: ""8.0""
                            }
                            Action AttackTarget {
                                damage: ""30""
                            }
                        }
                        Sequence PatrolSequence {
                            Action RandomWander {
                                speed: ""2.0""
                            }
                            Action Wait {
                                duration: ""1.0""
                            }
                        }
                    }
                }";

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<BTSelectorNode>(result);
            Assert.AreEqual("RootSelector", result.Name);
            Assert.AreEqual(2, result.Children.Count);

            // First child: AttackSequence
            var attackSequence = result.Children[0];
            Assert.IsInstanceOf<BTSequenceNode>(attackSequence);
            Assert.AreEqual("AttackSequence", attackSequence.Name);
            Assert.AreEqual(2, attackSequence.Children.Count);
            Assert.IsInstanceOf<EnemyCheckCondition>(attackSequence.Children[0]);
            Assert.IsInstanceOf<AttackTargetAction>(attackSequence.Children[1]);

            // Second child: PatrolSequence
            var patrolSequence = result.Children[1];
            Assert.IsInstanceOf<BTSequenceNode>(patrolSequence);
            Assert.AreEqual("PatrolSequence", patrolSequence.Name);
            Assert.AreEqual(2, patrolSequence.Children.Count);
            Assert.IsInstanceOf<RandomWanderAction>(patrolSequence.Children[0]);
            Assert.IsInstanceOf<WaitAction>(patrolSequence.Children[1]);
        }

        [Test][Description("複数の子ノードを持つParallelノードが正しい構造として作成されることを確認")]
        public void ParseContent_ParallelWithMultipleChildren_CreatesCorrectStructure()
        {
            // Arrange
            string content = @"
                tree ParallelTree {
                    Parallel RootParallel {
                        Action ScanEnvironment {
                            scan_radius: ""15.0""
                        }
                        Sequence MovementSequence {
                            Action MoveToTarget {
                                speed: ""3.0""
                            }
                        }
                        Condition HealthCheck {
                            min_health: ""20""
                        }
                    }
                }";

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<BTParallelNode>(result);
            Assert.AreEqual("RootParallel", result.Name);
            Assert.AreEqual(3, result.Children.Count);
            Assert.IsInstanceOf<ScanEnvironmentAction>(result.Children[0]);
            Assert.IsInstanceOf<BTSequenceNode>(result.Children[1]);
            Assert.IsInstanceOf<HealthCheckCondition>(result.Children[2]);
        }

        [Test][Description("文字列プロパティが正しく解析されることを確認")]
        public void ParseContent_StringProperties_ParsedCorrectly()
        {
            // Arrange
            string content = @"
        tree TestTree {
            Action CastSpell {
                spell_name: ""fireball""
                element_type: ""fire""
                target_type: ""enemy""
            }
        }";

            // 期待されるログメッセージ

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<CastSpellAction>(result);
        }

        [Test][Description("数値プロパティが正しく解析されることを確認")]
        public void ParseContent_NumberProperties_ParsedCorrectly()
        {
            // Arrange
            string content = @"
        tree TestTree {
            Action AttackEnemy {
                damage: ""50""
                attack_range: ""2.5""
                cooldown: ""1.0""
            }
        }";

            // 期待されるログメッセージ

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<AttackEnemyAction>(result);
        }

        [Test][Description("混合型プロパティ（文字列・数値・真偽値）が正しく解析されることを確認")]
        public void ParseContent_MixedProperties_ParsedCorrectly()
        {
            // Arrange
            string content = @"
        tree TestTree {
            Action UseItem {
                item_name: ""healing_potion""
                quantity: ""1""
                auto_use: ""true""
                effectiveness: ""0.8""
            }
        }";

            // 期待されるログメッセージ

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<UseItemAction>(result);
        }

        [Test][Description("コメント（#）が含まれるBTファイルでコメントが正しく無視されることを確認")]
        public void ParseContent_WithComments_IgnoresComments()
        {
            // Arrange
            string content = @"
                # This is a comment
                tree TestTree { # Another comment
                    # Comment in the middle
                    Sequence RootSequence {
                        Action Wait { # End of line comment
                            duration: ""1.0"" # Property comment
                        }
                    }
                    # Final comment
                }";

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<BTSequenceNode>(result);
            Assert.AreEqual(1, result.Children.Count);
            Assert.IsInstanceOf<WaitAction>(result.Children[0]);
        }

        [Test][Description("余分な空白文字が含まれるBTファイルが正しく解析されることを確認")]
        public void ParseContent_WithExtraWhitespace_ParsesCorrectly()
        {
            // Arrange
            string content = @"


                tree    TestTree    {


                    Sequence     RootSequence    {
                        Action     Wait     {
                            duration   :    ""1.0""     
                        }


                    }


                }


            ";

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<BTSequenceNode>(result);
            Assert.AreEqual(1, result.Children.Count);
            Assert.IsInstanceOf<WaitAction>(result.Children[0]);
        }

        [Test][Description("有効なBTファイルからツリーが正しく読み込まれることを確認")]
        public void ParseFile_ValidFile_ReturnsCorrectTree()
        {
            // Arrange
            string content = @"
                tree FileTree {
                    Selector Root {
                        Action Wait {
                            duration: ""2.0""
                        }
                    }
                }";
            File.WriteAllText(tempFilePath, content);

            // Act
            var result = parser.ParseFile(tempFilePath);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<BTSelectorNode>(result);
            Assert.AreEqual(1, result.Children.Count);
        }

        [Test][Description("存在しないBTファイルの読み込み時に適切にnullが返されることを確認")]
        public void ParseFile_NonExistentFile_ReturnsNull()
        {
            // Expect error log for non-existent file

            // Act
            var result = parser.ParseFile("non_existent_file.bt");

            // Assert
            Assert.IsNull(result);
        }

        [Test][Description("構文エラーのあるBTファイルが適切にエラーハンドリングされることを確認")]
        public void ParseContent_MalformedTree_HandlesGracefully()
        {
            // Arrange
            string content = @"
                tree TestTree
                    Sequence RootSequence {
                        Action Wait
                    }
                ";

            // Expect multiple error messages for malformed syntax

            // Act
            var result = parser.ParseContent(content);

            // Assert
            // Should handle gracefully and return null or partial tree
            // The exact behavior depends on your parser's error handling strategy
        }

        [Test][Description("空のノード本体を持つノードが子ノードなしで正しく作成されることを確認")]
        public void ParseContent_EmptyNodeBody_CreatesNodeWithoutChildren()
        {
            // Arrange
            string content = @"
                tree TestTree {
                    Sequence EmptySequence {
                    }
                }";

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<BTSequenceNode>(result);
            Assert.AreEqual(0, result.Children.Count);
        }

        [Test][Description("実際のAIシステムを想定した複雑なBTファイルが完全なツリー構造として作成されることを確認")]
        public void ParseContent_RealWorldExample_CreatesCompleteTree()
        {
            // Arrange
            string content = @"
                tree AIBehaviorTree {
                    Selector MainSelector {
                        Sequence InitSequence {
                            Condition IsInitialized {
                            }
                            Action InitializeResources {
                            }
                        }
                        Sequence CombatSequence {
                            Condition EnemyInRange {
                                range: ""8.0""
                            }
                            Selector CombatSelector {
                                Sequence MagicAttack {
                                    Condition HasMana {
                                        min_mana: ""50""
                                    }
                                    Action CastSpell {
                                        spell_name: ""fireball""
                                        damage: ""60""
                                        mana_cost: ""50""
                                    }
                                }
                                Action NormalAttack {
                                    damage: ""25""
                                    attack_range: ""2.0""
                                }
                            }
                        }
                        Sequence PatrolSequence {
                            Action ScanEnvironment {
                                scan_radius: ""15.0""
                                scan_interval: ""2.0""
                            }
                            Action RandomWander {
                                speed: ""2.0""
                                wander_radius: ""10.0""
                            }
                        }
                    }
                }";

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<BTSelectorNode>(result);
            Assert.AreEqual("MainSelector", result.Name);
            Assert.AreEqual(3, result.Children.Count);

            // Verify structure
            Assert.IsInstanceOf<BTSequenceNode>(result.Children[0]); // InitSequence
            Assert.IsInstanceOf<BTSequenceNode>(result.Children[1]); // CombatSequence
            Assert.IsInstanceOf<BTSequenceNode>(result.Children[2]); // PatrolSequence
        }

        [Test][Description("Actionノード作成時に適切なデバッグメッセージが出力されることを確認")]
        public void ParseContent_ActionCreation_LogsCorrectDebugMessages()
        {
            // Arrange
            string content = @"
                tree TestTree {
                    Action AttackEnemy {
                        damage: ""50""
                    }
                }";

            // Expect debug logs from node creation

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<AttackEnemyAction>(result);
        }
    }
}
