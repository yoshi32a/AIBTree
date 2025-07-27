using UnityEngine;
using UnityEditor;
using ArcBT.Core;
using ArcBT.Logger;
using ArcBT.Samples.RPG.Components;

namespace ArcBT.Editor
{
    public class AIBTreeTestEnvironmentSetup : EditorWindow
    {
        Vector3 aiPosition = Vector3.zero;
        Vector3 enemyPosition = new Vector3(5, 0, 3);
        string selectedBTFile = "blackboard_sample.bt";
        bool enableDebugMode = true;
        bool createMultipleEnemies = false;
        int enemyCount = 1;
        Vector3 cameraPosition = new Vector3(0, 15, -10);
        bool autoSetupCamera = true;
        
        [MenuItem("BehaviourTree/Setup Test Environment")]
        static void ShowWindow()
        {
            var window = GetWindow<AIBTreeTestEnvironmentSetup>("AIBTree Test Setup");
            window.minSize = new Vector2(400, 500);
        }
        
        void OnGUI()
        {
            GUILayout.Label("AIBTree Test Environment Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // AI設定
            GUILayout.Label("AI Configuration", EditorStyles.boldLabel);
            aiPosition = EditorGUILayout.Vector3Field("AI Position", aiPosition);
            selectedBTFile = EditorGUILayout.TextField("BT File", selectedBTFile);
            enableDebugMode = EditorGUILayout.Toggle("Enable Debug Mode", enableDebugMode);
            EditorGUILayout.Space();
            
            // 敵設定
            GUILayout.Label("Enemy Configuration", EditorStyles.boldLabel);
            enemyPosition = EditorGUILayout.Vector3Field("Enemy Position", enemyPosition);
            createMultipleEnemies = EditorGUILayout.Toggle("Multiple Enemies", createMultipleEnemies);
            if (createMultipleEnemies)
            {
                enemyCount = EditorGUILayout.IntSlider("Enemy Count", enemyCount, 1, 5);
            }
            EditorGUILayout.Space();
            
            // カメラ設定
            GUILayout.Label("Camera Configuration", EditorStyles.boldLabel);
            autoSetupCamera = EditorGUILayout.Toggle("Auto Setup Camera", autoSetupCamera);
            if (autoSetupCamera)
            {
                cameraPosition = EditorGUILayout.Vector3Field("Camera Position", cameraPosition);
            }
            EditorGUILayout.Space();
            
            // プリセットボタン
            GUILayout.Label("Quick Setup Presets", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("BlackBoard Test"))
            {
                SetupBlackBoardTest();
            }
            if (GUILayout.Button("Dynamic Condition Test"))
            {
                SetupDynamicConditionTest();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Team Coordination Test"))
            {
                SetupTeamCoordinationTest();
            }
            if (GUILayout.Button("Simple Patrol Test"))
            {
                SetupSimplePatrolTest();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Complex Example Test"))
            {
                SetupComplexExampleTest();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // メインセットアップボタン
            if (GUILayout.Button("Create Test Environment", GUILayout.Height(40)))
            {
                CreateTestEnvironment();
            }
            
            EditorGUILayout.Space();
            
            // クリーンアップボタン
            if (GUILayout.Button("Clear Test Environment", GUILayout.Height(30)))
            {
                ClearTestEnvironment();
            }
            
            EditorGUILayout.Space();
            GUILayout.Label("Tip: Make sure you have the BT file in Assets/BehaviourTrees/", EditorStyles.helpBox);
        }
        
        public void SetupBlackBoardTest()
        {
            selectedBTFile = "blackboard_sample.bt";
            aiPosition = Vector3.zero;
            enemyPosition = new Vector3(8, 0, 0);
            createMultipleEnemies = false;
            enableDebugMode = true;
        }
        
        public void SetupDynamicConditionTest()
        {
            selectedBTFile = "dynamic_condition_sample.bt";
            aiPosition = Vector3.zero;
            enemyPosition = new Vector3(5, 0, 5);
            createMultipleEnemies = false;
            enableDebugMode = true;
        }
        
        public void SetupTeamCoordinationTest()
        {
            selectedBTFile = "team_coordination_sample.bt";
            aiPosition = Vector3.zero;
            enemyPosition = new Vector3(10, 0, 0);
            createMultipleEnemies = true;
            enemyCount = 2;
            enableDebugMode = true;
        }
        
        public void SetupSimplePatrolTest()
        {
            selectedBTFile = "simple_example.bt";
            aiPosition = Vector3.zero;
            enemyPosition = new Vector3(12, 0, 0);
            createMultipleEnemies = false;
            enableDebugMode = true;
        }
        
        public void SetupComplexExampleTest()
        {
            selectedBTFile = "complex_example.bt";
            aiPosition = Vector3.zero;
            enemyPosition = new Vector3(4, 0, 2); // 敵を近くに配置
            createMultipleEnemies = true;
            enemyCount = 3; // 敵を増やして戦闘をトリガー
            enableDebugMode = true;
            cameraPosition = new Vector3(0, 20, -15);
        }
        
        public void CreateTestEnvironment()
        {
            // 必要なタグを作成
            CreateRequiredTags();
            
            // 既存のテストオブジェクトをクリア
            ClearTestEnvironment();
            
            // AIエージェント作成
            CreateAIAgent();
            
            // 敵作成
            CreateEnemies();
            
            // パトロールポイント作成
            CreatePatrolPoints();
            
            // その他の環境オブジェクト作成
            CreateEnvironmentObjects();
            
            // カメラ設定
            if (autoSetupCamera)
            {
                SetupCamera();
            }
            
            BTLogger.Info($"AIBTree Test Environment created successfully with BT file: {selectedBTFile}");
        }
        
        void CreateAIAgent()
        {
            // メインAIエージェント
            var aiObject = new GameObject("TestAI");
            aiObject.transform.position = aiPosition;
            
            // AIエージェント用のCollider追加（ScanForInterest用）
            var aiCollider = aiObject.AddComponent<SphereCollider>();
            aiCollider.radius = 0.5f;
            aiCollider.isTrigger = true;
            
            // Cube for visualization
            var visualCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visualCube.transform.SetParent(aiObject.transform);
            visualCube.transform.localPosition = Vector3.zero;
            visualCube.transform.localScale = Vector3.one * 0.8f;
            
            // Blue material from Assets/Materials
            var renderer = visualCube.GetComponent<Renderer>();
            var blueMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Blue.mat");
            if (blueMaterial != null)
            {
                renderer.material = blueMaterial;
            }
            
            // コンポーネント追加
            var health = aiObject.AddComponent<Health>();
            health.MaxHealth = 100;
            health.CurrentHealth = 100;
            
            var inventory = aiObject.AddComponent<Inventory>();
            
            // complex_example.bt用の特別設定
            if (selectedBTFile == "complex_example.bt")
            {
                // Manaコンポーネント追加
                var mana = aiObject.AddComponent<Mana>();
                
                // インベントリにアイテム追加
                inventory.AddItem("healing_potion", 2); // 少なくして戦略的使用を促す
                inventory.AddItem("mana_potion", 1);
                
                // 体力を少し低くして緊急時対応をテスト
                health.CurrentHealth = 60; // 満タンではなく60%で開始
            }
            
            var btRunner = aiObject.AddComponent<BehaviourTreeRunner>();
            // BehaviourTreeRunnerのSerializedFieldに直接アクセスするため、SerializedObjectを使用
            SerializedObject so = new SerializedObject(btRunner);
            so.FindProperty("behaviourTreeFilePath").stringValue = selectedBTFile;
            so.FindProperty("debugMode").boolValue = enableDebugMode;
            so.ApplyModifiedProperties();
            
            // ビジュアルフィードバックコンポーネント追加
            aiObject.AddComponent<UI.ActionIndicator>();
            
            // AIステータス表示UI作成
            CreateAIStatusDisplay(aiObject);
            
            // タグ設定
            aiObject.tag = "Player";
            
            Selection.activeGameObject = aiObject;
        }
        
        void CreateEnemies()
        {
            int enemiesToCreate = createMultipleEnemies ? enemyCount : 1;
            
            for (int i = 0; i < enemiesToCreate; i++)
            {
                var enemyObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                enemyObject.name = $"Enemy{i + 1}";
                
                // Position with some variation
                Vector3 pos = enemyPosition;
                if (i > 0)
                {
                    pos += new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
                }
                enemyObject.transform.position = pos;
                
                // Red material from Assets/Materials
                var renderer = enemyObject.GetComponent<Renderer>();
                var redMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Red.mat");
                if (redMaterial != null)
                {
                    renderer.material = redMaterial;
                }
                
                // Health component
                var health = enemyObject.AddComponent<Health>();
                health.MaxHealth = 50;
                health.CurrentHealth = 50;
                
                // Tag設定
                enemyObject.tag = "Enemy";
            }
        }
        
        void CreatePatrolPoints()
        {
            // パトロールポイント1
            var patrol1 = new GameObject("patrol_point_1");
            patrol1.transform.position = new Vector3(-5, 0, -5);
            CreatePatrolPointVisual(patrol1, Color.green);
            
            // パトロールポイント2
            var patrol2 = new GameObject("patrol_point_2");
            patrol2.transform.position = new Vector3(5, 0, -5);
            CreatePatrolPointVisual(patrol2, Color.green);
            
            // パトロールポイント3
            var patrol3 = new GameObject("patrol_point_3");
            patrol3.transform.position = new Vector3(0, 0, 5);
            CreatePatrolPointVisual(patrol3, Color.green);
        }
        
        void CreatePatrolPointVisual(GameObject point, Color color)
        {
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.transform.SetParent(point.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = Vector3.one * 0.5f;
            
            var renderer = visual.GetComponent<Renderer>();
            var material = GetMaterialByColor(color);
            if (material != null)
            {
                renderer.material = material;
            }
            
            // Colliderは不要
            DestroyImmediate(visual.GetComponent<Collider>());
        }
        
        void CreateEnvironmentObjects()
        {
            // セーフゾーン
            var safeZone = new GameObject("SafeZone");
            safeZone.transform.position = new Vector3(-10, 0, 0);
            safeZone.tag = "SafeZone";
            CreateEnvironmentVisual(safeZone, Color.magenta, new Vector3(2, 0.1f, 2));
            
            // リソースポイント
            var resourcePoint = new GameObject("resource_point");
            resourcePoint.transform.position = new Vector3(10, 0, 10);
            CreateEnvironmentVisual(resourcePoint, Color.yellow, Vector3.one);
            
            // ガードポスト
            var guardPost = new GameObject("guard_post");
            guardPost.transform.position = Vector3.zero;
            CreateEnvironmentVisual(guardPost, Color.cyan, new Vector3(1, 2, 1));
            
            // complex_example.bt用の特別オブジェクト
            if (selectedBTFile == "complex_example.bt")
            {
                // 興味深いオブジェクト（調査用）- AIの近くに配置
                var interestObject = new GameObject("InterestingObject");
                interestObject.transform.position = new Vector3(4, 0, -3); // より近い位置
                interestObject.tag = "Interactable";
                CreateEnvironmentVisual(interestObject, Color.cyan, new Vector3(1, 1.5f, 1));
                BTLogger.Info($"Created InterestingObject at {interestObject.transform.position} with tag: {interestObject.tag}");
                
                // アイテムオブジェクト
                var itemObject = new GameObject("PickupItem");
                itemObject.transform.position = new Vector3(-4, 0, 3); // より近い位置
                itemObject.tag = "Item";
                CreateEnvironmentVisual(itemObject, Color.yellow, new Vector3(0.5f, 0.5f, 0.5f));
                BTLogger.Info($"Created PickupItem at {itemObject.transform.position} with tag: {itemObject.tag}");
                
                // 追加のテスト用オブジェクト（さらに近い位置）
                var nearbyObject = new GameObject("NearbyTestObject");
                nearbyObject.transform.position = new Vector3(2, 0, 0); // 非常に近い位置
                nearbyObject.tag = "Interactable";
                CreateEnvironmentVisual(nearbyObject, Color.green, new Vector3(0.8f, 0.8f, 0.8f));
                BTLogger.Info($"Created NearbyTestObject at {nearbyObject.transform.position} with tag: {nearbyObject.tag}");
                
                // 追加のセーフゾーン
                var safeZone2 = new GameObject("SafeZone2");
                safeZone2.transform.position = new Vector3(15, 0, -8);
                safeZone2.tag = "SafeZone";
                CreateEnvironmentVisual(safeZone2, Color.magenta, new Vector3(1.5f, 0.1f, 1.5f));
            }
        }
        
        void CreateEnvironmentVisual(GameObject obj, Color color, Vector3 scale)
        {
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.transform.SetParent(obj.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = scale;
            
            // 子オブジェクトにも親と同じタグを設定（Physics.OverlapSphere用）
            visual.tag = obj.tag;
            
            var renderer = visual.GetComponent<Renderer>();
            var material = GetMaterialByColor(color);
            if (material != null)
            {
                renderer.material = material;
            }
            
            // Colliderは残す（相互作用用）
        }
        
        void SetupCamera()
        {
            // メインカメラを探す
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                // メインカメラが存在しない場合は作成
                var cameraObject = new GameObject("Main Camera");
                mainCamera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
                cameraObject.tag = "MainCamera";
            }
            
            // SceneCameraコンポーネントを追加
            var sceneCamera = mainCamera.GetComponent<CameraControl.SceneCamera>();
            if (sceneCamera == null)
            {
                sceneCamera = mainCamera.gameObject.AddComponent<CameraControl.SceneCamera>();
            }
            
            // AIターゲットを設定
            GameObject aiObject = GameObject.Find("TestAI");
            if (aiObject != null)
            {
                sceneCamera.SetFollowTarget(aiObject.transform);
                sceneCamera.SetFollowOffset(cameraPosition - aiPosition);
            }
            
            // カメラ位置を設定（AIエージェントを見下ろす位置）
            mainCamera.transform.position = cameraPosition;
            mainCamera.transform.LookAt(aiPosition);
            
            // カメラ設定を最適化
            mainCamera.fieldOfView = 60f;
            mainCamera.nearClipPlane = 0.3f;
            mainCamera.farClipPlane = 100f;
            
            BTLogger.Info($"Enhanced camera with SceneCamera controller positioned at {mainCamera.transform.position}");
        }
        
        Material GetMaterialByColor(Color color)
        {
            if (color == Color.red)
                return AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Red.mat");
            else if (color == Color.blue)
                return AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Blue.mat");
            else if (color == Color.green)
                return AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Green.mat");
            else if (color == Color.magenta)
                return AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Purple.mat");
            else if (color == Color.yellow || color == Color.cyan)
                return AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Gold.mat");
            
            // Fallback to first available material
            return AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Blue.mat");
        }
        
        void CreateAIStatusDisplay(GameObject aiObject)
        {
            // AIステータス表示UI作成
            var statusDisplayObject = new GameObject("AIStatusDisplayManager");
            var statusDisplay = statusDisplayObject.AddComponent<UI.AIStatusDisplay>();
            
            // AI参照を設定
            statusDisplay.aiRunner = aiObject.GetComponent<BehaviourTreeRunner>();
            statusDisplay.aiHealth = aiObject.GetComponent<Health>();
            statusDisplay.aiMana = aiObject.GetComponent<Mana>();
            statusDisplay.aiInventory = aiObject.GetComponent<Inventory>();
            
            // 詳細ログ表示を有効化
            statusDisplay.showDetailedLogs = true;
            statusDisplay.updateInterval = 0.1f;
            
            BTLogger.Info("AI Status Display created for enhanced visual feedback");
        }

        public void ClearTestEnvironment()
        {
            // 既存のテストオブジェクトを削除
            string[] testObjectNames = {
                "TestAI", "TeamMemberAI", "Enemy1", "Enemy2", "Enemy3", "Enemy4", "Enemy5",
                "patrol_point_1", "patrol_point_2", "patrol_point_3",
                "SafeZone", "SafeZone2", "resource_point", "guard_post",
                "InterestingObject", "PickupItem", "NearbyTestObject", "AIStatusDisplayManager", "AIStatusCanvas"
            };
            
            foreach (string name in testObjectNames)
            {
                var obj = GameObject.Find(name);
                if (obj != null)
                {
                    DestroyImmediate(obj);
                }
            }
            
            BTLogger.Info("Test environment cleared.");
        }
        
        void CreateRequiredTags()
        {
            CreateTag("SafeZone");
            CreateTag("Interactable");
            CreateTag("Item");
        }
        
        void CreateTag(string tagName)
        {
            // TagManagerアセットを取得
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            
            // タグが既に存在するかチェック
            bool found = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
                if (t.stringValue.Equals(tagName))
                {
                    found = true;
                    break;
                }
            }
            
            // タグが存在しない場合は追加
            if (!found)
            {
                tagsProp.InsertArrayElementAtIndex(0);
                SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
                newTagProp.stringValue = tagName;
                tagManager.ApplyModifiedProperties();
            }
        }
    }
    
    // メニューアイテム用の便利メソッド
    public class AIBTreeQuickSetup
    {
        [MenuItem("BehaviourTree/Quick Setup/BlackBoard Test Environment")]
        static void CreateBlackBoardTest()
        {
            var setup = ScriptableObject.CreateInstance<AIBTreeTestEnvironmentSetup>();
            setup.SetupBlackBoardTest();
            setup.CreateTestEnvironment();
        }
        
        [MenuItem("BehaviourTree/Quick Setup/Dynamic Condition Test Environment")]
        static void CreateDynamicConditionTest()
        {
            var setup = ScriptableObject.CreateInstance<AIBTreeTestEnvironmentSetup>();
            setup.SetupDynamicConditionTest();
            setup.CreateTestEnvironment();
        }
        
        [MenuItem("BehaviourTree/Quick Setup/Complex Example Test Environment")]
        static void CreateComplexExampleTest()
        {
            var setup = ScriptableObject.CreateInstance<AIBTreeTestEnvironmentSetup>();
            setup.SetupComplexExampleTest();
            setup.CreateTestEnvironment();
        }
        
        [MenuItem("BehaviourTree/Quick Setup/Clear All Test Objects")]
        static void ClearAllTestObjects()
        {
            var setup = ScriptableObject.CreateInstance<AIBTreeTestEnvironmentSetup>();
            setup.ClearTestEnvironment();
        }
    }
}