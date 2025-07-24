# CLAUDE.md

このファイルは、Claude Code (claude.ai/code) がこのリポジトリのコードを扱う際のガイダンスを提供します。

## プロジェクト概要

これは Universal Render Pipeline (URP) を使用した Unity 6000.1.10f1 プロジェクト「AIBTree」です。このプロジェクトは URP Empty Template をベースとした新しい Unity プロジェクトで、最小限のカスタムコードが含まれています。

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
プロジェクトには Unity Test Framework が含まれています：
- **Window > General > Test Runner** からアクセス
- `Assets/Tests/` にテストを書く（必要に応じてフォルダを作成）
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

### 現在のカスタムスクリプト
プロジェクトには現在テンプレートスクリプトのみが含まれています：
- `Assets/TutorialInfo/Scripts/Readme.cs` - readme 表示用の ScriptableObject
- `Assets/TutorialInfo/Scripts/Editor/ReadmeEditor.cs` - readme 用のカスタムエディタ

## パッケージ依存関係

含まれる主要なパッケージ：
- **com.unity.render-pipelines.universal** (17.1.0) - URP レンダリング
- **com.unity.inputsystem** (1.14.0) - 新しい Input System
- **com.unity.ai.navigation** (2.0.8) - AI ナビゲーション/NavMesh
- **com.unity.test-framework** (1.5.1) - ユニットテストフレームワーク
- **com.unity.timeline** (1.8.7) - シネマティクス用タイムラインシステム

## 開発ガイドライン

### スクリプト構成
- カスタムスクリプトは `Assets/Scripts/` に配置（このフォルダを作成）
- 適切な C# 名前空間規約を使用
- Unity の命名規約に従う（パブリックメンバーには PascalCase）

### シーン管理
- メインシーンは `Assets/Scenes/SampleScene.unity`
- 追加のシーンは `Assets/Scenes/` に配置

### アセット管理
- `Assets/` 下で適切なフォルダ構造を使用（Models、Textures、Materials など）
- Unity がアセットのインポートと meta ファイルを自動的に処理

### バージョン管理に関する注意
- `Library/`、`Temp/`、`Logs/`、`obj/` フォルダは .gitignore で無視する
- `.meta` ファイルは Unity にとって重要で、バージョン管理の対象
- `Packages/packages-lock.json` はバージョン管理の対象

## Input System
プロジェクトは Unity の新しい Input System を使用します。入力アクションは `Assets/InputSystem_Actions.inputactions` で定義されています。

## レンダリングパイプライン
プロジェクトは Universal Render Pipeline を使用し、カスタムレンダーパイプラインアセットが `Assets/Settings/` にあります。