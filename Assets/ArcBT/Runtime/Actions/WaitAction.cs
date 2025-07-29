using System;
using ArcBT.Core;
using ArcBT.Logger;
using UnityEngine;

namespace ArcBT.Actions
{
    /// <summary>指定時間待機するアクション</summary>
    [Serializable]
    [BTNode("Wait")]
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
            BTLogger.LogSystem(this, "=== WaitAction EXECUTING ===");

            if (startTime < 0)
            {
                startTime = Time.time;
                BTLogger.LogSystem(this, $"Starting wait for {duration} seconds ⏱️");
            }

            var elapsed = Time.time - startTime;
            var remaining = duration - elapsed;

            if (elapsed >= duration)
            {
                BTLogger.LogSystem(this, $"Wait completed ✅ (waited {elapsed:F1}s)");
                startTime = -1f; // リセット
                return BTNodeResult.Success;
            }

            BTLogger.LogSystem(this, $"Waiting... ({remaining:F1}s remaining) ⏳");
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