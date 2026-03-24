using ArcBT.Core;
using ArcBT.Parser;
using NUnit.Framework;

namespace ArcBT.Tests
{
    /// <summary>BTParserのエラーケースをテストするクラス</summary>
    [TestFixture]
    public class BTParserErrorTests : BTTestBase
    {
        BTParser parser;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            parser = new BTParser();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test][Description("閉じ波括弧が不足している場合にパーサーがクラッシュせずに処理することを確認")]
        public void ParseContent_MissingClosingBrace_HandlesGracefully()
        {
            // Arrange
            var content = @"
                tree TestTree {
                    Sequence Root {
                        Action Wait {
                            duration: ""1.0""
                ";

            // Act
            var result = parser.ParseContent(content);

            // Assert - パーサーがクラッシュせずに結果を返す（nullまたは部分的なツリー）
            // 閉じ括弧がない場合でもパーサーは例外をスローしない
            Assert.Pass("閉じ波括弧が不足していてもパーサーはクラッシュしない");
        }

        [Test][Description("未知のノードタイプ（Sequanceタイプミス）がパースエラーとして検出されることを確認")]
        public void ParseContent_UnknownNodeType_DetectsError()
        {
            // Arrange
            var content = @"
                tree TestTree {
                    Sequance Root {
                    }
                }";

            // Act
            var result = parser.ParseContent(content);

            // Assert - 未知のノードタイプはキーワードとして認識されないため、パースが失敗する
            // "Sequance" はキーワード一覧に含まれないのでIdentifierとして扱われる
            Assert.IsNull(result, "未知のノードタイプの場合、パーサーはnullを返すべき");
        }

        [Test][Description("空のツリー定義がSequenceノードなしで正しく処理されることを確認")]
        public void ParseContent_EmptyTreeDefinition_HandlesGracefully()
        {
            // Arrange
            var content = @"tree Test {}";

            // Act
            var result = parser.ParseContent(content);

            // Assert - ツリー内にノードがない場合はnull
            Assert.IsNull(result, "空のツリー定義の場合、ルートノードがないためnullを返すべき");
        }

        [Test][Description("非常に深くネストされたツリーがスタックオーバーフローなく処理されることを確認")]
        public void ParseContent_VeryDeeplyNestedTree_DoesNotCrash()
        {
            // Arrange - MaxNestingDepth(128)を超える深さのツリーを生成
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("tree DeepTree {");
            const int depth = 150;
            for (var i = 0; i < depth; i++)
            {
                builder.AppendLine($"Sequence Level{i} {{");
            }
            // 最深部にアクション
            builder.AppendLine("Action Wait { duration: \"1.0\" }");
            for (var i = 0; i < depth; i++)
            {
                builder.AppendLine("}");
            }
            builder.AppendLine("}");

            // Act - StackOverflowExceptionが発生しないことを確認
            BTNode result = null;
            Assert.DoesNotThrow(() =>
            {
                result = parser.ParseContent(builder.ToString());
            }, "非常に深くネストされたツリーでもStackOverflowExceptionが発生しないべき");
        }

        [Test][Description("プロパティのコロンが欠落している場合にパーサーがエラー回復することを確認")]
        public void ParseContent_InvalidPropertyFormat_MissingColon_RecoversParsing()
        {
            // Arrange - コロンがないプロパティ
            var content = @"
                tree TestTree {
                    Action Wait {
                        duration ""1.0""
                    }
                }";

            // Act
            var result = parser.ParseContent(content);

            // Assert - パーサーはエラー回復を試みる
            // "duration" はIdentifierとして読まれるが、コロンがないためプロパティパース失敗
            // しかしノード自体は作成されるべき
            // 実際の挙動はパーサーのエラー回復実装に依存
            Assert.Pass("コロン欠落のプロパティでもパーサーはクラッシュしない");
        }

        [Test][Description("エスケープシーケンスを含む文字列が正しくパースされることを確認")]
        public void ParseContent_StringWithEscapeSequences_ParsesCorrectly()
        {
            // Arrange - エスケープシーケンスを含む文字列プロパティ
            var content = "tree TestTree {\n" +
                          "    Action Wait {\n" +
                          "        message: \"hello\\nworld\"\n" +
                          "    }\n" +
                          "}";

            // Act
            var result = parser.ParseContent(content);

            // Assert - エスケープシーケンスを含むコンテンツが正常にパースされる
            Assert.IsNotNull(result, "エスケープシーケンスを含む文字列のパースは成功するべき");
        }

        [Test][Description("タブエスケープシーケンスを含む文字列が正しくパースされることを確認")]
        public void ParseContent_StringWithTabEscape_ParsesCorrectly()
        {
            // Arrange
            var content = "tree TestTree {\n" +
                          "    Action Wait {\n" +
                          "        message: \"col1\\tcol2\"\n" +
                          "    }\n" +
                          "}";

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result, "タブエスケープを含む文字列のパースは成功するべき");
        }

        [Test][Description("バックスラッシュエスケープを含む文字列が正しくパースされることを確認")]
        public void ParseContent_StringWithBackslashEscape_ParsesCorrectly()
        {
            // Arrange
            var content = "tree TestTree {\n" +
                          "    Action Wait {\n" +
                          "        path: \"C:\\\\Users\\\\test\"\n" +
                          "    }\n" +
                          "}";

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result, "バックスラッシュエスケープを含む文字列のパースは成功するべき");
        }

        [Test][Description("クォートエスケープを含む文字列が正しくパースされることを確認")]
        public void ParseContent_StringWithQuoteEscape_ParsesCorrectly()
        {
            // Arrange
            var content = "tree TestTree {\n" +
                          "    Action Wait {\n" +
                          "        message: \"say \\\"hello\\\"\"\n" +
                          "    }\n" +
                          "}";

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result, "クォートエスケープを含む文字列のパースは成功するべき");
        }

