using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    private static GameOverManager _instance;
    public static GameOverManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // 1. Cari GameOverManager yang aktif
                _instance = Object.FindFirstObjectByType<GameOverManager>();

                // 2. Jika tidak ada yang aktif, cari GameOverManager yang non-aktif (karena panel GameOver biasanya non-aktif di awal)
                if (_instance == null)
                {
                    GameOverManager[] managers = Resources.FindObjectsOfTypeAll<GameOverManager>();
                    foreach (var m in managers)
                    {
                        // Pastikan object ada di scene dan bukan asset prefab di Project window
                        if (m.gameObject.scene.name != null && !string.IsNullOrEmpty(m.gameObject.scene.name))
                        {
                            _instance = m;
                            Debug.Log($"[GameOverManager] Menemukan instance non-aktif '{_instance.gameObject.name}' di scene '{m.gameObject.scene.name}'.");
                            break;
                        }
                    }
                }

                // 3. Jika komponen GameOverManager belum dipasang sama sekali di editor, cari GameObject bernama "GameOverPanel"
                if (_instance == null)
                {
                    GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                    foreach (var go in allObjects)
                    {
                        if (go.name == "GameOverPanel" && go.scene.name != null && !string.IsNullOrEmpty(go.scene.name))
                        {
                            _instance = go.AddComponent<GameOverManager>();
                            Debug.Log("[GameOverManager] Menempelkan script GameOverManager secara dinamis pada GameObject 'GameOverPanel' di scene.");
                            break;
                        }
                    }
                }

                if (_instance == null)
                {
                    Debug.LogWarning("[GameOverManager] Gagal menemukan GameOverManager atau GameOverPanel di scene manapun!");
                }
            }
            return _instance;
        }
    }

    [Header("Panels")]
    [Tooltip("Panel utama Game Over (Root)")]
    public GameObject gameOverPanel;
    [Tooltip("Main Panel (Child)")]
    public GameObject mainPanel;

    [Header("UI Buttons")]
    [Tooltip("Tombol untuk Ulang / Retry")]
    public Button retryButton;
    [Tooltip("Tombol untuk kembali ke Main Menu")]
    public Button mainMenuButton;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Cari panel & tombol secara otomatis jika tidak dipasang di Inspector
        FindPanels();
        FindButtons();

        // Pastikan panel mati saat game dimulai
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        if (mainPanel != null)
        {
            mainPanel.SetActive(true);
        }
    }

    private void FindPanels()
    {
        if (gameOverPanel == null)
        {
            if (gameObject.name == "GameOverPanel")
            {
                gameOverPanel = gameObject;
            }
            else
            {
                // Cari transform bernama "GameOverPanel" di scene secara global (termasuk non-aktif)
                GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (var go in allObjects)
                {
                    if (go.name == "GameOverPanel" && go.scene.name != null && !string.IsNullOrEmpty(go.scene.name))
                    {
                        gameOverPanel = go;
                        break;
                    }
                }
            }
        }

        if (mainPanel == null && gameOverPanel != null)
        {
            Transform t = gameOverPanel.transform.Find("MainPanel");
            if (t != null)
            {
                mainPanel = t.gameObject;
            }
        }
    }

    private void FindButtons()
    {
        // Cari tombol di dalam mainPanel / gameOverPanel agar akurat meskipun script diletakkan di luar
        GameObject searchRoot = mainPanel != null ? mainPanel : (gameOverPanel != null ? gameOverPanel : gameObject);

        if (retryButton == null)
        {
            // Cari tombol Ulang/Retry di dalam child searchRoot
            Button[] buttons = searchRoot.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                if (b.name.Contains("Ulang") || b.name.Contains("Retry"))
                {
                    retryButton = b;
                    break;
                }
            }
        }

        if (mainMenuButton == null)
        {
            Button[] buttons = searchRoot.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                if (b.name.Contains("MainMenu") || b.name.Contains("Menu"))
                {
                    mainMenuButton = b;
                    break;
                }
            }
        }

        // Daftarkan listener klik
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(OnRetryClicked);
        }
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
    }

    public void ShowGameOver()
    {
        // Pemicuan FindPanels di sini menjamin panel ter-bind sebelum diaktifkan,
        // meskipun Awake() dari GameObject non-aktif belum dijalankan!
        FindPanels();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        if (mainPanel != null)
        {
            mainPanel.SetActive(true);
        }

        // Matikan OptionsPanel jika ada di root canvas agar tidak bentrok
        Canvas parentCanvas = GetComponentInParent<Canvas>(true);
        if (parentCanvas == null)
        {
            // Cari Canvas secara global jika script dipasang di luar hirarki Canvas
            GameObject canvasObj = GameObject.Find("Canvas");
            if (canvasObj != null)
            {
                parentCanvas = canvasObj.GetComponent<Canvas>();
            }
        }

        if (parentCanvas != null)
        {
            Transform optionsPanelTrans = parentCanvas.transform.Find("OptionsPanel");
            if (optionsPanelTrans != null)
            {
                optionsPanelTrans.gameObject.SetActive(false);
                Debug.Log("[GameOverManager] Menonaktifkan OptionsPanel dari Canvas parent.");
            }
        }

        FindButtons(); // Pastikan tombol terikat dengan benar saat panel muncul
        
        // Pause permainan agar tidak ada pergerakan musuh di background
        Time.timeScale = 0f;
    }

    private void OnRetryClicked()
    {
        Time.timeScale = 1f;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        StartCoroutine(ResetAndRespawnRoutine());
    }

    private void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Bersihkan objek persisten DontDestroyOnLoad agar tidak bertumpuk/duplikat
        CleanPersistentObjects();

        // Pindah ke MainMenu
        SceneManager.LoadScene("Boot Scene");
    }

    private void CleanPersistentObjects()
    {
        // Hancurkan objek manager yang persisten
        QuestManager[] questManagers = Object.FindObjectsByType<QuestManager>(FindObjectsSortMode.None);
        foreach (var q in questManagers) Destroy(q.gameObject);

        TransitionManager[] transitionManagers = Object.FindObjectsByType<TransitionManager>(FindObjectsSortMode.None);
        foreach (var t in transitionManagers) Destroy(t.gameObject);

        // Hancurkan RescueManager yang persisten
        RescueManager[] rescueManagers = Object.FindObjectsByType<RescueManager>(FindObjectsSortMode.None);
        foreach (var r in rescueManagers) Destroy(r.gameObject);

        // Hancurkan player yang persisten
        PlayerMovement[] players = Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        foreach (var p in players) Destroy(p.gameObject);

        // Hancurkan Canvas dari Core Scene agar tidak bertumpuk
        Canvas parentCanvas = GetComponentInParent<Canvas>(true);
        if (parentCanvas != null)
        {
            Destroy(parentCanvas.gameObject);
        }
        else
        {
            GameObject coreCanvas = GameObject.Find("Canvas");
            if (coreCanvas != null) Destroy(coreCanvas);
        }

        GameObject coreCamera = GameObject.Find("Camera");
        if (coreCamera != null) Destroy(coreCamera);

        // Hancurkan DontDestroyOnLoad container jika ada
        GameObject dontDestroyObj = GameObject.Find("DontDestroyOnLoad");
        if (dontDestroyObj != null) Destroy(dontDestroyObj);
    }

    private IEnumerator ResetAndRespawnRoutine()
    {
        // Sembunyikan UI Quest sementara agar bersih saat reload
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.HideObjective();
        }

        // Cari scene gameplay aktif saat ini (bukan Core Scene)
        string activeGameplayScene = "";
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            string sceneName = SceneManager.GetSceneAt(i).name;
            if (sceneName != "Core Scene" && sceneName != "LoadingScene" && sceneName != "MainMenu" && sceneName != "Boot Scene")
            {
                activeGameplayScene = sceneName;
                break;
            }
        }

        if (!string.IsNullOrEmpty(activeGameplayScene))
        {
            Debug.Log($"[GameOverManager] Mereset scene '{activeGameplayScene}' dari awal...");

            // Unload scene gameplay saat ini
            yield return SceneManager.UnloadSceneAsync(activeGameplayScene);

            // Muat ulang scene tersebut secara additive
            yield return SceneManager.LoadSceneAsync(activeGameplayScene, LoadSceneMode.Additive);

            // Set scene baru sebagai active scene
            Scene gameplayScene = SceneManager.GetSceneByName(activeGameplayScene);
            if (gameplayScene.IsValid())
            {
                SceneManager.SetActiveScene(gameplayScene);
            }
        }

        // Tunggu 1 frame agar semua objek ter-instantiate dengan sempurna
        yield return null;

        // Cari player di scene (termasuk yang non-aktif agar bisa di-switch)
        PlayerMovement[] players = Resources.FindObjectsOfTypeAll<PlayerMovement>();
        PlayerMovement activePlayer = null;

        foreach (var p in players)
        {
            if (string.IsNullOrEmpty(p.gameObject.scene.name)) continue;

            bool useOrangUtan = activeGameplayScene.Contains("Hutan");
            if (p.CompareTag("Player-Orang Utan"))
            {
                p.gameObject.SetActive(useOrangUtan);
                if (useOrangUtan) activePlayer = p;
            }
            else if (p.CompareTag("Player"))
            {
                p.gameObject.SetActive(!useOrangUtan);
                if (!useOrangUtan) activePlayer = p;
            }
        }

        // Hidupkan kembali visual player, isi HP, dan teleportasikan ke Spawn_Utama
        if (activePlayer != null)
        {
            Health playerHealth = activePlayer.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.currentHealth = playerHealth.maxHealth;
            }

            SpriteRenderer sr = activePlayer.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = true;

            activePlayer.canMove = true;

            // Teleportasikan player
            GameObject spawn = GameObject.Find("Spawn_Utama");
            if (spawn != null)
            {
                activePlayer.transform.position = spawn.transform.position;
                Debug.Log($"[GameOverManager] Player berhasil diteleportasikan kembali ke Spawn_Utama.");
            }

            // Targetkan kembali kamera Cinemachine ke player yang aktif
            SetupCameraTarget(activePlayer.transform);
        }

        // Tampilkan kembali UI Quest & reset ke checkpoint quest
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ResetToCheckpointQuest();
        }
    }

    private void SetupCameraTarget(Transform target)
    {
        MonoBehaviour[] activeScripts = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var script in activeScripts)
        {
            if (script == null) continue;
            string scriptName = script.GetType().Name;
            if (scriptName == "CinemachineVirtualCamera" || scriptName == "CinemachineCamera")
            {
                var followProp = script.GetType().GetProperty("Follow");
                if (followProp != null)
                {
                    followProp.SetValue(script, target);
                }

                var lookAtProp = script.GetType().GetProperty("LookAt");
                if (lookAtProp != null)
                {
                    lookAtProp.SetValue(script, target);
                }
            }
        }
    }
}
