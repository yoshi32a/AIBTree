using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ArcBT.Core;
using ArcBT.Logger;

namespace ArcBT.Tests
{
    /// <summary>BlackBoardシステムの機能をテストするクラス</summary>
    [TestFixture]
    public class BlackBoardTests
    {
        BlackBoard blackBoard;

        [SetUp]
        public void SetUp()
        {
            blackBoard = new BlackBoard();
            BTLogger.EnableTestMode(); // テストモードを有効化
        }

        [TearDown]
        public void TearDown()
        {
            BTLogger.ResetToDefaults();
        }

        [Test][Description("新しいキーで値を設定すると正しく保存され取得できることを確認")]
        public void SetValue_NewKey_StoresValue()
        {
            // Arrange & Act
            blackBoard.SetValue("test_key", "test_value");

            // Assert
            Assert.IsTrue(blackBoard.HasKey("test_key"));
            Assert.AreEqual("test_value", blackBoard.GetValue<string>("test_key"));
        }

        [Test][Description("既存のキーに新しい値を設定すると値が更新されることを確認")]
        public void SetValue_ExistingKey_UpdatesValue()
        {
            // Arrange
            blackBoard.SetValue("test_key", "old_value");

            // Act
            blackBoard.SetValue("test_key", "new_value");

            // Assert
            Assert.AreEqual("new_value", blackBoard.GetValue<string>("test_key"));
        }

        [Test][Description("存在しないキーの値を取得するとデフォルト値が返されることを確認")]
        public void GetValue_NonExistentKey_ReturnsDefaultValue()
        {
            // Act & Assert
            Assert.AreEqual("default", blackBoard.GetValue("non_existent", "default"));
            Assert.AreEqual(0, blackBoard.GetValue<int>("non_existent"));
            Assert.AreEqual(false, blackBoard.GetValue<bool>("non_existent"));
        }

        [Test][Description("型が一致しない値を取得するとデフォルト値を返し警告をログ出力することを確認")]
        public void GetValue_TypeMismatch_ReturnsDefaultAndLogsWarning()
        {
            // Arrange
            blackBoard.SetValue("test_key", "string_value");
            LogAssert.Expect(LogType.Warning, 
                "[WRN][BBD]: 🗂️ BlackBoard: Type mismatch for key 'test_key'. Expected Int32, got String");

            // Act
            var result = blackBoard.GetValue<int>("test_key", 999);

            // Assert
            Assert.AreEqual(999, result);
        }

        [Test][Description("存在するキーに対してHasKeyがtrueを返すことを確認")]
        public void HasKey_ExistingKey_ReturnsTrue()
        {
            // Arrange
            blackBoard.SetValue("existing_key", 42);

            // Act & Assert
            Assert.IsTrue(blackBoard.HasKey("existing_key"));
        }

        [Test][Description("存在しないキーに対してHasKeyがfalseを返すことを確認")]
        public void HasKey_NonExistentKey_ReturnsFalse()
        {
            // Act & Assert
            Assert.IsFalse(blackBoard.HasKey("non_existent_key"));
        }

        [Test][Description("存在するキーの値を削除すると正しく削除され成功ログが出力されることを確認")]
        public void RemoveValue_ExistingKey_RemovesAndLogsSuccess()
        {
            // Arrange
            blackBoard.SetValue("test_key", "test_value");
            LogAssert.Expect(LogType.Log, "[INF][BBD]: 🗂️ BlackBoard: Removed 'test_key'");

            // Act
            blackBoard.RemoveValue("test_key");

            // Assert
            Assert.IsFalse(blackBoard.HasKey("test_key"));
            Assert.AreEqual("default", blackBoard.GetValue("test_key", "default"));
        }

        [Test][Description("存在しないキーの値を削除してもログが出力されないことを確認")]
        public void RemoveValue_NonExistentKey_DoesNotLog()
        {
            // Act (非存在キーの削除はログを出力しない)
            blackBoard.RemoveValue("non_existent_key");

            // Assert (ログアサートなし = 期待されるログがない)
        }

        [Test][Description("データが存在する状態でClearを実行すると全データが削除されることを確認")]
        public void Clear_WithData_RemovesAllData()
        {
            // Arrange
            blackBoard.SetValue("key1", "value1");
            blackBoard.SetValue("key2", 42);
            blackBoard.SetValue("key3", true);
            LogAssert.Expect(LogType.Log, "[INF][BBD]: 🗂️ BlackBoard: Cleared all data");

            // Act
            blackBoard.Clear();

            // Assert
            Assert.IsFalse(blackBoard.HasKey("key1"));
            Assert.IsFalse(blackBoard.HasKey("key2"));
            Assert.IsFalse(blackBoard.HasKey("key3"));
            Assert.AreEqual(0, blackBoard.GetAllKeys().Length);
        }

