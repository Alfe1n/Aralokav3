using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeUI : MonoBehaviour
{
    public static FadeUI instance;

    [Header("UI References")]
    public Image fadeImage;

    [Header("Settings")]
    public float fadeSpeed = 2f;

    private bool isInitialized = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        if (fadeImage == null)
        {
            fadeImage = GetComponent<Image>();
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        // Hanya lakukan auto-hide fader jika fader belum sengaja diaktifkan oleh transisi di awal scene
        if (!isInitialized)
        {
            SetTransparentInstant();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Abaikan scene non-gameplay
        if (scene.name == "Core Scene" || 
            scene.name == "LoadingScene" || 
            scene.name == "OpeningScene" || 
            scene.name == "MainMenu")
        {
            return;
        }

        // Jika transisi sedang berjalan diatur oleh TransitionManager,
        // biarkan TransitionManager yang memicu FadeIn secara manual dan eksplisit
        if (TransitionManager.Instance != null && TransitionManager.Instance.isTransitioning)
        {
            return;
        }

        // Jika scene baru adalah Void, kita hilangkan fader instan agar video cutscene langsung terlihat
        if (scene.name == "Void")
        {
            SetTransparentInstant();
            return;
        }

        // Otomatis jalankan fade-in saat scene gameplay baru selesai dimuat!
        StartCoroutine(FadeIn());
    }

    /// <summary>
    /// Instantly turns the fader overlay solid black (no fade duration).
    /// </summary>
    public void SetBlackInstant()
    {
        isInitialized = true;
        gameObject.SetActive(true);
        if (fadeImage != null)
        {
            fadeImage.enabled = true;
            fadeImage.color = Color.black;
        }
    }

    /// <summary>
    /// Instantly turns the fader overlay completely transparent and deactivates the GameObject.
    /// </summary>
    public void SetTransparentInstant()
    {
        isInitialized = true;
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
            fadeImage.enabled = false;
        }
        gameObject.SetActive(false);
    }

    // ── Static helpers — pakai dari script manapun tanpa cari instance manual ──
    public static IEnumerator Out()
    {
        if (instance != null) yield return instance.FadeOut();
    }

    public static IEnumerator In()
    {
        if (instance != null) yield return instance.FadeIn();
    }

    public static void BlackInstant()
    {
        if (instance != null) instance.SetBlackInstant();
    }

    public static void ClearInstant()
    {
        if (instance != null) instance.SetTransparentInstant();
    }
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Fades the screen overlay from transparent (or current alpha) to solid black.
    /// </summary>
    public IEnumerator FadeOut()
    {
        isInitialized = true;
        gameObject.SetActive(true);
        if (fadeImage != null) fadeImage.enabled = true;

        float alpha = fadeImage != null ? fadeImage.color.a : 0f;

        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            alpha = Mathf.Clamp01(alpha);
            if (fadeImage != null)
            {
                fadeImage.color = new Color(0, 0, 0, alpha);
            }
            yield return null;
        }

        if (fadeImage != null)
        {
            fadeImage.color = Color.black;
        }
    }

    /// <summary>
    /// Fades the screen overlay from solid black (or current alpha) to transparent.
    /// </summary>
    public IEnumerator FadeIn()
    {
        isInitialized = true;
        gameObject.SetActive(true);
        if (fadeImage != null) fadeImage.enabled = true;

        float alpha = fadeImage != null ? fadeImage.color.a : 1f;

        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            alpha = Mathf.Clamp01(alpha);
            if (fadeImage != null)
            {
                fadeImage.color = new Color(0, 0, 0, alpha);
            }
            yield return null;
        }

        SetTransparentInstant();
    }
}
