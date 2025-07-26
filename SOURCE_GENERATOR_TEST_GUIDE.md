# Source Generator 動作テストガイド

## テスト手順

### 1. Unity を起動

1. Unity Hub で AIBTree プロジェクトを開く
2. Unity のコンパイルが完了するまで待つ

### 2. Source Generator の確認

#### 方法1: エディターメニューから確認

1. Unity メニュー → `BehaviourTree → Source Generator → Show BTNode Attributes`
   - BTNode 属性が付いたクラスの一覧が表示される
   - 例: MoveToPositionAction, WaitAction, AttackEnemyAction, HealthCheckCondition

2. Unity メニュー → `BehaviourTree → Source Generator → Test Registration`
   - 登録されているノードの一覧と作成テストが実行される

#### 方法2: テストシーンで確認

1. `Assets/Scenes/SourceGeneratorTest.unity` シーンを開く
2. Play ボタンを押す
3. Console ウィンドウで結果を確認

期待される出力:
```
=== Testing Source Generator ===
✓ MoveToPosition created successfully: MoveToPositionAction
✓ Wait created successfully: WaitAction
✓ HasSharedEnemyInfo created successfully: HasSharedEnemyInfoCondition
✓ AttackEnemy created successfully: AttackEnemyAction
✓ HealthCheck created successfully: HealthCheckCondition
=== Test Complete ===
```

### 3. 生成されたコードの確認

生成されたコードは以下の場所に作成されます：
- `Library/Bee/artifacts/1900b0aP.dag/ArcBT.NodeRegistration.g.cs`
- `Library/Bee/artifacts/1900b0aP.dag/ArcBT.Samples.RPG.NodeRegistration.g.cs`

※ フォルダ名は環境により異なります

## トラブルシューティング

### Source Generator が動作しない場合

1. **DLL の確認**
   - `Assets/ArcBT/RoslynAnalyzers/ArcBT.Generators.dll` が存在すること
   - Inspector で `RoslynAnalyzer` ラベルが設定されていること

2. **Unity バージョン**
   - Unity 2021.2 以降であること
   - Unity 2022.3 以降推奨

3. **再起動**
   - Unity を完全に終了して再起動
   - `Library` フォルダを削除して再インポート

4. **ログ確認**
   - Console ウィンドウでエラーがないか確認
   - `Window → General → Console` でフィルターを All にする

### 手動登録へのフォールバック

Source Generator が動作しない場合でも、既存の `RPGNodeRegistration.cs` による手動登録がフォールバックとして機能します。

## 成功の確認ポイント

1. ✅ BTNode 属性を持つクラスが認識される
2. ✅ エディターメニューのテストが成功する
3. ✅ テストシーンで全ノードが作成できる
4. ✅ .bt ファイルの実行が正常に動作する

## 次のステップ

Source Generator が正常に動作したら：
1. 新しいノードクラスに `[BTNode]` 属性を追加
2. Unity を再コンパイル
3. 自動的に登録コードが生成される