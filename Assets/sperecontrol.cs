using UnityEngine;
using UnityEngine.InputSystem;

public class sperecontrol : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSpeed = 360f;
    [SerializeField] private float touchStickRadius = 120f;
    private Rigidbody body;
    private bool touchMoving;
    private Vector2 touchStart;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 新しいInput Systemでの入力取得
        Vector2 input = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) input.y += 1f;
            if (Keyboard.current.sKey.isPressed) input.y -= 1f;
            if (Keyboard.current.aKey.isPressed) input.x -= 1f;
            if (Keyboard.current.dKey.isPressed) input.x += 1f;
        }

        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            Vector2 touchPosition = touch.position.ReadValue();

            if (touch.press.wasPressedThisFrame && touchPosition.x < Screen.width * 0.5f)
            {
                touchStart = touchPosition;
                touchMoving = true;
            }

            if (touchMoving)
            {
                if (!touch.press.isPressed)
                {
                    touchMoving = false;
                }
                else
                {
                    Vector2 drag = touchPosition - touchStart;
                    input += Vector2.ClampMagnitude(drag / touchStickRadius, 1f);
                }
            }
        }

        // 入力が無い場合は何もしない
        if (input.sqrMagnitude < 0.01f)
        {
            if (body != null)
            {
                Vector3 velocity = body.linearVelocity;
                velocity.x = 0f;
                velocity.z = 0f;
                body.linearVelocity = velocity;
                body.angularVelocity = Vector3.zero;
            }
            return;
        }

        // メインカメラの向きを取得
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        // カメラの向きに基づいた移動方向の計算（水平面上）
        Vector3 cameraForward = Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up).normalized;
        Vector3 cameraRight = Vector3.ProjectOnPlane(mainCamera.transform.right, Vector3.up).normalized;

        Vector3 moveDirection = (cameraForward * input.y + cameraRight * input.x).normalized;

        // 移動
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // 球体の向きを移動方向に向ける
        if (moveDirection != Vector3.zero)
        {
            // Y軸を固定して回転させる（回転のバタつきを抑える）
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
        }
    }

    void OnGUI()
    {
        if (Touchscreen.current == null) return;

        GUIStyle hintStyle = new GUIStyle(GUI.skin.label);
        hintStyle.fontSize = 18;
        hintStyle.normal.textColor = new Color(1f, 1f, 1f, 0.7f);
        GUI.Label(new Rect(24, Screen.height - 70, 260, 36), "Drag here to move", hintStyle);
    }
}
