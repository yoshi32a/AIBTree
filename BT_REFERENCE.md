# BehaviourTree (.bt) ファイル リファレンス

## 基本構文

### 1. ツリー定義
```bt
tree TreeName {
    # ルートノードをここに定義
}
```

### 2. ノードタイプ

#### Composite Nodes（複合ノード）

**Sequence（シーケンス）**
- 子ノードを順番に実行
- すべての子が成功した場合のみ成功
```bt
sequence node_name {
    # 子ノードをここに配置
}
```

**Selector（セレクター）**
- 子ノードを順番に試行
- いずれかの子が成功すれば成功
```bt
Selector node_name {
    # 子ノードをここに配置
}
```

**Parallel（並列）**
- 複数の子ノードを同時実行
- success_policy/failure_policyで成功・失敗条件を指定
```bt
Parallel node_name {
    success_policy: "require_one"  # require_one | require_all
    failure_policy: "require_all"   # require_one | require_all
    # 子ノードをここに配置
}
```

#### Decorator Nodes（デコレーターノード）

**Inverter（インバーター）**
- 子ノードの実行結果を反転
- Success → Failure, Failure → Success, Running → Running
```bt
Inverter node_name {
    # 子ノード（1つのみ）
}
```

**Repeat（リピート）**
- 子ノードを指定回数または無限に繰り返し実行
```bt
Repeat node_name {
    count: 5                # 繰り返し回数（-1で無限）
    stop_on_failure: true   # 失敗時に停止するか
    reset_child: true       # 各回の開始時に子ノードをリセット
    # 子ノード（1つのみ）
}
```

**Retry（リトライ）**
- 子ノードが失敗した場合に指定回数まで再試行
```bt
Retry node_name {
    max_retries: 3      # 最大リトライ回数
    retry_delay: 1.0    # リトライ間隔（秒）
    # 子ノード（1つのみ）
}
```

**Timeout（タイムアウト）**
- 子ノードの実行に時間制限を設定
```bt
Timeout node_name {
    time: 5.0                  # タイムアウト時間（秒）
    success_on_timeout: false  # タイムアウト時の結果
    # 子ノード（1つのみ）
}
```

#### Leaf Nodes（リーフノード）

**Action（アクション）**
- 実際の動作を実行
```bt
Action ScriptName {
    # パラメータ
}
```

**Condition（条件）**
- 条件をチェック
```bt
Condition ScriptName {
    # パラメータ
}
```

## プロパティ一覧

### 共通プロパティ
- ノード名がそのままUnity C#クラス名になります
- 全ノードでBlackBoardにアクセス可能
- GameplayTagSystemによる高速オブジェクト検索対応

### Action専用プロパティ
- `target: "target_name"` - 移動先やターゲット
- `speed: 3.5` - 移動速度（float）
- `damage: 25` - ダメージ量（int）
- `duration: 2.0` - 実行時間（秒）
- `tolerance: 0.5` - 位置の許容誤差
- `cooldown: 1.0` - クールダウン時間（秒）
- `scan_radius: 15.0` - スキャンの範囲（ScanEnvironment用）
- `attack_range: 2.0` - 攻撃範囲（AttackTarget用）
- `wander_radius: 10.0` - 徘徨範囲（RandomWander用）
- `tolerance: 0.5` - 目標地点への到達判定距離

### Condition専用プロパティ
- `min_health: 50` - 最小体力しきい値
- `detection_range: 10.0` - 検出範囲
- `bb_key: "key_name"` - BlackBoardのキー名（HasSharedEnemyInfo用）
- `min_mana: 30` - 最小マナ量（未実装）
- `condition: "key1 >= value"` - BlackBoard値の比較式（CompareBlackBoard用）

### Parallel専用プロパティ
- `success_policy: "require_one"` - 成功条件（require_one/require_all）
- `failure_policy: "require_all"` - 失敗条件（require_one/require_all）

### Decorator専用プロパティ
- **Repeat**: `count: 5` - 繰り返し回数（-1で無限）
- **Repeat**: `stop_on_failure: true` - 失敗時停止
- **Repeat**: `reset_child: true` - 各回開始時のリセット
- **Retry**: `max_retries: 3` - 最大リトライ回数
- **Retry**: `retry_delay: 1.0` - リトライ間隔（秒）
- **Timeout**: `time: 5.0` - タイムアウト時間（秒）
- **Timeout**: `success_on_timeout: false` - タイムアウト時の結果

