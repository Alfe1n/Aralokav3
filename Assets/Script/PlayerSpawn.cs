using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerSpawn : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 🔥 skip core scene
        if (scene.name == "Core Scene")
            return;

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        yield return null;

        string spawnName =
            SpawnManager.spawnPointName;

        if (string.IsNullOrEmpty(spawnName))
        {
            spawnName = "Spawn_Utama";
        }

        GameObject spawn =
            GameObject.Find(spawnName);

        if (spawn != null)
        {
            Debug.Log(
                "Spawn ditemukan: " + spawnName
            );

            transform.position =
                spawn.transform.position;
        }
        else
        {
            Debug.LogWarning(
                "Spawn point tidak ditemukan: "
                + spawnName
            );
        }
    }
}