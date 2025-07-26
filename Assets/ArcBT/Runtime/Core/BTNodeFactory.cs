using System;
using System.Collections.Generic;
using ArcBT.Core;

namespace ArcBT.Core
{
    /// <summary>
    /// ノード生成を高速化するファクトリークラス
    /// リフレクションの代わりにデリゲートベースの生成を使用
    /// </summary>
    public static class BTNodeFactory
    {
        // アクションノード生成デリゲート
        static readonly Dictionary<string, Func<BTActionNode>> actionFactories = new();
        
        // 条件ノード生成デリゲート
        static readonly Dictionary<string, Func<BTConditionNode>> conditionFactories = new();
        
        static bool isInitialized = false;
        
        /// <summary>ファクトリーを初期化（アプリケーション起動時に1回だけ）</summary>
        public static void Initialize()
        {
            if (isInitialized) return;
            
            // BTNodeRegistryから型情報を取得して、コンパイル済みデリゲートを作成
            foreach (var actionName in BTNodeRegistry.GetRegisteredActionNames())
            {
                var type = BTNodeRegistry.GetActionType(actionName);
                if (type != null)
                {
                    // Expression Treeを使用して高速なファクトリーメソッドを生成
                    var constructor = type.GetConstructor(Type.EmptyTypes);
                    if (constructor != null)
                    {
                        var newExpr = System.Linq.Expressions.Expression.New(constructor);
                        var lambda = System.Linq.Expressions.Expression.Lambda<Func<BTActionNode>>(newExpr);
                        var factory = lambda.Compile();
                        actionFactories[actionName] = factory;
                    }
                }
            }
            
            foreach (var conditionName in BTNodeRegistry.GetRegisteredConditionNames())
            {
                var type = BTNodeRegistry.GetConditionType(conditionName);
                if (type != null)
                {
                    var constructor = type.GetConstructor(Type.EmptyTypes);
                    if (constructor != null)
                    {
                        var newExpr = System.Linq.Expressions.Expression.New(constructor);
                        var lambda = System.Linq.Expressions.Expression.Lambda<Func<BTConditionNode>>(newExpr);
                        var factory = lambda.Compile();
                        conditionFactories[conditionName] = factory;
                    }
                }
            }
            
            isInitialized = true;
            BTLogger.LogSystem($"BTNodeFactory initialized with {actionFactories.Count} actions and {conditionFactories.Count} conditions");
        }
        
        /// <summary>高速なアクションノード生成</summary>
        public static BTActionNode CreateAction(string scriptName)
        {
            if (!isInitialized) Initialize();
            
            if (actionFactories.TryGetValue(scriptName, out var factory))
            {
                return factory();
            }
            
            // フォールバック: 従来のリフレクションベース生成
            return BTNodeRegistry.CreateAction(scriptName);
        }
        
        /// <summary>高速な条件ノード生成</summary>
        public static BTConditionNode CreateCondition(string scriptName)
        {
            if (!isInitialized) Initialize();
            
            if (conditionFactories.TryGetValue(scriptName, out var factory))
            {
                return factory();
            }
            
            // フォールバック: 従来のリフレクションベース生成
            return BTNodeRegistry.CreateCondition(scriptName);
        }
        
        /// <summary>パフォーマンス統計をリセット</summary>
        public static void ResetStatistics()
        {
            // 将来的にパフォーマンス計測を追加する場合用
        }
    }
}