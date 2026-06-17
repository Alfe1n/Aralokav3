using UnityEngine;

public class CameraRegister : MonoBehaviour
{
    [Tooltip("Isi dengan 'Normal' untuk kamera default, atau 'Locked' untuk kamera arena/locked")]
    public string cameraKey;

    public static GameObject normalCamera;
    public static GameObject lockedCamera;

    private void Awake()
    {
        RegisterCamera();
    }

    private void OnEnable()
    {
        RegisterCamera();
    }

    private void RegisterCamera()
    {
        if (string.IsNullOrEmpty(cameraKey)) return;

        if (cameraKey.Equals("Normal", System.StringComparison.OrdinalIgnoreCase))
        {
            normalCamera = gameObject;
        }
        else if (cameraKey.Equals("Locked", System.StringComparison.OrdinalIgnoreCase))
        {
            lockedCamera = gameObject;
        }
    }

    private void OnDestroy()
    {
        if (cameraKey.Equals("Normal", System.StringComparison.OrdinalIgnoreCase) && normalCamera == gameObject)
        {
            normalCamera = null;
        }
        else if (cameraKey.Equals("Locked", System.StringComparison.OrdinalIgnoreCase) && lockedCamera == gameObject)
        {
            lockedCamera = null;
        }
    }
}
