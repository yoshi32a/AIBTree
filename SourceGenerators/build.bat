@echo off
echo Building ArcBT Source Generator...
echo.

cd ArcBT.Generators
dotnet build -c Release

if %errorlevel% neq 0 (
    echo.
    echo [ERROR] Build failed!
    cd ..
    pause
    exit /b 1
)

echo.
echo [SUCCESS] Build completed!
echo.
echo === DLL Output ===
echo bin\Release\netstandard2.0\ArcBT.Generators.dll
echo.
echo === NuGet Package ===
echo Creating NuGet package...
dotnet pack -c Release --no-build

echo.
echo Package: bin\Release\ArcBT.Generators.1.0.0.nupkg
echo.
echo Copy the DLL to: Assets\ArcBT\RoslynAnalyzers\
echo.
cd ..
pause