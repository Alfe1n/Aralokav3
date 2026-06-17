#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public static class EditorSceneBootstrapper
{
    private static string activeGameplayScene;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        // Hanya jalankan di Editor dan jika scene yang aktif bukan scene pembuka
        string activeScene = SceneManager.GetActiveScene().name;
        if (activeScene == "Boot Scene" || activeScene == "MainMenu" || activeScene == "Core Scene" || activeScene == "LoadingScene")
        {
            return;
        }

        activeGameplayScene = activeScene;
        Debug.Log($"[EditorSceneBootstrapper] Memulai play langsung dari scene '{activeGameplayScene}'. Memuat Core Scene secara otomatis...");

        // Muat Core Scene secara additive
        SceneManager.LoadScene("Core Scene", LoadSceneMode.Additive);
        
        // Daftarkan callback saat Core Scene selesai dimuat
        SceneManager.sceneLoaded += OnCoreSceneLoaded;
    }

    private static void OnCoreSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Core Scene")
        {
            SceneManager.sceneLoaded -= OnCoreSceneLoaded;

            // Set scene gameplay kembali sebagai Active Scene
            Scene gameplayScene = SceneManager.GetSceneByName(activeGameplayScene);
            if (gameplayScene.IsValid())
            {
                SceneManager.SetActiveScene(gameplayScene);
            }

            // Jalankan setup pergerakan dan kamera menggunakan runner sementara
            GameObject runnerObj = new GameObject("EditorBootstrapperRunner");
            var runner = runnerObj.AddComponent<BootstrapperRunner>();
            runner.StartCoroutine(runner.SetupRoutine(activeGameplayScene));
        }
    }

    private class BootstrapperRunner : MonoBehaviour
    {
        public IEnumerator SetupRoutine(string gameplaySceneName)
        {
            // Tunggu 1 frame agar semua objek player ter-instantiate di Core Scene
            yield return null;

            // Cek apakah scene saat ini adalah Hutan (membutuhkan Orang Utan)
            bool useOrangUtan = gameplaySceneName.Contains("Hutan");

            // Cari player di scene (termasuk yang non-aktif agar bisa di-switch)
            PlayerMovement[] players = Resources.FindObjectsOfTypeAll<PlayerMovement>();
            PlayerMovement activePlayer = null;

            foreach (var p in players)
            {
                // Abaikan prefab
                if (string.IsNullOrEmpty(p.gameObject.scene.name)) continue;

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

            if (activePlayer != null)
            {
                // Cari spawn point default di scene gameplay saat ini
                GameObject spawn = GameObject.Find("Spawn_Utama");
                if (spawn != null)
                {
                    activePlayer.transform.position = spawn.transform.position;
                }
            }

            if (activePlayer != null)
            {
                Debug.Log($"[EditorSceneBootstrapper] Player '{activePlayer.name}' berhasil diteleportasikan ke Spawn_Utama di scene '{gameplaySceneName}'.");

                // Targetkan kamera ke player secara dinamis
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
                            followProp.SetValue(script, activePlayer.transform);
                        }

                        var lookAtProp = script.GetType().GetProperty("LookAt");
                        if (lookAtProp != null)
                        {
                            lookAtProp.SetValue(script, activePlayer.transform);
                        }
                    }
                }
            }

            // Hancurkan runner sementara
            Destroy(gameObject);
        }
    }
}
#endif
