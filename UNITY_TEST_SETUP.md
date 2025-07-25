# Unity テストシーン設定ガイド

## 1. シーン準備

### AIエージェントの作成
1. **空のGameObjectを作成**
   - Hierarchy → 右クリック → Create Empty
   - 名前を「AdvancedGuardAI」に変更

2. **必要なコンポーネントを追加**
   - `Health` コンポーネントを追加
   - `Inventory` コンポーネントを追加
   - `BehaviourTreeRunner` コンポーネントを追加

3. **BehaviourTreeRunnerの設定**
   - Behaviour Tree File に `Assets/BehaviourTrees/advanced_guard.bt` を設定

### 複雑なテスト環境の作成

1. **パトロールポイントの作成**
   - 空のGameObject「patrol_point_1」を作成（青いCubeを子に追加）
   - 空のGameObject「patrol_point_2」を作成（緑のCubeを子に追加）
   - 空のGameObject「guard_post」を作成（黄色のCubeを子に追加）
   - 空のGameObject「safe_zone」を作成（紫のCubeを子に追加）

2. **敵オブジェクトの作成**
   - 3D Object → Capsule を作成
   - 名前を「Enemy」に変更
   - Tagを「Enemy」に設定
   - `Health` コンポーネントを追加
   - 赤いMaterialを適用

3. **視覚的配置**
   - AIを中央に配置
   - パトロールポイントを四角く配置
   - 敵をパトロール範囲内に配置
   - safe_zoneを端に配置

## 2. テスト手順

### 基本動作テスト
1. **Playボタンを押してテスト開始**
2. **Consoleウィンドウを開く**（Window → General → Console）
3. **期待される動作**：
   - AIが体力チェックを行う
   - 体力が30以上なら成功
   - Targetに向かって移動開始
   - Target到達で成功

### 体力テスト
1. **TestAIを選択**
2. **Healthコンポーネントを確認**
3. **右クリックメニューから「Take 25 Damage」を実行**
4. **体力が30未満になると移動しなくなることを確認**

## 3. デバッグ情報

### Consoleで確認できるログ
```
HealthCheck: Current health 100, Required: 30, Result: SUCCESS
MoveToPosition: Target found 'Target' at (5.0, 0.0, 5.0)
MoveToPosition: Moving to 'Target' - Distance: 7.07
MoveToPosition: Reached target 'Target'
```

### よくある問題と解決法

**問題**: "Target 'Target' not found!"
- **解決法**: TargetオブジェクトのGameObject名が正確に「Target」になっているか確認

**問題**: "No Health component found"
- **解決法**: TestAIにHealthコンポーネントが追加されているか確認

**問題**: "BT file not found"
- **解決法**: .btファイルのパスが正確か確認（Assets/BehaviourTrees/test_simple.bt）

## 4. さらなるテスト

### 複雑なテストシーン
1. **複数のターゲットを配置**
   - Target, Target2, patrol_point_1 など
2. **example.btファイルを使用**
   - より複雑な行動パターンをテスト

### パフォーマンステスト
1. **複数のAIエージェントを配置**
2. **同じ.btファイルを使用**
3. **フレームレートをチェック**（Stats表示をON）

## 5. 新しいスクリプトのテスト方法

1. **新しいActionやConditionスクリプトを作成**
2. **対応する.btファイルを作成**
3. **テストシーンで確認**
4. **ログ情報で動作を確認**

## トラブルシューティング

### コンパイルエラーが出る場合
- すべてのスクリプトが正しいフォルダにあるか確認
- クラス名とファイル名が一致しているか確認

### 実行時エラーが出る場合
- Consoleでエラーメッセージを確認
- 必要なコンポーネントが追加されているか確認
- GameObject名が.btファイル内の名前と一致しているか確認