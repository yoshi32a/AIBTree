# CLAUDE.md

このファイルは、Claude Code (claude.ai/code) がこのリポジトリのコードを扱う際のガイダンスを提供します。

## 言語設定
- **すべてのやり取りは日本語で行う**
- コメントやドキュメントも日本語で記述する
- エラーメッセージの説明も日本語で提供する
- コードの説明や質問への回答は必ず日本語で行う

## プロジェクト概要

これは **Universal Render Pipeline (URP)** を使用した Unity 6000.1.10f1 プロジェクト「AIBTree」です。**BehaviourTree AI システム**の実装が主な目的で、.btファイル形式でのAI定義とVSCode統合を提供します。

**重要**: このプロジェクトはURPグラフィック環境を使用しているため、すべてのシェーダーとマテリアル設定はURP用である必要があります。

### 主要機能
- **BehaviourTree実行エンジン**: C#による階層的なノード構造の実装
- **BlackBoardシステム**: AIノード間でのデータ共有とリアルタイム状態管理
- **動的条件チェック**: Action実行中の条件変化に即座に対応
- **.btファイル形式**: 人間が読みやすい階層形式でのAI定義
- **VSCode拡張機能**: シンタックスハイライト、自動補完、診断機能
- **動的スクリプトローダー**: .btファイルからC#クラスへの動的マッピング
- **視覚的フィードバックシステム**: リアルタイムAI状態表示、3Dアクションインジケーター、高度なカメラ制御

## Unity 開発ワークフロー

Unity プロジェクトのため、従来のコマンドライン プロジェクトとは異なる開発ワークフローを使用します：

### プロジェクトを開く
- Unity Hub を開き、このプロジェクトフォルダを追加する
- Unity が自動的にすべてのアセットをコンパイルし、インポートする
- メインシーンは `Assets/Scenes/SampleScene.unity` にあります

### プロジェクトをビルドする
Unity プロジェクトは Unity エディタを通じてビルドされます：
1. **File > Build Settings** に移動
2. ターゲットプラットフォームを選択（デフォルトは PC, Mac, Linux Standalone）
3. **Build** または **Build and Run** をクリック

### テストフレームワーク
プロジェクトには Unity Test Framework と包括的なBTテストスイートが含まれています：
- **Window > General > Test Runner** からアクセス
- `Assets/Tests/` にテストスイート実装済み
- **BTParsingTests.cs**: 全.btファイルのパーステスト
- **BTFileValidationTests.cs**: ファイル構造の詳細検証
- **BTTestRunner.cs**: エディターメニュー統合（BehaviourTree → Run BT File Tests）
- ユニットテストには `[Test]` 属性、コルーチンベースのテストには `[UnityTest]` 属性を使用

## プロジェクト構造

### 主要なディレクトリ
- `Assets/` - すべてのプロジェクトアセット、スクリプト、シーン、リソース
- `Assets/Scenes/` - Unity シーンファイル
- `Assets/Settings/` - URP レンダーパイプラインとボリュームプロファイル設定
- `Assets/TutorialInfo/` - テンプレートチュートリアルファイル（本番環境では削除可能）
- `Packages/` - Package Manager 依存関係
- `ProjectSettings/` - Unity プロジェクト設定ファイル
- `Library/` - Unity のコンパイル済みアセットとキャッシュ（自動生成、バージョン管理対象外）

### BehaviourTree システム構造
- `Assets/Scripts/BehaviourTree/Core/` - コアシステム
  - `BTNode.cs` - ベースノードクラス（BlackBoard対応）
  - `BTNodeResult.cs` - 実行結果の列挙型
  - `BTActionNode.cs` / `BTConditionNode.cs` - 抽象ベースクラス（動的条件チェック対応）
  - `BTCompositeNode.cs` - 複合ノードの基底クラス
  - `BTSequenceNode.cs` - Sequenceノード（順次実行）
  - `BTSelectorNode.cs` - Selectorノード（選択実行）
  - `BTParallelNode.cs` - Parallelノード（並列実行）
  - `BlackBoard.cs` - データ共有システム
  - `BehaviourTreeRunner.cs` - .btファイル実行エンジン（BlackBoard管理）
  - `BTLogger.cs` - 統合ログシステム（条件付きコンパイル対応）
