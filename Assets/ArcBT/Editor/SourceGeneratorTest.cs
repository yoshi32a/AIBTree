#if UNITY_EDITOR
using System;
using System.Linq;
using ArcBT.Core;
using UnityEditor;
using UnityEngine;

namespace ArcBT.Editor
{
    /// <summary>
    /// Source Generator の動作テスト用エディタースクリプト
    /// </summary>
    public static class SourceGeneratorTest
    {
        [MenuItem("BehaviourTree/Source Generator/Test Registration")]
        public static void TestNodeRegistration()
        {
            Debug.Log("=== Source Generator Registration Test ===");
            
            // 登録されているアクションを表示
            var actionNames = BTStaticNodeRegistry.GetActionNames().ToList();
            Debug.Log($"Registered Actions ({actionNames.Count}):");
            foreach (var name in actionNames.OrderBy(n => n))
            {
                Debug.Log($"  - {name}");
            }
            
            // 登録されている条件を表示
            var conditionNames = BTStaticNodeRegistry.GetConditionNames().ToList();
            Debug.Log($"\nRegistered Conditions ({conditionNames.Count}):");
            foreach (var name in conditionNames.OrderBy(n => n))
            {
                Debug.Log($"  - {name}");
            }
            
            // テスト作成
            Debug.Log("\n=== Testing Node Creation ===");
            TestCreateNode("MoveToPosition", true);
            TestCreateNode("Wait", true);
            TestCreateNode("AttackEnemy", true);
            TestCreateNode("HealthCheck", false);
            TestCreateNode("NonExistentNode", true);
        }
        
        static void TestCreateNode(string scriptName, bool isAction)
        {
            if (isAction)
            {
                var node = BTStaticNodeRegistry.CreateAction(scriptName);
                if (node != null)
                {
                    Debug.Log($"✓ Successfully created Action: {scriptName} -> {node.GetType().Name}");
                }
                else
                {
                    Debug.LogWarning($"✗ Failed to create Action: {scriptName}");
                }
            }
            else
            {
                var node = BTStaticNodeRegistry.CreateCondition(scriptName);
                if (node != null)
                {
                    Debug.Log($"✓ Successfully created Condition: {scriptName} -> {node.GetType().Name}");
                }
                else
                {
                    Debug.LogWarning($"✗ Failed to create Condition: {scriptName}");
                }
            }
        }
        
        [MenuItem("BehaviourTree/Source Generator/Show BTNode Attributes")]
        public static void ShowBTNodeAttributes()
        {
            Debug.Log("=== Classes with BTNode Attribute ===");
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            int count = 0;
            
            foreach (var assembly in assemblies)
            {
                if (!assembly.FullName.StartsWith("ArcBT"))
                    continue;
                    
                try
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        var btNodeAttr = type.GetCustomAttributes(typeof(BTNodeAttribute), false)
                            .FirstOrDefault() as BTNodeAttribute;
                            
                        if (btNodeAttr != null)
                        {
                            count++;
                            
                            // 基底クラスからNodeTypeを自動判定
                            string nodeType = "Unknown";
                            if (typeof(BTActionNode).IsAssignableFrom(type))
                            {
                                nodeType = "Action";
                            }
                            else if (typeof(BTConditionNode).IsAssignableFrom(type))
                            {
                                nodeType = "Condition";
                            }
                            
                            Debug.Log($"[{btNodeAttr.AssemblyName ?? "ArcBT"}] {type.FullName} -> Script: '{btNodeAttr.ScriptName}', Type: {nodeType} (auto-detected)");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Error reading assembly {assembly.FullName}: {e.Message}");
                }
            }
            
            Debug.Log($"\nTotal classes with BTNode attribute: {count}");
        }
        
        [MenuItem("BehaviourTree/Source Generator/Generate Registration Code (Manual)")]
        public static void GenerateRegistrationCodeManually()
        {
            // Unity 2021.2より前のバージョン用の手動生成
            #if !UNITY_2021_2_OR_NEWER
            Debug.Log("=== Manual Registration Code Generation ===");
            Debug.Log("Source Generators require Unity 2021.2 or newer.");
            Debug.Log("For older versions, please use the existing RPGNodeRegistration.cs pattern.");
            #else
            Debug.Log("Source Generators are supported in this Unity version.");
            Debug.Log("Generated code should be automatically created when you compile.");
            Debug.Log("Check: Library/Bee/artifacts/ for generated files.");
            #endif
        }
    }
}
#endif
