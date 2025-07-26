@echo off
echo Building ArcBT Source Generator...
echo.

REM ソースジェネレータープロジェクトのビルド
dotnet build ArcBT.Generators.csproj -c Release

REM ビルド成功チェック
if %errorlevel% neq 0 (
    echo.
    echo [ERROR] Build failed!
    pause
    exit /b 1
)

echo.
echo [SUCCESS] Build completed!
echo Output: bin\Release\netstandard2.0\ArcBT.Generators.dll
echo.
echo Please copy the DLL to Assets/ArcBT/RoslynAnalyzers/ folder
echo and set the "RoslynAnalyzer" label in Unity Inspector.
echo.
pause