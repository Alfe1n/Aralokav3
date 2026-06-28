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

    [Header("Quest Tracking Trail")]
    public float trailSpawnInterval = 1.5f;
    private float trailSpawnTimer = 0f;
    private bool isTrackingActive = false;
    private string customObjectiveText = "";

    public bool IsTrackingActive => isTrackingActive;

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
        if (scene.name != "Core Scene")
        {
            // Jika masuk ke scene gameplay baru, cari ulang target quest di scene tersebut
            RefreshQuestTarget();
            return;
        }

        FindUIElements();
        Debug.Log($"[QuestManager] OnSceneLoaded Core Scene: objectivePanel={objectivePanel}, objectiveText={objectiveText}");
    }

    private GameObject pauseButton;

    private void FindUIElements()
    {
        // Cari di semua object dalam scene, termasuk yang inactive (karena QuestUI di Core Scene bisa dimulai dari keadaan mati)
        if (objectivePanel == null)
        {
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject go in allObjects)
            {
                if (go.name == "QuestUI" && !string.IsNullOrEmpty(go.scene.name))
                {
                    objectivePanel = go;
                    break;
                }
            }
        }

        if (objectiveText == null && objectivePanel != null)
        {
            objectiveText = objectivePanel.GetComponentInChildren<TMP_Text>(true);
        }

        if (pauseButton == null)
        {
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject go in allObjects)
            {
                if (go.name == "PauseButton" && !string.IsNullOrEmpty(go.scene.name))
                {
                    pauseButton = go;
                    break;
                }
            }
        }
    }

    private void Start()
    {
        currentQuest = PlayerPrefs.GetInt("CurrentQuest", 0);
        checkpointQuest = PlayerPrefs.GetInt("CheckpointQuest", currentQuest);
        Debug.Log($"[QuestManager] Loaded quest: {currentQuest}, checkpoint quest: {checkpointQuest}");

        FindUIElements();
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
        RefreshQuestTarget();
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

        if (objectivePanel == null || pauseButton == null)
        {
            FindUIElements();
        }

        if (objectivePanel != null)
        {
            objectivePanel.SetActive(true);
        }

        if (pauseButton != null)
        {
            pauseButton.SetActive(true);
        }
    }

    public void HideObjective()
    {
        Debug.Log("HIDE QUEST UI");

        if (objectivePanel == null || pauseButton == null)
        {
            FindUIElements();
        }

        if (objectivePanel != null)
        {
            objectivePanel.SetActive(false);
        }

        if (pauseButton != null)
        {
            pauseButton.SetActive(false);
        }
    }

    public void SetQuest(int id)
    {
        if (id < 0 || id >= quests.Count)
            return;

        currentQuest = id;
        customObjectiveText = ""; // Reset custom text saat quest berganti ke index baru
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
        RefreshQuestTarget();
    }

    public void NextQuest()
    {
        SetQuest(currentQuest + 1);
    }

    /// <summary>
    /// Mengubah teks objektif secara dinamis (misal saat keputusan ending).
    /// </summary>
    public void SetCustomObjectiveText(string text)
    {
        customObjectiveText = text;
        if (objectiveText != null)
        {
            objectiveText.text = text;
            ShowObjective();
        }
        RefreshQuestTarget();
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

        string baseText = quests[currentQuest].objectiveText;
        // Tampilkan petunjuk track (V) di bagian atas agar tidak terpotong di kayu box UI
        string trackingHint = isTrackingActive 
            ? "<size=75%><color=#FFA500>[V] Untrack Quest</color></size>\n" 
            : "<size=75%><color=#A0A0A0>[V] Track Quest</color></size>\n";

        objectiveText.text = trackingHint + baseText;
    }

    private void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
            return;
        }
