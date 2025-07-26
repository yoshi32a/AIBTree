using UnityEngine;
using UnityEngine.UI;
using ArcBT.Core;
using ArcBT.Samples.RPG.Components;

namespace UI
{
    /// <summary>AIの状態をリアルタイムで表示するUIコンポーネント</summary>
    public class AIStatusDisplay : MonoBehaviour
    {
        [Header("UI References")]
        public Canvas statusCanvas;
        public Text healthText;
        public Text manaText;
        public Text currentActionText;
        public Text currentTargetText;
        public Text blackBoardStatusText;
        public Image healthBar;
        public Image manaBar;
        public Text inventoryText;

        [Header("AI References")]
        public BehaviourTreeRunner aiRunner;
        public Health aiHealth;
        public Mana aiMana;
        public Inventory aiInventory;

        [Header("Display Settings")]
        public bool showDetailedLogs = true;
        public float updateInterval = 0.1f;

        float lastUpdateTime;
        Camera mainCamera;

        void Start()
        {
            mainCamera = Camera.main;
            
            // AIコンポーネントを自動検出
            if (aiRunner == null)
                aiRunner = FindFirstObjectByType<BehaviourTreeRunner>();
            
            if (aiRunner != null)
            {
                aiHealth = aiRunner.GetComponent<Health>();
                aiMana = aiRunner.GetComponent<Mana>();
                aiInventory = aiRunner.GetComponent<Inventory>();
            }

            // UIキャンバスを作成
            CreateStatusUI();
        }

        void Update()
        {
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateStatusDisplay();
                lastUpdateTime = Time.time;
            }

            // カメラがAIを向くように調整
            if (aiRunner != null && mainCamera != null)
            {
                UpdateCameraFocus();
            }
        }

        void CreateStatusUI()
        {
            if (statusCanvas != null) return;

            // UIキャンバス作成
            var canvasObject = new GameObject("AIStatusCanvas");
            statusCanvas = canvasObject.AddComponent<Canvas>();
            statusCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            statusCanvas.sortingOrder = 100;

            var canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1280, 720);

            canvasObject.AddComponent<GraphicRaycaster>();

            // メインパネル作成
            var panelObject = new GameObject("StatusPanel");
            panelObject.transform.SetParent(statusCanvas.transform, false);
            
