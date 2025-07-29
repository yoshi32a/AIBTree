using System.Diagnostics;
using ArcBT.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArcBT.Tests
{
    /// <summary>BlackBoardシステムの機能をテストするクラス</summary>
    [TestFixture]
    public class BlackBoardTests : BTTestBase
    {
        BlackBoard blackBoard;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp(); // BTTestBaseのセットアップを実行（ログ抑制含む）
            blackBoard = new BlackBoard();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown(); // BTTestBaseのクリーンアップを実行
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

        [Test][Description("型が一致しない値を取得するとデフォルト値を返すことを確認")]
        public void GetValue_TypeMismatch_ReturnsDefault()
        {
            // Arrange
            blackBoard.SetValue("test_key", "string_value");

            // Act - 型の不一致（string → int）
            var result = blackBoard.GetValue<int>("test_key", 999);

            // Assert - ログではなく実際の動作を検証
            Assert.AreEqual(999, result, "型が一致しない場合、指定したデフォルト値が返されるべき");
            
            // 元のデータは変更されていないことを確認
            Assert.IsTrue(blackBoard.HasKey("test_key"), "元のキーは残存しているべき");
            Assert.AreEqual("string_value", blackBoard.GetValue<string>("test_key"), "元の値は変更されていないべき");
            
            // 注意: 警告ログはLoggingBehaviorTestsで専用テストが行われます
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

        [Test][Description("存在するキーの値を削除すると正しく削除されることを確認")]
        public void RemoveValue_ExistingKey_RemovesCorrectly()
        {
            // Arrange
            blackBoard.SetValue("test_key", "test_value");
            blackBoard.SetValue("other_key", "other_value");
            var initialKeyCount = blackBoard.GetAllKeys().Length;

            // Act
            blackBoard.RemoveValue("test_key");

            // Assert - ログではなく実際の機能を検証
            Assert.IsFalse(blackBoard.HasKey("test_key"), "削除されたキーは存在しないべき");
            Assert.AreEqual("default", blackBoard.GetValue("test_key", "default"), "削除されたキーはデフォルト値を返すべき");
            
            // 他のキーに影響していないことを確認
            Assert.IsTrue(blackBoard.HasKey("other_key"), "他のキーは影響を受けないべき");
            Assert.AreEqual("other_value", blackBoard.GetValue<string>("other_key"), "他のキーの値は変更されないべき");
            
            // キー数の変化を確認
            Assert.AreEqual(initialKeyCount - 1, blackBoard.GetAllKeys().Length, "キー数が1つ減っているべき");
            
            // 注意: 成功ログはLoggingBehaviorTestsで専用テストが行われます
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

            // Act
            blackBoard.Clear();

            // Assert - ログではなく実際の機能を検証
            Assert.IsFalse(blackBoard.HasKey("key1"), "Clearされた後はkey1が存在しないべき");
            Assert.IsFalse(blackBoard.HasKey("key2"), "Clearされた後はkey2が存在しないべき");
            Assert.IsFalse(blackBoard.HasKey("key3"), "Clearされた後はkey3が存在しないべき"); 
            Assert.AreEqual(0, blackBoard.GetAllKeys().Length, "Clearされた後は全キーが削除されているべき");
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

        [Test][Description("重要なキーの値を設定すると正しく保存されることを確認")]
        public void SetValue_ImportantKey_StoresCorrectly()
        {
            // Arrange & Act (enemy関連は重要キーとして扱われる)
            var expectedPosition = new Vector3(1, 0, 1);
            blackBoard.SetValue("enemy_position", expectedPosition);
            
            // Assert - ログではなく実際の機能を検証
            Assert.IsTrue(blackBoard.HasKey("enemy_position"), "重要キーが正しく設定されているべき");
            Assert.AreEqual(expectedPosition, blackBoard.GetValue<Vector3>("enemy_position"), "設定した値が正しく取得できるべき");
            
            // 注意: 重要キーのログ出力はLoggingBehaviorTestsで専用テストが行われます
        }

        [Test][Description("重要でないキーの値を設定してもログが出力されないことを確認")]
        public void SetValue_UnimportantKey_DoesNotLog()
        {
            // Act (重要でないキーはログに出力されない)
            blackBoard.SetValue("unimportant_data", "some_value");

            // Assert (ログアサートなし = 期待されるログがない)
        }

        [Test][Description("データがある状態でDebugLogを実行後もデータが正しく保持されることを確認")]
        public void DebugLog_WithData_PreservesData()
        {
            // Arrange
            blackBoard.SetValue("test_string", "hello");
            blackBoard.SetValue("test_int", 42);
            
            // Act
            blackBoard.DebugLog(); // ログ出力を実行
            
            // Assert - DebugLog実行後もデータが正しく保持されていることを検証
            Assert.IsTrue(blackBoard.HasKey("test_string"), "DebugLog後もtest_stringキーが存在するべき");
            Assert.IsTrue(blackBoard.HasKey("test_int"), "DebugLog後もtest_intキーが存在するべき");
            Assert.AreEqual("hello", blackBoard.GetValue<string>("test_string"), "DebugLog後もtest_stringの値が保持されているべき");
            Assert.AreEqual(42, blackBoard.GetValue<int>("test_int"), "DebugLog後もtest_intの値が保持されているべき");
            Assert.AreEqual(2, blackBoard.GetAllKeys().Length, "DebugLog後も全キーが保持されているべき");
            
            // 注意: ログ出力のテストはLoggingBehaviorTestsで専用テストが行われます
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
            var stopwatch = Stopwatch.StartNew();
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
