using System;
using System.Collections.Generic;
using ArcBT.Logger;

namespace ArcBT.Core
{
    /// <summary>
    /// 統一された静的ノードレジストリ
    /// 全ノードタイプを単一Dictionaryで管理
    /// </summary>
    public static class BTStaticNodeRegistry
    {
        // 統一ノード生成関数の登録（全ノードタイプ対応）
        static readonly Dictionary<string, Func<BTNode>> allNodes = new();

        /// <summary>ノードを作成（統一メソッド）</summary>
        public static BTNode CreateNode(string nodeTypeString, string scriptName)
        {
            // 登録済みノードから検索
            if (allNodes.TryGetValue(scriptName, out var factory))
            {
                var node = factory.Invoke();
                if (node != null)
                {
                    node.Name = scriptName;
                    return node;
                }
            }

            BTLogger.LogSystemError("Parser", $"Failed to create node: {nodeTypeString} {scriptName}");
            return null;
        }

        /// <summary>登録済みのすべてのノード名を取得</summary>
        public static IEnumerable<string> GetAllNodeNames()
        {
            return allNodes.Keys;
        }

        /// <summary>ノードを動的に登録（統一メソッド）</summary>
        public static void RegisterNode(string scriptName, Func<BTNode> factory)
        {
            if (allNodes.ContainsKey(scriptName))
            {
                BTLogger.LogSystem("System", $"Node '{scriptName}' is already registered. Overwriting.");
            }

            allNodes[scriptName] = factory;
            BTLogger.LogSystem("NodeRegistry", $"Registered node: {scriptName}");
        }


        /// <summary>登録されているノード名</summary>
        public static IEnumerable<string> GetRegisteredNodeNames() => allNodes.Keys;
    }
}
