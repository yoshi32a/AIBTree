using UnityEngine;
using ArcBT.Parser;
using System.IO;
using ArcBT.Logger;

namespace ArcBT.Core
{
    public class BehaviourTreeRunner : MonoBehaviour
    {
        [SerializeField] string behaviourTreeFilePath;
        [SerializeField] float tickInterval = 0.1f; // AI判定は0.1秒間隔
        [SerializeField] bool debugMode = true;

        float lastTickTime;
        BTParser parser;

        // スマートログ用の状態追跡
        BTNodeResult lastResult = BTNodeResult.Running;
        string lastExecutedNodeName = "";
        float lastLogTime;
        int executionCount;
        int repetitionCount;
        string lastLogPattern = "";

        public BTNode RootNode { get; set; }

        public BlackBoard BlackBoard { get; private set; }

        void Awake()
        {
            parser = new BTParser();
            BlackBoard = new BlackBoard();
        }

        void Start()
        {
            if (!string.IsNullOrEmpty(behaviourTreeFilePath))
            {
                LoadBehaviourTree(behaviourTreeFilePath);
            }
        }

        void Update()
        {
            if (RootNode != null && Time.time - lastTickTime >= tickInterval)
            {
                executionCount++;
                var result = RootNode.Execute();

                // スマートログ: 状態変化時または定期的にのみログ出力
                var shouldLog = debugMode && (
                    result != lastResult || // 結果が変わった
                    RootNode.Name != lastExecutedNodeName || // 実行ノードが変わった
                    Time.time - lastLogTime > 5f || // 5秒間隔で生存確認
                    executionCount <= 3 // 最初の3回は必ずログ
                );

                if (shouldLog)
                {
                    LogExecutionState(result);
                    lastResult = result;
                    lastExecutedNodeName = RootNode.Name;
                    lastLogTime = Time.time;
                }

                lastTickTime = Time.time;
            }
        }

        void LogExecutionState(BTNodeResult result)
        {
            var changeInfo = result != lastResult ? " [状態変化]" : "";

            // 同じパターンの繰り返しをチェック
            var currentPattern = $"{result}-{RootNode.Name}";
            var isRepeating = currentPattern == lastLogPattern;

            if (isRepeating)
            {
                repetitionCount++;
                // 10回以上同じパターンが続く場合は、5秒おきにのみログ出力
                if (repetitionCount > 10 && Time.time - lastLogTime < 5.0f)
                {
                    return; // ログをスキップ
                }
            }
            else
            {
                repetitionCount = 0;
                lastLogPattern = currentPattern;
            }

            // 構造化ログを使用
            var logLevel = result == BTNodeResult.Failure ? Microsoft.Extensions.Logging.LogLevel.Warning : Microsoft.Extensions.Logging.LogLevel.Information;
            BTLogger.LogSystem(RootNode.Name,
                $"BT[{RootNode.Name}] → {result}{changeInfo} " +
                $"(実行回数: {executionCount}, 時刻: {Time.time:F1}s)" +
                (repetitionCount > 10 ? $" [繰り返し×{repetitionCount}]" : ""));

            // BlackBoard状態も表示（変化があった場合）
            if (BlackBoard.HasRecentChanges())
            {
                BTLogger.LogBlackBoard($"BlackBoard更新: {BlackBoard.GetRecentChangeSummary()}", "");
            }

            lastLogTime = Time.time;
        }

        public bool LoadBehaviourTree(string filePath)
        {
            try
            {
                // Assetsフォルダ内のパスを絶対パスに変換
                var fullPath = Path.Combine(Application.dataPath, filePath.TrimStart('/'));

                if (!File.Exists(fullPath))
                {
                    // Assets/BehaviourTrees/内を試す
                    var assetsPath = Path.Combine(Application.dataPath, "BehaviourTrees", Path.GetFileName(filePath));
                    if (File.Exists(assetsPath))
                    {
                        fullPath = assetsPath;
                    }
                    else
                    {
                        BTLogger.LogSystemError("System", $"BT file not found: {filePath}");
                        return false;
                    }
                }

                RootNode = parser.ParseFile(fullPath);

                if (RootNode != null)
                {
                    // ルートノードとすべての子ノードを初期化
                    InitializeNodeTree(RootNode);

                    // 動的条件チェックを設定
                    SetupDynamicConditionChecking(RootNode);

                    BTLogger.LogSystem($"Successfully loaded behaviour tree from: {filePath}", "");
                    BTLogger.LogSystem($"Root node: {RootNode.Name}", "");
                    LogTreeStructure(RootNode, 0);
                    return true;
                }

                BTLogger.LogSystemError("Parser", $"Failed to parse behaviour tree: {filePath}");
                return false;
            }
            catch (System.Exception e)
            {
                BTLogger.LogSystemError("System", $"Error loading behaviour tree: {e.Message}");
                return false;
            }
        }

