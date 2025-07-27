#if UNITY_EDITOR
using System;
using System.Linq;
using ArcBT.Core;
using ArcBT.Logger;
using UnityEditor;

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
            BTLogger.Info("=== Source Generator Registration Test ===");
            
            // 登録されているアクションを表示
            var actionNames = BTStaticNodeRegistry.GetActionNames().ToList();
            BTLogger.Info($"Registered Actions ({actionNames.Count}):");
            foreach (var name in actionNames.OrderBy(n => n))
            {
                BTLogger.Info($"  - {name}");
            }
            
            // 登録されている条件を表示
            var conditionNames = BTStaticNodeRegistry.GetConditionNames().ToList();
            BTLogger.Info($"\nRegistered Conditions ({conditionNames.Count}):");
            foreach (var name in conditionNames.OrderBy(n => n))
            {
                BTLogger.Info($"  - {name}");
            }
            
            // テスト作成
            BTLogger.Info("\n=== Testing Node Creation ===");
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
                    BTLogger.Info($"✓ Successfully created Action: {scriptName} -> {node.GetType().Name}");
                }
                else
                {
                    BTLogger.Warning($"✗ Failed to create Action: {scriptName}");
                }
            }
            else
            {
                var node = BTStaticNodeRegistry.CreateCondition(scriptName);
                if (node != null)
                {
                    BTLogger.Info($"✓ Successfully created Condition: {scriptName} -> {node.GetType().Name}");
                }
                else
                {
                    BTLogger.Warning($"✗ Failed to create Condition: {scriptName}");
                }
            }
        }
        
        [MenuItem("BehaviourTree/Source Generator/Show BTNode Attributes")]
        public static void ShowBTNodeAttributes()
        {
            BTLogger.Info("=== Classes with BTNode Attribute ===");
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var count = 0;
            
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
                            var nodeType = "Unknown";
                            if (typeof(BTActionNode).IsAssignableFrom(type))
                            {
                                nodeType = "Action";
                            }
                            else if (typeof(BTConditionNode).IsAssignableFrom(type))
                            {
                                nodeType = "Condition";
                            }
                            
                            BTLogger.Info($"{type.FullName} -> Script: '{btNodeAttr.ScriptName}', Type: {nodeType} (auto-detected)");
                        }
                    }
                }
                catch (Exception e)
                {
                    BTLogger.Warning($"Error reading assembly {assembly.FullName}: {e.Message}");
                }
            }
            
            BTLogger.Info($"\nTotal classes with BTNode attribute: {count}");
        }
        
        [MenuItem("BehaviourTree/Source Generator/Generate Registration Code (Manual)")]
        public static void GenerateRegistrationCodeManually()
        {
            // Unity 2021.2より前のバージョン用の手動生成
            #if !UNITY_2021_2_OR_NEWER
            BTLogger.Info("=== Manual Registration Code Generation ===");
            BTLogger.Info("Source Generators require Unity 2021.2 or newer.");
            BTLogger.Info("For older versions, please use the existing RPGNodeRegistration.cs pattern.");
            #else
            BTLogger.Info("Source Generators are supported in this Unity version.");
            BTLogger.Info("Generated code should be automatically created when you compile.");
            BTLogger.Info("Check: Library/Bee/artifacts/ for generated files.");
            #endif
        }
    }
}
#endif
