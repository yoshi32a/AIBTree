# AIBTree - Behaviour Tree System for Unity

BehaviourTreeで動くAIシステムです。階層型の`.bt`ファイル形式を使用し、VSCodeでシンタックスハイライトと入力補完をサポートします。

## 特徴

- **直感的な階層型記述**: ネストした構文でツリー構造が一目で分かる
- **VSCode完全対応**: シンタックスハイライト、入力補完、エラーチェック
- **Unity統合**: C#で実装され、Unityで直接実行可能
- **拡張可能**: カスタムノードとスクリプトを簡単に追加

## プロジェクト構成

```
Assets/
├── Scripts/
│   └── BehaviourTree/
│       ├── Core/                 # 基本クラス
│       ├── Nodes/                # ノード実装
│       ├── Parser/               # btファイルパーサー
│       └── Examples/             # サンプルAI
├── BehaviourTrees/               # btファイル保存場所
│   ├── example.bt
│   ├── simple_example.bt
│   └── complex_example.bt
vscode-bt-extension/              # VSCode拡張機能
├── package.json
├── syntaxes/
├── snippets/
└── src/
```

## btファイル形式

### 基本構文

```bt
tree TreeName {
    # ノードタイプ ノード名 { プロパティとネストした子ノード }
    sequence root {
        condition health_check {
            script: "HealthCheck"
            min_health: 50
        }
        
        action move {
            script: "MoveToPosition"
            target: "patrol_point_1"
            speed: 3.5
        }
    }
}
```

### サポートするノードタイプ

- **sequence**: 全ての子ノードが成功するまで順次実行
- **selector**: いずれかの子ノードが成功するまで実行
- **action**: 実際のアクションを実行
- **condition**: 条件をチェック

### よく使用するプロパティ

- `script`: 実行するスクリプト名
- `target`: 移動先やターゲット名
- `speed`: 移動速度
- `damage`: ダメージ量
- `duration`: 実行時間
- `min_health`: 最小体力閾値
- `detection_range`: 検出範囲

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

1. GameObjectに`BehaviourTreeRunner`と`ExampleAI`コンポーネントを追加
2. `Behaviour Tree File Path`に`.bt`ファイル名を設定（例：`example.bt`）
3. パトロールポイントやターゲットを設定

### 2. カスタムAIの作成

```csharp
public class MyCustomAI : MonoBehaviour
{
    private BehaviourTreeRunner treeRunner;

    void Start()
    {
        treeRunner = GetComponent<BehaviourTreeRunner>();
        treeRunner.LoadBehaviourTree("my_ai.bt");
    }
    
    // btファイルから呼び出されるメソッド
    public bool MyCondition()
    {
        return someCondition;
    }
    
    public void MyAction()
    {
        // アクション実装
    }
}
```

### 3. カスタムノードの追加

新しいノードタイプを追加する場合：

1. `BTNode`を継承したクラスを作成
2. `BTParser.CreateNode()`メソッドに追加
3. VSCode拡張のキーワードリストに追加

## サンプル

### シンプルなAI

```bt
tree SimpleAI {
    selector root {
        sequence attack_behavior {
            condition has_target {
                script: "HasTarget"
            }
            action attack {
                script: "Attack"
                damage: 10
            }
        }
        action search_for_enemy {
            script: "SearchForEnemy"
            search_radius: 15.0
        }
    }
}
```

### 複雑なパトロールAI

```bt
tree PatrolAI {
    sequence root {
        condition check_health {
            script: "HealthCheck"
            min_health: 50
        }
        
        selector main_behavior {
            sequence combat_sequence {
                condition enemy_detected {
                    script: "EnemyCheck"
                    detection_range: 10.0
                }
                action attack_enemy {
                    script: "AttackEnemy"
                    damage: 25
                    attack_range: 2.0
                }
            }
            
            sequence patrol_sequence {
                action move_to_patrol {
                    script: "MoveToPosition"
                    target: "patrol_point_1"
                    speed: 3.5
                    tolerance: 0.5
                }
                action wait_at_point {
                    script: "Wait"
                    duration: 2.0
                }
            }
        }
    }
}
```

## 開発・拡張

### 新しいスクリプトの追加

1. `CustomActionNode`または`CustomConditionNode`の`switch`文に追加
2. `ExampleAI`クラスに対応するメソッドを追加
3. VSCode拡張の補完リストに追加

### デバッグ

- `BehaviourTreeRunner`の`Debug Mode`を有効にしてコンソール出力を確認
- Unity Inspectorで右クリック → `Reload Behaviour Tree`でファイルを再読み込み
- Unity Inspectorで右クリック → `Reset Tree State`でツリー状態をリセット

## ライセンス

MIT License

## 貢献

プルリクエストやイシューの報告を歓迎します。

## 今後の予定

- [ ] 並列実行ノード（parallel）の実装
- [ ] デコレータノード（repeat, timeout, etc.）の実装
- [ ] ビジュアルエディタの開発
- [ ] より詳細なデバッグ機能
- [ ] パフォーマンス最適化