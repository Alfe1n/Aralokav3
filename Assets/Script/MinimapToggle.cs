using UnityEngine;
using UnityEngine.SceneManagement;

public class MinimapToggle : MonoBehaviour
{
    public static MinimapToggle Instance { get; private set; }

    [Header("UI Groups")]
    [Tooltip("UI Minimap kecil di pojok layar")]
    public GameObject minimapGroup;
    [Tooltip("UI Map besar di tengah layar")]
    public GameObject fullMapGroup;

    [Header("Camera Settings")]
    [Tooltip("Kamera minimap")]
    public Camera minimapCamera;
    [Tooltip("Orthographic Size saat minimap kecil (zoom dekat)")]
    public float normalOrthoSize = 15f;
    [Tooltip("Orthographic Size saat map besar aktif (zoom jauh/luas)")]
    public float fullMapOrthoSize = 40f;

    [Header("Key Settings")]
    public KeyCode toggleKey = KeyCode.M;

    private bool isFullMapActive = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Pastikan state awal benar
        RestoreNormalMap();
    }

    void Update()
    {
        // Hanya izinkan buka map jika di scene Hutan dan player adalah Orang Utan
        if (!IsOrangUtanInHutan())
        {
            if (isFullMapActive) RestoreNormalMap();
            return;
        }

        // Deteksi tombol M
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMap();
        }
    }

    private bool IsOrangUtanInHutan()
    {
        if (PlayerMovement.ActivePlayerInstance == null) return false;
        if (!PlayerMovement.ActivePlayerInstance.CompareTag("Player-Orang Utan")) return false;

        string sceneName = SceneManager.GetActiveScene().name;
        return sceneName != null && sceneName.Contains("Hutan");
    }

    public void ToggleMap()
    {
        if (isFullMapActive)
        {
            RestoreNormalMap();
        }
        else
        {
            ShowFullMap();
        }
    }

    private void ShowFullMap()
    {
        isFullMapActive = true;

        if (minimapGroup != null) minimapGroup.SetActive(false);
        if (fullMapGroup != null) fullMapGroup.SetActive(true);

        if (minimapCamera != null)
        {
            minimapCamera.orthographicSize = fullMapOrthoSize;
        }

        // Kunci pergerakan player saat melihat map besar
        if (PlayerMovement.ActivePlayerInstance != null)
        {
            PlayerMovement.ActivePlayerInstance.canMove = false;
            // Hentikan sisa kecepatan player agar tidak meluncur
            Rigidbody2D rb = PlayerMovement.ActivePlayerInstance.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
    }

    private void RestoreNormalMap()
    {
        isFullMapActive = false;

        if (minimapGroup != null) minimapGroup.SetActive(true);
        if (fullMapGroup != null) fullMapGroup.SetActive(false);

        if (minimapCamera != null)
        {
            minimapCamera.orthographicSize = normalOrthoSize;
        }

        // Kembalikan pergerakan player
        if (PlayerMovement.ActivePlayerInstance != null)
        {
            PlayerMovement.ActivePlayerInstance.canMove = true;
        }
    }

    void OnDisable()
    {
        // Kembalikan pergerakan player jika skrip atau objek dinonaktifkan
        if (isFullMapActive && PlayerMovement.ActivePlayerInstance != null)
        {
            PlayerMovement.ActivePlayerInstance.canMove = true;
        }
    }
}