- `Assets/Scripts/BehaviourTree/Parser/` - .btファイルパーサー
  - `BTParser.cs` - トークンベースパーサー
- `Assets/Scripts/BehaviourTree/Actions/` - アクションノード実装
  - `MoveToPositionAction.cs` - 移動アクション（BlackBoard対応）
  - `AttackEnemyAction.cs` - 攻撃アクション
  - `WaitAction.cs` - 待機アクション
  - `ScanEnvironmentAction.cs` - 環境スキャン（BlackBoard連携）
  - `MoveToEnemyAction.cs` - 敵への移動（BlackBoard参照）
  - `AttackTargetAction.cs` - ターゲット攻撃（BlackBoard連携）
  - `RandomWanderAction.cs` - ランダム徘徊
- `Assets/Scripts/BehaviourTree/Conditions/` - 条件ノード実装
  - `HealthCheckCondition.cs` - 体力チェック条件（BlackBoard対応）
  - `EnemyCheckCondition.cs` - 敵検出条件
  - `HasItemCondition.cs` - アイテム所持チェック
  - `HasSharedEnemyInfoCondition.cs` - BlackBoard敵情報チェック
- `Assets/Scripts/BehaviourTree/Nodes/` - レガシーノード（後方互換性）
  - `CustomActionNode.cs` / `CustomConditionNode.cs`
- `Assets/Scripts/Components/` - 汎用コンポーネント
  - `Health.cs` - 体力管理コンポーネント
  - `Inventory.cs` - インベントリ管理コンポーネント（GetAllItemsメソッド対応）
  - `InventoryItem.cs` - インベントリアイテム定義
  - `Mana.cs` - マナ管理コンポーネント
- `Assets/Scripts/UI/` - 視覚的フィードバックシステム
  - `AIStatusDisplay.cs` - リアルタイムAI状態表示UI（体力/マナバー、現在アクション、BlackBoard情報）
  - `ActionIndicator.cs` - 3Dアクションインジケーター（AIの頭上に浮かぶ状態表示）
- `Assets/Scripts/Camera/` - カメラ制御システム
  - `SceneCamera.cs` - 高度なカメラ制御（自動追従、手動操作、Input System対応）
- `Assets/Editor/` - Unity エディター拡張
  - `BTLoggerEditor.cs` - ログシステム管理UI（カテゴリフィルター、ログレベル制御）
- `Assets/Tests/` - テストスイート
  - `BTParsingTests.cs` - 全BTファイルパーステスト
  - `BTFileValidationTests.cs` - ファイル構造詳細検証
  - `BTTestRunner.cs` - エディターメニュー統合
  - `BTLoggerTests.cs` - ログシステム基本機能テスト
  - `BTLoggerPerformanceTests.cs` - ログシステムパフォーマンステスト
  - `Tests.asmdef` - テスト用アセンブリ定義
- `Assets/BehaviourTrees/` - .btファイルとサンプル
  - `blackboard_sample.bt` - BlackBoard基本使用例
  - `team_coordination_sample.bt` - チーム連携例
  - `resource_management_sample.bt` - リソース管理例
  - `dynamic_condition_sample.bt` - 動的条件チェック例

### VSCode拡張機能
- `vscode-bt-extension/` - VSCode拡張機能
  - `src/extension.js` - メイン拡張機能コード
  - `syntaxes/bt.tmLanguage.json` - シンタックスハイライト定義
  - `snippets/bt.json` - コードスニペット
  - `package.json` - 拡張機能設定

## パッケージ依存関係

含まれる主要なパッケージ：
- **com.unity.render-pipelines.universal** (17.1.0) - URP レンダリング
- **com.unity.inputsystem** (1.14.0) - 新しい Input System
- **com.unity.ai.navigation** (2.0.8) - AI ナビゲーション/NavMesh
- **com.unity.test-framework** (1.5.1) - ユニットテストフレームワーク
- **com.unity.timeline** (1.8.7) - シネマティクス用タイムラインシステム

