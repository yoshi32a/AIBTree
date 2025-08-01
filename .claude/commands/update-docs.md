# /update-docs - プロジェクトドキュメント一括更新スラッシュコマンド

プロジェクトの主要な変更を全ての関連Markdownファイルに自動的に反映し、gitにコミットするスラッシュコマンドです。

## 実行内容

1. **プロジェクト構造の分析**
   - Assets/ArcBT/ 配下のファイル構成を確認
   - 新しく追加されたスクリプトやテストを検出
   - アセンブリ定義の変更を確認
   - パッケージ依存関係の変更検出

2. **対象ドキュメントファイルの更新**
   - **CLAUDE.md**: プロジェクト概要・構造・Memory Log
   - **README.md**: プロジェクト説明・セットアップ手順・最新機能
   - **BT_REFERENCE.md**: .btファイル形式・新ノード追加
   - **UNITY_TEST_SETUP.md**: テスト環境・カバレッジ情報
   - **Assets/ArcBT/README.md**: ArcBTパッケージの概要
   - **Assets/ArcBT/Samples/RPGExample/README.md**: RPGサンプル説明
   - **Assets/ArcBT/Samples/Documentation/RPG_IMPLEMENTATION_GUIDE.md**: 実装ガイド

3. **変更のコミット**
   - 適切な日本語コミットメッセージでgitコミット
   - Claude Code署名付きでコミット

## 使用方法

```
/update-docs
```

または特定の更新内容を指定：

```
/update-docs "新機能XYZの実装完了"
```

## 更新対象ファイルと内容

### CLAUDE.md
- **プロジェクト構造**: ArcBTパッケージ構造の更新
- **テスト実装状況**: 新しいテスト・カバレッジ率の更新
- **主要成果**: 実装した機能や修正した問題
- **開発ガイドライン**: API変更に伴うガイドライン更新
- **Memory Log**: 最新の学習内容と重要な変更記録

### README.md
- **プロジェクト概要**: 最新機能の説明
- **セットアップ手順**: 新しい依存関係やツール
- **使用例**: 新しいノードやサンプルの紹介
- **コードカバレッジ**: 最新のテスト結果

### BT_REFERENCE.md
- **新ノードリファレンス**: 追加されたActionNode/ConditionNode
- **.btファイル形式**: 新しい構文や機能
- **サンプルコード**: 実用的な使用例

### UNITY_TEST_SETUP.md
- **テスト環境**: 新しいテストフレームワーク設定
- **カバレッジ設定**: コードカバレッジツールの設定
- **実行手順**: 自動化されたテスト実行方法

### Assets/ArcBT/README.md
- **パッケージ概要**: ArcBTコアライブラリの説明
- **アーキテクチャ**: 内部構造と設計思想
- **拡張方法**: カスタムノードの作成ガイド

### Assets/ArcBT/Samples/RPGExample/README.md
- **RPGサンプル**: ゲーム要素の実装例
- **コンポーネント**: Health、Mana、Inventoryの使用法
- **実装パターン**: 一般的なRPG AI行動の実装

### Assets/ArcBT/Samples/Documentation/RPG_IMPLEMENTATION_GUIDE.md
- **詳細実装ガイド**: ステップバイステップの実装手順
- **ベストプラクティス**: 効率的な開発パターン
- **トラブルシューティング**: 一般的な問題と解決策

## 自動検出項目

- 新しいActionノード・Conditionノードの追加
- テストファイルの追加・更新
- アセンブリ定義の変更
- パッケージ依存関係の変更
- .btファイルサンプルの追加
- ドキュメント間の整合性チェック

## 除外ファイル

- **AI_INSTRUCTIONS.md**: 開発用一時ファイル（自動更新対象外）
- **ProjectAnalyze.md**: 分析用一時ファイル（必要に応じて手動更新）

これにより、プロジェクトの全ドキュメントを常に最新の状態でシンクロナイズし、開発チームとの情報共有を効率化できます。
