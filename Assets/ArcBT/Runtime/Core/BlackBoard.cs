using System.Collections.Generic;
using ArcBT.Logger;
using UnityEngine;

namespace ArcBT.Core
{
    /// <summary>AI間でデータを共有するためのBlackBoardシステム</summary>
    public class BlackBoard
    {
        readonly Dictionary<string, object> data = new();
        readonly Dictionary<string, System.Type> dataTypes = new();

        // 変更追跡用
        readonly List<string> recentChanges = new();
        float lastChangeTime;

        /// <summary>値を設定する</summary>
        public void SetValue<T>(string key, T value)
        {
            var isNewKey = !data.ContainsKey(key);
            var isValueChanged = false;

            if (!isNewKey)
            {
                var oldValue = data[key];
                // null安全な比較
                if (oldValue == null && value == null)
                {
                    isValueChanged = false;
                }
                else if (oldValue == null || value == null)
                {
                    isValueChanged = true;
                }
                else
                {
                    isValueChanged = !oldValue.Equals(value);
                }
            }

            data[key] = value;
            dataTypes[key] = typeof(T);
            BTLogger.LogBlackBoardSet(key, value, typeof(T).Name);

            // 変更追跡
            if (isNewKey || isValueChanged)
            {
                recentChanges.Add($"{key}={value}");
                lastChangeTime = Time.time;

                // 冗長ログを避けて重要な変更のみログ出力
                if (IsImportantKey(key))
                {
                    var changeType = isNewKey ? "新規" : "更新";
                    var displayValue = value?.ToString() ?? "null";
                    BTLogger.LogSystem("BlackBoard", $"🗂️ BlackBoard[{changeType}]: {key} = {displayValue}");
                }
            }
        }

        bool IsImportantKey(string key)
        {
            // 重要なキーのみログに出力（ノイズ低減）
            return key.Contains("enemy") || key.Contains("target") ||
                   key.Contains("position") || key.Contains("health") ||
                   key.Contains("state") || key.Contains("action");
        }

        /// <summary>値を取得する</summary>
        public T GetValue<T>(string key, T defaultValue = default)
        {
            if (data.TryGetValue(key, out var value))
            {
                if (value is T typedValue)
                {
                    return typedValue;
                }

                var valueTypeName = value?.GetType().Name ?? "null";
                BTLogger.LogSystem("BlackBoard",
                    $"🗂️ BlackBoard: Type mismatch for key '{key}'. Expected {typeof(T).Name}, got {valueTypeName}");
            }

            return defaultValue;
        }

        /// <summary>キーが存在するかチェック</summary>
        public bool HasKey(string key)
        {
            return data.ContainsKey(key);
        }

        /// <summary>値を削除する</summary>
        public void RemoveValue(string key)
        {
            if (data.Remove(key))
            {
                dataTypes.Remove(key);
                BTLogger.LogSystem("BlackBoard", $"🗂️ BlackBoard: Removed '{key}'");
            }
        }

        /// <summary>全てのデータをクリア</summary>
        public void Clear()
        {
            data.Clear();
            dataTypes.Clear();
            BTLogger.LogSystem("BlackBoard", "🗂️ BlackBoard: Cleared all data");
        }

        /// <summary>デバッグ用：全てのキーと値を表示</summary>
        public void DebugLog()
        {
            BTLogger.LogSystem("BlackBoard", "🗂️ BlackBoard Contents:");
            foreach (var kvp in data)
            {
                BTLogger.LogSystem("BlackBoard", $"  - {kvp.Key}: {kvp.Value} ({dataTypes[kvp.Key].Name})");
            }
        }

        /// <summary>全てのキーを取得</summary>
        public string[] GetAllKeys()
        {
            var keys = new string[data.Count];
            data.Keys.CopyTo(keys, 0);
            return keys;
        }

        /// <summary>値の型を取得</summary>
        public System.Type GetValueType(string key)
        {
            return dataTypes.GetValueOrDefault(key);
        }

        /// <summary>値を文字列として取得（UI表示用）</summary>
        public string GetValueAsString(string key)
        {
            if (data.TryGetValue(key, out var value))
            {
                if (value == null)
                    return "null";

                // GameObject の場合は名前を表示
                if (value is GameObject gameObj)
                    return gameObj.name;

                // Vector3 の場合は座標を簡潔に表示
                if (value is Vector3 vec3)
                    return $"({vec3.x:F1}, {vec3.y:F1}, {vec3.z:F1})";

                // float の場合は小数点1桁まで表示
                if (value is float floatVal)
                    return floatVal.ToString("F1");

                return value.ToString();
            }

            return "未設定";
        }

        /// <summary>最近変更があったかチェック</summary>
        public bool HasRecentChanges()
        {
            return recentChanges.Count > 0 && Time.time - lastChangeTime < 1f;
        }

        /// <summary>最近の変更のサマリーを取得</summary>
        public string GetRecentChangeSummary()
        {
            if (recentChanges.Count == 0)
            {
                return "変更なし";
            }

            var summary = string.Join(", ", recentChanges);
            recentChanges.Clear(); // サマリーを取得したらクリア
            return summary;
        }
    }
}