## 新機能

### 1. GameplayTagSystem

ArcBT v1.0.0で導入された革新的なタグシステムです。Unity標準のGameObject.tagを完全に置換し、10-100倍の性能向上を実現します。

#### 階層的タグ構造
```csharp
// 階層的タグの例
"Character.Enemy.Boss"     // ボス敵
"Character.Player"         // プレイヤー
"Object.Item.Weapon"       // 武器アイテム
"Effect.Magic.Fire"        // 炎魔法エフェクト
```

#### 高速検索とパフォーマンス最適化
- **ReadOnlySpan活用**: 0アロケーション文字列比較
- **階層マッチング**: 親子関係の高速判定
- **キャッシュシステム**: 検索結果の効率的なキャッシュ管理
- **プール管理**: GameObjectArrayPoolによるメモリ最適化

#### .btファイルでの使用
```bt
# GameplayTagを使用したオブジェクト検索
Action AttackTarget {
    target_tag: "Character.Enemy"  # 階層的タグ検索
    damage: 25
}

Condition EnemyInRange {
    target_tag: "Character.Enemy.Boss"  # 特定の敵タイプのみ
    max_distance: 5.0
}
```

#### Unity互換性レイヤー
既存のUnityタグAPIと互換性を保ちつつ、段階的な移行をサポート：
```csharp
// 従来の書き方
if (gameObject.CompareTag("Enemy"))

// 新しい書き方（互換レイヤー経由）
if (gameObject.CompareGameplayTag("Character.Enemy"))

// 直接GameplayTagManager使用（最高性能）
if (GameplayTagManager.HasTag(gameObject, "Character.Enemy"))
```

### 2. BlackBoard システム
- AIノード間でデータを共有するグローバルストレージ
- 型安全な値の設定・取得が可能
- デバッグ機能付き

```csharp
// C#でのBlackBoard使用例
blackBoard.SetValue("player_health", 75);
int health = blackBoard.GetValue<int>("player_health", 100);
```

```bt
# .btファイルでのBlackBoard操作
Action SetBlackBoard {
    # 自動型判定される
    health: 100          # int
    speed: 5.5          # float
    is_active: true     # bool
    position: "(10,0,5)" # Vector3
    name: "Player1"     # string
}
```

### 2. 動的条件チェック
- Action実行中に条件が変化した場合、即座に停止
- SequenceノードでConditionとActionを並べると自動的に監視関係が設定される

```bt
Sequence patrol_with_health_check {
    Condition HealthCheck {
        min_health: 30
    }
    
    Action MoveToPosition {  # 体力が30未満になると即座に停止
        target: "patrol_point"
        speed: 3.0
    }
}
```

### 3. CompareBlackBoard条件ノード
BlackBoard内の値を比較する条件ノード。柔軟な比較演算子をサポート。

```bt
# 基本的な使い方
Condition CompareBlackBoard {
    condition: "current_health <= 20"
}

# 使用可能な演算子
# - == : 等しい
# - != : 等しくない
# - > : より大きい
# - < : より小さい
# - >= : 以上
# - <= : 以下

# BlackBoardのキー同士の比較
Condition CompareBlackBoard {
    condition: "player_health < enemy_health"
}

# 数値との比較
Condition CompareBlackBoard {
    condition: "mana_points >= 50"
}

# 文字列との比較（引用符付き）
Condition CompareBlackBoard {
    condition: "ai_state == \"attacking\""
}
```

## 利用可能スクリプト

### Action用スクリプト

#### 移動系アクション（MovementController統一済み）
- `MoveToTarget` - ターゲットへの移動（MovementController対応）
- `FleeToSafety` - 安全地帯への逃走（MovementController対応）
- `RandomWander` - ランダム徘徊（MovementController対応）
- `MoveToPosition` - 指定位置への移動
- `MoveToEnemy` - BlackBoardから敵位置を取得して移動

#### 戦闘系アクション
- `AttackTarget` - BlackBoardの敵情報を使用して攻撃
- `NormalAttack` - 通常攻撃
- `AttackEnemy` - 敵への攻撃
- `CastSpell` - 魔法詠唱

#### 環境・相互作用系アクション
- `ScanEnvironment` - 環境スキャンして敵情報をBlackBoardに保存
- `EnvironmentScan` - 環境スキャン（代替）
- `Interact` - オブジェクトとの相互作用
- `SearchForEnemy` - 敵探索

