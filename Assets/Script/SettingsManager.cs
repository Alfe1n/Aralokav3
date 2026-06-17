using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Panels")]
    public GameObject optionsPanel;

    public GameObject mainPanel;
    public GameObject soundsPanel;
    public GameObject controlsPanel;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // force hide pas awal
        optionsPanel.SetActive(false);

        mainPanel.SetActive(true);

        soundsPanel.SetActive(false);
        controlsPanel.SetActive(false);

        // IMPORTANT
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {

            ToggleOptions();
        }
    }

    public void ToggleOptions()
    {
        bool isActive =
            !optionsPanel.activeSelf;

        optionsPanel.SetActive(isActive);

        Time.timeScale =
            isActive ? 0f : 1f;

        // kalau buka setting
        if (isActive)
        {
            // reset panel
            mainPanel.SetActive(true);

            soundsPanel.SetActive(false);
            controlsPanel.SetActive(false);
        }
    }

    public void OpenSounds()
    {
        mainPanel.SetActive(false);

        soundsPanel.SetActive(true);
        controlsPanel.SetActive(false);
    }

    public void OpenControls()
    {
        mainPanel.SetActive(false);

        controlsPanel.SetActive(true);
        soundsPanel.SetActive(false);
    }

    public void ResumeGame()
    {
        // tutup semua sub panel
        soundsPanel.SetActive(false);
        controlsPanel.SetActive(false);

        // balik ke main
        mainPanel.SetActive(true);

        // hide setting
        optionsPanel.SetActive(false);

        // resume game
        Time.timeScale = 1f;
    }

    public void ToggleSound()
    {
        AudioListener.volume =
            AudioListener.volume > 0 ? 0 : 1;
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ExitToMainMenu()
    {
        // reset pause
        Time.timeScale = 1f;

        // Hancurkan objek manager yang persisten
        QuestManager[] questManagers = Object.FindObjectsByType<QuestManager>(FindObjectsSortMode.None);
        foreach (var q in questManagers) Destroy(q.gameObject);

        TransitionManager[] transitionManagers = Object.FindObjectsByType<TransitionManager>(FindObjectsSortMode.None);
        foreach (var t in transitionManagers) Destroy(t.gameObject);

        RescueManager[] rescueManagers = Object.FindObjectsByType<RescueManager>(FindObjectsSortMode.None);
        foreach (var r in rescueManagers) Destroy(r.gameObject);

        // Hancurkan player yang persisten
        PlayerMovement[] players = Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        foreach (var p in players) Destroy(p.gameObject);

        // Hancurkan DontDestroyOnLoad container jika ada
        GameObject dontDestroyObj = GameObject.Find("DontDestroyOnLoad");
        if (dontDestroyObj != null) Destroy(dontDestroyObj);

        // Unload Core Scene secara additive agar bisa dimuat ulang dengan fresh
        Scene coreScene = SceneManager.GetSceneByName("Core Scene");
        if (coreScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(coreScene);
        }

        // tutup setting
        optionsPanel.SetActive(false);

        // balik ke main panel
        mainPanel.SetActive(true);

        soundsPanel.SetActive(false);
        controlsPanel.SetActive(false);

        // load menu
        SceneManager.LoadScene("Boot Scene");
    }

    public void OpenMainPanel()
    {
        mainPanel.SetActive(true);

        soundsPanel.SetActive(false);
        controlsPanel.SetActive(false);
    }

    public void SoundOn()
    {
        AudioListener.volume = 1f;
    }

    public void SoundOff()
    {
        AudioListener.volume = 0f;
    }

}

