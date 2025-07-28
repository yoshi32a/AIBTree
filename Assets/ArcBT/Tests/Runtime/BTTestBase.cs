using UnityEngine;
using NUnit.Framework;
using ArcBT.Logger;

namespace ArcBT.Tests
{
    /// <summary>
    /// すべてのBehaviourTreeテストで使用する基底クラス
    /// ログ制御とテスト共通機能を提供
    /// </summary>
    public abstract class BTTestBase
    {
        [SetUp]
        public virtual void SetUp()
        {
            // テストモードを有効化（ログ抑制あり）
            BTLogger.EnableTestMode(suppressLogs: true);
            BTLogger.ClearHistory();
        }

        [TearDown]
        public virtual void TearDown()
        {
            // テストモードを無効化
            BTLogger.DisableTestMode();
        }

        /// <summary>
        /// テスト用のGameObjectを作成
        /// </summary>
        protected GameObject CreateTestGameObject(string name = "TestObject")
        {
            var gameObject = new GameObject(name);
            // テスト終了時に自動削除されるようにDontDestroyOnLoadは設定しない
            return gameObject;
        }

        /// <summary>
        /// テスト用のコンポーネント付きGameObjectを作成
        /// </summary>
        protected T CreateTestGameObjectWithComponent<T>(string name = "TestObject") where T : Component
        {
            var gameObject = CreateTestGameObject(name);
            return gameObject.AddComponent<T>();
        }

        /// <summary>
        /// ログテストが必要な場合のみログを有効化
        /// </summary>
        protected void EnableLoggingForTest()
        {
            BTLogger.EnableTestMode(suppressLogs: false);
        }

        /// <summary>
        /// テスト終了時にGameObjectを確実に削除
        /// </summary>
        protected void DestroyTestObject(GameObject obj)
        {
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
            }
        }
    }
}