#### その他のアクション
- `Wait` - 指定時間待機
- `UseItem` - アイテム使用
- `Attack` - 汎用攻撃アクション
- `InitializeResources` - リソース初期化
- `RestoreSmallMana` - 少量マナ回復
- `SetBlackBoard` - BlackBoardに値を設定（自動型判定：int、float、bool、Vector3、string）
- `Log` - ログ出力（level、message、include_blackboard、blackboard_key対応）

#### ExampleAI用Simple系アクション
- `SimpleAttack` - ExampleAI用のシンプルな攻撃アクション
- `MoveToNamedPosition` - 名前付き位置への移動アクション
- `WaitSimple` - ExampleAI用のシンプルな待機アクション

### Condition用スクリプト
- `HealthCheck` - 体力チェック
- `EnemyCheck` - 敵検出
- `HasItem` - アイテム所持確認
- `HasSharedEnemyInfo` - BlackBoardに共有された敵情報の有無をチェック
- `CompareBlackBoard` - BlackBoard内の値を比較（condition式で指定）
- `HasTarget` - ターゲット所持確認
- `HasMana` - マナ量確認（RPGサンプルで実装済み）
- `IsInitialized` - 初期化状態確認（RPGサンプルで実装済み）
- `EnemyHealthCheck` - 敵の体力確認（RPGサンプルで実装済み）
- `EnemyInRange` - 攻撃範囲内に敵がいるかチェック（RPGサンプルで実装済み）
- `CheckManaResource` - マナリソースチェック
- `CheckAlertFlag` - アラート状態フラグチェック
- `DistanceCheck` - 3D距離チェック（target/target_tag、distance式（"<= 5.0"等）、use_blackboard対応）
- `Distance2DCheck` - 2D距離チェック（Y軸無視、target/target_tag、distance式、use_blackboard対応）
- `Random` - 確率判定（probability: 0.0～1.0）
- `ScanForInterest` - 興味のあるオブジェクトをスキャン

#### ExampleAI用Simple系条件
- `SimpleHasTarget` - ExampleAI用のシンプルなターゲット確認条件
- `EnemyDetection` - ExampleAI用の敵検出条件
- `SimpleHealthCheck` - ExampleAI用のシンプルな体力チェック条件

### 距離チェック条件の使用例

```bt
# ターゲットが5メートル以内にいるか
Condition DistanceCheck {
    target: "Player"
    distance: "<= 5.0"
}

# タグで検索し、10メートルより遠いか（2D距離）
Condition Distance2DCheck {
    target_tag: "Enemy"
    distance: "> 10.0"
}

# BlackBoardの位置を使用
Condition DistanceCheck {
    use_blackboard: true
    blackboard_position_key: "enemy_position"
    distance: "<= 3.0"
}
```

### Decorator用スクリプト
デコレーターは子ノードの実行を制御・修飾するノードです。ArcBT v1.0.0で完全実装されました。

- `Timeout` - タイムアウト処理（time、success_on_timeout対応）
- `Repeat` - 繰り返し実行（count、stop_on_failure、reset_child対応）
- `Retry` - リトライ処理（max_retries、retry_delay対応）
- `Inverter` - 結果反転（成功→失敗、失敗→成功）

#### 使用例
```bt
# タイムアウト付きアクション
Timeout escape_with_timeout {
    timeout: 8.0
    success_on_timeout: false
    
    Action MoveToPosition {
        target: "SafeZone"
        speed: 8.0
    }
}

# 5回繰り返し（失敗で中断）
Repeat combat_loop {
    count: 5
    stop_on_failure: true
    reset_child: true
    
    Action AttackEnemy {
        damage: 30
    }
}

# 3回までリトライ
Retry escape_attempt {
    max_retries: 3
    retry_delay: 1.5
    
    Action FleeToSafety {
        target: "SafeZone"
    }
}

# 条件を反転
Inverter no_enemies {
    Condition HasTarget {
        target_tag: "Enemy"
    }
}
```

## リフレクション削除による高速化

### BTStaticNodeRegistry
ArcBTは最新バージョンでリフレクションを完全に削除し、10-100倍の性能改善を実現しました。

