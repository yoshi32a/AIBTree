# RoslynAnalyzers フォルダ

このフォルダはUnity 2022.3以降でSource Generatorを使用するためのものです。

## セットアップ手順

1. Source Generator DLLをビルド:
   ```bash
   cd Assets/ArcBT/SourceGenerators
   dotnet build -c Release
   ```

2. ビルドされたDLLをこのフォルダにコピー:
   - `ArcBT.Generators.dll`

3. DLLのインポート設定:
   - Inspector で DLL を選択
   - "Platform settings" → "Any Platform" → Settings
   - "Asset Labels" に `RoslynAnalyzer` を追加

## Unity バージョン対応

- **Unity 2021.2-2022.2**: Source Generator は実験的機能
- **Unity 2022.3+**: 正式サポート（推奨）
- **Unity 6000.1.10f1**: 完全対応

## 注意事項

- Source Generatorが動作しない場合は、手動登録（RPGNodeRegistration.cs）が使用されます
- 生成されたコードは `Library/Bee/artifacts/` に出力されます
- エディターの再起動が必要な場合があります