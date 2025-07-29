using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using ArcBT.Logger;

namespace ArcBT.Tests
{
    /// <summary>
    /// BTテストを手動実行するためのエディタークラス
    /// Unity Test Runnerが使えない場合の代替手段
    /// </summary>
    public class BTTestRunner
    {
        /// <summary>メニューから全BTファイルの簡易テストを実行</summary>
        [MenuItem("BehaviourTree/Run BT File Tests")]
        public static void RunBTFileTests()
        {
            BTLogger.LogSystem("🧪 Starting BT File Tests...");
            
            var parser = new ArcBT.Parser.BTParser();
            var btDirectory = Path.Combine(Application.dataPath, "BehaviourTrees");
            
            if (!Directory.Exists(btDirectory))
            {
                BTLogger.LogSystemError("System", $"❌ BehaviourTrees directory not found: {btDirectory}");
                return;
            }
            
            var btFiles = Directory.GetFiles(btDirectory, "*.bt");
            BTLogger.LogSystem($"📁 Found {btFiles.Length} BT files to test");
            
            var successCount = 0;
            var failCount = 0;
            var failedFiles = new List<string>();
            
            foreach (var filePath in btFiles)
            {
                var fileName = Path.GetFileName(filePath);
                BTLogger.LogSystem($"🔍 Testing: {fileName}");
                
                try
                {
                    var rootNode = parser.ParseFile(filePath);
                    
                    if (rootNode != null)
                    {
                        successCount++;
                        BTLogger.LogSystem($"✅ {fileName} - PASSED");
                        
                        // 追加情報を表示
                        LogNodeInfo(rootNode, fileName);
                    }
                    else
                    {
                        failCount++;
                        failedFiles.Add(fileName);
                        BTLogger.LogSystemError("Parser", $"❌ {fileName} - FAILED (returned null)");
                    }
                }
                catch (System.Exception ex)
                {
                    failCount++;
                    failedFiles.Add(fileName);
                    BTLogger.LogSystemError("Parser", $"❌ {fileName} - FAILED ({ex.Message})");
                }
            }
            
            // 結果サマリー
            BTLogger.LogSystem($"\n🎯 BT File Test Results:");
            BTLogger.LogSystem($"📊 Total: {btFiles.Length} files");
            BTLogger.LogSystem($"✅ Passed: {successCount}");
            BTLogger.LogSystem($"❌ Failed: {failCount}");
            
            if (failedFiles.Count > 0)
            {
                BTLogger.LogSystemError("System", $"💥 Failed files: {string.Join(", ", failedFiles)}");
            }
            else
            {
                BTLogger.LogSystem("🎉 All BT files parsed successfully!");
            }
        }
        
        /// <summary>個別ファイルテスト用メニュー</summary>
        [MenuItem("BehaviourTree/Test BlackBoard Sample")]
        public static void TestBlackBoardSample()
        {
            TestSpecificFile("blackboard_sample.bt");
        }
        
        [MenuItem("BehaviourTree/Test Team Coordination Sample")]
        public static void TestTeamCoordinationSample()
        {
            TestSpecificFile("team_coordination_sample.bt");
        }
        
        [MenuItem("BehaviourTree/Test Dynamic Condition Sample")]
        public static void TestDynamicConditionSample()
        {
            TestSpecificFile("dynamic_condition_sample.bt");
        }
        
        /// <summary>特定ファイルのテスト実行</summary>
        static void TestSpecificFile(string fileName)
        {
            BTLogger.Info($"🧪 Testing specific file: {fileName}");
            
            var parser = new ArcBT.Parser.BTParser();
            var filePath = Path.Combine(Application.dataPath, "BehaviourTrees", fileName);
            
            if (!File.Exists(filePath))
            {
                BTLogger.Error($"❌ File not found: {fileName}");
                return;
            }
            
            try
            {
                var rootNode = parser.ParseFile(filePath);
                
                if (rootNode != null)
                {
                    BTLogger.Info($"✅ {fileName} parsed successfully!");
                    LogDetailedNodeInfo(rootNode, fileName, 0);
                }
                else
                {
                    BTLogger.Error($"❌ {fileName} failed to parse (returned null)");
                }
            }
            catch (System.Exception ex)
            {
                BTLogger.Error($"❌ {fileName} failed with exception: {ex.Message}");
            }
        }
        
        /// <summary>ノード情報を簡潔にログ出力</summary>
        static void LogNodeInfo(ArcBT.Core.BTNode node, string fileName)
        {
            if (node == null) return;
            
            var totalNodes = CountNodes(node);
            BTLogger.Info($"📋 {fileName}: Root='{node.Name}', Total nodes={totalNodes}");
        }
        
        /// <summary>ノード情報を詳細にログ出力</summary>
        static void LogDetailedNodeInfo(ArcBT.Core.BTNode node, string fileName, int depth)
        {
            if (node == null) return;
            
            var indent = new string(' ', depth * 2);
            BTLogger.Info($"{indent}🔹 {node.Name} ({node.GetType().Name})");
            
            if (node.Children != null && node.Children.Count > 0)
            {
                foreach (var child in node.Children)
                {
                    LogDetailedNodeInfo(child, fileName, depth + 1);
                }
            }
        }
        
        /// <summary>ノード数を再帰的にカウント</summary>
        static int CountNodes(ArcBT.Core.BTNode node)
        {
            if (node == null) return 0;
            
            var count = 1; // 自分自身
            
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    count += CountNodes(child);
                }
            }
            
            return count;
        }
        
        /// <summary>パフォーマンステスト用メニュー</summary>
        [MenuItem("BehaviourTree/Performance Test")]
        public static void RunPerformanceTest()
        {
            BTLogger.Info("⏱️ Starting BT Performance Test...");
            
            var parser = new ArcBT.Parser.BTParser();
            var btDirectory = Path.Combine(Application.dataPath, "BehaviourTrees");
            var btFiles = Directory.GetFiles(btDirectory, "*.bt");
            
            var stopwatch = new System.Diagnostics.Stopwatch();
            
            foreach (var filePath in btFiles)
            {
                var fileName = Path.GetFileName(filePath);
                
                // 10回パースして平均時間を計測
                long totalMs = 0;
                var iterations = 10;
                
                for (var i = 0; i < iterations; i++)
                {
                    stopwatch.Restart();
                    var result = parser.ParseFile(filePath);
                    stopwatch.Stop();
                    totalMs += stopwatch.ElapsedMilliseconds;
                }
                
                var avgMs = totalMs / (double)iterations;
                BTLogger.Info($"⏱️ {fileName}: {avgMs:F2}ms average ({totalMs}ms total for {iterations} iterations)");
                
                if (avgMs > 100)
                {
                    BTLogger.Warning($"⚠️ {fileName} is slow: {avgMs:F2}ms average");
                }
            }
            
            BTLogger.Info("✅ Performance test completed");
        }
    }
}