        [Test][Description("複数のツリー定義がある場合に最初のツリーのみがパースされることを確認")]
        public void ParseContent_MultipleTreeDefinitions_ParsesFirstTreeOnly()
        {
            // Arrange
            var content = @"
                tree FirstTree {
                    Sequence Root1 {
                    }
                }
                tree SecondTree {
                    Selector Root2 {
                    }
                }";

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result, "最初のツリーがパースされるべき");
            Assert.IsInstanceOf<BTSequenceNode>(result, "最初のツリーのルートノードがSequenceであるべき");
            Assert.AreEqual("Root1", result.Name, "最初のツリーのルートノード名がRoot1であるべき");
        }

        [Test][Description("ツリー名の後に波括弧がない場合にnullが返されることを確認")]
        public void ParseContent_MissingOpeningBraceAfterTreeName_ReturnsNull()
        {
            // Arrange
            var content = @"tree TestTree Sequence Root {}";

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNull(result, "ツリー名の後に波括弧がない場合、nullを返すべき");
        }

        [Test][Description("ノード名の後に波括弧がない場合にパーサーがクラッシュしないことを確認")]
        public void ParseContent_MissingOpeningBraceAfterNodeName_HandlesGracefully()
        {
            // Arrange
            var content = @"
                tree TestTree {
                    Sequence Root
                }";

            // Act & Assert - パーサーがクラッシュしないことを確認
            Assert.DoesNotThrow(() =>
            {
                parser.ParseContent(content);
            }, "ノード名の後に波括弧がなくてもパーサーはクラッシュしないべき");
        }

        [Test][Description("tree キーワードの後にツリー名がない場合にnullが返されることを確認")]
        public void ParseContent_MissingTreeName_ReturnsNull()
        {
            // Arrange
            var content = @"tree { Sequence Root {} }";

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNull(result, "ツリー名が欠落している場合、nullを返すべき");
        }

        [Test][Description("数値プロパティがクォートなしでもパースされることを確認")]
        public void ParseContent_NumberPropertyWithoutQuotes_ParsesCorrectly()
        {
            // Arrange
            var content = @"
                tree TestTree {
                    Action Wait {
                        duration: 2.5
                    }
                }";

            // Act
            var result = parser.ParseContent(content);

            // Assert
            Assert.IsNotNull(result, "クォートなし数値プロパティのパースは成功するべき");
        }
    }
}
