# CLAUDE.md

このファイルは、Claude Code (claude.ai/code) がこのリポジトリのコードを扱う際のガイダンスを提供します。

## プロジェクト概要

これは Universal Render Pipeline (URP) を使用した Unity 6000.1.10f1 プロジェクト「AIBTree」です。**BehaviourTree AI システム**の実装が主な目的で、.btファイル形式でのAI定義とVSCode統合を提供します。

### 主要機能
- **BehaviourTree実行エンジン**: C#による階層的なノード構造の実装
- **BlackBoardシステム**: AIノード間でのデータ共有とリアルタイム状態管理
- **動的条件チェック**: Action実行中の条件変化に即座に対応
- **.btファイル形式**: 人間が読みやすい階層形式でのAI定義
- **VSCode拡張機能**: シンタックスハイライト、自動補完、診断機能
- **動的スクリプトローダー**: .btファイルからC#クラスへの動的マッピング

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
  - `Inventory.cs` - インベントリ管理コンポーネント
  - `InventoryItem.cs` - インベントリアイテム定義
- `Assets/Tests/` - テストスイート
  - `BTParsingTests.cs` - 全BTファイルパーステスト
  - `BTFileValidationTests.cs` - ファイル構造詳細検証
  - `BTTestRunner.cs` - エディターメニュー統合
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
- **ログ確認**: Consoleウィンドウで実行状況をモニター
- **体力テスト**: Healthコンポーネントの右クリックメニューを使用
- **BlackBoardデバッグ**: BehaviourTreeRunnerの右クリック → "Show BlackBoard Contents"
- **ツリー状態リセット**: BehaviourTreeRunnerの右クリック → "Reset Tree State"
- **動的条件チェック確認**: 実行中の条件変化でActionが即座に中断されることを確認

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

### コンパイルエラー対応
- すべてのスクリプトで `SetProperty(string, string)` 形式に統一
- `CheckCondition()` は `protected override BTNodeResult` 形式に統一
- `owner` → `ownerComponent`, `owner.transform` → `transform` に統一
- `debugMode` 参照を削除してログ直接出力に変更
- `Components.Health` → `Health` に修正（namespace修正）

## Memory Log

### Claude Code との対話で学んだこと
- "to memorize" というメモを追加（この情報自体は意味がありません）