using ArcBT.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArcBT.Tests
{
    [TestFixture]
    public class ManualTickTests : BTTestBase
    {
        GameObject owner;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            owner = CreateTestGameObject("ManualTickOwner");
        }

        [TearDown]
        public override void TearDown()
        {
            DestroyTestObject(owner);
            base.TearDown();
        }

        [Test][Description("InitializeForManualTick は enabled=false でも BlackBoard を生成し、二重呼び出しで作り直さない")]
        public void InitializeForManualTick_DisabledRunner_CreatesBlackBoardIdempotently()
        {
            var runner = owner.AddComponent<BehaviourTreeRunner>();
            runner.enabled = false;

            runner.InitializeForManualTick();
            var first = runner.BlackBoard;
            runner.InitializeForManualTick();

            Assert.IsNotNull(first, "BlackBoard が生成されていない");
            Assert.AreSame(first, runner.BlackBoard, "二重初期化で BlackBoard が作り直された");
        }

        [Test][Description("TickOnce は RootNode 未設定なら Failure を返し、設定済みなら実行結果を返す")]
        public void TickOnce_ExecutesRootNode()
        {
            var runner = owner.AddComponent<BehaviourTreeRunner>();
            runner.enabled = false;
            runner.InitializeForManualTick();

            Assert.AreEqual(BTNodeResult.Failure, runner.TickOnce(), "RootNode 無しは Failure のはず");

            runner.RootNode = new BTSequenceNode();
            Assert.AreEqual(BTNodeResult.Success, runner.TickOnce(), "空 Sequence は Success のはず");
        }
    }
}
