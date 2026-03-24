using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ArcBT.Core;

namespace ArcBT.Editor
{
    /// <summary>
    /// BehaviourTree のリアルタイムデバッガーウィンドウ（UIToolkit版）。
    /// 選択中の GameObject の BehaviourTreeRunner からツリー構造とBlackBoard状態を可視化する。
    /// </summary>
    public class BTTreeDebuggerWindow : EditorWindow
    {
        // 現在デバッグ対象の Runner
        BehaviourTreeRunner targetRunner;

        // ノードごとの最新実行結果を追跡
        readonly Dictionary<BTNode, BTNodeResult> lastResults = new();

        // 前回の Execute 結果をキャプチャするためのラップ用（フレーム単位で更新）
        readonly HashSet<BTNode> visitedThisFrame = new();

        // クリックで選択されたノード
        BTNode selectedNode;

        // ノード→VisualElement のマッピング（状態更新用）
        readonly Dictionary<BTNode, VisualElement> nodeRowElements = new();

        // ツリー構造のルートコンテナ
        VisualElement treeContainer;

        // 右パネルの参照
        VisualElement detailPanel;
        VisualElement detailContent;
        VisualElement blackboardContent;
        VisualElement blackboardWarning;
        VisualElement blackboardEmpty;
        ScrollView blackboardScroll;
        Label targetLabel;
        Label playStateLabel;
        VisualElement emptyState;
        VisualElement noRootMessage;
        TwoPaneSplitView splitView;

        // 定期リフレッシュの制御
        IVisualElementScheduledItem refreshSchedule;

        // 深さクラスの最大値（USS で定義済み）
        const int MaxDepthClass = 10;

        [MenuItem("Window/BehaviourTree/Tree Debugger")]
        static void ShowWindow()
        {
            var window = GetWindow<BTTreeDebuggerWindow>();
            window.titleContent = new GUIContent("BT Debugger");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
        }

        void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        /// <summary>
        /// UIToolkit のエントリポイント。ウィンドウ生成時に一度だけ呼ばれる。
        /// </summary>
        public void CreateGUI()
        {
            var root = rootVisualElement;

            // USS を読み込み
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/ArcBT/Editor/BTTreeDebuggerWindow.uss");
            if (styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
            }

            root.AddToClassList("debugger-root");

            // ツールバー
            BuildToolbar(root);

            // メインコンテンツ領域
            var mainContent = new VisualElement();
            mainContent.AddToClassList("main-content");
            root.Add(mainContent);

            // 空状態メッセージ
            BuildEmptyState(mainContent);

            // ルートノード未設定メッセージ
            BuildNoRootMessage(mainContent);

            // TwoPaneSplitView（左: ツリー, 右: 詳細+BlackBoard）
            BuildSplitView(mainContent);

            // Play Mode 中の定期リフレッシュ
            refreshSchedule = root.schedule.Execute(OnScheduledRefresh).Every(100);

            // 初期状態の適用
            OnSelectionChanged();
        }

        // --- ツールバー構築 ---

        void BuildToolbar(VisualElement root)
        {
            var toolbar = new VisualElement();
            toolbar.AddToClassList("toolbar");

            targetLabel = new Label("Target: (none)");
            targetLabel.AddToClassList("toolbar__target-label");
            toolbar.Add(targetLabel);

            playStateLabel = new Label("Edit Mode");
            playStateLabel.AddToClassList("toolbar__play-state");
            toolbar.Add(playStateLabel);

            var spacer = new VisualElement();
            spacer.AddToClassList("toolbar__spacer");
            toolbar.Add(spacer);

            var clearButton = new Button(OnClearResults) { text = "Clear Results" };
            clearButton.AddToClassList("toolbar__button");
            toolbar.Add(clearButton);

            var findButton = new Button(FindAnyRunner) { text = "Find Runner" };
            findButton.AddToClassList("toolbar__button");
            toolbar.Add(findButton);

            root.Add(toolbar);
        }

        // --- 空状態画面 ---

