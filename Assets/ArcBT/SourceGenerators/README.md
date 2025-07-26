# Source Generator について

Source Generator は Unity プロジェクト外で管理されるようになりました。

## 新しい場所

```
/SourceGenerators/
├── ArcBT.Generators/
│   ├── ArcBT.Generators.csproj
│   └── BTNodeRegistrationGenerator.cs
├── ArcBT.Generators.sln
├── build.bat
├── build.sh
└── README.md
```

## ビルド方法

プロジェクトルートで：
```bash
cd SourceGenerators
./build.sh  # Mac/Linux
build.bat   # Windows
```

## Unity への統合

ビルドされた DLL を以下の場所にコピー：
```
SourceGenerators/ArcBT.Generators/bin/Release/netstandard2.0/ArcBT.Generators.dll
→ Assets/ArcBT/RoslynAnalyzers/ArcBT.Generators.dll
```

Unity で DLL に `RoslynAnalyzer` ラベルを設定してください。