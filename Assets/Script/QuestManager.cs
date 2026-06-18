using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    /// <summary>
    /// Dipanggil setiap kali quest berubah. Digunakan oleh QuestObjectToggle
    /// agar tidak perlu polling di Update() setiap frame.
    /// </summary>
    public static event System.Action<int> OnQuestChanged;

    [Header("UI")]
    public TMP_Text objectiveText;
    public GameObject objectivePanel;

    [Header("Quest List")]
    public List<QuestData> quests = new();

    [SerializeField]
    private int currentQuest = 0;

    private int checkpointQuest = 0;

    public int CurrentQuest => currentQuest;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Setelah Core Scene di-reload, cari ulang referensi UI yang mungkin menjadi null.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Core Scene") return;

        if (objectivePanel == null)
        {
            GameObject questUI = GameObject.Find("QuestUI");
            if (questUI != null) objectivePanel = questUI;
        }
        if (objectiveText == null)
        {
            var found = GameObject.Find("ObjectiveText");
            if (found != null) objectiveText = found.GetComponent<TMPro.TMP_Text>();
        }
        Debug.Log($"[QuestManager] OnSceneLoaded Core Scene: objectivePanel={objectivePanel}, objectiveText={objectiveText}");
    }

    private void Start()
    {
        currentQuest = PlayerPrefs.GetInt("CurrentQuest", 0);
        checkpointQuest = PlayerPrefs.GetInt("CheckpointQuest", currentQuest);
        Debug.Log($"[QuestManager] Loaded quest: {currentQuest}, checkpoint quest: {checkpointQuest}");

        HideObjective();
        UpdateObjective();

        string activeScene = SceneManager.GetActiveScene().name;
        bool isMenuOrSystemScene = activeScene == "MainMenu"
            || activeScene == "Boot Scene"
            || activeScene == "LoadingScene"
            || activeScene == "OpeningScene";

        if (!isMenuOrSystemScene)
            ShowObjective();

        // Notify subscribers agar QuestObjectToggle bisa langsung update
        OnQuestChanged?.Invoke(currentQuest);
    }

    // =============================================
    // SAVE / LOAD SCENE DATA
    // =============================================

    /// <summary>
    /// Menyimpan scene terakhir yang dikunjungi untuk fitur Continue.
    /// Dipanggil setiap kali checkpoint quest disimpan (saat transisi scene).
    /// </summary>
    public void SaveLastScene(string sceneName, string spawnName)
    {
        PlayerPrefs.SetString("LastScene", sceneName);
        PlayerPrefs.SetString("LastSpawn", spawnName);
        PlayerPrefs.SetInt("HasSaveData", 1);
        PlayerPrefs.Save();
        Debug.Log($"[QuestManager] Last scene saved: {sceneName} @ {spawnName}");
    }

    /// <summary>
    /// Apakah ada data simpan untuk fitur Continue.
    /// </summary>
    public static bool HasSaveData()
    {
        return PlayerPrefs.GetInt("HasSaveData", 0) == 1;
    }

    /// <summary>
    /// Nama scene terakhir yang disimpan (default: Kamar Bara).
    /// </summary>
    public static string GetLastScene()
    {
        return PlayerPrefs.GetString("LastScene", "Kamar Bara");
    }

    /// <summary>
    /// Nama spawn point terakhir yang disimpan.
    /// </summary>
    public static string GetLastSpawn()
    {
        return PlayerPrefs.GetString("LastSpawn", "");
    }

    /// <summary>
    /// Reset semua data quest (quest index dan scene simpan) ke awal.
    /// Dipanggil saat New Game dimulai.
    /// </summary>
    public void ResetAllData()
    {
        currentQuest = 0;
        checkpointQuest = 0;
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        if (Inventory.instance != null)
        {
            Inventory.instance.Clear();
        }

        UpdateObjective();
        HideObjective();

        Debug.Log("[QuestManager] All save data reset for New Game.");
    }

    // =============================================
    // CHECKPOINT QUEST
    // =============================================

    public void SaveCheckpointQuest()
    {
        checkpointQuest = currentQuest;
        PlayerPrefs.SetInt("CheckpointQuest", checkpointQuest);
        PlayerPrefs.Save();
        Debug.Log($"[QuestManager] Checkpoint quest saved: {checkpointQuest}");
    }

    public void ResetToCheckpointQuest()
    {
        SetQuest(checkpointQuest);
        Debug.Log($"[QuestManager] Reset quest to checkpoint: {currentQuest}");
    }

    // =============================================
    // OBJECTIVE UI
    // =============================================

    public void ShowObjective()
    {
        Debug.Log("SHOW QUEST UI");

        if (objectivePanel == null)
        {
            Debug.LogError("OBJECTIVE PANEL NULL");
            return;
        }

        objectivePanel.SetActive(true);
    }

    public void HideObjective()
    {
        Debug.Log("HIDE QUEST UI");

        if (objectivePanel == null)
        {
            Debug.LogError("OBJECTIVE PANEL NULL");
            return;
        }

        objectivePanel.SetActive(false);
    }

    public void SetQuest(int id)
    {
        if (id < 0 || id >= quests.Count)
            return;

        currentQuest = id;
        PlayerPrefs.SetInt("CurrentQuest", currentQuest);
        PlayerPrefs.Save();

        Debug.Log($"QUEST CHANGED -> {currentQuest}");

        // Auto-save scene & quest setiap kali quest berganti di dalam gameplay scene
        string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (activeScene != "MainMenu" && activeScene != "Boot Scene" && activeScene != "LoadingScene" && activeScene != "OpeningScene")
        {
            // Pertahankan spawn terakhir yang tersimpan — jangan hapus posisi saat transisi
            string lastSpawn = PlayerPrefs.GetString("LastSpawn", "");
            SaveLastScene(activeScene, lastSpawn);
            SaveCheckpointQuest();
        }

        // Notify subscribers (QuestObjectToggle, dll)
        OnQuestChanged?.Invoke(currentQuest);

        UpdateObjective();
        ShowObjective();
    }

    public void NextQuest()
    {
        SetQuest(currentQuest + 1);
    }

    private void UpdateObjective()
    {
        if (objectiveText == null)
            return;

        if (quests.Count == 0)
            return;

        if (currentQuest >= quests.Count)
            return;

        Debug.Log("Quest Update: " + currentQuest);

        objectiveText.text = quests[currentQuest].objectiveText;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            if (Inventory.instance != null)
            {
                Inventory.instance.Clear();
            }
            Debug.Log("[Araloka Debug] PlayerPrefs and Inventory cleared via debug hotkey!");
            currentQuest = 0;
            checkpointQuest = 0;
            string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            UnityEngine.SceneManagement.SceneManager.LoadScene(activeScene);
        }
    }
#endif
}