#### ノード登録方法
```csharp
// Runtimeノードの登録（BTStaticNodeRegistry.csに直接記述）
static readonly Dictionary<string, Func<BTActionNode>> actionCreators = new()
{
    ["MoveToPosition"] = () => new Actions.MoveToPositionAction(),
    ["Wait"] = () => new Actions.WaitAction(),
    // ...他のノード
};

// Samplesノードの自己登録（RPGNodeRegistration.cs）
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
static void RegisterNodes()
{
    BTStaticNodeRegistry.RegisterAction("AttackEnemy", () => new AttackEnemyAction());
    BTStaticNodeRegistry.RegisterCondition("HealthCheck", () => new HealthCheckCondition());
}
```

#### IHealthインターフェース
Healthコンポーネントへのアクセスもリフレクションフリーに：
```csharp
// 従来: var health = GetComponent("Health");
// 新方式:
var health = target.GetComponent<IHealth>();
if (health != null)
{
    health.TakeDamage(damage);
}
```

## BTLoggerシステム

ArcBTには高性能なログシステムが統合されており、Debug.Logの性能問題を解決します。

### 主要機能
- **条件付きコンパイル**: 本番ビルドでログ処理を完全除去
- **カテゴリベースフィルタリング**: Combat、Movement、Parser、System等のカテゴリ別制御
- **ログレベル制御**: Error、Warning、Info、Debug、Traceの5段階
- **エディター統合**: `Window > BehaviourTree > Logger Settings`でリアルタイム制御

### 使用方法
```csharp
// カテゴリ別ログ出力
BTLogger.LogSystem("システム初期化完了");
BTLogger.LogCombat("戦闘開始", nodeName, context);
BTLogger.LogMovement("移動先到達", nodeName, context);
BTLogger.LogParser("パース成功", nodeName, context);

// レベル指定ログ
BTLogger.Log(LogLevel.Warning, LogCategory.System, "警告メッセージ");
BTLogger.Log(LogLevel.Error, LogCategory.Parser, "エラーメッセージ");

// Debug.Logからの移行用
BTLogger.Info("情報メッセージ");
BTLogger.Warning("警告メッセージ");
BTLogger.Error("エラーメッセージ");
```

### 出力フォーマット
```
[INF][SYS]: システム情報メッセージ
[ERR][PRS]: パーサーエラーメッセージ  
[WRN][MOV]: 移動関連警告
[DBG][CMB][NodeName]: 戦闘デバッグ情報
```

## RPGサンプルノード詳細

### RPG戦闘アクション
- **AttackAction**: 基本物理攻撃（ダメージ、射程設定可能）
- **AttackEnemyAction**: 特定敵への攻撃（BlackBoard連携）
- **CastSpellAction**: 魔法詠唱システム（マナ消費、各種呪文対応）
- **FleeToSafetyAction**: 安全地帯への逃走（SafeZoneタグ検索）

### RPG条件ノード
- **HealthCheckCondition**: 体力閾値チェック（実装済み）
- **HasManaCondition**: マナ量確認（実装済み）
- **EnemyInRangeCondition**: 敵との距離判定
- **EnemyHealthCheckCondition**: 敵の体力監視（実装済み）
- **HasItemCondition**: アイテム所持確認
- **IsInitializedCondition**: 初期化状態確認（実装済み）

### 使用例：RPG戦闘AI
```bt
tree RPGCombatAI {
    Selector main {
        # 緊急時：体力が25%以下で回復アイテム使用
        Sequence emergency_heal {
            Condition HealthCheck { min_health: 25 }
            Condition HasItem { item_type: "healing_potion", min_quantity: 1 }
            Action UseItem { item_type: "healing_potion" }
        }
        
        # 魔法攻撃：マナが30以上で敵が8m以内
        Sequence magic_attack {
            Condition HasMana { required_mana: 30 }
            Condition EnemyInRange { max_distance: 8.0 }
            Action CastSpell { spell_name: "fireball", mana_cost: 30, damage: 50 }
        }
        
        # 物理攻撃：敵が2m以内
        Sequence physical_attack {
            Condition EnemyInRange { max_distance: 2.0 }
            Action AttackEnemy { damage: 25, attack_speed: 1.2 }
        }
        
        # 逃走：体力が低い場合
        Sequence flee_sequence {
            Condition HealthCheck { min_health: 30 }
            Action FleeToSafety { min_distance: 15.0, speed_multiplier: 1.5 }
        }
        
        # 探索：デフォルト行動
        Action SearchForEnemy { search_radius: 12.0 }
    }
}
```

## 完全な例

