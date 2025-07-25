namespace BehaviourTree.Core
{
    // Condition用ベースクラス
    [System.Serializable]
    public abstract class BTConditionNode : BTNode
    {
        public override BTNodeResult Execute()
        {
            return CheckCondition();
        }

        protected abstract BTNodeResult CheckCondition();
    }
}
