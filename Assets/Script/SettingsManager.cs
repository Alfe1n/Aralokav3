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
            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            bool isGameplay = scene != "MainMenu" && scene != "MainMenu2" &&
                              scene != "OpeningScene" && scene != "LoadingScene" &&
                              scene != "Boot Scene" && scene != "Core Scene";
            if (isGameplay)
                ToggleOptions();
        }
    }

    public void ToggleOptions()
    {
        bool isActive = !optionsPanel.activeSelf;

        optionsPanel.SetActive(isActive);
        Time.timeScale = isActive ? 0f : 1f;

        // Tampilkan cursor saat pause, sembunyikan saat resume
        // Tidak pakai Locked — game 2D tidak perlu lock cursor
        Cursor.visible = isActive;
        Cursor.lockState = CursorLockMode.None;

        if (isActive)
        {
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
        soundsPanel.SetActive(false);
        controlsPanel.SetActive(false);
        mainPanel.SetActive(true);
        optionsPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
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
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;

        // Sembunyikan objective panel sebelum QuestManager di-destroy
        if (QuestManager.Instance != null)
            QuestManager.Instance.HideObjective();

        QuestManager[] questManagers = Object.FindObjectsByType<QuestManager>(FindObjectsSortMode.None);
        foreach (var q in questManagers) Destroy(q.gameObject);

        TransitionManager[] transitionManagers = Object.FindObjectsByType<TransitionManager>(FindObjectsSortMode.None);
        foreach (var t in transitionManagers) Destroy(t.gameObject);

        PlayerMovement[] players = Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        foreach (var p in players) Destroy(p.gameObject);

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

