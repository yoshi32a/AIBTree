# AIBTree - Advanced Behaviour Tree System for Unity

Unity用の高度なBehaviourTreeシステムです。BlackBoardによるデータ共有、動的条件チェック、VSCode完全対応を特徴とします。

## ✨ 主要機能

- **🧠 BlackBoardシステム**: AIノード間でのリアルタイムデータ共有
- **⚡ 動的条件チェック**: Action実行中の条件変化に即座に対応
- **🌲 階層型.btファイル**: 直感的な階層構造でAI定義
- **🔧 VSCode完全対応**: シンタックスハイライト、スニペット、自動補完
- **⚙️ Unity統合**: C#による高性能な実行エンジン
- **🚀 並列実行**: Parallelノードによる複数行動の同時実行

## プロジェクト構成

```
Assets/
├── Scripts/
│   └── BehaviourTree/
│       ├── Core/                 # コアシステム
│       │   ├── BTNode.cs        # ベースノード（BlackBoard対応）
│       │   ├── BlackBoard.cs    # データ共有システム
│       │   ├── BTActionNode.cs  # アクション基底（動的条件対応）
│       │   ├── BTConditionNode.cs
│       │   ├── BTSequenceNode.cs # Sequenceノード
│       │   ├── BTSelectorNode.cs # Selectorノード
│       │   └── BTParallelNode.cs # Parallelノード
│       ├── Actions/              # アクション実装
│       │   ├── MoveToPositionAction.cs
│       │   ├── AttackEnemyAction.cs
│       │   └── WaitAction.cs
│       ├── Conditions/           # 条件実装
│       │   ├── HealthCheckCondition.cs
│       │   ├── EnemyCheckCondition.cs
│       │   └── HasItemCondition.cs
│       ├── Parser/               # .btファイルパーサー
│       └── Components/           # 汎用コンポーネント
├── BehaviourTrees/               # .btファイルとサンプル
│   ├── blackboard_sample.bt      # BlackBoard基本例
│   ├── team_coordination_sample.bt # チーム連携例
│   ├── resource_management_sample.bt # リソース管理例
│   └── dynamic_condition_sample.bt # 動的条件例
vscode-bt-extension/              # VSCode拡張機能 v1.1.0
├── package.json
├── syntaxes/bt.tmLanguage.json   # 新形式対応
├── snippets/bt.json              # BlackBoard対応スニペット
└── src/extension.js
```

## btファイル形式

### 基本構文

```bt
tree TreeName {
    # ノードタイプ ノード名 { プロパティとネストした子ノード }
    Sequence root {
        Condition health_check {
            script: "HealthCheck"
            min_health: 50
        }
        
        Action move {
            script: "MoveToPosition"
            target: "patrol_point_1"
            speed: 3.5
        }
    }
}
```

### サポートするノードタイプ

- **Sequence**: 全ての子ノードが成功するまで順次実行
- **Selector**: いずれかの子ノードが成功するまで実行
- **Parallel**: 複数の子ノードを並行実行
- **Action**: 実際のアクションを実行
- **Condition**: 条件をチェック

### よく使用するプロパティ

- `script`: 実行するスクリプト名
- `target`: 移動先やターゲット名
- `speed`: 移動速度
- `damage`: ダメージ量
- `duration`: 実行時間
- `min_health`: 最小体力閾値
- `detection_range`: 検出範囲
- `bb_key`: BlackBoardのキー名（データ読み書き用）
- `bb_value`: BlackBoardに書き込む値

## VSCode拡張機能のインストール

### 1. 拡張機能のビルド

```bash
cd vscode-bt-extension
npm install
```

### 2. VSCodeでの開発モード実行

1. VSCodeで`vscode-bt-extension`フォルダを開く
2. F5キーを押して拡張開発ホストを起動
3. 新しいVSCodeウィンドウが開く
4. `.bt`ファイルを開いてシンタックスハイライトを確認

### 3. 拡張機能のパッケージ化

```bash
npm install -g vsce
vsce package
```

生成された`.vsix`ファイルをVSCodeにインストール：
```bash
code --install-extension behaviour-tree-language-1.0.0.vsix
```

## Unity使用方法

### 1. BehaviourTreeRunnerの設定

1. GameObjectに`BehaviourTreeRunner`と`Health`コンポーネントを追加
2. `Behaviour Tree File Path`に`.bt`ファイル名を設定（例：`blackboard_sample.bt`）
3. BlackBoardとHealthコンポーネントが自動的に連携されます

### 2. BlackBoardの使用方法

