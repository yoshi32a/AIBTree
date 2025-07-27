using UnityEngine;
using UnityEngine.InputSystem;
using ArcBT.Logger;

namespace CameraControl
{
    /// <summary>シーン観察用のカメラコントローラー</summary>
    public class SceneCamera : MonoBehaviour
    {
        [Header("Target Following")]
        public Transform followTarget;
        public Vector3 followOffset = new Vector3(0, 15, -10);
        public float followSmoothness = 2.0f;
        public bool autoFindAI = true;

        [Header("Manual Control")]
        public float moveSpeed = 10.0f;
        public float rotateSpeed = 100.0f;
        public bool enableManualControl = true;

        [Header("View Settings")]
        public float fieldOfView = 60f;
        public float nearClip = 0.3f;
        public float farClip = 100f;

        Camera mainCamera;
        Vector3 lastTargetPosition;
        bool isFollowingTarget = true;

        void Start()
        {
            mainCamera = GetComponent<Camera>();
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera != null)
            {
                mainCamera.fieldOfView = fieldOfView;
                mainCamera.nearClipPlane = nearClip;
                mainCamera.farClipPlane = farClip;
            }

            // AIを自動検索
            if (autoFindAI && followTarget == null)
            {
                FindAITarget();
            }
        }

        void Update()
        {
            HandleInput();
            
            if (isFollowingTarget && followTarget != null)
            {
                FollowTarget();
            }
        }

        void HandleInput()
        {
            if (!enableManualControl) return;

            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            // フォローモード切り替え
            if (keyboard.fKey.wasPressedThisFrame)
            {
                isFollowingTarget = !isFollowingTarget;
                BTLogger.Info($"Camera Follow Mode: {isFollowingTarget}");
            }

            // カメラリセット
            if (keyboard.rKey.wasPressedThisFrame)
            {
                ResetCamera();
            }

            // 手動カメラ操作（フォローモード無効時）
            if (!isFollowingTarget)
            {
                HandleManualMovement();
            }

            // ズーム操作
            HandleZoom();
        }

        void HandleManualMovement()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard == null || mouse == null) return;

            // WASD移動
            Vector3 moveDirection = Vector3.zero;
            if (keyboard.wKey.isPressed) moveDirection += transform.forward;
            if (keyboard.sKey.isPressed) moveDirection -= transform.forward;
            if (keyboard.aKey.isPressed) moveDirection -= transform.right;
            if (keyboard.dKey.isPressed) moveDirection += transform.right;
            if (keyboard.qKey.isPressed) moveDirection += Vector3.up;
            if (keyboard.eKey.isPressed) moveDirection -= Vector3.up;

            transform.position += moveDirection * moveSpeed * Time.deltaTime;

            // マウス回転
            if (mouse.rightButton.isPressed) // 右クリック
            {
                Vector2 mouseDelta = mouse.delta.ReadValue();
                float mouseX = mouseDelta.x * 0.1f; // スケール調整
                float mouseY = mouseDelta.y * 0.1f;

                transform.Rotate(-mouseY * rotateSpeed * Time.deltaTime, mouseX * rotateSpeed * Time.deltaTime, 0);
            }
        }

        void HandleZoom()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            Vector2 scrollDelta = mouse.scroll.ReadValue();
            float scroll = scrollDelta.y * 0.001f; // スケール調整

            if (scroll != 0)
            {
                if (isFollowingTarget)
                {
                    // フォローモード時はオフセット距離を調整
                    followOffset = followOffset.normalized * Mathf.Clamp(followOffset.magnitude - scroll * 5f, 5f, 30f);
                }
                else
                {
                    // 手動モード時はFOVを調整
                    fieldOfView = Mathf.Clamp(fieldOfView - scroll * 10f, 20f, 100f);
                    if (mainCamera != null)
                    {
                        mainCamera.fieldOfView = fieldOfView;
                    }
                }
            }
        }

        void FollowTarget()
        {
            if (followTarget == null) return;

            Vector3 targetPosition = followTarget.position + followOffset;
            
            // スムーズに移動
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSmoothness * Time.deltaTime);
            
            // ターゲットを見る
            Vector3 lookDirection = followTarget.position - transform.position;
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, followSmoothness * Time.deltaTime);
            }

            // ターゲットが大きく移動した場合は即座に追従
            if (Vector3.Distance(followTarget.position, lastTargetPosition) > 10f)
            {
                transform.position = targetPosition;
            }
            
            lastTargetPosition = followTarget.position;
        }

        void FindAITarget()
        {
            // BehaviourTreeRunnerを持つオブジェクトを検索
            var btRunner = FindFirstObjectByType<ArcBT.Core.BehaviourTreeRunner>();
            if (btRunner != null)
            {
                followTarget = btRunner.transform;
                BTLogger.Info($"Camera target set to AI: {followTarget.name}");
            }
            else
            {
                // TestAIオブジェクトを検索
                GameObject testAI = GameObject.Find("TestAI");
                if (testAI != null)
                {
                    followTarget = testAI.transform;
                    BTLogger.Info($"Camera target set to: {followTarget.name}");
                }
            }
        }

        void ResetCamera()
        {
            if (followTarget != null)
            {
                followOffset = new Vector3(0, 15, -10);
                transform.position = followTarget.position + followOffset;
                transform.LookAt(followTarget.position);
                isFollowingTarget = true;
                fieldOfView = 60f;
                
                if (mainCamera != null)
                {
                    mainCamera.fieldOfView = fieldOfView;
                }
                
                BTLogger.Info("Camera reset to default follow position");
            }
        }

        void OnGUI()
        {
            // 画面左下にコントロール情報を表示
            GUILayout.BeginArea(new Rect(10, Screen.height - 150, 300, 140));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("Camera Controls:", GUI.skin.label);
            GUILayout.Label("F - Toggle Follow Mode");
            GUILayout.Label("R - Reset Camera");
            GUILayout.Label("WASD - Move Camera");
            GUILayout.Label("QE - Up/Down");
            GUILayout.Label("Right Click + Mouse - Rotate");
            GUILayout.Label("Mouse Wheel - Zoom");
            GUILayout.Label($"Mode: {(isFollowingTarget ? "Follow" : "Manual")}");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        // パブリックメソッド
        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
            isFollowingTarget = true;
            BTLogger.Info($"Camera follow target set to: {target.name}");
        }

        public void SetFollowOffset(Vector3 offset)
        {
            followOffset = offset;
        }

        public void ToggleFollowMode()
        {
            isFollowingTarget = !isFollowingTarget;
        }
    }
}