#endif

        if (PlayerMovement.ActivePlayerInstance != null)
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                isTrackingActive = !isTrackingActive;
                UpdateObjective();
            }

            if (isTrackingActive)
            {
                trailSpawnTimer += Time.deltaTime;
                if (trailSpawnTimer >= trailSpawnInterval)
                {
                    trailSpawnTimer = 0f;
                    SpawnGuideTrail();
                }
            }
        }
        else
        {
            isTrackingActive = false;
        }
    }

    [Header("Dynamic Tracking Cache")]
    private Transform activeQuestTarget;
    public Transform ActiveQuestTarget => activeQuestTarget;

    public void RefreshQuestTarget()
    {
        activeQuestTarget = FindQuestTargetForCurrentQuest();
        Debug.Log($"[QuestTracker] Active tracking target refreshed: {(activeQuestTarget != null ? activeQuestTarget.name : "None")}");
    }

    private Transform FindQuestTargetForCurrentQuest()
    {
        // Temukan semua GameObject aktif sekali saja di awal untuk optimasi performa dan kemudahan akses
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        // 1. Cari target yang dipasang secara manual terlebih dahulu
        QuestTargetMarker[] markers = Object.FindObjectsByType<QuestTargetMarker>(FindObjectsSortMode.None);
        foreach (var marker in markers)
        {
            if (marker.gameObject.activeInHierarchy && currentQuest >= marker.minQuest && currentQuest <= marker.maxQuest)
            {
                return marker.transform;
            }
        }

        // Jika sedang di Hutan4 mencari Gajah (Quest 16), sebelum masuk maze, arahkan ke pintu masuk maze (MazeEnter)
        string activeSceneName = SceneManager.GetActiveScene().name;
        if (activeSceneName.Contains("Hutan4") && currentQuest == 16)
        {
            if (MazeDarkness.instance == null || !MazeDarkness.instance.isInMaze)
            {
                GameObject mazeEnter = GameObject.Find("MazeEnter");
                if (mazeEnter != null && mazeEnter.activeInHierarchy)
                {
                    return mazeEnter.transform;
                }
            }
        }

        // Koreksi khusus: Jika sedang di Hutan5 pada Quest 18 (Masuki Hutan bagian Utara), arahkan langsung ke Desicion Trigger
        if (activeSceneName.Contains("Hutan5") && currentQuest == 18)
        {
            GameObject decisionTrigger = GameObject.Find("Desicion Trigger");
            if (decisionTrigger == null)
            {
                decisionTrigger = GameObject.Find("Decision Trigger");
            }
            if (decisionTrigger != null && decisionTrigger.activeInHierarchy)
            {
                Debug.Log("[QuestTracker] Guide to Desicion Trigger in Hutan5");
                return decisionTrigger.transform;
            }
        }

        // 2. Cari EventAreaTrigger yang memerlukan quest aktif saat ini secara spesifik
        EventAreaTrigger[] eventTriggers = Object.FindObjectsByType<EventAreaTrigger>(FindObjectsSortMode.None);
        foreach (var trigger in eventTriggers)
        {
            if (trigger.gameObject.activeInHierarchy && trigger.useQuest && trigger.requiredQuest == currentQuest)
            {
                Debug.Log($"[QuestTracker] Auto-matched EventAreaTrigger: '{trigger.gameObject.name}' for Quest ID {currentQuest}");
                return trigger.transform;
            }
        }

        // 3. Cari InteractableObject yang memerlukan quest aktif saat ini secara spesifik (Contoh: Kudanil)
        InteractableObject[] interactables = Object.FindObjectsByType<InteractableObject>(FindObjectsSortMode.None);
        foreach (var io in interactables)
        {
            if (io.gameObject.activeInHierarchy && io.useQuest && io.requiredQuest == currentQuest)
            {
                Debug.Log($"[QuestTracker] Auto-matched InteractableObject: '{io.gameObject.name}' for Quest ID {currentQuest}");
                return io.transform;
            }
        }

        // 4. Cari ArenaManager yang memerlukan quest aktif saat ini secara spesifik
        ArenaManager[] arenaManagers = Object.FindObjectsByType<ArenaManager>(FindObjectsSortMode.None);
        foreach (var arena in arenaManagers)
        {
            if (arena.gameObject.activeInHierarchy && arena.useQuest && arena.requiredQuest == currentQuest)
            {
                Debug.Log($"[QuestTracker] Auto-matched ArenaManager: '{arena.gameObject.name}' for Quest ID {currentQuest}");
                return arena.transform;
            }
        }

        // 5. Cari semua transition/portal dan hitung skor kecocokannya secara dinamis (Hanya jika tidak ada target gameplay di atas)
        string objText = !string.IsNullOrEmpty(customObjectiveText) ? customObjectiveText : (quests.Count > 0 && currentQuest < quests.Count ? quests[currentQuest].objectiveText : "");
        string textLower = objText.ToLower();
        string activeSceneNameLower = activeSceneName.ToLower();

        // Jika kita sudah berada di Hutan Barat (Hutan2), jangan cari portal ke Barat lagi karena sudah sampai
        bool alreadyAtWest = activeSceneNameLower.Contains("hutan2") && textLower.Contains("barat");
        bool alreadyAtEast = activeSceneNameLower.Contains("hutan3") && textLower.Contains("timur");
        bool alreadyAtNorth = activeSceneNameLower.Contains("hutan4") && textLower.Contains("utara");
        bool alreadyAtSouth = activeSceneNameLower.Contains("hutan5") && (textLower.Contains("selatan") || textLower.Contains("rawa"));

        if (!alreadyAtWest && !alreadyAtEast && !alreadyAtNorth && !alreadyAtSouth)
        {
            GameObject bestTransition = null;
            float bestTransScore = -1f;

            foreach (GameObject go in allObjects)
            {
                if (!go.activeInHierarchy) continue;
                if (go.CompareTag("Player") || go.CompareTag("Player-Orang Utan") || go.name == "QuestManager" || go.name == "QuestUI")
                    continue;

                float score = 0f;

                // Cek komponen SceneTransition
                SceneTransition trans = go.GetComponent<SceneTransition>();
                if (trans != null)
                {
                    if (trans.useQuest && (trans.requiredQuest == currentQuest || trans.requiredMinQuest == currentQuest))
                        score += 15f;

                    if (!string.IsNullOrEmpty(trans.targetScene))
                    {
                        score += GetTransitionDestinationScore(trans.targetScene, go.name, textLower);
                    }
                }

                // Cek komponen DoorTransition
                DoorTransition door = go.GetComponent<DoorTransition>();
                if (door != null)
                {
                    if (!string.IsNullOrEmpty(door.targetScene))
                    {
                        score += GetTransitionDestinationScore(door.targetScene, go.name, textLower);
                    }
                }

                // Cek komponen InteractableObject dengan transition
                InteractableObject io = go.GetComponent<InteractableObject>();
                if (io != null)
                {
                    if (io.useQuest && io.requiredQuest == currentQuest)
                        score += 15f;

                    if (io.useSceneTransition && !string.IsNullOrEmpty(io.targetScene))
                    {
                        score += GetTransitionDestinationScore(io.targetScene, go.name, textLower);
                    }
                }

                if (score > bestTransScore && score > 0f)
                {
                    bestTransScore = score;
                    bestTransition = go;
                }
            }

            if (bestTransition != null && bestTransScore >= 10f)
            {
                Debug.Log($"[QuestTracker] Auto-matched transition target: '{bestTransition.name}' (Score: {bestTransScore}) for objective '{objText}'");
                return bestTransition.transform;
            }
        }

        // 6. Fallback Cerdas: Scan teks objektif untuk mencocokkan dengan nama objek di scene
        if (!string.IsNullOrEmpty(objText))
        {
            GameObject bestMatch = null;
            float bestPriority = -1f;

            foreach (GameObject go in allObjects)
            {
                if (!go.activeInHierarchy) continue;

                // Abaikan objek player diri sendiri dan sistem UI
                if (go.CompareTag("Player") || go.CompareTag("Player-Orang Utan") || go.name == "QuestManager" || go.name == "QuestUI" || go.name == "DialogueManager")
                    continue;

                float priority = GetMatchPriority(go.name, objText, go);
                if (priority > bestPriority)
                {
                    bestPriority = priority;
                    bestMatch = go;
                }
            }

            if (bestMatch != null && bestPriority > 0f)
            {
                return bestMatch.transform;
            }
        }

        return null;
    }

    private bool IsTransitionBlocked(GameObject go)
    {
        SceneTransition st = go.GetComponent<SceneTransition>();
        if (st != null && st.useQuest)
        {
            if (st.requiredQuest >= 0 && st.requiredQuest != currentQuest)
                return true;
            if (st.requiredMinQuest >= 0 && currentQuest < st.requiredMinQuest)
                return true;
        }
        InteractableObject io = go.GetComponent<InteractableObject>();
        if (io != null && io.useQuest)
        {
            if (io.requiredQuest >= 0 && io.requiredQuest != currentQuest)
                return true;
        }
        return false;
    }

    private bool IsAnyDirectPortalBlockedTo(string textLower)
    {
        string targetDest = "";
        // Koreksi khusus: Quest 18 "Masuki Hutan bagian Utara" menargetkan Hutan5 (karena requiredMinQuest Hutan5 = 18)
        if (currentQuest == 18) targetDest = "hutan5";
        else if (textLower.Contains("timur laut")) targetDest = "hutan4";
        else if (textLower.Contains("timur") || textLower.Contains("hutan3") || textLower.Contains("burung") || textLower.Contains("kakak") || textLower.Contains("tua")) targetDest = "hutan3";
        else if (textLower.Contains("barat daya")) targetDest = "hutan1";
        else if (textLower.Contains("barat") || textLower.Contains("hutan2")) targetDest = "hutan2";
        else if (textLower.Contains("utara") || textLower.Contains("hutan4")) targetDest = "hutan4";
        else if (textLower.Contains("selatan") || textLower.Contains("rawa") || textLower.Contains("hutan5")) targetDest = "hutan5";

        if (string.IsNullOrEmpty(targetDest)) return false;

        SceneTransition[] transitions = Object.FindObjectsByType<SceneTransition>(FindObjectsSortMode.None);
        bool foundDirectPortal = false;
        foreach (var trans in transitions)
        {
            if (trans.gameObject.activeInHierarchy && !string.IsNullOrEmpty(trans.targetScene) && trans.targetScene.ToLower() == targetDest)
            {
                foundDirectPortal = true;
                if (IsTransitionBlocked(trans.gameObject))
                    return true;
            }
        }

        // Jika tidak ada portal langsung ke wilayah tujuan sama sekali di scene ini,
        // berarti harus memutar/transit lewat hub Hutan1
        if (!foundDirectPortal)
            return true;

        return false;
    }

    private float GetTransitionDestinationScore(string targetScene, string goName, string textLower)
    {
        GameObject go = GameObject.Find(goName);
        if (go != null && IsTransitionBlocked(go))
        {
            return 0f;
        }

        float score = 0f;
        string targetLower = targetScene.ToLower();
        string nameLower = goName.ToLower();

        // Koreksi khusus: Quest 18 "Masuki Hutan bagian Utara" menargetkan Hutan5 (karena requiredMinQuest Hutan5 = 18)
        if (currentQuest == 18)
        {
            if (targetLower.Contains("hutan5") || targetLower == "hutan5" || nameLower.Contains("exithutan5"))
            {
                return 35f;
            }
            if (targetLower.Contains("hutan4") || targetLower == "hutan4")
            {
                return 0f; // Blokir Hutan4 yang sudah selesai
            }
        }

        // Kecocokan nama scene langsung
        if (textLower.Contains(targetLower))
            score += 25f;

        // Arah Timur Laut -> Hutan4
        if (textLower.Contains("timur laut") && (targetLower.Contains("hutan4") || targetLower == "hutan4" || nameLower.Contains("timur laut") || nameLower.Contains("northeast") || nameLower.Contains("utara") || nameLower.Contains("north")))
        {
            score += 35f;
        }
        // Arah Barat -> Hutan2
        else if (textLower.Contains("barat") && (targetLower.Contains("hutan2") || targetLower == "hutan2" || nameLower.Contains("barat") || nameLower.Contains("west")))
        {
            if (nameLower.Contains("exithutan1") || nameLower.Contains("barat") || nameLower.Contains("west"))
                score += 30f;
            else
                score += 10f;
        }
        // Arah Timur / Misi Burung Kakak Tua -> Hutan3
        else if ((textLower.Contains("timur") || textLower.Contains("burung") || textLower.Contains("kakak") || textLower.Contains("tua")) 
                 && (targetLower.Contains("hutan3") || targetLower == "hutan3" || nameLower.Contains("timur") || nameLower.Contains("east")))
        {
            score += 20f;
        }
        // Arah Utara -> Hutan4
        else if (textLower.Contains("utara") && (targetLower.Contains("hutan4") || targetLower == "hutan4" || nameLower.Contains("utara") || nameLower.Contains("north")))
        {
            score += 20f;
        }
        // Arah Selatan -> Hutan5 / Rawa
        else if ((textLower.Contains("selatan") || textLower.Contains("rawa")) && (targetLower.Contains("hutan5") || targetLower == "hutan5" || nameLower.Contains("selatan") || nameLower.Contains("south")))
        {
            score += 20f;
        }

        if (targetLower == "hutan1" && IsAnyDirectPortalBlockedTo(textLower))
        {
            score += 22f;
        }

        return score;
    }

    private float GetMatchPriority(string goName, string objectiveText, GameObject go)
    {
        string nameLower = goName.ToLower();
        string textLower = objectiveText.ToLower();

        // 1. Cek singkatan khusus (HP <=> Handphone)
        if (textLower.Contains("hp") && (nameLower.Contains("handphone") || nameLower.Contains("phone")))
        {
            return 10f; // Prioritas sangat tinggi
        }

        // 2. Cek sinonim tidur / istirahat -> Kasur / Bed
        bool isRestObjective = textLower.Contains("istirahat") || textLower.Contains("tidur") || textLower.Contains("rest") || textLower.Contains("sleep");
        bool isBedObject = nameLower.Contains("kasur") || nameLower.Contains("bed") || nameLower.Contains("tidur");
        if (isRestObjective && isBedObject)
        {
            return 9.5f; // Prioritas sangat tinggi
        }

        // 3. Cek sinonim mandi / wastafel / bak
        bool isWashObjective = textLower.Contains("mandi") || textLower.Contains("cuci") || textLower.Contains("wash") || textLower.Contains("bath") || textLower.Contains("bersih");
        bool isWaterObject = nameLower.Contains("wastafel") || nameLower.Contains("bak") || nameLower.Contains("toilet") || nameLower.Contains("shower");
        if (isWashObjective && isWaterObject)
        {
            return 9.5f;
        }

        // 4. Cek gerbang keluar / pintu transisi jika objektif bertema pergi/keluar
        bool isExitObjective = textLower.Contains("keluar") || textLower.Contains("pergi") || textLower.Contains("jalan") || textLower.Contains("hutan");
        
        bool isTransitionObject = false;
        InteractableObject io = go.GetComponent<InteractableObject>();
        if (io != null && io.useSceneTransition)
        {
            isTransitionObject = true;
        }
        else if (go.GetComponent("DoorTransition") != null || 
                 go.GetComponent("SceneTransition") != null || 
                 nameLower.Contains("exit") || 
                 nameLower.Contains("door") || 
                 nameLower.Contains("portal"))
        {
            isTransitionObject = true;
        }
        
        if (isExitObjective && isTransitionObject)
        {
            return 8f; // Prioritas tinggi untuk pintu keluar transisi
        }

        // Daftar kata hubung / preposisi Bahasa Indonesia yang diabaikan (Stop-words)
        System.Collections.Generic.HashSet<string> stopWords = new System.Collections.Generic.HashSet<string> {
            "di", "ke", "dari", "yang", "dan", "ada", "pada", "dengan", "untuk", "oleh", "ini", "itu", "atau", "saya", "kamu", "dia", "mereka", "kita", "kami"
        };

        // Pecah teks objektif menjadi kata-kata
        string[] words = textLower.Split(new char[] { ' ', ',', '.', '!', '?', '_', '-' }, System.StringSplitOptions.RemoveEmptyEntries);
        
        // Pecah nama objek menjadi kata-kata untuk pencocokan batas kata yang akurat (mencegah "batuan" mencocokkan "tua")
        string[] goWords = nameLower.Split(new char[] { ' ', ',', '.', '!', '?', '_', '-', '(', ')', '[', ']', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (string word in words)
        {
            if (word.Length <= 2 || stopWords.Contains(word)) continue; // Abaikan stop-words dan kata <= 2 huruf

            // Jika nama objek sama persis dengan salah satu kata di objektif
            if (nameLower == word)
            {
                if (go.GetComponent("InteractableObject") != null) return 9f;
                return 7f;
            }

            // Cek apakah kata objektif terdaftar sebagai kata utuh dalam nama objek (mencegah batuan <=> tua)
            bool hasExactWord = false;
            foreach (var goWord in goWords)
            {
                if (goWord == word)
                {
                    hasExactWord = true;
                    break;
                }
            }

            if (hasExactWord)
            {
                if (go.GetComponent("InteractableObject") != null) return 6f;
                return 4f;
            }
        }

        return 0f;
    }

    private void SpawnGuideTrail()
    {
        if (PlayerMovement.ActivePlayerInstance == null) return;

        // Sembunyikan jejak emas penunjuk jalan saat pemain berada di dalam maze di Hutan4 (Quest ID 16)
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Hutan4" && currentQuest == 16)
        {
            if (MazeDarkness.instance != null && MazeDarkness.instance.isInMaze)
            {
                return;
            }
        }

        // Selalu perbarui target secara dinamis sebelum spawn untuk mencegah race condition (misal saat NPC baru di-set aktif)
        RefreshQuestTarget();

        if (activeQuestTarget == null) return;

        GameObject trailObj = new GameObject("QuestGuideTrailInstance");
        trailObj.transform.position = PlayerMovement.ActivePlayerInstance.transform.position;

        QuestGuideTrail trailScript = trailObj.AddComponent<QuestGuideTrail>();
        trailScript.Initialize(activeQuestTarget, 6f);
    }
}