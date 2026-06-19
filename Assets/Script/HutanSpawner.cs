using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

// HutanSpawner — copy langsung, tidak ada dependency konflik
public class HutanSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnEntry
    {
        public GameObject prefab;
        public int count = 3;
        public float spawnRadius = 5f;
    }

    [Header("Enemy Types")]
    public SpawnEntry[] enemyTypes;

    [Header("Settings")]
    public bool spawnOnStart = true; // matikan jika dipakai di arena (ArenaTrigger yang trigger)
    public bool respawnOnAllDead = true;
    public float respawnDelay = 5f;

    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private bool isRespawning = false;

    void Start()
    {
        if (spawnOnStart) SpawnAll();
    }

    void Update()
    {
        if (!respawnOnAllDead || isRespawning) return;

        if (spawnedEnemies.Count > 0 && AllDead())
        {
            isRespawning = true;
            Invoke(nameof(SpawnAll), respawnDelay);
        }
    }

    public void SpawnAll()
    {
        spawnedEnemies.RemoveAll(e => e == null);
        isRespawning = false;

        foreach (SpawnEntry entry in enemyTypes)
        {
            if (entry.prefab == null) continue;

            for (int i = 0; i < entry.count; i++)
            {
                Vector2 offset = Random.insideUnitCircle * entry.spawnRadius;
                Vector3 spawnPos = transform.position +
                    new Vector3(offset.x, offset.y, 0f); // PERBAIKAN Z ke 0

                GameObject enemy = Instantiate(
                    entry.prefab, spawnPos, Quaternion.identity
                );

                // Paksa enemy masuk ke scene milik spawner, bukan active scene
                if (enemy.scene != gameObject.scene)
                    SceneManager.MoveGameObjectToScene(enemy, gameObject.scene);

                spawnedEnemies.Add(enemy);
            }
        }
    }

    public bool AllDead()
    {
        foreach (GameObject e in spawnedEnemies)
        {
            if (e != null) return false;
        }
        return true;
    }

    public bool IsAllDead()
    {
        return AllDead();
    }

    void OnDrawGizmosSelected()
    {
        if (enemyTypes == null) return;

        Color[] colors = { Color.red, Color.yellow, Color.cyan, Color.green };
        for (int i = 0; i < enemyTypes.Length; i++)
        {
            Gizmos.color = colors[i % colors.Length];
            Gizmos.DrawWireSphere(
                transform.position, enemyTypes[i].spawnRadius
            );
        }
    }
}
