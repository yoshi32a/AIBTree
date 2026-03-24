using ArcBT.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArcBT.Tests
{
    /// <summary>BTParallelNodeの機能をテストするクラス</summary>
    [TestFixture]
    public class BTParallelNodeTests : BTTestBase
    {
        GameObject testOwner;
        BlackBoard blackBoard;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            testOwner = CreateTestGameObject("TestOwner");
            blackBoard = new BlackBoard();
        }

        [TearDown]
        public override void TearDown()
        {
            DestroyTestObject(testOwner);
            base.TearDown();
        }

        [Test][Description("全ての子ノードがSuccessを返す場合にParallelがSuccessを返すことを確認")]
        public void Execute_AllChildrenSucceed_ReturnsSuccess()
        {
            // Arrange
            var parallel = new BTParallelNode();
            parallel.SuccessPolicy = BTParallelNode.ParallelPolicy.RequireAll;
            parallel.FailurePolicy = BTParallelNode.ParallelPolicy.RequireOne;
            parallel.AddChild(new MockSuccessNode());
            parallel.AddChild(new MockSuccessNode());
            parallel.AddChild(new MockSuccessNode());

            // Act
            var result = parallel.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result, "全ての子ノードがSuccessの場合、Parallelは RequireAll ポリシーでSuccessを返すべき");
        }

        [Test][Description("1つの子ノードがFailureを返す場合にRequireOneポリシーでFailureを返すことを確認")]
        public void Execute_OneChildFails_RequireOneFailurePolicy_ReturnsFailure()
        {
            // Arrange
            var parallel = new BTParallelNode();
            parallel.SuccessPolicy = BTParallelNode.ParallelPolicy.RequireAll;
            parallel.FailurePolicy = BTParallelNode.ParallelPolicy.RequireOne;
            parallel.AddChild(new MockSuccessNode());
            parallel.AddChild(new MockFailureNode());
            parallel.AddChild(new MockSuccessNode());

            // Act
            var result = parallel.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result, "RequireOne失敗ポリシーでは1つでもFailureがあればFailureを返すべき");
        }

        [Test][Description("全ての子ノードがFailureを返す場合にRequireAllポリシーでFailureを返すことを確認")]
        public void Execute_AllChildrenFail_RequireAllFailurePolicy_ReturnsFailure()
        {
            // Arrange
            var parallel = new BTParallelNode();
            parallel.SuccessPolicy = BTParallelNode.ParallelPolicy.RequireAll;
            parallel.FailurePolicy = BTParallelNode.ParallelPolicy.RequireAll;
            parallel.AddChild(new MockFailureNode());
            parallel.AddChild(new MockFailureNode());
            parallel.AddChild(new MockFailureNode());

            // Act
            var result = parallel.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result, "RequireAll失敗ポリシーでは全てFailureの場合にFailureを返すべき");
        }

        [Test][Description("一部がFailureでもRequireAll失敗ポリシーではFailureにならないことを確認")]
        public void Execute_SomeChildrenFail_RequireAllFailurePolicy_DoesNotReturnFailure()
        {
            // Arrange
            var parallel = new BTParallelNode();
            parallel.SuccessPolicy = BTParallelNode.ParallelPolicy.RequireAll;
            parallel.FailurePolicy = BTParallelNode.ParallelPolicy.RequireAll;
            parallel.AddChild(new MockSuccessNode());
            parallel.AddChild(new MockFailureNode());
            parallel.AddChild(new MockSuccessNode());

            // Act
            var result = parallel.Execute();

            // Assert
            Assert.AreNotEqual(BTNodeResult.Failure, result, "RequireAll失敗ポリシーでは全て失敗しない限りFailureにならないべき");
        }

        [Test][Description("RunningとSuccessが混在する場合にParallelがRunningを返すことを確認")]
        public void Execute_MixOfRunningAndSuccess_ReturnsRunning()
        {
            // Arrange
            var parallel = new BTParallelNode();
            parallel.SuccessPolicy = BTParallelNode.ParallelPolicy.RequireAll;
            parallel.FailurePolicy = BTParallelNode.ParallelPolicy.RequireOne;
            parallel.AddChild(new MockSuccessNode());
            parallel.AddChild(new MockRunningNode());
            parallel.AddChild(new MockSuccessNode());

            // Act
            var result = parallel.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Running, result, "RunningとSuccessが混在し全条件が未達の場合はRunningを返すべき");
        }

        [Test][Description("子ノードがRunningを返す場合にParallelがRunningを継続することを確認")]
        public void Execute_ChildReturnsRunning_KeepsParallelRunning()
        {
            // Arrange
            var parallel = new BTParallelNode();
            parallel.SuccessPolicy = BTParallelNode.ParallelPolicy.RequireAll;
            parallel.FailurePolicy = BTParallelNode.ParallelPolicy.RequireOne;
            parallel.AddChild(new MockRunningNode());

            // Act
            var result = parallel.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Running, result, "子ノードがRunning中はParallelもRunningを返すべき");
        }

        [Test][Description("RequireOne成功ポリシーで1つの子がSuccessならSuccessを返すことを確認")]
        public void Execute_OneChildSucceeds_RequireOneSuccessPolicy_ReturnsSuccess()
        {
            // Arrange
            var parallel = new BTParallelNode();
            parallel.SuccessPolicy = BTParallelNode.ParallelPolicy.RequireOne;
            parallel.FailurePolicy = BTParallelNode.ParallelPolicy.RequireAll;
            parallel.AddChild(new MockRunningNode());
            parallel.AddChild(new MockSuccessNode());
            parallel.AddChild(new MockRunningNode());

            // Act
            var result = parallel.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Success, result, "RequireOne成功ポリシーでは1つでもSuccessがあればSuccessを返すべき");
        }

        [Test][Description("Resetで全ての子ノードの状態がクリアされることを確認")]
        public void Reset_ClearsAllChildrenState()
        {
            // Arrange
            var parallel = new BTParallelNode();
            var countingChild1 = new MockCountingNode();
            var countingChild2 = new MockCountingNode();
            parallel.AddChild(countingChild1);
            parallel.AddChild(countingChild2);

            // Act - 実行してからリセット
            parallel.Execute();
            Assert.AreEqual(1, countingChild1.ExecuteCount, "実行後のカウントが1であるべき");
            Assert.AreEqual(1, countingChild2.ExecuteCount, "実行後のカウントが1であるべき");

            parallel.Reset();

            // Assert - Resetが子ノードにも伝播することを確認
            Assert.AreEqual(0, countingChild1.ExecuteCount, "Reset後のカウントが0であるべき");
            Assert.AreEqual(0, countingChild2.ExecuteCount, "Reset後のカウントが0であるべき");
        }

        [Test][Description("子ノードが空の場合にSuccessを返すことを確認")]
        public void Execute_EmptyChildrenList_ReturnsSuccess()
        {
            // Arrange
            var parallel = new BTParallelNode();
            parallel.SuccessPolicy = BTParallelNode.ParallelPolicy.RequireAll;
            parallel.FailurePolicy = BTParallelNode.ParallelPolicy.RequireOne;

            // Act
            var result = parallel.Execute();

            // Assert
            // RequireAll成功ポリシー: successCount(0) >= Children.Count(0) は true なのでSuccess
            Assert.AreEqual(BTNodeResult.Success, result, "子ノードが空の場合、RequireAllは全条件達成でSuccessを返すべき");
        }

        [Test][Description("全ての子ノードがRunningの場合にRunningを返すことを確認")]
        public void Execute_AllChildrenRunning_ReturnsRunning()
        {
            // Arrange
            var parallel = new BTParallelNode();
            parallel.SuccessPolicy = BTParallelNode.ParallelPolicy.RequireAll;
            parallel.FailurePolicy = BTParallelNode.ParallelPolicy.RequireOne;
            parallel.AddChild(new MockRunningNode());
            parallel.AddChild(new MockRunningNode());
            parallel.AddChild(new MockRunningNode());

            // Act
            var result = parallel.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Running, result, "全ての子ノードがRunningの場合はRunningを返すべき");
        }

        [Test][Description("FailureポリシーがSuccessポリシーより先に評価されることを確認")]
        public void Execute_FailurePolicyEvaluatedBeforeSuccessPolicy()
        {
            // Arrange - 1つのFailureと1つのSuccessがある場合、RequireOne失敗ポリシーが先に評価される
            var parallel = new BTParallelNode();
            parallel.SuccessPolicy = BTParallelNode.ParallelPolicy.RequireOne;
            parallel.FailurePolicy = BTParallelNode.ParallelPolicy.RequireOne;
            parallel.AddChild(new MockFailureNode());
            parallel.AddChild(new MockSuccessNode());

            // Act
            var result = parallel.Execute();

            // Assert
            Assert.AreEqual(BTNodeResult.Failure, result, "FailureポリシーがSuccessポリシーより先に評価されるべき");
        }

        [Test][Description("デフォルトのポリシー設定が正しいことを確認")]
        public void DefaultPolicy_IsRequireAllSuccessAndRequireOneFailure()
        {
            // Arrange & Act
            var parallel = new BTParallelNode();

            // Assert
            Assert.AreEqual(BTParallelNode.ParallelPolicy.RequireAll, parallel.SuccessPolicy, "デフォルト成功ポリシーはRequireAllであるべき");
            Assert.AreEqual(BTParallelNode.ParallelPolicy.RequireOne, parallel.FailurePolicy, "デフォルト失敗ポリシーはRequireOneであるべき");
        }
    }

    /// <summary>実行回数をカウントし、Resetでカウントをクリアするモックノード</summary>
    public class MockCountingNode : BTNode
    {
        public int ExecuteCount { get; set; }

        public override BTNodeResult Execute()
        {
            ExecuteCount++;
            return BTNodeResult.Success;
        }

        public override void Reset()
        {
            base.Reset();
            ExecuteCount = 0;
        }

        public override void SetProperty(string key, string value) { }
    }
}
