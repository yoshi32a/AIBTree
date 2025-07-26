using UnityEngine;
using BehaviourTree.Parser;
using System.IO;

namespace BehaviourTree.Core
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
            var resultIcon = result == BTNodeResult.Success ? "✅" :
                result == BTNodeResult.Failure ? "❌" :
                result == BTNodeResult.Running ? "🔄" : "❓";
            
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
            
            Debug.Log($"🌳 BT[{rootNode.Name}] → {result} {resultIcon}{changeInfo} " +
                     $"(実行回数: {executionCount}, 時刻: {Time.time:F1}s)" +
                     (repetitionCount > 10 ? $" [繰り返し×{repetitionCount}]" : ""));
            
            // BlackBoard状態も表示（変化があった場合）
            if (blackBoard.HasRecentChanges())
            {
                Debug.Log($"📋 BlackBoard更新: {blackBoard.GetRecentChangeSummary()}");
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
                        Debug.LogError($"BT file not found: {filePath}");
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

                    Debug.Log($"Successfully loaded behaviour tree from: {filePath}");
                    Debug.Log($"Root node: {rootNode.Name} ({rootNode.GetType().Name})");
                    LogTreeStructure(rootNode, 0);
                    return true;
                }
                else
                {
                    Debug.LogError($"Failed to parse behaviour tree: {filePath}");
                    return false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading behaviour tree: {e.Message}");
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
                Debug.LogError($"❌ InitializeNodeTree: BlackBoard is null for node {node.Name}");
                return;
            }

            // このMonoBehaviourとBlackBoardを渡してノードを初期化
            node.Initialize(this, blackBoard);
            Debug.Log($"✅ Initialized node: {node.Name} ({node.GetType().Name})");

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
                    Debug.Log("Successfully loaded behaviour tree from content");
                    LogTreeStructure(rootNode, 0);
                    return true;
                }
                else
                {
                    Debug.LogError("Failed to parse behaviour tree content");
                    return false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing behaviour tree content: {e.Message}");
                return false;
            }
        }

        public void SetRootNode(BTNode node)
        {
            rootNode = node;
        }

        public void ResetTree()
        {
            rootNode?.Reset();
        }

        public void SetTickInterval(float interval)
        {
            tickInterval = Mathf.Max(0.01f, interval);
        }

        public void SetDebugMode(bool enabled)
        {
            debugMode = enabled;
        }

        void LogTreeStructure(BTNode node, int depth)
        {
            if (node == null)
            {
                return;
            }

            var indent = new string(' ', depth * 2);
            Debug.Log($"{indent}{node.Name} ({node.GetType().Name})");

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
        void ResetTreeState()
        {
            ResetTree();
            Debug.Log("Behaviour tree state reset");
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
                Debug.Log("BlackBoard is null");
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