        void BuildEmptyState(VisualElement parent)
        {
            emptyState = new VisualElement();
            emptyState.AddToClassList("empty-state");

            var message = new Label("BehaviourTreeRunner を持つ GameObject を選択してください");
            message.AddToClassList("empty-state__message");
            emptyState.Add(message);

            var findButton = new Button(FindAnyRunner) { text = "シーン内の Runner を検索" };
            findButton.AddToClassList("empty-state__button");
            emptyState.Add(findButton);

            parent.Add(emptyState);
        }

        // --- ルートノード未設定メッセージ ---

        void BuildNoRootMessage(VisualElement parent)
        {
            noRootMessage = new Label("BehaviourTreeRunner にルートノードが設定されていません。");
            noRootMessage.AddToClassList("info-box");
            noRootMessage.style.display = DisplayStyle.None;
            parent.Add(noRootMessage);
        }

        // --- スプリットビュー ---

        void BuildSplitView(VisualElement parent)
        {
            splitView = new TwoPaneSplitView(0, 300f, TwoPaneSplitViewOrientation.Horizontal);
            splitView.style.display = DisplayStyle.None;

            // 左パネル: ツリービュー
            var treePanel = new VisualElement();
            treePanel.AddToClassList("tree-panel");

            var treeHeader = new Label("Tree Structure");
            treeHeader.AddToClassList("tree-panel__header");
            treePanel.Add(treeHeader);

            var treeScroll = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            treeScroll.AddToClassList("tree-panel__scroll");
            treePanel.Add(treeScroll);

            treeContainer = new VisualElement();
            treeContainer.AddToClassList("tree-panel__container");
            treeScroll.Add(treeContainer);

            splitView.Add(treePanel);

            // 右パネル: 詳細 + BlackBoard
            var rightPanel = new VisualElement();
            rightPanel.AddToClassList("right-panel");

            BuildDetailPanel(rightPanel);
            BuildBlackBoardPanel(rightPanel);

            splitView.Add(rightPanel);

            parent.Add(splitView);
        }

        // --- ノード詳細パネル ---

        void BuildDetailPanel(VisualElement parent)
        {
            detailPanel = new VisualElement();
            detailPanel.AddToClassList("detail-panel");
            detailPanel.style.display = DisplayStyle.None;

            var header = new Label("Node Details");
            header.AddToClassList("detail-panel__header");
            detailPanel.Add(header);

            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.AddToClassList("detail-panel__scroll");
            detailPanel.Add(scroll);

            detailContent = new VisualElement();
            scroll.Add(detailContent);

            // セパレーター
            var separator = new VisualElement();
            separator.AddToClassList("separator");
            detailPanel.Add(separator);

            parent.Add(detailPanel);
        }

        // --- BlackBoard パネル ---

        void BuildBlackBoardPanel(VisualElement parent)
        {
            var bbPanel = new VisualElement();
            bbPanel.AddToClassList("blackboard-panel");

            var header = new Label("BlackBoard");
            header.AddToClassList("blackboard-panel__header");
            bbPanel.Add(header);

            // BlackBoard null 警告
            blackboardWarning = new Label("BlackBoard が null です。");
            blackboardWarning.AddToClassList("warning-box");
            blackboardWarning.style.display = DisplayStyle.None;
            bbPanel.Add(blackboardWarning);

            // 空メッセージ
            blackboardEmpty = new Label("(empty)");
            blackboardEmpty.AddToClassList("blackboard-panel__empty");
            blackboardEmpty.style.display = DisplayStyle.None;
            bbPanel.Add(blackboardEmpty);

            blackboardScroll = new ScrollView(ScrollViewMode.Vertical);
            blackboardScroll.AddToClassList("blackboard-panel__scroll");
            bbPanel.Add(blackboardScroll);

            blackboardContent = new VisualElement();
            blackboardScroll.Add(blackboardContent);

            parent.Add(bbPanel);
        }

        // --- 定期リフレッシュ ---

        void OnScheduledRefresh()
        {
            // PlayState ラベル更新
            if (playStateLabel != null)
            {
                playStateLabel.text = EditorApplication.isPlaying ? "Playing" : "Edit Mode";
            }

            if (!EditorApplication.isPlaying || targetRunner == null)
            {
                return;
            }

            CaptureNodeResults();
            RefreshTreeNodeStates();
            RefreshNodeDetails();
            RefreshBlackBoard();
        }