        [Test][Description("異なる型の値を設定すると各型が正しく保存・取得できることを確認")]
        public void SetValue_DifferentTypes_StoresCorrectTypes()
        {
            // Arrange & Act
            blackBoard.SetValue("string_val", "hello");
            blackBoard.SetValue("int_val", 42);
            blackBoard.SetValue("float_val", 3.14f);
            blackBoard.SetValue("bool_val", true);
            blackBoard.SetValue("vector_val", new Vector3(1, 2, 3));

            // Assert
            Assert.AreEqual("hello", blackBoard.GetValue<string>("string_val"));
            Assert.AreEqual(42, blackBoard.GetValue<int>("int_val"));
            Assert.AreEqual(3.14f, blackBoard.GetValue<float>("float_val"));
            Assert.AreEqual(true, blackBoard.GetValue<bool>("bool_val"));
            Assert.AreEqual(new Vector3(1, 2, 3), blackBoard.GetValue<Vector3>("vector_val"));
        }

        [Test][Description("存在するキーの型情報を取得すると正しい型が返されることを確認")]
        public void GetValueType_ExistingKey_ReturnsCorrectType()
        {
            // Arrange
            blackBoard.SetValue("string_key", "value");
            blackBoard.SetValue("int_key", 42);

            // Act & Assert
            Assert.AreEqual(typeof(string), blackBoard.GetValueType("string_key"));
            Assert.AreEqual(typeof(int), blackBoard.GetValueType("int_key"));
        }

        [Test][Description("存在しないキーの型情報を取得するとnullが返されることを確認")]
        public void GetValueType_NonExistentKey_ReturnsNull()
        {
            // Act & Assert
            Assert.IsNull(blackBoard.GetValueType("non_existent"));
        }

        [Test][Description("null値を設定すると正しく保存され取得できることを確認")]
        public void SetValue_NullValue_StoresAndRetrievesCorrectly()
        {
            // Arrange & Act
            blackBoard.SetValue<string>("null_key", null);

            // Assert
            Assert.IsTrue(blackBoard.HasKey("null_key"));
            Assert.IsNull(blackBoard.GetValue<string>("null_key"));
        }

        [Test][Description("nullから値への変更が正しく変更追跡されることを確認")]
        public void SetValue_NullToValue_TriggersChange()
        {
            // Arrange
            blackBoard.SetValue<string>("test_key", null);

            // Act (null → 値への変更)
            blackBoard.SetValue("test_key", "new_value");

            // Assert
            Assert.AreEqual("new_value", blackBoard.GetValue<string>("test_key"));
        }

        [Test][Description("値からnullへの変更が正しく変更追跡されることを確認")]
        public void SetValue_ValueToNull_TriggersChange()
        {
            // Arrange
            blackBoard.SetValue("test_key", "old_value");

            // Act (値 → nullへの変更)
            blackBoard.SetValue<string>("test_key", null);

            // Assert
            Assert.IsNull(blackBoard.GetValue<string>("test_key"));
        }

        [Test][Description("nullからnullへの変更は変更追跡されないことを確認")]
        public void SetValue_NullToNull_NoChange()
        {
            // Arrange
            blackBoard.SetValue<string>("test_key", null);
            // 最初の変更をクリア
            blackBoard.GetRecentChangeSummary();

            // Act (null → nullへの変更なし)
            blackBoard.SetValue<string>("test_key", null);

            // Assert (変更追跡に含まれない)
            Assert.IsFalse(blackBoard.HasRecentChanges());
        }

        [Test][Description("複数のキーが存在する状態でGetAllKeysが全てのキーを返すことを確認")]
        public void GetAllKeys_WithMultipleKeys_ReturnsAllKeys()
        {
            // Arrange
            blackBoard.SetValue("key1", "value1");
            blackBoard.SetValue("key2", 42);
            blackBoard.SetValue("key3", true);

            // Act
            var keys = blackBoard.GetAllKeys();

            // Assert
            Assert.AreEqual(3, keys.Length);
            Assert.Contains("key1", keys);
            Assert.Contains("key2", keys);
            Assert.Contains("key3", keys);
        }

        [Test][Description("空のBlackBoardでGetAllKeysが空の配列を返すことを確認")]
        public void GetAllKeys_EmptyBlackBoard_ReturnsEmptyArray()
        {
            // Act
            var keys = blackBoard.GetAllKeys();

            // Assert
            Assert.AreEqual(0, keys.Length);
        }

