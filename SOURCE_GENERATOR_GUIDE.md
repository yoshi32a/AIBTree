# ArcBT Source Generator ガイド

## 概要

ArcBT Source Generatorは、BTNodeの登録コードを自動生成することで、リフレクションを完全に排除し、パフォーマンスを向上させる仕組みです。

## 必要要件

- **Unity 2021.2以降**（Source Generator サポート）
- **Unity 2022.3以降**（推奨、正式サポート）
- **.NET Standard 2.0** 以上

## セットアップ

### 1. Source Generator のビルド

Source Generator は Unity プロジェクト外で管理されています：

```bash
# プロジェクトルートから
cd SourceGenerators
./build.sh  # Mac/Linux
build.bat   # Windows
```

### 2. Unity への統合

1. ビルドされた DLL をコピー：
   ```
   SourceGenerators/ArcBT.Generators/bin/Release/netstandard2.0/ArcBT.Generators.dll
   → Assets/ArcBT/RoslynAnalyzers/ArcBT.Generators.dll
   ```
2. Unity で DLL を選択し、Inspector で設定:
   - Platform settings → Any Platform
   - Asset Labels → `RoslynAnalyzer` を追加
3. Unity を再起動

### 3. 自動ビルド (CI/CD)

GitHub Actions でプッシュ時に自動ビルド：
- Source Generator の変更時に自動でビルドとアーティファクト生成
- タグ付きリリース時に DLL と NuGet パッケージを配布

## 使用方法

### BTNode属性の追加

アクションノードやコンディションノードに `BTNode` 属性を追加するだけです：

```csharp
using ArcBT.Core;

namespace MyGame.AI
{
    [BTNode("MyCustomAction", NodeType.Action)]
    public class MyCustomAction : BTActionNode
    {
        // 実装...
    }
    
    [BTNode("MyCondition", NodeType.Condition, AssemblyName = "MyGame.AI")]
    public class MyCondition : BTConditionNode
    {
        // 実装...
    }
}
```

### 属性パラメータ

- **ScriptName** (必須): .btファイルで使用するスクリプト名
- **NodeType** (必須): `NodeType.Action` または `NodeType.Condition`
- **AssemblyName** (オプション): 生成される登録クラスのアセンブリ名（デフォルト: "ArcBT"）

### 生成されるコード

Source Generatorは以下のような登録コードを自動生成します：

```csharp
// <auto-generated/>
namespace ArcBT.Generated
{
    [DefaultExecutionOrder(-1000)]
    public static class ArcBTNodeRegistration
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RegisterNodes()
        {
            // Action ノードの登録
            BTStaticNodeRegistry.RegisterAction("MoveToPosition", () => new MoveToPositionAction());
            BTStaticNodeRegistry.RegisterAction("Wait", () => new WaitAction());
            
            // Condition ノードの登録
            BTStaticNodeRegistry.RegisterCondition("HasSharedEnemyInfo", () => new HasSharedEnemyInfoCondition());
            
            BTLogger.LogSystem($"ArcBT ノードを自動登録しました (Actions: 2, Conditions: 1)");
        }
    }
}
```

## 移行ガイド

### 既存プロジェクトからの移行

1. **手動登録の削除**: `RPGNodeRegistration.cs` などの手動登録コードを削除
2. **属性の追加**: 各ノードクラスに `BTNode` 属性を追加
3. **ビルドと確認**: Source Generator をビルドして Unity で確認

### 例: 既存ノードの更新

```csharp
// Before
[BTScript("AttackEnemy")]
public class AttackEnemyAction : BTActionNode { }

// After
[BTScript("AttackEnemy")]
[BTNode("AttackEnemy", NodeType.Action)]
public class AttackEnemyAction : BTActionNode { }
```

## デバッグとトラブルシューティング

### エディターメニュー

- `BehaviourTree → Source Generator → Test Registration`: 登録されたノードの確認
- `BehaviourTree → Source Generator → Show BTNode Attributes`: 属性が付いたクラスの一覧

### よくある問題

**Q: Source Generator が動作しない**
- A: Unity 2021.2以降を使用しているか確認
- A: DLL に `RoslynAnalyzer` ラベルが設定されているか確認
- A: Unity の再起動を試す

**Q: ノードが登録されない**
- A: `BTNode` 属性が正しく設定されているか確認
- A: クラスが `public` であることを確認
- A: ビルドエラーがないか確認

**Q: 生成されたコードが見つからない**
- A: `Library/Bee/artifacts/` フォルダを確認
- A: コンパイルエラーがないか確認

## パフォーマンス比較

| 方式 | ノード生成時間 | メモリ使用量 |
|------|---------------|-------------|
| Reflection (旧) | ~20-30μs | 高 |
| Source Generator (新) | ~0.2-0.3μs | 低 |

**結果**: 約100倍の高速化を実現

## ベストプラクティス

1. **アセンブリ分離**: 大規模プロジェクトではアセンブリごとに登録を分離
2. **命名規則**: ScriptName は .bt ファイルと一致させる
3. **属性の一元管理**: BTScript と BTNode の ScriptName を同じにする

## 制限事項

- Unity 2021.2より前のバージョンでは使用不可（手動登録を使用）
- ジェネリッククラスはサポート外
- 入れ子クラスは推奨されない

## 今後の拡張

- インクリメンタルジェネレーターへの移行
- 診断機能の追加
- Visual Studio 統合の改善