        // --- Selection 変更ハンドラ ---

        void OnSelectionChanged()
        {
            targetRunner = null;

            var selected = Selection.activeGameObject;
            if (selected != null)
            {
                targetRunner = selected.GetComponent<BehaviourTreeRunner>();
            }

            if (targetRunner != null)
            {
                lastResults.Clear();
                selectedNode = null;
            }

            UpdateVisibility();
            RebuildTree();
            RefreshBlackBoard();
        }

        /// <summary>
        /// Runner と RootNode の有無に応じて各パネルの表示/非表示を切り替える。
        /// </summary>
        void UpdateVisibility()
        {
            if (targetRunner == null)
            {
                SetDisplay(emptyState, true);
                SetDisplay(noRootMessage, false);
                SetDisplay(splitView, false);
            }
            else if (targetRunner.RootNode == null)
            {
                SetDisplay(emptyState, false);
                SetDisplay(noRootMessage, true);
                SetDisplay(splitView, false);
            }
            else
            {
                SetDisplay(emptyState, false);
                SetDisplay(noRootMessage, false);
                SetDisplay(splitView, true);
            }

            // ツールバーのターゲット表示更新
            if (targetLabel != null)
            {
                targetLabel.text = targetRunner != null
                    ? $"Target: {targetRunner.gameObject.name}"
                    : "Target: (none)";
            }

            // 詳細パネル
            SetDisplay(detailPanel, selectedNode != null);
        }

        // --- ツリー構築 ---

        /// <summary>
        /// ツリー全体を VisualElement として再構築する。
        /// Runner やルートが変わったタイミングで呼ばれる。
        /// </summary>
        void RebuildTree()
        {
            if (treeContainer == null)
            {
                return;
            }

            treeContainer.Clear();
            nodeRowElements.Clear();

            if (targetRunner == null || targetRunner.RootNode == null)
            {
                return;
            }

            BuildNodeRecursive(treeContainer, targetRunner.RootNode, 0);
        }

        void BuildNodeRecursive(VisualElement parent, BTNode node, int depth)
        {
            if (node == null)
            {
                return;
            }

            var row = new VisualElement();
            row.AddToClassList("node-row");

            // インデント用のクラス（USS で定義済み、上限超えは最大値で止める）
            var depthClass = Math.Min(depth, MaxDepthClass);
            row.AddToClassList($"node-row--depth-{depthClass}");

            // 初期状態クラス
            row.AddToClassList("node--idle");

            // アイコン
            var icon = new VisualElement();
            icon.AddToClassList("node__icon");
            row.Add(icon);

            // ラベル
            var nodeTypeTag = GetNodeTypeTag(node);
            var displayName = string.IsNullOrEmpty(node.Name) ? node.GetType().Name : node.Name;
            var label = new Label($"[{nodeTypeTag}] {displayName}");
            label.AddToClassList("node__label");
            row.Add(label);

            // クリックで選択
            row.RegisterCallback<ClickEvent>(_ => OnNodeClicked(node));

            // マッピング登録
            nodeRowElements[node] = row;

            parent.Add(row);

            // 子ノードを再帰的に構築
            foreach (var child in node.Children)
            {
                BuildNodeRecursive(parent, child, depth + 1);
            }
        }

        // --- ノードクリック ---

        void OnNodeClicked(BTNode node)
        {
            // 前回の選択を解除
            if (selectedNode != null && nodeRowElements.TryGetValue(selectedNode, out var prevRow))
            {
                prevRow.RemoveFromClassList("node--selected");
            }

            selectedNode = node;

            // 新しい選択にクラスを付与
            if (nodeRowElements.TryGetValue(node, out var newRow))
            {
                newRow.AddToClassList("node--selected");
            }

            SetDisplay(detailPanel, true);
            RefreshNodeDetails();
        }

        // --- ツリーノード状態の更新（色の切り替え） ---

