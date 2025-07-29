using ArcBT.Core;
using ArcBT.Logger;
using ArcBT.TagSystem;
using UnityEngine;

namespace ArcBT.Samples.RPG.Conditions
{
    [BTNode("HasTarget")]
    public class HasTargetCondition : BTConditionNode
    {
        GameplayTag targetTag = "Character.Enemy";

        public override void SetProperty(string key, string value)
        {
            switch (key.ToLower())
            {
                case "tag":
                case "target_tag":
                    targetTag = new GameplayTag(value);
                    break;
            }
        }

        protected override BTNodeResult CheckCondition()
        {
            // BlackBoardからターゲット情報をチェック
            if (blackBoard != null)
            {
                var targetInfo = blackBoard.GetValue<GameObject>("target_enemy");
                if (targetInfo != null)
                {
                    BTLogger.LogCondition($"BlackBoardにターゲット情報があります: {targetInfo.name}", Name);
                    return BTNodeResult.Success;
                }
            }

            // シーン内でターゲットを検索
            var targets = GameplayTagManager.FindGameObjectsWithTag(targetTag);
            if (targets is { Length: > 0 })
            {
                BTLogger.LogCondition($"ターゲット発見: {targets.Length}体の{targetTag}", Name);
                return BTNodeResult.Success;
            }

            BTLogger.LogCondition($"ターゲット未発見: {targetTag}", Name);
            return BTNodeResult.Failure;
        }
    }
}