```bt
tree GuardAI {
    Sequence main_loop {
        # 体力チェック（動的監視）
        Condition HealthCheck {
            min_health: 30
        }
        
        # メイン行動の選択
        Selector behavior_selection {
            # 戦闘行動（体力監視付き）
            Sequence combat_behavior {
                Condition EnemyCheck {
                    detection_range: 15.0
                }
                
                Sequence attack_sequence {
                    Action MoveToPosition {
                        target: "detected_enemy"
                        speed: 4.0
                        tolerance: 2.0
                    }
                    
                    Action AttackEnemy {
                        damage: 30
                        cooldown: 1.5
                    }
                }
            }
            
            # パトロール行動
            Sequence patrol_behavior {
                Action MoveToPosition {
                    target: "patrol_1"
                    speed: 2.5
                    tolerance: 1.0
                }
                
                Action Wait {
                    duration: 3.0
                }
                
                Action MoveToPosition {
                    target: "patrol_2"
                    speed: 2.5
                    tolerance: 1.0
                }
                
                Action Wait {
                    duration: 2.0
                }
            }
            
            # 待機行動（フォールバック）
            Action Wait {
                duration: 1.0
            }
        }
    }
}
```

## コメント
- `#` で始まる行はコメント
- 行の途中からのコメントは未対応

## VSCode機能
- **シンタックスハイライト**: キーワードとプロパティの色分け
- **自動補完**: ノードタイプ、プロパティ名、スクリプト名の補完
- **ホバーヘルプ**: キーワードにマウスを置くと説明表示
- **スニペット**: よく使うパターンの自動挿入
- **構文チェック**: 基本的な構文エラーの検出

## スクリプトの実装

### 1. スクリプト名とは
.btファイル内のノード名（例：`Action MoveToPosition`）が、Unity側で作成するC#クラス名になります。

### 2. Action用スクリプトの作成例（BlackBoard対応）
```csharp
// Assets/Scripts/BehaviourTree/Actions/MoveToPosition.cs
using UnityEngine;
using BehaviourTree.Core;

[System.Serializable]
public class MoveToPositionAction : BTActionNode
{
    [SerializeField] string target;
    [SerializeField] float speed = 3.5f;
    [SerializeField] float tolerance = 0.5f;
    
    protected override BTNodeResult ExecuteAction()
    {
        // target位置への移動ロジック
        Vector3 targetPosition = GetTargetPosition(target);
        
        if (Vector3.Distance(transform.position, targetPosition) <= tolerance)
        {
            // BlackBoardに到達状態を記録
            if (blackBoard != null)
            {
                blackBoard.SetValue($"{Name}_reached_target", true);
            }
            return BTNodeResult.Success;
        }
        
        // 移動処理
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        
        // BlackBoardに移動状態を記録
        if (blackBoard != null)
        {
            blackBoard.SetValue($"{Name}_is_moving", true);
            blackBoard.SetValue($"{Name}_current_distance", 
                Vector3.Distance(transform.position, targetPosition));
        }
        
        return BTNodeResult.Running;
    }
    
    protected override void OnConditionFailed()
    {
        // 条件失敗時の処理（動的条件チェック）
        if (blackBoard != null)
        {
            blackBoard.SetValue($"{Name}_is_moving", false);
            blackBoard.SetValue($"{Name}_stopped_reason", "condition_failed");
        }
    }
    
    private Vector3 GetTargetPosition(string targetName)
    {
        GameObject targetObj = GameObject.Find(targetName);
        return targetObj ? targetObj.transform.position : transform.position;
    }
}
```

### 3. Condition用スクリプトの作成例（BlackBoard対応）
```csharp
// Assets/Scripts/BehaviourTree/Conditions/HealthCheck.cs
using UnityEngine;
using BehaviourTree.Core;

[System.Serializable]
public class HealthCheckCondition : BTConditionNode
{
    [SerializeField] int minHealth = 50;
    
    protected override BTNodeResult CheckCondition()
    {
        // 体力コンポーネントを取得（例）
        Health healthComponent = GetComponent<Health>();
        if (healthComponent == null)
            return BTNodeResult.Failure;
        
        var currentHealth = healthComponent.CurrentHealth;
        var healthOK = currentHealth >= minHealth;
        
        // BlackBoardに健康状態を記録
        if (blackBoard != null)
        {
            blackBoard.SetValue("current_health", currentHealth);
            blackBoard.SetValue("health_status", healthOK ? "healthy" : "low_health");
            blackBoard.SetValue("min_health_threshold", minHealth);
        }
            
        return healthOK ? BTNodeResult.Success : BTNodeResult.Failure;
    }
}
```

