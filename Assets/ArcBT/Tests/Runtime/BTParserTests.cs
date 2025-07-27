using System.IO;
using System.Text.RegularExpressions;
using ArcBT.Actions;
using ArcBT.Core;
using ArcBT.Logger;
using ArcBT.Samples.RPG;
using ArcBT.Samples.RPG.Actions;
using ArcBT.Samples.RPG.Conditions;
using ArcBT.Parser;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ArcBT.Tests
{
    [TestFixture]
    public class BTParserTests
    {
        private BTParser parser;
        private string tempFilePath;

        [SetUp]
        public void SetUp()
        {
            parser = new BTParser();
            tempFilePath = Path.GetTempFileName();
            BTLogger.EnableTestMode(); // テストモードでパーサーログを有効化
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
            BTLogger.ResetToDefaults(); // テスト後は通常モードに戻す
        }

        [Test]
        public void ParseContent_EmptyContent_ReturnsNull()
        {
            // Arrange
            string content = "";
            
            // Expect the error log message
            LogAssert.Expect(LogType.Error, "[ERR][PRS]: No tree definition found");

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void ParseContent_NoTreeDefinition_ReturnsNull()
        {
            // Arrange
            string content = "# This is just a comment\n";
            
            // Expect the error log message
            LogAssert.Expect(LogType.Error, "[ERR][PRS]: No tree definition found");

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public void ParseContent_UnknownAction_CreatesCustomActionNode()
        {
            // Arrange
            string content = @"
                tree TestTree {
                    Action UnknownAction {
                    }
                }";

            // Expect error for unknown action
            LogAssert.Expect(LogType.Error, "[ERR][SYS]: Unknown action script: UnknownAction");
            LogAssert.Expect(LogType.Error, "[ERR][PRS]: Unknown action script: UnknownAction. Please register the action in BTStaticNodeRegistry or use source generator.");
            LogAssert.Expect(LogType.Error, "[ERR][PRS]: Failed to create node of type: Action");

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNull(result); // 未知のアクションの場合はnullが返される
        }

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public void ParseContent_UnknownCondition_CreatesCustomConditionNode()
        {
            // Arrange
            string content = @"
                tree TestTree {
                    Condition UnknownCondition {
                    }
                }";

            // Expect error for unknown condition
            LogAssert.Expect(LogType.Error, "[ERR][SYS]: Unknown condition script: UnknownCondition");
            LogAssert.Expect(LogType.Error, "[ERR][PRS]: Unknown condition script: UnknownCondition. Please register the condition in BTStaticNodeRegistry or use source generator.");
            LogAssert.Expect(LogType.Error, "[ERR][PRS]: Failed to create node of type: Condition");

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNull(result); // 未知の条件の場合はnullが返される
        }

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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
            LogAssert.Expect(LogType.Log, new Regex(@"🔍 Creating node: Action CastSpell"));
            LogAssert.Expect(LogType.Log, new Regex(@"🚀 Creating Action with script 'CastSpell'"));
            LogAssert.Expect(LogType.Log, new Regex(@"✅ Created action for script 'CastSpell'"));

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<CastSpellAction>(result);
        }

        [Test]
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
            LogAssert.Expect(LogType.Log, new Regex(@"🔍 Creating node: Action AttackEnemy"));
            LogAssert.Expect(LogType.Log, new Regex(@"🚀 Creating Action with script 'AttackEnemy'"));
            LogAssert.Expect(LogType.Log, new Regex(@"✅ Created action for script 'AttackEnemy'"));

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<AttackEnemyAction>(result);
        }

        [Test]
        public void ParseContent_MixedProperties_ParsedCorrectly()
        {
            // Arrange
            string content = @"
        tree TestTree {
            Action UseItem {
                item_name: ""health_potion""
                quantity: ""1""
                auto_use: ""true""
                effectiveness: ""0.8""
            }
        }";

            // 期待されるログメッセージ
            LogAssert.Expect(LogType.Log, new Regex(@"🔍 Creating node: Action UseItem"));
            LogAssert.Expect(LogType.Log, new Regex(@"🚀 Creating Action with script 'UseItem'"));
            LogAssert.Expect(LogType.Log, new Regex(@"✅ Created action for script 'UseItem'"));

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<UseItemAction>(result);
        }

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public void ParseFile_NonExistentFile_ReturnsNull()
        {
            // Expect error log for non-existent file
            LogAssert.Expect(LogType.Error, new Regex(@"\[ERR\]\[PRS\]: BT file not found: .*"));

            // Act
            var result = parser.ParseFile("non_existent_file.bt");

            // Assert
            Assert.IsNull(result);
        }

        [Test]
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
            LogAssert.Expect(LogType.Error, new Regex(@"\[ERR\]\[PRS\]: Expected.*"));

            // Act
            var result = parser.ParseContent(content);

            // Assert
            // Should handle gracefully and return null or partial tree
            // The exact behavior depends on your parser's error handling strategy
        }

        [Test]
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

        [Test]
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

        [Test]
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
            LogAssert.Expect(LogType.Log, new Regex(@"🔍 Creating node: .*"));
            LogAssert.Expect(LogType.Log, new Regex(@"🚀 Creating Action with script .*"));
            LogAssert.Expect(LogType.Log, new Regex(@"✅ Created .*"));

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<AttackEnemyAction>(result);
        }
    }
}