        [Test][Description("最近の変更がある場合HasRecentChangesがtrueを返すことを確認")]
        public void HasRecentChanges_WithRecentChange_ReturnsTrue()
        {
            // Act
            blackBoard.SetValue("enemy_position", new Vector3(1, 0, 1));

            // Assert
            Assert.IsTrue(blackBoard.HasRecentChanges());
        }

        [Test][Description("変更がない場合HasRecentChangesがfalseを返すことを確認")]
        public void HasRecentChanges_NoChanges_ReturnsFalse()
        {
            // Assert
            Assert.IsFalse(blackBoard.HasRecentChanges());
        }

        [Test][Description("変更がある場合GetRecentChangeSummaryがフォーマットされたサマリーを返すことを確認")]
        public void GetRecentChangeSummary_WithChanges_ReturnsFormattedSummary()
        {
            // Arrange & Act
            blackBoard.SetValue("key1", "value1");
            blackBoard.SetValue("key2", 42);

            // Act
            var summary = blackBoard.GetRecentChangeSummary();

            // Assert
            Assert.IsTrue(summary.Contains("key1=value1"));
            Assert.IsTrue(summary.Contains("key2=42"));
        }

        [Test][Description("変更がない場合GetRecentChangeSummaryが変更なしメッセージを返すことを確認")]
        public void GetRecentChangeSummary_NoChanges_ReturnsNoChangeMessage()
        {
            // Act
            var summary = blackBoard.GetRecentChangeSummary();

            // Assert
            Assert.AreEqual("変更なし", summary);
        }

        [Test][Description("GetRecentChangeSummaryを2回呼び出すと1回目の後に変更履歴がクリアされることを確認")]
        public void GetRecentChangeSummary_CalledTwice_ClearsAfterFirst()
        {
            // Arrange
            blackBoard.SetValue("test_key", "test_value");

            // Act
            var firstSummary = blackBoard.GetRecentChangeSummary();
            var secondSummary = blackBoard.GetRecentChangeSummary();

            // Assert
            Assert.IsTrue(firstSummary.Contains("test_key=test_value"));
            Assert.AreEqual("変更なし", secondSummary);
        }

        [Test][Description("文字列値をGetValueAsStringで取得すると文字列として返されることを確認")]
        public void GetValueAsString_StringValue_ReturnsString()
        {
            // Arrange
            blackBoard.SetValue("test_key", "hello world");

            // Act
            var result = blackBoard.GetValueAsString("test_key");

            // Assert
            Assert.AreEqual("hello world", result);
        }

        [Test][Description("GameObjectをGetValueAsStringで取得するとオブジェクト名が返されることを確認")]
        public void GetValueAsString_GameObject_ReturnsName()
        {
            // Arrange
            var gameObject = new GameObject("TestObject");
            blackBoard.SetValue("game_object", gameObject);

            // Act
            var result = blackBoard.GetValueAsString("game_object");

            // Assert
            Assert.AreEqual("TestObject", result);

            // Cleanup
            Object.DestroyImmediate(gameObject);
        }

        [Test][Description("Vector3をGetValueAsStringで取得するとフォーマットされた座標が返されることを確認")]
        public void GetValueAsString_Vector3_ReturnsFormattedCoordinates()
        {
            // Arrange
            blackBoard.SetValue("position", new Vector3(1.234f, 2.567f, 3.891f));

            // Act
            var result = blackBoard.GetValueAsString("position");

            // Assert
            Assert.AreEqual("(1.2, 2.6, 3.9)", result);
        }

        [Test][Description("float値をGetValueAsStringで取得するとフォーマットされた数値が返されることを確認")]
        public void GetValueAsString_Float_ReturnsFormattedFloat()
        {
            // Arrange
            blackBoard.SetValue("health", 85.6789f);

            // Act
            var result = blackBoard.GetValueAsString("health");

            // Assert
            Assert.AreEqual("85.7", result);
        }

        [Test][Description("null値をGetValueAsStringで取得するとnull文字列が返されることを確認")]
        public void GetValueAsString_NullValue_ReturnsNull()
        {
            // Arrange
            blackBoard.SetValue<GameObject>("null_object", null);

            // Act
            var result = blackBoard.GetValueAsString("null_object");

            // Assert
            Assert.AreEqual("null", result);
        }

        [Test][Description("存在しないキーをGetValueAsStringで取得すると未設定メッセージが返されることを確認")]
        public void GetValueAsString_NonExistentKey_ReturnsNotSet()
        {
            // Act
            var result = blackBoard.GetValueAsString("non_existent");

            // Assert
            Assert.AreEqual("未設定", result);
        }