### 4. スクリプト登録システム
BTパーサーは文字列からクラスを動的に生成します：

```csharp
// BTParser.cs内での実装例
private BTNode CreateNodeFromScript(string scriptName, Dictionary<string, object> properties)
{
    // クラス名にサフィックスを追加
    string className = scriptName + (isAction ? "Action" : "Condition");
    
    // リフレクションでクラスを取得・生成
    System.Type scriptType = System.Type.GetType(className);
    if (scriptType != null)
    {
        BTNode nodeInstance = (BTNode)System.Activator.CreateInstance(scriptType);
        
        // プロパティを設定
        foreach (var prop in properties)
        {
            SetProperty(nodeInstance, prop.Key, prop.Value);
        }
        
        return nodeInstance;
    }
    
    Debug.LogError($"Script class not found: {className}");
    return null;
}
```

## Unity側での使用方法
```csharp
// C#コードでの読み込み例
BehaviourTreeRunner runner = GetComponent<BehaviourTreeRunner>();
runner.LoadBehaviourTree("Assets/BehaviourTrees/GuardAI.bt");

// BlackBoardにアクセス
BlackBoard blackBoard = runner.BlackBoard;
blackBoard.SetValue("player_position", playerTransform.position);
Vector3 pos = blackBoard.GetValue<Vector3>("player_position");
```

## デバッグ機能
- **Inspector右クリックメニュー**:
  - "Show BlackBoard Contents" - BlackBoardの内容を表示
  - "Clear BlackBoard" - BlackBoardをクリア
  - "Reset Tree State" - ツリー状態をリセット
  - "Reload Behaviour Tree" - .btファイルを再読み込み

## 新しいスクリプトの追加手順（最新版）
1. `Assets/ArcBT/Runtime/Actions/`、`Assets/ArcBT/Runtime/Conditions/`、または`Assets/ArcBT/Samples/RPGExample/`配下にC#スクリプトを作成（1ファイル1クラス）
2. `ArcBT.Core.BTActionNode`または`ArcBT.Core.BTConditionNode`を継承
3. **BTNode属性を追加**: `[BTNode("ScriptName")]` （NodeTypeは基底クラスから自動判定）
4. `SetProperty(string key, string value)` メソッドをオーバーライド（パラメータは`string`型）
5. `ExecuteAction()` または `protected override BTNodeResult CheckCondition()` をオーバーライド
6. `Initialize(MonoBehaviour owner, BlackBoard blackBoard)` メソッドをオーバーライド
7. BlackBoard機能を使用する場合は`blackBoard.SetValue()`/`GetValue()`を活用
8. 動的条件チェックに対応する場合は`OnConditionFailed()`をオーバーライド
9. **自動登録**: ソースジェネレーターが自動的に `{AssemblyName}.NodeRegistration.g.cs` を生成
10. .btファイルで`Action ScriptName { ... }`または`Condition ScriptName { ... }`として使用（script属性不要）

### BTNode属性の使用例
```csharp
[BTNode("SimpleAttack")]
public class SimpleAttackAction : BTActionNode
{
    // NodeTypeは自動的にActionと判定される
}

[BTNode("EnemyDetection")]  
public class EnemyDetectionCondition : BTConditionNode
{
    // NodeTypeは自動的にConditionと判定される
}
```

## 新機能の活用例

### BlackBoardを使ったデータ共有

#### 1. 基本的なデータ共有
```bt
tree DataSharingExample {
    Sequence main {
        Action ScanEnvironment {
            # 環境をスキャンして敵の位置をBlackBoardに保存
            scan_radius: 15.0
        }
        
        Selector movement_behavior {
            # 敵が見つかった場合
            Sequence move_to_enemy {
                Condition HasSharedEnemyInfo {
                    # BlackBoardの"enemy_location"をチェック
                }
                
                Action MoveToEnemy {
                    # BlackBoardから敵の位置を取得して移動
                    speed: 4.0
                    tolerance: 1.5
                }
                
                Action AttackTarget {
                    # BlackBoardの敵情報を使用して攻撃
                    damage: 30
                }
            }
            
            # 敵が見つからない場合
            Action RandomWander {
                wander_radius: 10.0
                speed: 2.0
            }
        }
    }
}
```

