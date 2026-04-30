using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    private Vector3 offset;

    void Start()
    {
        offset = transform.position - target.position;
    }

    void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position + offset;

        // 🔥 paksa kamera selalu lihat player
        transform.LookAt(target);
    }
}