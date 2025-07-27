using ArcBT.Core;
using ArcBT.Logger;
using UnityEngine;

namespace ArcBT.Conditions
{
    [BTNode("HasTarget")]
    public class HasTargetCondition : BTConditionNode
    {
        string targetTag = "Enemy";

        public override void SetProperty(string key, string value)
        {
            switch (key.ToLower())
            {
                case "tag":
                case "target_tag":
                    targetTag = value;
                    break;
            }
        }

        protected override BTNodeResult CheckCondition()
        {
            // BlackBoardからターゲット情報をチェック
            if (blackBoard != null)
            {
                var targetInfo = blackBoard.GetValue<GameObject>("target_enemy", null);
                if (targetInfo != null)
                {
                    BTLogger.LogCondition($"BlackBoardにターゲット情報があります: {targetInfo.name}", Name);
                    return BTNodeResult.Success;
                }
            }

            // シーン内でターゲットを検索
            var targets = GameObject.FindGameObjectsWithTag(targetTag);
            if (targets != null && targets.Length > 0)
            {
                BTLogger.LogCondition($"ターゲット発見: {targets.Length}体の{targetTag}", Name);
                return BTNodeResult.Success;
            }

            BTLogger.LogCondition($"ターゲット未発見: {targetTag}", Name);
            return BTNodeResult.Failure;
        }
    }
}
