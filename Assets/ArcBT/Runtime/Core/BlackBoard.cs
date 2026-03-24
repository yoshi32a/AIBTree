using System;
using System.Collections.Generic;
using ArcBT.Logger;
using UnityEngine;

namespace ArcBT.Core
{
    /// <summary>
    /// AI間でデータを共有するためのBlackBoardシステム。
    /// スレッドセーフではありません。メインスレッド（Unityのメインループ）からのみ使用してください。
    /// UniTaskのawait後もメインスレッドで実行される限り安全ですが、
    /// Task.Run等で別スレッドからアクセスする場合は外部で同期が必要です。
    /// </summary>
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
            // StringComparison.Ordinalを使用して文字列比較を高速化
            return key.Contains("enemy", StringComparison.OrdinalIgnoreCase) ||
                   key.Contains("target", StringComparison.OrdinalIgnoreCase) ||
                   key.Contains("position", StringComparison.OrdinalIgnoreCase) ||
                   key.Contains("health", StringComparison.OrdinalIgnoreCase) ||
                   key.Contains("state", StringComparison.OrdinalIgnoreCase) ||
                   key.Contains("action", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 値を取得する。キーが存在しない場合はdefaultValueを返す。
        /// キーが存在するが型が一致しない場合はInvalidCastExceptionをスローする。
        /// 型不一致を例外なく処理したい場合は <see cref="TryGetValue{T}"/> を使用する。
        /// </summary>
        public T GetValue<T>(string key, T defaultValue = default)
        {
            if (data.TryGetValue(key, out var value))
            {
                if (value is T typedValue)
                {
                    return typedValue;
                }

                var valueTypeName = value?.GetType().Name ?? "null";
                throw new System.InvalidCastException(
                    $"BlackBoard: Type mismatch for key '{key}'. Expected {typeof(T).Name}, got {valueTypeName}");
            }

            return defaultValue;
        }

        /// <summary>
        /// 値の取得を試みる。キーが存在し型が一致する場合はtrueを返す。
        /// キーが存在しない場合、または型が一致しない場合はfalseを返しvalueにはdefaultが設定される。
        /// </summary>
        public bool TryGetValue<T>(string key, out T value)
        {
            if (data.TryGetValue(key, out var rawValue) && rawValue is T typedValue)
            {
                value = typedValue;
                return true;
            }

            value = default;
            return false;
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
