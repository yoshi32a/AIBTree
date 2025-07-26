#!/bin/bash
echo "Building ArcBT Source Generator..."
echo ""

# ソースジェネレータープロジェクトのビルド
dotnet build ArcBT.Generators.csproj -c Release

# ビルド成功チェック
if [ $? -ne 0 ]; then
    echo ""
    echo "[ERROR] Build failed!"
    exit 1
fi

echo ""
echo "[SUCCESS] Build completed!"
echo "Output: bin/Release/netstandard2.0/ArcBT.Generators.dll"
echo ""
echo "Please copy the DLL to Assets/ArcBT/RoslynAnalyzers/ folder"
echo "and set the 'RoslynAnalyzer' label in Unity Inspector."
echo ""