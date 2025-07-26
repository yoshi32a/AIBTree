using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ArcBT.Core
{
    /// <summary>
    /// BTノードクラスのレジストリ
    /// BTScript属性を持つクラスを自動的に発見・登録します
    /// </summary>
    public static class BTNodeRegistry
    {
        static readonly Dictionary<string, Type> actionTypes = new Dictionary<string, Type>();
        static readonly Dictionary<string, Type> conditionTypes = new Dictionary<string, Type>();
        static bool isInitialized = false;

        /// <summary>レジストリを初期化し、BTScript属性を持つクラスを検索・登録</summary>
        public static void Initialize()
        {
            if (isInitialized) return;

            actionTypes.Clear();
            conditionTypes.Clear();

            // 全アセンブリからBTScript属性を持つクラスを検索
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.GetCustomAttribute<BTScriptAttribute>() != null);

                    foreach (var type in types)
                    {
                        var attribute = type.GetCustomAttribute<BTScriptAttribute>();
                        var scriptName = attribute.ScriptName;

                        if (typeof(BTActionNode).IsAssignableFrom(type))
                        {
                            actionTypes[scriptName] = type;
                            BTLogger.LogSystem($"🎯 Registered Action: '{scriptName}' → {type.Name}");
                        }
                        else if (typeof(BTConditionNode).IsAssignableFrom(type))
                        {
                            conditionTypes[scriptName] = type;
                            BTLogger.LogSystem($"🎯 Registered Condition: '{scriptName}' → {type.Name}");
                        }
                        else
                        {
                            BTLogger.LogSystem($"⚠️ Skipped invalid BTScript class: {type.Name} (not BTActionNode or BTConditionNode)");
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // アセンブリの読み込みエラーは無視（Unityの一部アセンブリで発生する場合がある）
                    BTLogger.LogSystem($"⚠️ Failed to load types from assembly: {assembly.FullName}");
                }
                catch (Exception ex)
                {
                    BTLogger.LogSystem($"⚠️ Error scanning assembly {assembly.FullName}: {ex.Message}");
                }
            }

            isInitialized = true;
            BTLogger.LogSystem($"✅ BTNodeRegistry initialized with {actionTypes.Count} actions, {conditionTypes.Count} conditions");
        }

        /// <summary>指定されたスクリプト名のActionクラスを作成</summary>
        public static BTActionNode CreateAction(string scriptName)
        {
            if (!isInitialized) Initialize();

            if (actionTypes.TryGetValue(scriptName, out var type))
            {
                return (BTActionNode)Activator.CreateInstance(type);
            }

            return null;
        }

        /// <summary>指定されたスクリプト名のConditionクラスを作成</summary>
        public static BTConditionNode CreateCondition(string scriptName)
        {
            if (!isInitialized) Initialize();

            if (conditionTypes.TryGetValue(scriptName, out var type))
            {
                return (BTConditionNode)Activator.CreateInstance(type);
            }

            return null;
        }

        /// <summary>登録されているActionスクリプト名の一覧を取得</summary>
        public static IEnumerable<string> GetRegisteredActionNames()
        {
            if (!isInitialized) Initialize();
            return actionTypes.Keys;
        }

        /// <summary>登録されているConditionスクリプト名の一覧を取得</summary>
        public static IEnumerable<string> GetRegisteredConditionNames()
        {
            if (!isInitialized) Initialize();
            return conditionTypes.Keys;
        }

        /// <summary>デバッグ用：登録されているすべてのクラスをログ出力</summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void DebugLogRegistry()
        {
            if (!isInitialized) Initialize();

            BTLogger.LogSystem("=== BTNodeRegistry Debug Info ===");
            BTLogger.LogSystem($"Registered Actions ({actionTypes.Count}):");
            foreach (var kvp in actionTypes)
            {
                BTLogger.LogSystem($"  '{kvp.Key}' → {kvp.Value.FullName}");
            }

            BTLogger.LogSystem($"Registered Conditions ({conditionTypes.Count}):");
            foreach (var kvp in conditionTypes)
            {
                BTLogger.LogSystem($"  '{kvp.Key}' → {kvp.Value.FullName}");
            }
            BTLogger.LogSystem("=== End Registry Debug ===");
        }
    }
}