        void InitializeNodeTree(BTNode node)
        {
            if (node == null)
            {
                return;
            }

            // BlackBoardの確認
            if (BlackBoard == null)
            {
                BTLogger.LogSystemError("BlackBoard", $"❌ InitializeNodeTree: BlackBoard is null for node {node.Name}");
                return;
            }

            // このMonoBehaviourとBlackBoardを渡してノードを初期化
            node.Initialize(this, BlackBoard);
            BTLogger.LogSystem($"✅ Initialized node: {node.Name}", node.Name);

            // 子ノードも再帰的に初期化
            foreach (var child in node.Children)
            {
                InitializeNodeTree(child);
            }
        }

        /// <summary>動的条件チェックを設定する</summary>
        void SetupDynamicConditionChecking(BTNode node)
        {
            if (node is BTCompositeNode composite)
            {
                composite.SetupDynamicConditionChecking();
            }

            // 子ノードも再帰的に設定
            foreach (var child in node.Children)
            {
                SetupDynamicConditionChecking(child);
            }
        }

        public bool LoadBehaviourTreeFromContent(string content)
        {
            try
            {
                RootNode = parser.ParseContent(content);

                if (RootNode != null)
                {
                    BTLogger.LogSystem("Successfully loaded behaviour tree from content", "");
                    LogTreeStructure(RootNode, 0);
                    return true;
                }

                BTLogger.LogSystemError("Parser", "Failed to parse behaviour tree content");
                return false;
            }
            catch (System.Exception e)
            {
                BTLogger.LogSystemError("System", $"Error parsing behaviour tree content: {e.Message}");
                return false;
            }
        }

        /// <summary>ルートノード設定（テスト用）</summary>
        internal void SetRootNode(BTNode node)
        {
            RootNode = node;
        }

        /// <summary>ツリー状態リセット（内部用）</summary>
        internal void ResetTree()
        {
            RootNode?.Reset();
            BlackBoard?.Clear();
        }

        void LogTreeStructure(BTNode node, int depth)
        {
            if (node == null)
            {
                return;
            }

            var indent = new string(' ', depth * 2);
            BTLogger.LogSystem($"{indent}{node.Name}", node.Name);

            foreach (var child in node.Children)
            {
                LogTreeStructure(child, depth + 1);
            }
        }

        // Inspectorで使用するヘルパーメソッド
        [ContextMenu("Reload Behaviour Tree")]
        void ReloadBehaviourTree()
        {
            if (!string.IsNullOrEmpty(behaviourTreeFilePath))
            {
                LoadBehaviourTree(behaviourTreeFilePath);
            }
        }

        [ContextMenu("Reset Tree State")]
        internal void ResetTreeState()
        {
            ResetTree();
            BTLogger.LogSystem("Behaviour tree state reset", "");
        }

        [ContextMenu("Show BlackBoard Contents")]
        void ShowBlackBoardContents()
        {
            if (BlackBoard != null)
            {
                BlackBoard.DebugLog();
            }
            else
            {
                BTLogger.LogSystemError("BlackBoard", "BlackBoard is null");
            }
        }

        [ContextMenu("Clear BlackBoard")]
        void ClearBlackBoard()
        {
            if (BlackBoard != null)
            {
                BlackBoard.Clear();
            }
        }

        // テスト用メソッド
        /// <summary>ワンショット実行（テスト用）</summary>
        internal BTNodeResult ExecuteOnce()
        {
            return RootNode?.Execute() ?? BTNodeResult.Failure;
        }

        /// <summary>BlackBoard内容の文字列取得（テスト用）</summary>
        internal string GetBlackBoardContents()
        {
            if (BlackBoard == null) return "BlackBoard is null";

            var keys = BlackBoard.GetAllKeys();
            if (keys.Length == 0) return "BlackBoard is empty";

            var contents = new System.Text.StringBuilder();
            contents.AppendLine("BlackBoard Contents:");
            foreach (var key in keys)
            {
                var value = BlackBoard.GetValueAsString(key);
                var type = BlackBoard.GetValueType(key)?.Name ?? "unknown";
                contents.AppendLine($"  {key}: {value} ({type})");
            }

            return contents.ToString();
        }

        /// <summary>ティック間隔設定（テスト用）</summary>
        internal void SetTickInterval(float interval)
        {
            tickInterval = interval;
        }

        /// <summary>デバッグモード設定（テスト用）</summary>
        internal void SetDebugMode(bool isEnabled)
        {
            debugMode = isEnabled;
        }

        // デバッグ用
        void OnDrawGizmos()
        {
            if (debugMode && RootNode != null)
            {
                // 将来的にツリー構造をGizmosで表示
            }
        }
    }
}
