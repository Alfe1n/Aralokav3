using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Mengaktifkan GUI Orang Utan HANYA saat di scene Hutan.
/// Otomatis hide saat cutscene/event (via ForceHide()) dan restore sesudahnya (via ForceRefresh()).
/// Script ini harus ada di GameObject yang SELALU AKTIF di Core Scene.
/// </summary>
public class OrangUtanUIVisibility : MonoBehaviour
{
    public static OrangUtanUIVisibility Instance { get; private set; }

    [Header("GUI Root to Show/Hide")]
    [Tooltip("Drag GUI_OrangUtan GameObject ke sini (opsional, jika kosong akan dicari otomatis)")]
    public GameObject guiOrangUtan;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Auto-find jika tidak di-assign di inspector
        if (guiOrangUtan == null)
            guiOrangUtan = GameObject.Find("GUI_OrangUtan");
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
        Refresh();
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        // Re-find jika null setelah Core Scene reload
        if (guiOrangUtan == null)
            guiOrangUtan = GameObject.Find("GUI_OrangUtan");

        Refresh();
    }

    /// <summary>
    /// Panggil ini saat cutscene/event/video dimulai agar GUI OrangUtan hilang.
    /// </summary>
    public void ForceHide()
    {
        if (guiOrangUtan != null)
            guiOrangUtan.SetActive(false);
    }

    /// <summary>
    /// Panggil ini saat cutscene/event/video selesai untuk restore visibilitas sesuai scene.
    /// </summary>
    public void ForceRefresh()
    {
        if (guiOrangUtan == null)
            guiOrangUtan = GameObject.Find("GUI_OrangUtan");
        Refresh();
    }

    private void Refresh()
    {
        if (guiOrangUtan == null) return;

        string sceneName = SceneManager.GetActiveScene().name;
        bool isHutan = sceneName.Contains("Hutan");
        guiOrangUtan.SetActive(isHutan);
    }
}
