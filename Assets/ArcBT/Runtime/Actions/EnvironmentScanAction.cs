using UnityEngine;
using ArcBT.Core;

namespace ArcBT.Actions
{
    [BTScript("EnvironmentScan")]
    public class EnvironmentScanAction : BTActionNode
    {
        float scanRange = 10f;
        string[] scanTags = { "Enemy", "Item", "Interactable" };

        public override void SetProperty(string key, string value)
        {
            switch (key.ToLower())
            {
                case "range":
                case "scan_range":
                    if (float.TryParse(value, out float r)) scanRange = r;
                    break;
                case "tags":
                case "scan_tags":
                    scanTags = value.Split(',');
                    for (int i = 0; i < scanTags.Length; i++)
                    {
                        scanTags[i] = scanTags[i].Trim();
                    }
                    break;
            }
        }

        protected override BTNodeResult ExecuteAction()
        {
            int foundObjects = 0;

            foreach (string tag in scanTags)
            {
                var objects = GameObject.FindGameObjectsWithTag(tag);
                
                foreach (var obj in objects)
                {
                    float distance = Vector3.Distance(transform.position, obj.transform.position);
                    if (distance <= scanRange)
                    {
                        foundObjects++;
                        
                        // BlackBoardに情報を記録
                        if (blackBoard != null)
                        {
                            string key = $"scanned_{tag.ToLower()}";
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
            else
            {
                BTLogger.LogSystem("環境スキャン: 範囲内にオブジェクトが見つかりませんでした", Name);
                return BTNodeResult.Failure;
            }
        }
    }
}