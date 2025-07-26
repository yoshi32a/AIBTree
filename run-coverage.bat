@echo off
echo 🧪 Running Unity Code Coverage for ArcBT...

REM Unity エディターパスを設定（環境に応じて調整）
set UNITY_PATH="C:\Program Files\Unity\Hub\Editor\2023.3.10f1\Editor\Unity.exe"

REM カバレッジ付きでテスト実行
%UNITY_PATH% ^
  -batchmode ^
  -quit ^
  -projectPath "%~dp0" ^
  -runTests ^
  -testPlatform EditMode ^
  -testResults TestResults.xml ^
  -enableCodeCoverage ^
  -coverageResultsPath CodeCoverage ^
  -coverageOptions "generateAdditionalMetrics;generateBadgeReport;generateHtmlReport" ^
  -assemblyNames "ArcBT;ArcBT.Samples;App" ^
  -logFile coverage-log.txt

echo ✅ Code Coverage完了！
echo 📊 結果: CodeCoverage\Report\index.html を確認してください
pause