            var panelImage = panelObject.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);
            
            var panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 1);
            panelRect.anchorMax = new Vector2(0, 1);
            panelRect.pivot = new Vector2(0, 1); // 左上をピボットに設定
            panelRect.anchoredPosition = new Vector2(10, -10); // 画面左上から10px内側
            panelRect.sizeDelta = new Vector2(350, 200); // 固定サイズ

            // テキスト要素を作成
            CreateStatusText(panelObject, "Health: ", ref healthText, new Vector2(10, -30));
            CreateStatusText(panelObject, "Mana: ", ref manaText, new Vector2(10, -60));
            CreateStatusText(panelObject, "Action: ", ref currentActionText, new Vector2(10, -90));
            CreateStatusText(panelObject, "Target: ", ref currentTargetText, new Vector2(10, -120));
            CreateStatusText(panelObject, "Inventory: ", ref inventoryText, new Vector2(10, -150));
            CreateStatusText(panelObject, "BlackBoard: ", ref blackBoardStatusText, new Vector2(10, -180));

            // ヘルスバー作成
            CreateStatusBar(panelObject, ref healthBar, new Vector2(120, -30), Color.red);
            
            // マナバー作成
            CreateStatusBar(panelObject, ref manaBar, new Vector2(120, -60), Color.blue);
        }

        void CreateStatusText(GameObject parent, string label, ref Text textComponent, Vector2 position)
        {
            var textObject = new GameObject($"Text_{label}");
            textObject.transform.SetParent(parent.transform, false);
            
            textComponent = textObject.AddComponent<Text>();
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = 16;
            textComponent.color = Color.white;
            textComponent.text = label + "Loading...";
            
            var rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1); // 左上をピボットに設定
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = new Vector2(200, 25);
        }

        void CreateStatusBar(GameObject parent, ref Image barImage, Vector2 position, Color color)
        {
            // バー背景
            var bgObject = new GameObject("BarBackground");
            bgObject.transform.SetParent(parent.transform, false);
            var bgImage = bgObject.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            var bgRect = bgObject.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 1);
            bgRect.anchorMax = new Vector2(0, 1);
            bgRect.pivot = new Vector2(0, 1); // 左上をピボットに設定
            bgRect.anchoredPosition = position;
            bgRect.sizeDelta = new Vector2(150, 20);

            // バー本体
            var barObject = new GameObject("Bar");
            barObject.transform.SetParent(bgObject.transform, false);
            barImage = barObject.AddComponent<Image>();
            barImage.color = color;
            barImage.type = Image.Type.Filled;
            barImage.fillMethod = Image.FillMethod.Horizontal;
            
            var barRect = barObject.GetComponent<RectTransform>();
            barRect.anchorMin = Vector2.zero;
            barRect.anchorMax = Vector2.one;
            barRect.offsetMin = Vector2.zero;
            barRect.offsetMax = Vector2.zero;
        }

        void UpdateStatusDisplay()
        {
            if (aiRunner == null) return;

            // ヘルス表示
            if (aiHealth != null)
            {
                healthText.text = $"Health: {aiHealth.CurrentHealth}/{aiHealth.MaxHealth}";
                healthBar.fillAmount = (float)aiHealth.CurrentHealth / aiHealth.MaxHealth;
            }

            // マナ表示
            if (aiMana != null)
            {
                manaText.text = $"Mana: {aiMana.CurrentMana}/{aiMana.MaxMana}";
                manaBar.fillAmount = (float)aiMana.CurrentMana / aiMana.MaxMana;
            }

            // 現在のアクション表示
            var blackBoard = aiRunner.BlackBoard;
            if (blackBoard != null)
            {
                var currentAction = blackBoard.GetValue<string>("current_action") ?? "Idle";
                currentActionText.text = $"Action: {currentAction}";

                var currentTarget = blackBoard.GetValue<GameObject>("current_target");
                if (currentTarget != null)
                {
                    currentTargetText.text = $"Target: {currentTarget.name}";
                }
                else
                {
                    currentTargetText.text = "Target: None";
                }

                // BlackBoard主要情報表示
                var isInCombat = blackBoard.GetValue<bool>("is_in_combat");
                var isFleeing = blackBoard.GetValue<bool>("is_fleeing");
                var isPatrolling = blackBoard.GetValue<bool>("is_patrolling");
                
                string status = "";
                if (isInCombat) status += "[COMBAT] ";
                if (isFleeing) status += "[FLEEING] ";
                if (isPatrolling) status += "[PATROL] ";
                if (string.IsNullOrEmpty(status)) status = "[IDLE]";
                
                blackBoardStatusText.text = $"Status: {status}";
            }

            // インベントリ表示
            if (aiInventory != null)
            {
                var items = aiInventory.GetAllItems();
                string itemText = "Items: ";
                foreach (var item in items)
                {
                    itemText += $"{item.Key}({item.Value}) ";
                }
                inventoryText.text = itemText;
            }
        }

        void UpdateCameraFocus()
        {
            // AIの位置を中心にカメラを配置
            Vector3 aiPosition = aiRunner.transform.position;
            Vector3 desiredCameraPos = aiPosition + new Vector3(0, 15, -10);
            
            // スムーズにカメラを移動
            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position, 
                desiredCameraPos, 
                Time.deltaTime * 2.0f
            );
            
            // AIを見下ろす角度に調整
            mainCamera.transform.LookAt(aiPosition);
        }

        void OnGUI()
        {
            if (!showDetailedLogs || aiRunner == null) return;

            // 画面右側に詳細ログ表示
            GUILayout.BeginArea(new Rect(Screen.width - 400, 10, 390, Screen.height - 20));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("AI Behavior Tree Debug", GUI.skin.box);
            
            var blackBoard = aiRunner.BlackBoard;
            if (blackBoard != null)
            {
                var allKeys = blackBoard.GetAllKeys();
                foreach (var key in allKeys)
                {
                    var value = blackBoard.GetValueAsString(key);
                    GUILayout.Label($"{key}: {value}");
                }
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}