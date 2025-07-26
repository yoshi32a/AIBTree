# RoslynAnalyzers フォルダ

このフォルダはUnity 2022.3以降でSource Generatorを使用するためのものです。

## 配置されているファイル

- `ArcBT.Generators.dll` - BTNode属性を持つクラスの自動登録コードを生成するSource Generator

## 重要な設定

Unity でこの DLL を選択し、Inspector で以下を確認：
1. **Asset Labels** に `RoslynAnalyzer` が設定されていること
2. **Platform settings** が適切に設定されていること

## 動作確認

Unity メニューから：
- `BehaviourTree → Source Generator → Test Registration`
- `BehaviourTree → Source Generator → Show BTNode Attributes`

## トラブルシューティング

Source Generator が動作しない場合：
1. Unity を再起動
2. `Library` フォルダを削除して再インポート
3. Unity 2021.2 以降であることを確認