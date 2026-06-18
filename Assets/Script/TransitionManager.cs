using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;
    public bool isTransitioning = false;

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

    public void StartTransition(
        string targetScene,
        string targetSpawn,
        int nextQuest = -1,
        bool useFade = true
    )
    {
        if (isTransitioning)
        {
            Debug.LogWarning($"[TransitionManager] Transition to {targetScene} ignored because a transition is already in progress.");
            return;
        }

        StartCoroutine(
            TransitionRoutine(
                targetScene,
                targetSpawn,
                nextQuest,
                useFade
            )
        );
    }

    private FadeUI GetFader()
    {
        if (FadeUI.instance != null) return FadeUI.instance;

        FadeUI[] faders = Resources.FindObjectsOfTypeAll<FadeUI>();
        if (faders != null && faders.Length > 0)
        {
            foreach (FadeUI f in faders)
            {
                if (!string.IsNullOrEmpty(f.gameObject.scene.name))
                {
                    FadeUI.instance = f;
                    return f;
                }
            }
        }
        return null;
    }

    IEnumerator TransitionRoutine(
        string targetScene,
        string targetSpawn,
        int nextQuest,
        bool useFade
    )
    {
        isTransitioning = true;
        Debug.Log($"TRANSITION -> {targetScene} (useFade: {useFade})");
        OrangUtanUIVisibility.Instance?.ForceHide();

        try
        {
            // Kunci gerakan player aktif saat ini sebelum transisi dimulai
            PlayerMovement currentActivePlayer = PlayerMovement.ActivePlayerInstance;
            if (currentActivePlayer == null)
            {
                currentActivePlayer = FindFirstObjectByType<PlayerMovement>();
            }
            if (currentActivePlayer != null)
            {
                currentActivePlayer.canMove = false;
            }

            SpawnManager.spawnPointName = targetSpawn;
            PlayerPrefs.SetString("LastSpawn", targetSpawn); // cache awal agar auto-save punya spawn name yang benar
            Scene currentScene = SceneManager.GetActiveScene();

            FadeUI activeFader = GetFader();

            // =====================================
            // 1. SET LAYAR HITAM SECARA INSTAN
            // =====================================
            if (useFade && activeFader != null)
            {
                activeFader.SetBlackInstant();
            }

            // =====================================
            // 2. LOAD SCENE BARU (ADDITIVE MODE)
            // =====================================
            // Harus Additive agar Core Scene (Player, Quest, UI) tidak ikut terhapus!
            Debug.Log($"[TransitionManager] Loading scene: {targetScene} additively. Current active scene is: {currentScene.name}");
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);
            
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            Debug.Log($"[TransitionManager] Additive load of scene: {targetScene} is done.");

            // Set scene baru sebagai scene aktif
            Scene newScene = SceneManager.GetSceneByName(targetScene);
            if (!newScene.IsValid())
            {
                Debug.LogWarning($"[TransitionManager] Scene '{targetScene}' not found by name. Searching through all loaded scenes...");
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene s = SceneManager.GetSceneAt(i);
                    if (s.name == targetScene)
                    {
                        newScene = s;
                        break;
                    }
                }
            }

            if (!newScene.IsValid())
            {
                Debug.LogError($"[TransitionManager] Loaded scene '{targetScene}' could not be resolved as a valid scene!");
            }
            else
            {
                Debug.Log($"[TransitionManager] Setting active scene to: {newScene.name}");
                bool activeSetResult = false;
                try
                {
                    activeSetResult = SceneManager.SetActiveScene(newScene);
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }
                Debug.Log($"[TransitionManager] SetActiveScene result: {activeSetResult}. Active scene is now: {SceneManager.GetActiveScene().name}");
            }

            yield return null;

            // =====================================
            // 3. UNLOAD SCENE LAMA (DAN LEFTOVER GAMEPLAY SCENES LAINNYA)
            // =====================================
            System.Collections.Generic.List<Scene> scenesToUnload = new System.Collections.Generic.List<Scene>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.IsValid() && scene.name != "Core Scene" && 
                    scene.name != targetScene && 
                    scene.name != "Boot Scene" && 
                    scene.name != "MainMenu" && 
                    scene.name != "LoadingScene" && 
                    scene.name != "OpeningScene")
                {
                    scenesToUnload.Add(scene);
                }
            }

            foreach (Scene scene in scenesToUnload)
            {
                Debug.Log($"[TransitionManager] Unloading scene: {scene.name}");
                AsyncOperation asyncUnload = null;
                try
                {
                    asyncUnload = SceneManager.UnloadSceneAsync(scene);
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }

                if (asyncUnload != null)
                {
                    while (!asyncUnload.isDone)
                    {
                        yield return null;
                    }
                    Debug.Log($"[TransitionManager] Unload of scene: {scene.name} is done.");
                }
                else
                {
                    Debug.LogError($"[TransitionManager] SceneManager.UnloadSceneAsync returned null for scene '{scene.name}'!");
                }
            }

            // Beri waktu 1 frame agar Unity selesai melakukan inisialisasi object di scene baru
            yield return null;

            // ----------------------------------------------------
            // SWITCH PLAYER LOGIC & TELEPORT SAAT LAYAR HITAM!
            // ----------------------------------------------------
            PlayerMovement[] allPlayers = Resources.FindObjectsOfTypeAll<PlayerMovement>();
            bool useOrangUtan = targetScene.Contains("Hutan");
            Transform newTarget = null;

            foreach (PlayerMovement p in allPlayers)
            {
                if (string.IsNullOrEmpty(p.gameObject.scene.name)) continue;

                if (p.CompareTag("Player-Orang Utan"))
                {
                    p.gameObject.SetActive(useOrangUtan);
                    if (useOrangUtan) newTarget = p.transform;
                }
                else if (p.CompareTag("Player"))
                {
                    p.gameObject.SetActive(!useOrangUtan);
                    if (!useOrangUtan) newTarget = p.transform;
                }
            }

            // TELEPORT PLAYER KE SPAWN POINT BARU DI SCENE YANG BARU DIMUAT
            GameObject spawnObj = null;
            if (newScene.IsValid())
            {
                GameObject[] rootObjects = newScene.GetRootGameObjects();
                foreach (GameObject rootObj in rootObjects)
                {
                    if (rootObj.name == targetSpawn)
                    {
                        spawnObj = rootObj;
                        break;
                    }
                    Transform child = rootObj.transform.Find(targetSpawn);
                    if (child != null)
                    {
                        spawnObj = child.gameObject;
                        break;
                    }
                    var children = rootObj.GetComponentsInChildren<Transform>(true);
                    foreach (var c in children)
                    {
                        if (c.name == targetSpawn)
                        {
                            spawnObj = c.gameObject;
                            break;
                        }
                    }
                    if (spawnObj != null) break;
                }
            }

            // Fallback jika tidak ditemukan secara lokal, cari secara global
            if (spawnObj == null)
            {
                spawnObj = GameObject.Find(targetSpawn);
            }

            if (spawnObj != null && newTarget != null)
            {
                newTarget.position = spawnObj.transform.position;
                Debug.Log($"[TransitionManager] Teleported player {newTarget.name} to {targetSpawn} at {spawnObj.transform.position}");
            }
            else
            {
                Debug.LogWarning($"[TransitionManager] Could not find spawn point {targetSpawn} for player!");
            }

            // UPDATE CINEMACHINE CAMERA TARGET
            if (newTarget != null)
            {
                // Hanya cari di MonoBehaviour yang aktif di scene (jauh lebih cepat dibanding FindObjectsOfTypeAll)
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
                            followProp.SetValue(script, newTarget);
                        }
                        else
                        {
                            var targetProp = script.GetType().GetProperty("Target");
                            if (targetProp != null)
                            {
                                object targetStruct = targetProp.GetValue(script);
                                var trackingField = targetStruct.GetType().GetField("TrackingTarget");
                                if (trackingField != null)
                                {
                                    trackingField.SetValue(targetStruct, newTarget);
                                    targetProp.SetValue(script, targetStruct);
                                }
                            }
                        }
                    }
                }
            }
            // ----------------------------------------------------

            // Tunggu sedikit agar physics stabil sebelum layar memudar terang
            yield return new WaitForSeconds(0.2f);

            if (QuestManager.Instance != null && nextQuest >= 0)
            {
                QuestManager.Instance.SetQuest(nextQuest);
            }

            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.SaveCheckpointQuest();
                // Simpan scene terakhir agar fitur Continue bisa melanjutkan dari sini
                QuestManager.Instance.SaveLastScene(targetScene, targetSpawn);
            }

            // =====================================
            // 4. MEMUDAR TERANG SECARA EKSPLISIT
            // =====================================
            if (useFade && activeFader != null)
            {
                yield return StartCoroutine(activeFader.FadeIn());
            }
            else if (activeFader != null)
            {
                activeFader.SetTransparentInstant();
            }

            PlayerMovement player = PlayerMovement.ActivePlayerInstance;
            if (player == null)
            {
                player = FindFirstObjectByType<PlayerMovement>();
            }
            if (player != null)
            {
                player.canMove = true;
            }

            yield return new WaitForSeconds(0.3f);

            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.ShowObjective();
            }

            OrangUtanUIVisibility.Instance?.ForceRefresh();

            Debug.Log("TRANSITION FINISHED");
        }
        finally
        {
            isTransitioning = false;
        }
    }
}
