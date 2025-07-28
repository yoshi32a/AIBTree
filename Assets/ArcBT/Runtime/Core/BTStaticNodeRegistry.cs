using System;
using System.Collections.Generic;
using System.Linq;
using ArcBT.Decorators;
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
            // 1. 登録済みノードから検索
            if (allNodes.TryGetValue(scriptName, out var factory))
            {
                var node = factory.Invoke();
                if (node != null)
                {
                    node.Name = scriptName;
                    return node;
                }
            }

            // 2. フォールバック：組み込みノード作成
            var builtinNode = CreateBuiltinNode(nodeTypeString, scriptName);
            if (builtinNode != null)
            {
                builtinNode.Name = scriptName;
                return builtinNode;
            }

            BTLogger.LogError(LogCategory.Parser, $"Failed to create node: {nodeTypeString} {scriptName}");
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
                BTLogger.Log(Microsoft.Extensions.Logging.LogLevel.Warning, LogCategory.System, $"Node '{scriptName}' is already registered. Overwriting.");
            }

            allNodes[scriptName] = factory;
            BTLogger.LogSystem($"Registered node: {scriptName}");
        }

        /// <summary>組み込みノード作成</summary>
        static BTNode CreateBuiltinNode(string nodeType, string scriptName)
        {
            return nodeType.ToLower() switch
            {
                "sequence" => new BTSequenceNode(),
                "selector" => new BTSelectorNode(),
                "parallel" => new BTParallelNode(),
                "inverter" => new InverterDecorator(),
                "repeat" => new RepeatDecorator(),
                "retry" => new RetryDecorator(),
                "timeout" => new TimeoutDecorator(),
                _ => null
            };
        }

        /// <summary>登録されているノード名</summary>
        public static IEnumerable<string> GetRegisteredNodeNames() => allNodes.Keys;
    }
}