## BehaviourTree開発ガイドライン

### .btファイル作成
- **場所**: `Assets/BehaviourTrees/` フォルダに配置
- **拡張子**: `.bt`
- **形式**: 階層型記述（例：`tree TreeName { Sequence root { ... } }`）
- **正しい形式**: 大文字始まり、script属性不要
- **リファレンス**: `BT_REFERENCE.md` を参照

### 新しいActionノード作成（BlackBoard対応）
1. `Assets/Scripts/BehaviourTree/Actions/` に `[ActionName]Action.cs` を作成（1ファイル1クラス）
2. `BTActionNode` を継承
3. `ExecuteAction()` メソッドをオーバーライド
4. `SetProperty(string key, string value)` メソッドをオーバーライド（パラメータは`string`型）
5. `Initialize(MonoBehaviour, BlackBoard)` メソッドをオーバーライド
6. BlackBoardを活用して状態管理・データ共有を実装
7. `OnConditionFailed()` で動的条件失敗時の処理を実装
8. .btファイルで `Action ActionName { ... }` として使用（script属性不要）
9. `BTParser.cs` の `CreateNodeFromScript()` にケースを追加

### 新しいConditionノード作成（BlackBoard対応）
1. `Assets/Scripts/BehaviourTree/Conditions/` に `[ConditionName]Condition.cs` を作成（1ファイル1クラス）
2. `BTConditionNode` を継承
3. `protected override BTNodeResult CheckCondition()` メソッドをオーバーライド（`protected`必須、戻り値は`BTNodeResult`）
4. `SetProperty(string key, string value)` メソッドをオーバーライド（パラメータは`string`型）
5. `Initialize(MonoBehaviour, BlackBoard)` メソッドをオーバーライド
6. BlackBoardに状態を記録してデータ共有を実現
7. .btファイルで `Condition ConditionName { ... }` として使用（script属性不要）
8. `BTParser.cs` の `CreateNodeFromScript()` にケースを追加

### テストとデバッグ
- **自動テスト実行**:
  - Unity Test Runner: `Window > General > Test Runner`
  - エディターメニュー: `BehaviourTree > Run BT File Tests`
  - 個別ファイルテスト: `BehaviourTree > Test [FileName] Sample`
  - パフォーマンステスト: `BehaviourTree > Performance Test`
- **テストシーン設定**: `UNITY_TEST_SETUP.md` を参照（BlackBoard対応版）
- **ログ確認**: BTLoggerシステムによる構造化ログ出力をConsoleウィンドウで確認
- **ログレベル制御**: `Window > BehaviourTree > Logger Settings` でカテゴリ別フィルタリング
- **体力テスト**: Healthコンポーネントの右クリックメニューを使用
- **BlackBoardデバッグ**: BehaviourTreeRunnerの右クリック → "Show BlackBoard Contents"
- **ツリー状態リセット**: BehaviourTreeRunnerの右クリック → "Reset Tree State"
- **動的条件チェック確認**: 実行中の条件変化でActionが即座に中断されることを確認

## BTLoggerシステム

### 概要
BTLoggerは統合ログシステムで、Debug.Logの性能問題を解決し、構造化されたログ出力を提供します。

### 主要機能
- **条件付きコンパイル**: 本番ビルドで完全にログ処理を除去
- **カテゴリベースフィルタリング**: Combat, Movement, Parser等のカテゴリ別制御
- **ログレベル制御**: Error, Warning, Info, Debug, Traceの5段階
- **パフォーマンス最適化**: 高負荷ログ処理でも25.8ms以内の高速処理

### 使用方法
```csharp
// 基本ログ出力
BTLogger.LogSystem("システムメッセージ");
BTLogger.LogCombat("戦闘ログ", nodeName, context);

// エラー・警告
BTLogger.LogError(LogCategory.Parser, "エラーメッセージ", nodeName, context);
BTLogger.Log(LogLevel.Warning, LogCategory.System, "警告メッセージ");

// Debug.Logからの移行用
BTLogger.Info("情報メッセージ");
BTLogger.Warning("警告メッセージ");
BTLogger.Error("エラーメッセージ");
```

