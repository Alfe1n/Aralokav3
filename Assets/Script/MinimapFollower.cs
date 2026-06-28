using UnityEngine;
using UnityEngine.SceneManagement;

public class MinimapFollower : MonoBehaviour
{
    [Header("Tracking Settings")]
    [Tooltip("Kecepatan transisi kamera mengikuti player (smooth damping)")]
    public float smoothTime = 0.15f;
    
    [Tooltip("Offset posisi kamera terhadap player (biasanya X=0, Y=0, Z=-10)")]
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    private Camera minimapCamera;
    private Vector3 velocity = Vector3.zero;

    void Awake()
    {
        minimapCamera = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        Transform target = null;
        bool shouldBeActive = false;

        // 1. Cari player aktif secara dinamis
        if (PlayerMovement.ActivePlayerInstance != null)
        {
            // Hanya aktif jika player yang sedang dikendalikan adalah Orang Utan
            if (PlayerMovement.ActivePlayerInstance.CompareTag("Player-Orang Utan"))
            {
                target = PlayerMovement.ActivePlayerInstance.transform;
                
                // Hanya aktif jika berada di scene Hutan
                string sceneName = SceneManager.GetActiveScene().name;
                if (sceneName != null && sceneName.Contains("Hutan"))
                {
                    shouldBeActive = true;
                }
            }
        }

        // 2. Aktifkan/nonaktifkan kamera minimap sesuai kondisi untuk menghemat performa rendering
        if (minimapCamera != null)
        {
            minimapCamera.enabled = shouldBeActive;
        }

        // 3. Ikuti target jika aktif
        if (shouldBeActive && target != null)
        {
            Vector3 targetPosition = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }
}
