using UnityEngine;

public class camera_sperehunt : MonoBehaviour
{
    [Header("追従対象 (Sphereをアサインしてください)")]
    public Transform target;

    [Header("設定")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -2f);
    [SerializeField] private float xRotation = 30f;

    void LateUpdate()
    {
        if (target == null)
        {
            // ヒエラルキーからSphereを探す
            GameObject go = GameObject.Find("Sphere");
            if (go == null) go = GameObject.Find("Spere");
            if (go != null) target = go.transform;
            else return;
        }

        // targetのローカル座標 (0, 2, -2) をワールド座標に変換してカメラの位置にする
        float yaw = target.eulerAngles.y;
        Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);
        transform.position = target.position + yawRotation * offset;

        // targetの回転を基準に、X軸を30度傾けてカメラの向きにする
        transform.rotation = yawRotation * Quaternion.Euler(xRotation, 0f, 0f);
    }
}