#### 2. 複数AIでの情報共有
```bt
tree TeamCoordinationAI {
    Parallel team_behavior {
        success_policy: "require_one"
        failure_policy: "require_all"
        
        # 偵察役：敵情報をBlackBoardに共有
        Sequence scout_role {
            Action PatrolArea {
                patrol_radius: 20.0
            }
            
            Action ReportEnemyLocation {
                # 発見した敵の位置を"enemy_location"キーで共有
            }
        }
        
        # 戦闘役：共有された敵情報を使用
        Sequence combat_role {
            Condition HasSharedEnemyInfo {
                # BlackBoardの"enemy_location"をチェック
            }
            
            Action AttackSharedTarget {
                # 共有された敵位置に攻撃
            }
        }
    }
}
```

#### 3. 状態管理とフラグ制御
```bt
tree StateManagedAI {
    Selector main {
        # アラート状態での行動
        Sequence alert_behavior {
            Condition CheckAlertFlag {
                # BlackBoardの"is_alert"フラグをチェック
            }
            
            Action SearchForThreat {
                search_intensity: "high"
            }
        }
        
        # 通常パトロール
        Sequence normal_patrol {
            Action PatrolRoute {
                # パトロール中にアラートフラグを監視
                route: "standard_route"
            }
            
            Action SetPatrolComplete {
                # パトロール完了をBlackBoardに記録
            }
        }
    }
}
```

#### 4. リソース管理
```bt
tree ResourceManagementAI {
    Selector resource_behavior {
        # マナが十分な場合の魔法攻撃
        Sequence magic_attack {
            Condition CheckManaResource {
                # BlackBoardの"current_mana"をチェック
                min_mana: 50
            }
            
            Action CastFireball {
                # マナを消費してBlackBoardを更新
                mana_cost: 50
                damage: 60
            }
        }
        
        # マナ不足時の物理攻撃
        Sequence physical_attack {
            Action MeleeAttack {
                damage: 25
            }
            
            Action RestoreSmallMana {
                # 少量のマナ回復をBlackBoardに反映
                mana_gain: 10
            }
        }
    }
}
```

#### 5. C#実装例：BlackBoardを活用するAction
```csharp
// 環境スキャンしてBlackBoardに敵情報を保存
[System.Serializable]
public class ScanEnvironmentAction : BTActionNode
{
    [SerializeField] float scanRadius = 15.0f;
    
    protected override BTNodeResult ExecuteAction()
    {
        // 敵を検索
        var enemies = FindEnemiesInRadius(scanRadius);
        
        if (enemies.Count > 0)
        {
            // BlackBoardに敵の位置を保存
            blackBoard.SetValue("enemy_count", enemies.Count);
            blackBoard.SetValue("nearest_enemy_position", enemies[0].transform.position);
            blackBoard.SetValue("nearest_enemy_health", enemies[0].GetComponent<Health>().CurrentHealth);
            blackBoard.SetValue("last_scan_time", Time.time);
            
            return BTNodeResult.Success;
        }
        
        // 敵が見つからない場合はBlackBoardをクリア
        blackBoard.SetValue("enemy_count", 0);
        blackBoard.RemoveValue("nearest_enemy_position");
        
        return BTNodeResult.Failure;
    }
}

// BlackBoardの敵情報を使用して移動
[System.Serializable]
public class MoveToEnemyAction : BTActionNode
{
    [SerializeField] float speed = 4.0f;
    [SerializeField] float tolerance = 1.5f;
    
    protected override BTNodeResult ExecuteAction()
    {
        // BlackBoardから敵位置を取得
        if (!blackBoard.HasKey("nearest_enemy_position"))
        {
            return BTNodeResult.Failure;
        }
        
        Vector3 enemyPosition = blackBoard.GetValue<Vector3>("nearest_enemy_position");
        float distance = Vector3.Distance(transform.position, enemyPosition);
        
        if (distance <= tolerance)
        {
            // 到達をBlackBoardに記録
            blackBoard.SetValue("reached_enemy", true);
            return BTNodeResult.Success;
        }
        
        // 敵に向かって移動
        Vector3 direction = (enemyPosition - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        
        // 移動状態をBlackBoardに記録
        blackBoard.SetValue("is_moving_to_enemy", true);
        blackBoard.SetValue("distance_to_enemy", distance);
        
        return BTNodeResult.Running;
    }
}

// チーム連携用：BlackBoardでアラート状態を管理
[System.Serializable]
public class CheckAlertFlagCondition : BTConditionNode
{
    protected override BTNodeResult CheckCondition()
    {
        // 他のAIがアラートフラグを設定したかチェック
        bool isAlert = blackBoard.GetValue<bool>("team_alert_status", false);
        string alertReason = blackBoard.GetValue<string>("alert_reason", "unknown");
        
        if (isAlert)
        {
            Debug.Log($"Alert detected: {alertReason}");
            return BTNodeResult.Success;
        }
        
        return BTNodeResult.Failure;
    }
}
```