### エディター設定
- `Window > BehaviourTree > Logger Settings` でリアルタイム制御
- カテゴリごとの有効/無効切り替え
- ログレベルの動的変更
- ログ履歴の確認

### 出力フォーマット
```
[INF][SYS]: システム情報メッセージ
[ERR][PRS]: パーサーエラーメッセージ
[WRN][MOV]: 移動関連警告
[DBG][ATK][NodeName]: 戦闘デバッグ情報
```

### Unity 6000.1.10f1 対応
- `Object.FindFirstObjectByType<T>()` を使用（`FindObjectOfType` は非推奨）
- MonoBehaviour継承クラスでBTNodeを実装

### VSCode拡張機能
- **インストール**: `vsce package` → `code --install-extension *.vsix`
- **最新版**: v1.1.0（BlackBoard・Parallel対応）
- **設定**: `.vscode/settings.json` で診断機能の有効/無効を制御
- **機能**: 
  - シンタックスハイライト（新形式対応）
  - 自動補完（BlackBoard対応スニペット）
  - ホバーヘルプ
  - スニペット（`tree`, `sequence`, `action`, `condition`, `parallel`, `blackboard`）

### バージョン管理に関する注意
- `Library/`、`Temp/`、`Logs/`、`obj/` フォルダは .gitignore で無視
- `.meta` ファイルは Unity にとって重要で、バージョン管理の対象
- `vscode-bt-extension/*.vsix` ファイルは配布用（バージョン管理対象外推奨）

## Git コミット規約

### Claude Code でのコミットメッセージ
- **言語**: **日本語**でコミットメッセージを作成する
- **形式**: `feat: BTLoggerシステムを実装` （日本語での説明）
- **例**:
  ```
  機能追加: BTLoggerによる統合ログシステムを実装
  
  - Debug.Logの性能問題を解決
  - カテゴリ別フィルタリング機能を追加
  - 条件付きコンパイルで本番最適化
  - 全テストが通過
  
  🤖 Claude Code で生成
  ```

## コーディング規約

### .editorconfig準拠
このプロジェクトでは `.editorconfig` に従ったC#コーディングスタイルを使用します：

**重要な規約:**
- `private` 修飾子は暗黙的（明示しない）
- `protected` 修飾子は**必須**（継承に必要なため削除禁止）
- `public` 修飾子は明示的に記述
- 単一ステートメントでも波括弧を必須
- varの使用を推奨
- **1ファイル1クラスの原則**を厳守

**例:**
```csharp
public class ExampleClass : BaseClass
{
    // ❌ private string name; 
    string name; // ✅ privateは暗黙的

    // ✅ protectedは継承に必要なので必須
    protected virtual void DoSomething() 
    {
        // 単一ステートメントでも波括弧必須
        if (condition)
        {
            ExecuteAction();
        }
    }
    
    // ✅ publicは明示的
    public override void Initialize() { }
}
```

## 重要なファイル
- `BT_REFERENCE.md` - .btファイル形式の完全リファレンス
- `UNITY_TEST_SETUP.md` - Unityテストシーンの設定方法（BlackBoard対応版）
- `README.md` - プロジェクト概要とBlackBoard機能説明
- `AI_INSTRUCTIONS.md` - コンパイル状況とエラーログ
- `.editorconfig` - C#コーディング規約設定

## 実装済みBlackBoard対応スクリプト一覧

### Action Scripts
- `ScanEnvironmentAction` - 環境スキャンして敵情報をBlackBoardに保存
- `MoveToEnemyAction` - BlackBoardから敵位置を取得して移動
- `AttackTargetAction` - BlackBoardの敵情報を使用して攻撃
- `RandomWanderAction` - ランダムに徘徊するアクション

### Condition Scripts  
- `HasSharedEnemyInfoCondition` - BlackBoardに共有された敵情報があるかチェック

### 最新の修正と改善

