using UnityEngine;
using BehaviourTree.Core;

namespace UI
{
    /// <summary>AIの行動を視覚的に表示するインジケーター</summary>
    public class ActionIndicator : MonoBehaviour
    {
        [Header("Visual Settings")]
        public float indicatorHeight = 2.0f;
        public float textSize = 0.1f;
        public Color textColor = Color.white;
        public Color backgroundColor = Color.black;

        [Header("Action Colors")]
        public Color moveColor = Color.green;
        public Color attackColor = Color.red;
        public Color fleeColor = Color.yellow;
        public Color idleColor = Color.gray;
        public Color magicColor = Color.magenta;

        GameObject indicatorObject;
        TextMesh indicatorText;
        GameObject backgroundQuad;
        BehaviourTreeRunner btRunner;
        BlackBoard blackBoard;
        
        string currentAction = "";
        float lastActionTime = 0f;
        Vector3 targetPosition;
        LineRenderer targetLine;

        void Start()
        {
            btRunner = GetComponent<BehaviourTreeRunner>();
            CreateVisualIndicator();
            CreateTargetLine();
        }

        void Update()
        {
            if (btRunner != null)
            {
                blackBoard = btRunner.BlackBoard;
                UpdateActionDisplay();
                UpdateTargetLine();
            }
        }

        void CreateVisualIndicator()
        {
            // インジケーター親オブジェクト
            indicatorObject = new GameObject("ActionIndicator");
            indicatorObject.transform.SetParent(transform);
            indicatorObject.transform.localPosition = new Vector3(0, indicatorHeight, 0);

            // 背景クアッド作成
            backgroundQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            backgroundQuad.transform.SetParent(indicatorObject.transform);
            backgroundQuad.transform.localPosition = Vector3.zero;
            backgroundQuad.transform.localScale = new Vector3(2.0f, 0.5f, 1.0f);
            backgroundQuad.transform.localRotation = Quaternion.LookRotation(Vector3.forward);
            
            var bgRenderer = backgroundQuad.GetComponent<Renderer>();
            var bgMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            bgMaterial.color = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0.7f);
            
            // URPのTransparent設定
            bgMaterial.SetFloat("_Surface", 1); // Transparent
            bgMaterial.SetFloat("_Blend", 0); // Alpha blend
            bgMaterial.SetFloat("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            bgMaterial.SetFloat("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            bgMaterial.SetFloat("_ZWrite", 0);
            bgMaterial.renderQueue = 3000;
            bgRenderer.material = bgMaterial;

            // テキストメッシュ作成
            var textObject = new GameObject("ActionText");
            textObject.transform.SetParent(indicatorObject.transform);
            textObject.transform.localPosition = new Vector3(0, 0, -0.01f); // 背景より少し前

            indicatorText = textObject.AddComponent<TextMesh>();
            indicatorText.text = "Initializing...";
            indicatorText.fontSize = 50;
            indicatorText.characterSize = textSize;
            indicatorText.color = textColor;
            indicatorText.anchor = TextAnchor.MiddleCenter;
            indicatorText.alignment = TextAlignment.Center;

            // 初期化時はカメラ向き設定をしない（Updateで処理）
        }

        void CreateTargetLine()
        {
            // ターゲットへのライン表示用
            var lineObject = new GameObject("TargetLine");
            lineObject.transform.SetParent(transform);
            
            targetLine = lineObject.AddComponent<LineRenderer>();
            targetLine.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            targetLine.startColor = Color.cyan;
            targetLine.endColor = Color.cyan;
            targetLine.startWidth = 0.1f;
            targetLine.endWidth = 0.1f;
            targetLine.positionCount = 2;
            targetLine.enabled = false;
        }

        void UpdateActionDisplay()
        {
            if (blackBoard == null) return;

            // 現在のアクションを取得
            string newAction = blackBoard.GetValue<string>("current_action") ?? "Idle";
            
            if (newAction != currentAction || Time.time - lastActionTime > 1.0f)
            {
                currentAction = newAction;
                lastActionTime = Time.time;
                
                // アクションに応じて表示を更新
                UpdateIndicatorAppearance(currentAction);
            }

            // インジケーターを常にカメラの方向に向ける（ビルボード効果）
            if (Camera.main != null && indicatorObject != null)
            {
                // カメラの方向を向く（テキストが正しく表示されるように）
                indicatorObject.transform.LookAt(Camera.main.transform);
                // テキストが反転しないように180度回転
                indicatorObject.transform.Rotate(0, 180, 0);
            }
        }

        void UpdateIndicatorAppearance(string action)
        {
            if (indicatorText == null) return;

            Color actionColor = idleColor;
            string displayText = action;

            // アクションタイプに応じて色と表示を設定
            switch (action.ToLower())
            {
                case "moveto":
                case "movetoposition":
                case "movetoenemy":
                case "randomwander":
                    actionColor = moveColor;
                    displayText = "🚶 Moving";
                    break;
                    
                case "normalattack":
                case "attackenemy":
                    actionColor = attackColor;
                    displayText = "⚔️ Attacking";
                    break;
                    
                case "fleetosafety":
                    actionColor = fleeColor;
                    displayText = "🏃 Fleeing";
                    break;
                    
                case "castspell":
                    actionColor = magicColor;
                    displayText = "✨ Magic";
                    break;
                    
                case "environmentscan":
                    actionColor = Color.blue;
                    displayText = "👁️ Scanning";
                    break;
                    
                case "wait":
                    actionColor = idleColor;
                    displayText = "⏸️ Waiting";
                    break;
                    
                case "useitem":
                    actionColor = Color.green;
                    displayText = "🧪 Using Item";
                    break;
                    
                default:
                    displayText = $"❓ {action}";
                    break;
            }

            indicatorText.text = displayText;
            indicatorText.color = actionColor;
            
            // 背景色も更新
            var bgRenderer = backgroundQuad.GetComponent<Renderer>();
            if (bgRenderer != null)
            {
                var material = bgRenderer.material;
                material.color = new Color(actionColor.r, actionColor.g, actionColor.b, 0.3f);
            }
        }

        void UpdateTargetLine()
        {
            if (blackBoard == null || targetLine == null) return;

            // ターゲット位置を取得
            GameObject target = blackBoard.GetValue<GameObject>("current_target");
            if (target == null)
            {
                target = blackBoard.GetValue<GameObject>("enemy_target");
            }

            if (target != null)
            {
                // ラインを表示
                targetLine.enabled = true;
                targetLine.SetPosition(0, transform.position + Vector3.up * 0.5f);
                targetLine.SetPosition(1, target.transform.position + Vector3.up * 0.5f);
                
                // ターゲットまでの距離を表示
                float distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance < 10.0f) // 近距離のみ表示
                {
                    targetLine.startColor = Color.red;
                    targetLine.endColor = Color.red;
                }
                else
                {
                    targetLine.startColor = Color.cyan;
                    targetLine.endColor = Color.cyan;
                }
            }
            else
            {
                targetLine.enabled = false;
            }
        }

        void OnDrawGizmos()
        {
            // エディタでの視覚化
            if (blackBoard != null)
            {
                // 検出範囲を表示
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 5.0f); // 攻撃範囲
                
                Gizmos.color = Color.cyan;  
                Gizmos.DrawWireSphere(transform.position, 12.0f); // スキャン範囲
                
                // 現在のターゲット表示
                GameObject target = blackBoard.GetValue<GameObject>("current_target");
                if (target != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(transform.position, target.transform.position);
                    Gizmos.DrawWireSphere(target.transform.position, 1.0f);
                }
            }
        }
    }
}