### 動的条件チェックの活用
```bt
tree DynamicExample {
    Selector main {
        # 体力が十分な場合のみ攻撃（実行中も監視）
        Sequence healthy_combat {
            Condition HealthCheck {
                min_health: 50
            }
            
            Action AttackEnemy {  # 体力が50未満になると即座に中断
                damage: 25
            }
        }
        
        # 体力が低い場合は逃走
        Action FleeToSafety {
            speed: 5.0
        }
    }
}
```

## BlackBoard活用のベストプラクティス

### 1. キー命名規則
```csharp
// 推奨：明確で一貫した命名
blackBoard.SetValue("player_last_seen_position", playerPos);
blackBoard.SetValue("current_health_percentage", healthPercent);
blackBoard.SetValue("is_combat_mode", true);

// 非推奨：曖昧な命名
blackBoard.SetValue("pos", playerPos);
blackBoard.SetValue("hp", healthPercent);
blackBoard.SetValue("mode", true);
```

### 2. 型安全性の確保
```csharp
// 推奨：デフォルト値を指定
Vector3 enemyPos = blackBoard.GetValue<Vector3>("enemy_position", Vector3.zero);
int health = blackBoard.GetValue<int>("current_health", 100);

// 推奨：存在チェック
if (blackBoard.HasKey("enemy_position"))
{
    Vector3 pos = blackBoard.GetValue<Vector3>("enemy_position");
    // 処理...
}
```

### 3. データのライフサイクル管理
```bt
tree DataLifecycleExample {
    Sequence main {
        Action InitializeData {
            # ゲーム開始時にBlackBoardを初期化
        }
        
        Selector game_loop {
            Sequence combat {
                Condition EnemyDetected {
                    # 敵発見時のデータ設定
                }
                
                Action CombatBehavior {
                    # 戦闘データの更新
                }
                
                Action CleanupCombatData {
                    # 戦闘終了後のデータクリア
                }
            }
            
            Action IdleBehavior {
                # 待機中のデータ管理
            }
        }
    }
}
```

### 4. デバッグ支援
```csharp
// デバッグ用のBlackBoard監視Action
[System.Serializable]
public class DebugBlackBoardAction : BTActionNode
{
    protected override BTNodeResult ExecuteAction()
    {
        if (blackBoard != null)
        {
            blackBoard.DebugLog();
        }
        return BTNodeResult.Success;
    }
}
```

## MovementController統一システム

### 概要
Unity 6 + URPプロジェクトでは、AI行動判定（0.1秒間隔）と移動描画（毎フレーム）を分離したMovementControllerシステムを採用しています。

### 対応済み移動アクション
- **MoveToTarget** - investigate/enemy/currentタイプに対応
- **FleeToSafety** - 安全地帯（SafeZoneタグ）への逃走
- **RandomWander** - 初期位置中心のランダム徘徊

### 技術仕様
```csharp
// MovementControllerを使用した滑らかな移動
movementController.SetTarget(targetPosition, speed);
movementController.OnTargetReached = () => {
    // 到達時のコールバック処理
};

// 移動状態の確認
bool isMoving = movementController.IsMoving;
float distanceToTarget = movementController.DistanceToTarget;
```

### .btファイルでの使用例
```bt
Action MoveToTarget {
    move_type: "investigate"
    speed: 20.0
    tolerance: 1.0
}

Action FleeToSafety {
    min_distance: 20.0
    speed_multiplier: 2.0
}

Action RandomWander {
    wander_radius: 10.0
    speed: 25.0
    tolerance: 0.5
}
```

### 利点
- **滑らかな移動**: 毎フレーム更新によるガクガク感の解消
- **統一された制御**: 全移動アクションで一貫したMovementController使用
- **パフォーマンス最適化**: AI判定とレンダリングの分離
- **回転の滑らかさ**: Quaternion.Slerpによる自然な向き変更