```csharp
public class MyCustomAI : MonoBehaviour
{
    private BehaviourTreeRunner treeRunner;
    private BlackBoard blackBoard;

    void Start()
    {
        treeRunner = GetComponent<BehaviourTreeRunner>();
        blackBoard = treeRunner.GetBlackBoard();
        
        // 初期値を設定
        blackBoard.SetValue("enemy_target", (GameObject)null);
        blackBoard.SetValue("patrol_index", 0);
        
        treeRunner.LoadBehaviourTree("my_ai.bt");
    }
    
    // BlackBoardから値を取得
    public GameObject GetEnemyTarget()
    {
        return blackBoard.GetValue<GameObject>("enemy_target");
    }
    
    // BlackBoardに値を設定
    public void SetEnemyTarget(GameObject target)
    {
        blackBoard.SetValue("enemy_target", target);
    }
}
```

### 3. カスタムノードの追加

新しいノードタイプを追加する場合：

1. `BTNode`を継承したクラスを作成
2. `BTParser.CreateNode()`メソッドに追加
3. VSCode拡張のキーワードリストに追加

## サンプル

### BlackBoard使用例

```bt
tree BlackBoardAI {
    Sequence root {
        Condition has_target {
            script: "HasItem"
            bb_key: "enemy_target"
        }
        Action attack {
            script: "AttackEnemy"
            damage: 10
            bb_target_key: "enemy_target"
        }
    }
}
```

### 動的条件チェック付きAI

```bt
tree DynamicPatrolAI {
    Sequence root {
        Condition check_health {
            script: "HealthCheck"
            min_health: 50
        }
        
        Selector main_behavior {
            Sequence combat_sequence {
                Condition enemy_detected {
                    script: "EnemyCheck"
                    detection_range: 10.0
                    bb_key: "enemy_target"
                }
                Action attack_enemy {
                    script: "AttackEnemy"
                    damage: 25
                    attack_range: 2.0
                    # この条件が満たされなくなったら中断
                    dynamic_conditions: ["enemy_detected"]
                }
            }
            
            Sequence patrol_sequence {
                Action move_to_patrol {
                    script: "MoveToPosition"
                    bb_key: "current_patrol_point"
                    speed: 3.5
                    tolerance: 0.5
                }
                Action wait_at_point {
                    script: "Wait"
                    duration: 2.0
                }
            }
        }
    }
}
```

## BlackBoard機能

### 基本的な使用方法

```csharp
// 値の設定
blackBoard.SetValue("player_health", 100);
blackBoard.SetValue("enemy_position", transform.position);
blackBoard.SetValue("has_key", true);

// 値の取得
int health = blackBoard.GetValue<int>("player_health", 0);
Vector3 pos = blackBoard.GetValue<Vector3>("enemy_position");
bool hasKey = blackBoard.GetValue<bool>("has_key", false);

// キーの存在確認
if (blackBoard.HasKey("enemy_target"))
{
    // 処理
}
```

### 動的条件チェック

Actionノード実行中に条件が変化した場合、自動的に行動を中断して上位ノードに制御を戻します：

```csharp
public class MyAction : BTActionNode
{
    protected override BTNodeResult ExecuteAction()
    {
        // 長時間実行されるアクション
        while (IsRunning())
        {
            // 監視中の条件が失敗したら自動的に中断
            if (AreWatchedConditionsFailing())
            {
                OnConditionFailed();
                return BTNodeResult.Failure;
            }
            
            // アクション処理
            DoAction();
        }
        
        return BTNodeResult.Success;
    }
}
```

## 開発・拡張

### 新しいActionノードの作成

1. `Assets/Scripts/BehaviourTree/Actions/`に新しいクラスを作成
2. `BTActionNode`を継承
3. `ExecuteAction()`メソッドを実装
4. `SetProperty()`でプロパティ処理を追加

### 新しいConditionノードの作成

1. `Assets/Scripts/BehaviourTree/Conditions/`に新しいクラスを作成
2. `BTConditionNode`を継承
3. `CheckCondition()`メソッドを実装
4. `SetProperty()`でプロパティ処理を追加

### デバッグ

- `BehaviourTreeRunner`の`Debug Mode`を有効にしてコンソール出力を確認
- Unity Inspectorで右クリック → `Show BlackBoard Contents`でデータ確認
- Unity Inspectorで右クリック → `Clear BlackBoard`でデータクリア
- Unity Inspectorで右クリック → `Reload Behaviour Tree`でファイルを再読み込み

## ライセンス

MIT License

## 貢献

プルリクエストやイシューの報告を歓迎します。

## 実装済み機能

- [x] BlackBoardシステム（データ共有）
- [x] 動的条件チェック（実行中の条件監視）
- [x] 並列実行ノード（Parallel）の実装
- [x] VSCode完全対応（v1.1.0）
- [x] 複合ノード（Sequence, Selector, Parallel）
- [x] デバッグ機能（BlackBoard表示、ツリー状態管理）

## 今後の予定

- [ ] デコレータノード（repeat, timeout, inverter等）の実装
- [ ] ビジュアルエディタの開発
- [ ] より詳細なパフォーマンス解析機能
- [ ] ネットワーク対応（マルチプレイヤーAI）
- [ ] AI行動の録画・再生機能