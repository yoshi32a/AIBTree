using UnityEngine;
using ArcBT.Parser;
using System.IO;

namespace ArcBT.Core
{
    public class BehaviourTreeRunner : MonoBehaviour
    {
        [SerializeField] string behaviourTreeFilePath;
        [SerializeField] float tickInterval = 0.1f; // AI判定は0.1秒間隔
        [SerializeField] bool debugMode = true;

        BTNode rootNode;
        float lastTickTime;
        BTParser parser;
        BlackBoard blackBoard;
        
        // スマートログ用の状態追跡
        BTNodeResult lastResult = BTNodeResult.Running;
        string lastExecutedNodeName = "";
        float lastLogTime = 0f;
        int executionCount = 0;
        int repetitionCount = 0;
        string lastLogPattern = "";

        public BTNode RootNode
        {
            get => rootNode;
            set => rootNode = value;
        }

        public BlackBoard BlackBoard
        {
            get => blackBoard;
        }

        void Awake()
        {
            parser = new BTParser();
            blackBoard = new BlackBoard();
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
            if (rootNode != null && Time.time - lastTickTime >= tickInterval)
            {
                executionCount++;
                var result = rootNode.Execute();
                
                // スマートログ: 状態変化時または定期的にのみログ出力
                bool shouldLog = debugMode && (
                    result != lastResult ||  // 結果が変わった
                    rootNode.Name != lastExecutedNodeName ||  // 実行ノードが変わった
                    Time.time - lastLogTime > 5f ||  // 5秒間隔で生存確認
                    executionCount <= 3  // 最初の3回は必ずログ
                );
                
                if (shouldLog)
                {
                    LogExecutionState(result);
                    lastResult = result;
                    lastExecutedNodeName = rootNode.Name;
                    lastLogTime = Time.time;
                }

                lastTickTime = Time.time;
            }
        }
        
        void LogExecutionState(BTNodeResult result)
        {
            var changeInfo = result != lastResult ? " [状態変化]" : "";
            
            // 同じパターンの繰り返しをチェック
            string currentPattern = $"{result}-{rootNode.Name}";
            bool isRepeating = currentPattern == lastLogPattern;
            
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
            var logLevel = result == BTNodeResult.Failure ? LogLevel.Warning : LogLevel.Info;
            BTLogger.Log(logLevel, LogCategory.System, 
                $"BT[{rootNode.Name}] → {result}{changeInfo} " +
                $"(実行回数: {executionCount}, 時刻: {Time.time:F1}s)" +
                (repetitionCount > 10 ? $" [繰り返し×{repetitionCount}]" : ""),
                rootNode.Name, this);
            
            // BlackBoard状態も表示（変化があった場合）
            if (blackBoard.HasRecentChanges())
            {
                BTLogger.LogBlackBoard($"BlackBoard更新: {blackBoard.GetRecentChangeSummary()}", "", this);
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
                        BTLogger.LogError(LogCategory.System, $"BT file not found: {filePath}", "", this);
                        return false;
                    }
                }

                rootNode = parser.ParseFile(fullPath);

                if (rootNode != null)
                {
                    // ルートノードとすべての子ノードを初期化
                    InitializeNodeTree(rootNode);

                    // 動的条件チェックを設定
                    SetupDynamicConditionChecking(rootNode);

                    BTLogger.LogSystem($"Successfully loaded behaviour tree from: {filePath}", "", this);
                    BTLogger.LogSystem($"Root node: {rootNode.Name} ({rootNode.GetType().Name})", "", this);
                    LogTreeStructure(rootNode, 0);
                    return true;
                }
                else
                {
                    BTLogger.LogError(LogCategory.Parser, $"Failed to parse behaviour tree: {filePath}", "", this);
                    return false;
                }
            }
            catch (System.Exception e)
            {
                BTLogger.LogError(LogCategory.System, $"Error loading behaviour tree: {e.Message}", "", this);
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
            if (blackBoard == null)
            {
                BTLogger.LogError(LogCategory.BlackBoard, $"❌ InitializeNodeTree: BlackBoard is null for node {node.Name}", node.Name, this);
                return;
            }

            // このMonoBehaviourとBlackBoardを渡してノードを初期化
            node.Initialize(this, blackBoard);
            BTLogger.LogSystem($"✅ Initialized node: {node.Name} ({node.GetType().Name})", node.Name, this);

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
                rootNode = parser.ParseContent(content);

                if (rootNode != null)
                {
                    BTLogger.LogSystem("Successfully loaded behaviour tree from content", "", this);
                    LogTreeStructure(rootNode, 0);
                    return true;
                }
                else
                {
                    BTLogger.LogError(LogCategory.Parser, "Failed to parse behaviour tree content", "", this);
                    return false;
                }
            }
            catch (System.Exception e)
            {
                BTLogger.LogError(LogCategory.System, $"Error parsing behaviour tree content: {e.Message}", "", this);
                return false;
            }
        }

        /// <summary>ルートノード設定（テスト用）</summary>
        internal void SetRootNode(BTNode node)
        {
            rootNode = node;
        }

        /// <summary>ツリー状態リセット（内部用）</summary>
        internal void ResetTree()
        {
            rootNode?.Reset();
            blackBoard?.Clear();
        }

        void LogTreeStructure(BTNode node, int depth)
        {
            if (node == null)
            {
                return;
            }

            var indent = new string(' ', depth * 2);
            BTLogger.LogSystem($"{indent}{node.Name} ({node.GetType().Name})", node.Name, this);

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
            BTLogger.LogSystem("Behaviour tree state reset", "", this);
        }

        [ContextMenu("Show BlackBoard Contents")]
        void ShowBlackBoardContents()
        {
            if (blackBoard != null)
            {
                blackBoard.DebugLog();
            }
            else
            {
                BTLogger.LogError(LogCategory.BlackBoard, "BlackBoard is null", "", this);
            }
        }

        [ContextMenu("Clear BlackBoard")]
        void ClearBlackBoard()
        {
            if (blackBoard != null)
            {
                blackBoard.Clear();
            }
        }

        // テスト用メソッド
        /// <summary>ワンショット実行（テスト用）</summary>
        internal BTNodeResult ExecuteOnce()
        {
            return rootNode?.Execute() ?? BTNodeResult.Failure;
        }

        /// <summary>BlackBoard内容の文字列取得（テスト用）</summary>
        internal string GetBlackBoardContents()
        {
            if (blackBoard == null) return "BlackBoard is null";
            
            var keys = blackBoard.GetAllKeys();
            if (keys.Length == 0) return "BlackBoard is empty";
            
            var contents = new System.Text.StringBuilder();
            contents.AppendLine("BlackBoard Contents:");
            foreach (var key in keys)
            {
                var value = blackBoard.GetValueAsString(key);
                var type = blackBoard.GetValueType(key)?.Name ?? "unknown";
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
        internal void SetDebugMode(bool enabled)
        {
            debugMode = enabled;
        }

        // デバッグ用
        void OnDrawGizmos()
        {
            if (debugMode && rootNode != null)
            {
                // 将来的にツリー構造をGizmosで表示
            }
        }
    }
}