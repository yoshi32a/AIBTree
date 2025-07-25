using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree.Core
{
    /// <summary>
    /// AI間でデータを共有するためのBlackBoardシステム
    /// </summary>
    public class BlackBoard
    {
        readonly Dictionary<string, object> data = new Dictionary<string, object>();
        readonly Dictionary<string, System.Type> dataTypes = new Dictionary<string, System.Type>();

        /// <summary>
        /// 値を設定する
        /// </summary>
        public void SetValue<T>(string key, T value)
        {
            data[key] = value;
            dataTypes[key] = typeof(T);
            
            Debug.Log($"🗂️ BlackBoard: Set '{key}' = {value} ({typeof(T).Name})");
        }

        /// <summary>
        /// 値を取得する
        /// </summary>
        public T GetValue<T>(string key, T defaultValue = default(T))
        {
            if (data.TryGetValue(key, out var value))
            {
                if (value is T typedValue)
                {
                    return typedValue;
                }
                else
                {
                    Debug.LogWarning($"🗂️ BlackBoard: Type mismatch for key '{key}'. Expected {typeof(T).Name}, got {value.GetType().Name}");
                    return defaultValue;
                }
            }
            
            return defaultValue;
        }

        /// <summary>
        /// キーが存在するかチェック
        /// </summary>
        public bool HasKey(string key)
        {
            return data.ContainsKey(key);
        }

        /// <summary>
        /// 値を削除する
        /// </summary>
        public void RemoveValue(string key)
        {
            if (data.Remove(key))
            {
                dataTypes.Remove(key);
                Debug.Log($"🗂️ BlackBoard: Removed '{key}'");
            }
        }

        /// <summary>
        /// 全てのデータをクリア
        /// </summary>
        public void Clear()
        {
            data.Clear();
            dataTypes.Clear();
            Debug.Log("🗂️ BlackBoard: Cleared all data");
        }

        /// <summary>
        /// デバッグ用：全てのキーと値を表示
        /// </summary>
        public void DebugLog()
        {
            Debug.Log("🗂️ BlackBoard Contents:");
            foreach (var kvp in data)
            {
                Debug.Log($"  - {kvp.Key}: {kvp.Value} ({dataTypes[kvp.Key].Name})");
            }
        }

        /// <summary>
        /// 全てのキーを取得
        /// </summary>
        public string[] GetAllKeys()
        {
            var keys = new string[data.Count];
            data.Keys.CopyTo(keys, 0);
            return keys;
        }

        /// <summary>
        /// 値の型を取得
        /// </summary>
        public System.Type GetValueType(string key)
        {
            return dataTypes.TryGetValue(key, out var type) ? type : null;
        }
    }
}