        void RefreshTreeNodeStates()
        {
            foreach (var (node, row) in nodeRowElements)
            {
                // 状態クラスを一旦すべて除去
                row.RemoveFromClassList("node--running");
                row.RemoveFromClassList("node--success");
                row.RemoveFromClassList("node--failure");
                row.RemoveFromClassList("node--idle");

                var stateClass = GetNodeStateClass(node);
                row.AddToClassList(stateClass);
            }
        }

        string GetNodeStateClass(BTNode node)
        {
            if (lastResults.TryGetValue(node, out var result))
            {
                return result switch
                {
                    BTNodeResult.Running => "node--running",
                    BTNodeResult.Success => "node--success",
                    BTNodeResult.Failure => "node--failure",
                    _ => "node--idle"
                };
            }

            // ActionNode の IsExecuting で Running を即時判定
            if (node is BTActionNode actionNode && actionNode.IsExecuting)
            {
                return "node--running";
            }

            return "node--idle";
        }

        // --- ノード詳細の更新 ---

        void RefreshNodeDetails()
        {
            if (detailContent == null || selectedNode == null)
            {
                return;
            }

            detailContent.Clear();

            AddDetailRow("Name", selectedNode.Name ?? "(unnamed)");
            AddDetailRow("Type", selectedNode.GetType().Name);
            AddDetailRow("Category", GetNodeTypeTag(selectedNode));

            var parentName = selectedNode.Parent != null
                ? (selectedNode.Parent.Name ?? "(unnamed)")
                : "(root)";
            AddDetailRow("Parent", parentName);

            AddDetailRow("Children", selectedNode.Children.Count.ToString());

            // 最後の実行結果
            if (lastResults.TryGetValue(selectedNode, out var result))
            {
                var resultRow = AddDetailRow("Last Result", result.ToString());
                var valueLabel = resultRow.Q<Label>(className: "detail-panel__value");
                if (valueLabel != null)
                {
                    valueLabel.AddToClassList(result switch
                    {
                        BTNodeResult.Running => "detail-value--running",
                        BTNodeResult.Success => "detail-value--success",
                        BTNodeResult.Failure => "detail-value--failure",
                        _ => ""
                    });
                }
            }
            else
            {
                AddDetailRow("Last Result", "Not executed");
            }

            // ノードタイプ固有の情報
            AddTypeSpecificInfo(selectedNode);
        }

        VisualElement AddDetailRow(string key, string value)
        {
            var row = new VisualElement();
            row.AddToClassList("detail-panel__row");

            var keyLabel = new Label(key);
            keyLabel.AddToClassList("detail-panel__key");
            row.Add(keyLabel);

            var valueLabel = new Label(value);
            valueLabel.AddToClassList("detail-panel__value");
            row.Add(valueLabel);

            detailContent.Add(row);
            return row;
        }

        void AddTypeSpecificInfo(BTNode node)
        {
            switch (node)
            {
                case BTActionNode actionNode:
                    AddDetailRow("Is Executing", actionNode.IsExecuting.ToString());
                    break;

                case BTDecoratorNode decoratorNode:
                    var childName = decoratorNode.Child != null
                        ? (decoratorNode.Child.Name ?? "(unnamed)")
                        : "(none)";
                    AddDetailRow("Child", childName);
                    break;

                case BTCompositeNode:
                    AddDetailRow("Child Count", node.Children.Count.ToString());
                    break;
            }
        }

        // --- BlackBoard 更新 ---