        [Test][Description("重要なキーの値を設定すると変更がログ出力されることを確認")]
        public void SetValue_ImportantKey_LogsChange()
        {
            // Arrange & Act (enemy関連は重要キーとしてログ出力される)
            LogAssert.Expect(LogType.Log, "[INF][BBD]: 🗂️ BlackBoard[新規]: enemy_position = (1.00, 0.00, 1.00)");
            blackBoard.SetValue("enemy_position", new Vector3(1, 0, 1));
        }

        [Test][Description("重要でないキーの値を設定してもログが出力されないことを確認")]
        public void SetValue_UnimportantKey_DoesNotLog()
        {
            // Act (重要でないキーはログに出力されない)
            blackBoard.SetValue("unimportant_data", "some_value");

            // Assert (ログアサートなし = 期待されるログがない)
        }

        [Test][Description("データがある状態でDebugLogを実行すると全ての内容がログ出力されることを確認")]
        public void DebugLog_WithData_LogsAllContents()
        {
            // Arrange
            blackBoard.SetValue("test_string", "hello");
            blackBoard.SetValue("test_int", 42);
            
            LogAssert.Expect(LogType.Log, "[INF][BBD]: 🗂️ BlackBoard Contents:");
            LogAssert.Expect(LogType.Log, "[INF][BBD]:   - test_string: hello (String)");
            LogAssert.Expect(LogType.Log, "[INF][BBD]:   - test_int: 42 (Int32)");

            // Act
            blackBoard.DebugLog();
        }

        [Test][Description("複雑なAIシナリオでBlackBoardの全機能が正しく動作することを確認する統合テスト")]
        public void IntegrationTest_ComplexScenario_WorksCorrectly()
        {
            // Arrange: 複雑なAI状況をシミュレート
            var enemyObject = new GameObject("Enemy");
            var playerObject = new GameObject("Player");

            // Act: AI状態を設定
            blackBoard.SetValue("current_state", "combat");
            blackBoard.SetValue("enemy_target", enemyObject);
            blackBoard.SetValue("enemy_position", new Vector3(5, 0, 3));
            blackBoard.SetValue("player_health", 75.5f);
            blackBoard.SetValue("is_initialized", true);
            blackBoard.SetValue("combat_distance", 8.2f);

            // Assert: 全てのデータが正しく保存・取得できる
            Assert.AreEqual("combat", blackBoard.GetValue<string>("current_state"));
            Assert.AreEqual(enemyObject, blackBoard.GetValue<GameObject>("enemy_target"));
            Assert.AreEqual(new Vector3(5, 0, 3), blackBoard.GetValue<Vector3>("enemy_position"));
            Assert.AreEqual(75.5f, blackBoard.GetValue<float>("player_health"));
            Assert.AreEqual(true, blackBoard.GetValue<bool>("is_initialized"));
            Assert.AreEqual(8.2f, blackBoard.GetValue<float>("combat_distance"));

            // 型情報も正しく保存されている
            Assert.AreEqual(typeof(string), blackBoard.GetValueType("current_state"));
            Assert.AreEqual(typeof(GameObject), blackBoard.GetValueType("enemy_target"));
            Assert.AreEqual(typeof(Vector3), blackBoard.GetValueType("enemy_position"));

            // UI表示用の文字列変換も動作
            Assert.AreEqual("combat", blackBoard.GetValueAsString("current_state"));
            Assert.AreEqual("Enemy", blackBoard.GetValueAsString("enemy_target"));
            Assert.AreEqual("(5.0, 0.0, 3.0)", blackBoard.GetValueAsString("enemy_position"));

            // 変更追跡も動作
            Assert.IsTrue(blackBoard.HasRecentChanges());

            // Cleanup
            Object.DestroyImmediate(enemyObject);
            Object.DestroyImmediate(playerObject);
        }

        [Test][Description("大量の操作を実行しても十分な性能で完了することを確認する性能テスト")]
        public void PerformanceTest_ManyOperations_CompletesQuickly()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            const int operationCount = 1000;

            // Act: 大量の操作を実行
            for (int i = 0; i < operationCount; i++)
            {
                blackBoard.SetValue($"key_{i}", $"value_{i}");
                blackBoard.GetValue<string>($"key_{i}");
                blackBoard.HasKey($"key_{i}");
            }

            stopwatch.Stop();

            // Assert: 適切な時間内で完了する（1000操作が100ms以内）
            Assert.Less(stopwatch.ElapsedMilliseconds, 100, 
                $"性能テスト失敗: {operationCount}操作に{stopwatch.ElapsedMilliseconds}ms掛かりました");

            // データ検証
            Assert.AreEqual(operationCount, blackBoard.GetAllKeys().Length);
            Assert.AreEqual("value_999", blackBoard.GetValue<string>("key_999"));
        }
    }
}
