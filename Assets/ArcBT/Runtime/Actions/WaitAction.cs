using UnityEngine;
using ArcBT.Core;

namespace ArcBT.Actions
{
    /// <summary>指定時間待機するアクション</summary>
    [System.Serializable]
    [BTScript("Wait")]
    [BTNode("Wait", NodeType.Action)]
    public class WaitAction : BTActionNode
    {
        [SerializeField] float duration = 1.0f;

        float startTime = -1f;

        public override void Initialize(MonoBehaviour owner, BlackBoard sharedBlackBoard = null)
        {
            base.Initialize(owner, sharedBlackBoard);
            startTime = -1f; // リセット
        }

        protected override BTNodeResult ExecuteAction()
        {
            Debug.Log($"=== WaitAction '{Name}' EXECUTING ===");

            if (startTime < 0)
            {
                startTime = Time.time;
                Debug.Log($"Wait '{Name}': Starting wait for {duration} seconds ⏱️");
            }

            var elapsed = Time.time - startTime;
            var remaining = duration - elapsed;

            if (elapsed >= duration)
            {
                Debug.Log($"Wait '{Name}': Wait completed ✅ (waited {elapsed:F1}s)");
                startTime = -1f; // リセット
                return BTNodeResult.Success;
            }

            Debug.Log($"Wait '{Name}': Waiting... ({remaining:F1}s remaining) ⏳");
            return BTNodeResult.Running;
        }

        public override void SetProperty(string propertyName, string value)
        {
            switch (propertyName.ToLower())
            {
                case "duration":
                    if (float.TryParse(value, out var dur))
                    {
                        duration = dur;
                    }

                    break;
                default:
                    base.SetProperty(propertyName, value);
                    break;
            }
        }
    }
}