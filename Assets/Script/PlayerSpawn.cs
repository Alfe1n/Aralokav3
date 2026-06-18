using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerSpawn : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.sceneLoaded +=
            OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -=
            OnSceneLoaded;
    }

    void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        // skip non gameplay scenes
        if (
            scene.name == "Core Scene"
            || scene.name == "LoadingScene"
            || scene.name == "OpeningScene"
            || scene.name == "MainMenu"
        )
        {
            return;
        }

        // Jika sedang dalam transisi aktif yang diatur oleh TransitionManager,
        // biarkan TransitionManager yang mengurus pemindahan player agar terhindar dari deadlock/double teleport
        if (TransitionManager.Instance != null && TransitionManager.Instance.isTransitioning)
        {
            return;
        }

        StartCoroutine(
            SpawnRoutine()
        );
    }

    IEnumerator SpawnRoutine()
    {
        // 1. Tunggu sampai scene lama benar-benar di-unload.
        // Karena kita pakai LoadSceneMode.Additive, scene lama dan baru sempat bertumpuk sebentar.
        // Jika kita langsung mencari "Spawn_Utama", Unity bisa salah mencari di scene yang lama!
        // Kita tunggu sampai total scene aktif kembali menjadi 2 (yaitu Core Scene + 1 Scene Gameplay).
        while (SceneManager.sceneCount > 2)
        {
            yield return null;
        }

        // 2. Beri waktu tambahan 1 frame agar Unity membersihkan memory object lama
        yield return null;

        // Pastikan hanya player yang aktif yang menjalankan teleport
        if (!gameObject.activeInHierarchy) yield break;

        string spawnName = SpawnManager.spawnPointName;

        if (string.IsNullOrEmpty(spawnName))
        {
            spawnName = "Spawn_Utama";
        }

        GameObject spawn = GameObject.Find(spawnName);

        if (spawn != null)
        {
            Vector3 spawnPos = spawn.transform.position;
            spawnPos.z = 1f;
            Debug.Log($"[PlayerSpawn] Teleporting {gameObject.name} to {spawnName} at {spawnPos}");
            transform.position = spawnPos;
        }
        else
        {
            Debug.LogWarning($"[PlayerSpawn] Spawn point tidak ditemukan: {spawnName}");
        }
    }
}