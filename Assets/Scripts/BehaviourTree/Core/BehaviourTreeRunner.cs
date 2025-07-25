using UnityEngine;
using BehaviourTree.Parser;
using System.IO;

namespace BehaviourTree.Core
{
    public class BehaviourTreeRunner : MonoBehaviour
    {
        [SerializeField] string behaviourTreeFilePath;
        [SerializeField] float tickInterval = 0.1f;
        [SerializeField] bool debugMode = true;

        BTNode rootNode;
        float lastTickTime;
        BTParser parser;
        BlackBoard blackBoard;

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
                if (debugMode)
                {
                    Debug.Log($"🌳 === BEHAVIOUR TREE UPDATE CYCLE === (Tick: {Time.frameCount})");
                    Debug.Log($"🌳 Executing root node: '{rootNode.Name}' ({rootNode.GetType().Name})");
                }

                var result = rootNode.Execute();

                if (debugMode)
                {
                    var resultIcon = result == BTNodeResult.Success ? "✅" :
                        result == BTNodeResult.Failure ? "❌" :
                        result == BTNodeResult.Running ? "🔄" : "❓";

                    Debug.Log($"🌳 Behaviour Tree '{rootNode.Name}' result: {result} {resultIcon}");
                    Debug.Log($"🌳 === END UPDATE CYCLE ===");
                }

                lastTickTime = Time.time;
            }
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

            // このMonoBehaviourとBlackBoardを渡してノードを初期化
            node.Initialize(this, blackBoard);

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