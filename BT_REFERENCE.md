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

### Condition専用プロパティ
- `min_health: 50` - 最小体力しきい値
- `detection_range: 10.0` - 検出範囲
- `bb_key: "key_name"` - BlackBoardのキー名（HasSharedEnemyInfo用）
- `min_mana: 30` - 最小マナ量（未実装）

### Parallel専用プロパティ
- `success_policy: "require_one"` - 成功条件（require_one/require_all）
- `failure_policy: "require_all"` - 失敗条件（require_one/require_all）

## 新機能

### 1. BlackBoard システム
- AIノード間でデータを共有するグローバルストレージ
- 型安全な値の設定・取得が可能
- デバッグ機能付き

```csharp
// C#でのBlackBoard使用例
blackBoard.SetValue("player_health", 75);
int health = blackBoard.GetValue<int>("player_health", 100);
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

## 利用可能スクリプト

### Action用スクリプト
- `MoveToPosition` - 指定位置への移動
- `Wait` - 指定時間待機
- `AttackEnemy` - 敵への攻撃
- `ScanEnvironment` - 環境スキャンして敵情報をBlackBoardに保存
- `MoveToEnemy` - BlackBoardから敵位置を取得して移動
- `AttackTarget` - BlackBoardの敵情報を使用して攻撃
- `RandomWander` - ランダム徘徨
- `UseItem` - アイテム使用（未実装）
- `FleeToSafety` - 安全地帯への逃走（未実装）
- `Interact` - オブジェクトとの相互作用（未実装）

### Condition用スクリプト
- `HealthCheck` - 体力チェック
- `EnemyCheck` - 敵検出
- `HasItem` - アイテム所持確認
- `HasSharedEnemyInfo` - BlackBoardに共有された敵情報の有無をチェック
- `HasTarget` - ターゲット所持確認（未実装）
- `HasMana` - マナ量確認（未実装）
- `IsInitialized` - 初期化状態確認（未実装）
- `EnemyHealthCheck` - 敵の体力確認（未実装）

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

## 新しいスクリプトの追加手順
1. `Assets/Scripts/BehaviourTree/Actions/`または`Conditions/`フォルダにC#スクリプトを作成（1ファイル1クラス）
2. `BTActionNode`または`BTConditionNode`を継承
3. `SetProperty(string key, string value)` メソッドをオーバーライド（パラメータは`string`型）
4. `ExecuteAction()` または `protected override BTNodeResult CheckCondition()` をオーバーライド
5. BlackBoard機能を使用する場合は`blackBoard.SetValue()`/`GetValue()`を活用
6. 動的条件チェックに対応する場合は`OnConditionFailed()`をオーバーライド
7. `BTParser.cs` の `CreateNodeFromScript()` メソッドにケースを追加
8. .btファイルで`Action NewScript { ... }`または`Condition NewScript { ... }`として使用（script属性不要）

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