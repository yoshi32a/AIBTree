# Unity テストシーン設定ガイド

**BlackBoard対応版** - 動的条件チェック機能付きBehaviourTreeのテスト方法

## 1. シーン準備

### AIエージェントの作成
1. **空のGameObjectを作成**
   - Hierarchy → 右クリック → Create Empty
   - 名前を「BlackBoardTestAI」に変更

2. **必要なコンポーネントを追加**
   - `Health` コンポーネントを追加
   - `Inventory` コンポーネントを追加
   - `BehaviourTreeRunner` コンポーネントを追加

3. **BehaviourTreeRunnerの設定**
   - Behaviour Tree File に `blackboard_sample.bt`、`dynamic_condition_sample.bt`、または `team_coordination_sample.bt` を設定
   - Debug Mode を有効にしてログ確認

### BlackBoard対応テスト環境

1. **パトロールポイントの作成**
   - 空のGameObject「patrol_point_1」を作成（青いCubeを子に追加）
   - 空のGameObject「patrol_point_2」を作成（緑のCubeを子に追加）
   - 空のGameObject「resource_point」を作成（金色のCubeを子に追加）
   - 空のGameObject「safe_zone」を作成（紫のCubeを子に追加）

2. **敵オブジェクトの作成**
   - 3D Object → Capsule を作成
   - 名前を「Enemy」に変更
   - Tagを「Enemy」に設定
   - `Health` コンポーネントを追加
   - 赤いMaterialを適用

3. **チーム連携用の追加AI**
   - 「BlackBoardTestAI」を複製して「TeamMemberAI」を作成
   - 同じBlackBoard参照で連携動作をテスト

4. **視覚的配置**
   - AIを中央に配置
   - パトロールポイントを四角く配置
   - 敵をパトロール範囲内に配置
   - safe_zoneを端に配置

## 2. テスト手順

### BlackBoard基本機能テスト
1. **Playボタンを押してテスト開始**
2. **Consoleウィンドウを開く**（Window → General → Console）
3. **BehaviourTreeRunnerを右クリック → "Show BlackBoard Contents"** でデータ確認
4. **期待される動作**：
   - BlackBoardにデータが格納される
   - AIがBlackBoard内のデータを参照して行動
   - データの変更が他のAIにも反映される

### 動的条件チェックテスト
1. **dynamic_condition_sample.btを使用**
2. **AIが敵に向かって移動中に敵を削除**
3. **期待される動作**：
   - Action実行中に条件が満たされなくなる
   - 自動的に行動を中断してパトロールに戻る

### 体力・条件変化テスト
1. **BlackBoardTestAIを選択**
2. **Healthコンポーネントを確認**
3. **右クリックメニューから"Take Damage"を実行**
4. **BlackBoardの"current_health"値の変化を確認**
5. **体力低下時の動的な行動変化を確認**

## 3. デバッグ情報

### Consoleで確認できるログ
```
[BlackBoard] Set: enemy_target = Enemy (UnityEngine.GameObject)
[BlackBoard] Get: current_health = 100
HealthCheck: Current health 100, Required: 30, Result: SUCCESS
EnemyCheck: Found enemy at distance 8.5, setting BlackBoard target
AttackEnemy: Attacking target from BlackBoard, damage: 25
[DynamicCondition] Condition 'EnemyCheck' failed during action, aborting
MoveToPosition: Using BlackBoard key 'current_patrol_point' = patrol_point_1
```

### BlackBoard専用デバッグ機能

**BlackBoard内容確認**:
- BehaviourTreeRunnerを右クリック → "Show BlackBoard Contents"
- 現在格納されているすべてのデータが表示される

**BlackBoardクリア**:
- BehaviourTreeRunnerを右クリック → "Clear BlackBoard"
- すべてのデータがリセットされる

### よくある問題と解決法

**問題**: "BlackBoard key 'xxx' not found!"
- **解決法**: .btファイル内のbb_keyが正確に設定されているか確認
- **解決法**: Initialize処理でBlackBoardに初期値が設定されているか確認

**問題**: "No BlackBoard reference in node"
- **解決法**: BehaviourTreeRunnerがBlackBoardを正しく初期化しているか確認

**問題**: "Dynamic condition checking not working"
- **解決法**: .btファイルでdynamic_conditions プロパティが設定されているか確認
- **解決法**: ConditionとActionが正しく関連付けられているか確認

## 4. 高度なテスト

### チーム連携テスト
1. **複数のAIエージェントを配置**
2. **team_coordination_sample.btを使用**
3. **共有BlackBoardでの連携動作を確認**：
   - 一つのAIが敵を発見 → 他のAIも同じ敵を共有
   - リソース管理の協調動作
   - 役割分担（偵察、攻撃、支援）

### リソース管理テスト
1. **resource_management_sample.btを使用**
2. **リソースポイントを複数配置**
3. **期待される動作**：
   - 効率的なリソース収集
   - 在庫管理とBlackBoard連携
   - 複数AIでのリソース競合回避

### パフォーマンステスト
1. **10体以上のAIエージェントを配置**
2. **同じBlackBoardを共有**
3. **フレームレートをチェック**（Stats表示をON）
4. **BlackBoard操作の頻度を監視**

## 5. カスタムノードのテスト方法

### 新しいActionノードの作成とテスト
1. **BTActionNodeを継承したクラスを作成**
2. **BlackBoard対応の処理を実装**:
   ```csharp
   public override void SetProperty(string key, object value)
   {
       if (key == "bb_key")
           blackBoardKey = value.ToString();
   }
   ```
3. **対応する.btファイルを作成**
4. **BlackBoard連携をテスト**

### 新しいConditionノードの作成とテスト
1. **BTConditionNodeを継承したクラスを作成**
2. **動的条件チェック対応を実装**
3. **Actionノードとの連携をテスト**

## 6. トラブルシューティング

### BlackBoard関連の問題
**問題**: "BlackBoard null reference"
- **解決法**: Initialize()メソッドでBlackBoard参照が正しく設定されているか確認
- **解決法**: BehaviourTreeRunnerが最新版を使用しているか確認

**問題**: "Type mismatch in BlackBoard"
- **解決法**: SetValue<T>()とGetValue<T>()で同じ型を使用しているか確認
- **解決法**: デフォルト値の型が正しいか確認

### 動的条件チェックの問題
**問題**: "Dynamic conditions not being checked"
- **解決法**: SetupDynamicConditionChecking()が呼ばれているか確認
- **解決法**: watchedConditionsリストが正しく設定されているか確認

### コンパイルエラーが出る場合
- 1ファイル1クラスの原則に従っているか確認
- Initialize()メソッドのシグネチャが正しいか確認
- 必要なusing文が追加されているか確認

### 実行時エラーが出る場合
- Consoleでエラーメッセージを詳細確認
- BlackBoard初期化タイミングを確認
- .btファイル内のプロパティ名が正確か確認
- GameObject名とBlackBoardキーの整合性を確認