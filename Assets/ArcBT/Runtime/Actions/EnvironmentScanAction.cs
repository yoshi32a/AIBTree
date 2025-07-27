using ArcBT.Core;
using ArcBT.Logger;
using ArcBT.TagSystem;
using UnityEngine;

namespace ArcBT.Actions
{
    [BTNode("EnvironmentScan")]
    public class EnvironmentScanAction : BTActionNode
    {
        float scanRange = 10f;
        GameplayTag[] scanTags = { "Character.Enemy", "Object.Item", "Object.Interactable" };

        public override void SetProperty(string key, string value)
        {
            switch (key.ToLower())
            {
                case "range":
                case "scan_range":
                    if (float.TryParse(value, out var r)) scanRange = r;
                    break;
                case "tags":
                case "scan_tags":
                    var tagStrings = value.Split(',');
                    scanTags = new GameplayTag[tagStrings.Length];
                    for (var i = 0; i < tagStrings.Length; i++)
                    {
                        scanTags[i] = new GameplayTag(tagStrings[i].Trim());
                    }
                    break;
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            var foundObjects = 0;

            foreach (var tag in scanTags)
            {
                var objects = GameplayTagManager.FindGameObjectsWithTag(tag);

                foreach (var obj in objects)
                {
                    var distance = Vector3.Distance(transform.position, obj.transform.position);
                    if (distance <= scanRange)
                    {
                        foundObjects++;

                        // BlackBoardに情報を記録
                        if (blackBoard != null)
                        {
                            var key = $"scanned_{tag.TagName.ToLower().Replace(".", "_")}";
                            blackBoard.SetValue(key, obj);
                            BTLogger.LogSystem($"環境スキャン: {tag} を発見 - {obj.name} (距離: {distance:F1})", Name);
                        }
                    }
                }
            }

            if (foundObjects > 0)
            {
                BTLogger.LogSystem($"環境スキャン完了: {foundObjects}個のオブジェクトを発見", Name);
                return BTNodeResult.Success;
            }

            BTLogger.LogSystem("環境スキャン: 範囲内にオブジェクトが見つかりませんでした", Name);
            return BTNodeResult.Failure;
        }
    }
}