#### 視覚的フィードバックシステム（2024年最新）
- **AIStatusDisplay.cs**: リアルタイム状態表示UI（画面左上に体力/マナバー、現在アクション、ターゲット情報、BlackBoard状況を表示）
- **ActionIndicator.cs**: 3Dアクションインジケーター（AIの頭上に浮かぶアイコン付きアクション表示、ターゲットライン表示）
- **SceneCamera.cs**: 高度なカメラ制御（F=フォロー切替、R=リセット、WASD移動、マウス回転、Input System対応）

#### 無限ループ問題の修正
- **FleeToSafetyAction**: 安全地帯到達後の10秒間安全期間設定
- **HealthCheckCondition**: 安全期間中の緊急時判定スキップ機能
- **BehaviourTreeRunner**: スマートログシステム（同パターン繰り返し抑制、5秒間隔制限）

#### Unity 6対応
- `FindObjectOfType<T>()` → `FindFirstObjectByType<T>()` に統一（非推奨警告解消）
- Input System対応（UnityEngine.Input → UnityEngine.InputSystem）
- LineRenderer.color → startColor/endColor に変更

#### URP グラフィック環境対応
- **重要**: このプロジェクトは **Universal Render Pipeline (URP)** を使用
- **ActionIndicator.cs**: `Shader.Find("Standard")` → `Shader.Find("Universal Render Pipeline/Lit")` に変更
- **透明度設定**: Built-inパイプライン用（`_Mode`, キーワード設定）→ URP用（`_Surface`, `_Blend`）に変更
- **影響範囲**: 背景クアッド、ターゲットライン表示の正常化
- **注意点**: 新しいマテリアル作成時は必ずURP対応シェーダーを使用すること

#### コンパイルエラー対応
- すべてのスクリプトで `SetProperty(string, string)` 形式に統一
- `CheckCondition()` は `protected override BTNodeResult` 形式に統一
- `owner` → `ownerComponent`, `owner.transform` → `transform` に統一
- `debugMode` 参照を削除してログ直接出力に変更
- `Components.Health` → `Health` に修正（namespace修正）
- `BlackBoard.GetValueAsString()` メソッド追加（UI表示用）
- `Inventory.GetAllItems()` メソッド追加（UI表示用）

## Memory Log

### Claude Code との対話で学んだこと
- **視覚的フィードバック問題**: ユーザーから「ゲーム画面で現象が起きないとわからない」という要求があり、包括的な視覚的フィードバックシステムを実装
- **ログ激しい問題**: 無限ループによる大量のログ出力問題を、安全期間機能とスマートログシステムで解決
- **Unity 6互換性**: 非推奨API（FindObjectOfType、Input、LineRenderer.color）を最新版に更新

## 視覚的フィードバックシステムの使用方法

### テスト環境の作成
```
Unity → BehaviourTree → Quick Setup → Complex Example Test Environment
```

### 視覚的要素
1. **左上UI**: 体力/マナバー、現在アクション、ターゲット、状態表示
2. **AI頭上**: アイコン付きアクション表示（🚶移動、⚔️攻撃、🏃逃走、✨魔法等）
3. **ターゲットライン**: AIから敵への青いライン
4. **右側パネル**: BlackBoard詳細情報

### カメラ操作
- **F キー**: 自動追従 ↔ 手動操作切り替え
- **R キー**: カメラリセット
- **WASD**: カメラ移動（手動モード時）
- **QE**: 上下移動
- **右クリック+マウス**: カメラ回転
- **マウスホイール**: ズーム

## Memory Log

### 2024年2月の最新の学びと記録
- Claude Code との対話を通じて、視覚的フィードバックシステムの重要性を再認識
- 無限ループとログの問題に対する包括的な解決策を実装
- Unity 6との互換性を確保するための具体的な技術的アプローチを策定

### 2025年7月の主要成果
- **BTLoggerシステム実装**: Debug.Logの性能問題を根本解決
- **272箇所のログ最適化**: 43ファイルにわたる包括的なログシステム統合
- **パフォーマンス大幅改善**: 条件付きコンパイルによる本番環境での完全最適化
- **テスト完全通過**: 54/54テスト（100%成功率）を達成
- **構造化ログ導入**: カテゴリベースフィルタリングとレベル制御システム