        void RefreshBlackBoard()
        {
            if (blackboardContent == null)
            {
                return;
            }

            blackboardContent.Clear();

            if (targetRunner == null)
            {
                SetDisplay(blackboardWarning, false);
                SetDisplay(blackboardEmpty, false);
                SetDisplay(blackboardScroll, false);
                return;
            }

            var blackBoard = targetRunner.BlackBoard;
            if (blackBoard == null)
            {
                SetDisplay(blackboardWarning, true);
                SetDisplay(blackboardEmpty, false);
                SetDisplay(blackboardScroll, false);
                return;
            }

            var keys = blackBoard.GetAllKeys();
            if (keys.Length == 0)
            {
                SetDisplay(blackboardWarning, false);
                SetDisplay(blackboardEmpty, true);
                SetDisplay(blackboardScroll, false);
                return;
            }

            SetDisplay(blackboardWarning, false);
            SetDisplay(blackboardEmpty, false);
            SetDisplay(blackboardScroll, true);

            // ヘッダー行
            var headerRow = new VisualElement();
            headerRow.AddToClassList("blackboard__header-row");

            var headerKey = new Label("Key");
            headerKey.AddToClassList("blackboard__header-cell");
            headerKey.AddToClassList("blackboard__header-key");
            headerRow.Add(headerKey);

            var headerValue = new Label("Value");
            headerValue.AddToClassList("blackboard__header-cell");
            headerValue.AddToClassList("blackboard__header-value");
            headerRow.Add(headerValue);

            var headerType = new Label("Type");
            headerType.AddToClassList("blackboard__header-cell");
            headerType.AddToClassList("blackboard__header-type");
            headerRow.Add(headerType);

            blackboardContent.Add(headerRow);

            // データ行をキーのアルファベット順で表示
            Array.Sort(keys, StringComparer.Ordinal);
            foreach (var key in keys)
            {
                var row = new VisualElement();
                row.AddToClassList("blackboard__row");

                var keyLabel = new Label(key);
                keyLabel.AddToClassList("blackboard__cell-key");
                row.Add(keyLabel);

                var valueStr = blackBoard.GetValueAsString(key);
                var valueLabel = new Label(valueStr);
                valueLabel.AddToClassList("blackboard__cell-value");
                row.Add(valueLabel);

                var typeName = blackBoard.GetValueType(key)?.Name ?? "?";
                var typeLabel = new Label(typeName);
                typeLabel.AddToClassList("blackboard__cell-type");
                row.Add(typeLabel);

                blackboardContent.Add(row);
            }
        }

        // --- ノード結果のキャプチャ ---

        /// <summary>
        /// ツリー全体を走査し、各ノードの状態を記録する。
        /// BTNode には直接 lastResult が無いため、ノードの型固有のプロパティから推定する。
        /// </summary>
        void CaptureNodeResults()
        {
            if (targetRunner == null || targetRunner.RootNode == null)
            {
                return;
            }

            visitedThisFrame.Clear();
            CaptureNodeResultRecursive(targetRunner.RootNode);
        }

        void CaptureNodeResultRecursive(BTNode node)
        {
            if (node == null || visitedThisFrame.Contains(node))
            {
                return;
            }

            visitedThisFrame.Add(node);

            // ActionNode は isExecuting で Running を判定可能
            if (node is BTActionNode actionNode)
            {
                if (actionNode.IsExecuting)
                {
                    lastResults[node] = BTNodeResult.Running;
                }
            }

            // 子ノードを再帰的に走査
            foreach (var child in node.Children)
            {
                CaptureNodeResultRecursive(child);
            }
        }

        // --- ボタンコールバック ---

        void OnClearResults()
        {
            lastResults.Clear();
            selectedNode = null;

            SetDisplay(detailPanel, false);
            RefreshTreeNodeStates();
        }

        void FindAnyRunner()
        {
            var runner = FindAnyObjectByType<BehaviourTreeRunner>();
            if (runner != null)
            {
                Selection.activeGameObject = runner.gameObject;
            }
            else
            {
                EditorUtility.DisplayDialog("BT Debugger", "シーン内に BehaviourTreeRunner が見つかりません。", "OK");
            }
        }

        // --- ユーティリティ ---

        static string GetNodeTypeTag(BTNode node)
        {
            return node switch
            {
                BTActionNode => "ACT",
                BTConditionNode => "CND",
                BTDecoratorNode => "DEC",
                BTCompositeNode => "CMP",
                _ => "NOD"
            };
        }

        /// <summary>
        /// VisualElement の display を切り替えるヘルパー。
        /// null 安全。
        /// </summary>
        static void SetDisplay(VisualElement element, bool visible)
        {
            if (element != null)
            {
                element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        /// <summary>
        /// 外部から実行結果を記録するための公開メソッド。
        /// BehaviourTreeRunner にフック可能な場合に利用する。
        /// </summary>
        public void RecordResult(BTNode node, BTNodeResult result)
        {
            if (node != null)
            {
                lastResults[node] = result;
            }
        }
    }
}
