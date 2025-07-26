# ArcBT Source Generators

Unity プロジェクト外で管理される Source Generator プロジェクトです。

## ビルド方法

### 1. コマンドラインでビルド

```bash
cd SourceGenerators
dotnet build -c Release
```

### 2. NuGet パッケージの作成

```bash
dotnet pack -c Release
```

出力: `ArcBT.Generators/bin/Release/ArcBT.Generators.1.0.0.nupkg`

## Unity への統合

### 方法1: DLL を直接配置

1. ビルドされた DLL をコピー:
   ```
   ArcBT.Generators/bin/Release/netstandard2.0/ArcBT.Generators.dll
   → Assets/ArcBT/RoslynAnalyzers/ArcBT.Generators.dll
   ```

2. Unity で DLL を選択し、Inspector で設定:
   - Asset Labels → `RoslynAnalyzer` を追加

### 方法2: ローカル NuGet パッケージとして参照

1. `Assets/NuGet.config` を作成:
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <configuration>
     <packageSources>
       <add key="local" value="../../SourceGenerators/ArcBT.Generators/bin/Release" />
     </packageSources>
   </configuration>
   ```

2. Unity Package Manager でパッケージを追加

## 開発環境

- .NET SDK 6.0 以上
- Visual Studio 2022 / VS Code
- Unity 2021.2 以上（Source Generator サポート）

## CI/CD

GitHub Actions でのビルドは `.github/workflows/build-generator.yml` を参照