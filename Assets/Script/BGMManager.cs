using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    public static BGMManager instance;

    [System.Serializable]
    public class SceneBGM
    {
        public string sceneName;
        public AudioClip clip;
    }

    [Header("BGM per Scene")]
    public SceneBGM[] sceneBGMs;

    [Header("Default BGM (jika scene tidak terdaftar)")]
    public AudioClip defaultClip;

    [Header("Settings")]
    public float fadeSpeed = 1f;

    private AudioSource audioSource;
    private AudioClip currentClip;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Core Scene" || scene.name == "LoadingScene" ||
            scene.name == "OpeningScene" || scene.name == "MainMenu") return;

        AudioClip clip = GetClipForScene(scene.name);
        if (clip == null) return;

        if (clip == currentClip && audioSource.isPlaying) return; // BGM sama, biarkan lanjut

        currentClip = clip;
        audioSource.clip = clip;
        audioSource.Play();
    }

    AudioClip GetClipForScene(string sceneName)
    {
        foreach (var entry in sceneBGMs)
        {
            if (entry.sceneName == sceneName)
                return entry.clip;
        }

        // Cek apakah scene mengandung kata "Hutan"
        if (sceneName.Contains("Hutan") && defaultClip != null)
            return defaultClip;

        return null;
    }

    public void StopBGM() => audioSource.Stop();
    public void PlayBGM() { if (currentClip != null) audioSource.Play(); }
}
