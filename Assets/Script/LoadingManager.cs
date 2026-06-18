using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;

    [Header("Fade")]
    public SceneFader sceneFader;

    [Header("Loading")]
    public float minimumLoadTime = 3f;

    IEnumerator Start()
    {
        // =========================
        // HIDE QUEST UI
        // =========================

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.HideObjective();
        }

        // =========================
        // FADE IN
        // =========================

        if (sceneFader != null)
        {
            yield return StartCoroutine(
                sceneFader.FadeIn()
            );
        }

        // =========================
        // PREPARE VIDEO (non-blocking — biarkan prepare sambil scene loading)
        // =========================

        if (videoPlayer != null)
        {
            videoPlayer.Prepare();
        }

        float timer = 0f;

        // =========================
        // LOAD CORE SCENE (WITH SELF-HEALING RE-LOAD CHECK)
        // =========================

        if (SceneManager.GetSceneByName("Core Scene").isLoaded)
        {
            // Cek apakah Core Scene rusak/tidak lengkap
            // Termasuk kasus QuestManager masih ada di DontDestroyOnLoad
            // tapi objectivePanel-nya null (stale reference setelah pindah scene)
            bool isCoreSceneBroken = QuestManager.Instance == null
                || QuestManager.Instance.objectivePanel == null
                || GameObject.Find("Canvas") == null;
            if (isCoreSceneBroken)
            {
                Debug.Log("[LoadingManager] Core Scene terdeteksi rusak/tidak lengkap. Memuat ulang secara fresh...");
                // Hancurkan QuestManager lama agar yang baru dari Core Scene bisa initialize
                if (QuestManager.Instance != null) Destroy(QuestManager.Instance.gameObject);
                if (TransitionManager.Instance != null) Destroy(TransitionManager.Instance.gameObject);
                yield return SceneManager.UnloadSceneAsync("Core Scene");
            }
        }

        if (!SceneManager.GetSceneByName("Core Scene").isLoaded)
        {
            yield return SceneManager.LoadSceneAsync(
                "Core Scene",
                LoadSceneMode.Additive
            );
        }

        // =========================
        // TENTUKAN SCENE GAMEPLAY TARGET
        // Baca dari PlayerPrefs agar Continue bisa masuk ke scene yang benar
        // =========================

        string targetGameplayScene = PlayerPrefs.GetString("LastScene", "Kamar Bara");
        string savedSpawn = PlayerPrefs.GetString("LastSpawn", "");

        Debug.Log($"[LoadingManager] Loading gameplay scene: {targetGameplayScene}, spawn: {savedSpawn}");

        // Set spawn point agar PlayerSpawn tahu di mana harus spawn
        if (!string.IsNullOrEmpty(savedSpawn))
        {
            SpawnManager.spawnPointName = savedSpawn;
        }

        // =========================
        // INITIAL SWITCH PLAYER LOGIC (Manusia vs Orang Utan)
        // =========================

        PlayerMovement[] allPlayers = Resources.FindObjectsOfTypeAll<PlayerMovement>();
        bool useOrangUtan = targetGameplayScene.Contains("Hutan");
        Transform newTarget = null;

        foreach (PlayerMovement p in allPlayers)
        {
            // Abaikan prefab
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

        // UPDATE CINEMACHINE CAMERA TARGET (Version Agnostic)
        if (newTarget != null)
        {
            MonoBehaviour[] allScripts = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
            foreach (var script in allScripts)
            {
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

        // =========================
        // LOAD GAMEPLAY SCENE
        // =========================

        AsyncOperation gameplayLoad = SceneManager.LoadSceneAsync(
            targetGameplayScene,
            LoadSceneMode.Additive
        );

        while (!gameplayLoad.isDone)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // =========================
        // PLAY VIDEO SEKARANG (semua scene selesai dimuat, CPU lebih bebas)
        // Audio tidak akan terpotong karena loading sudah beres
        // =========================

        if (videoPlayer != null)
        {
            // Tunggu prepare selesai (biasanya sudah selesai sejak lama)
            while (!videoPlayer.isPrepared)
                yield return null;

            videoPlayer.Play();
        }

        // =========================
        // MINIMUM LOAD TIME
        // =========================

        while (timer < minimumLoadTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // =========================
        // SET ACTIVE SCENE
        // =========================

        Scene gameplay = SceneManager.GetSceneByName(targetGameplayScene);
        SceneManager.SetActiveScene(gameplay);

        yield return null;
        yield return null;

        // =========================
        // TELEPORT PLAYER KE SAVED SPAWN (UNTUK CONTINUE)
        // =========================

        if (!string.IsNullOrEmpty(savedSpawn) && newTarget != null)
        {
            GameObject spawnObj = null;
            // Cari di scene gameplay yang baru dimuat
            GameObject[] rootObjects = gameplay.GetRootGameObjects();
            foreach (GameObject rootObj in rootObjects)
            {
                if (rootObj.name == savedSpawn)
                {
                    spawnObj = rootObj;
                    break;
                }
                var children = rootObj.GetComponentsInChildren<Transform>(true);
                foreach (var c in children)
                {
                    if (c.name == savedSpawn)
                    {
                        spawnObj = c.gameObject;
                        break;
                    }
                }
                if (spawnObj != null) break;
            }

            // Fallback: cari secara global
            if (spawnObj == null)
                spawnObj = GameObject.Find(savedSpawn);

            if (spawnObj != null)
            {
                newTarget.position = spawnObj.transform.position;
                Debug.Log($"[LoadingManager] Continue: teleported player to {savedSpawn} at {spawnObj.transform.position}");
            }
            else
            {
                Debug.LogWarning($"[LoadingManager] Continue: spawn point '{savedSpawn}' not found in scene {targetGameplayScene}");
            }
        }

        // =========================
        // STOP VIDEO
        // =========================

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        // =========================
        // FADE OUT
        // =========================

        if (sceneFader != null)
        {
            yield return StartCoroutine(
                sceneFader.FadeOut()
            );
        }

        // =========================
        // TUNGGU SEBENTAR
        // =========================

        yield return new WaitForSeconds(0.5f);

        // =========================
        // SHOW QUEST UI
        // =========================

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ShowObjective();
        }

        // kasih 1 frame biar aman
        yield return null;

        // =========================
        // UNLOAD LOADING SCENE
        // =========================

        yield return SceneManager.UnloadSceneAsync("LoadingScene");
    }
}