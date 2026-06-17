using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int spawnCount = 5;
    public float spawnRadius = 6f;
    public bool spawnOnStart = true;

    void Start()
    {
        if (spawnOnStart)
        {
            SpawnEnemies();
        }
    }

    public void SpawnEnemies()
    {
        Debug.Log($"[EnemySpawner] SpawnEnemies() dipanggil pada Game Object '{gameObject.name}' di Scene '{gameObject.scene.name}'.");

        if (enemyPrefab == null)
        {
            Debug.LogError($"[EnemySpawner] ERROR: enemyPrefab pada '{gameObject.name}' bernilai NULL! Pastikan Anda telah meng-assign prefab di Inspector.");
            return;
        }

        // Cari player di scene untuk disuntikkan ke AI musuh
        Transform playerTransform = null;
        PlayerMovement activePlayer = FindFirstObjectByType<PlayerMovement>();
        if (activePlayer != null)
        {
            playerTransform = activePlayer.transform;
            Debug.Log($"[EnemySpawner] Player aktif ditemukan secara langsung: '{activePlayer.gameObject.name}'");
        }
        else
        {
            Debug.LogWarning($"[EnemySpawner] Player aktif tidak ditemukan via FindFirstObjectByType. Mencari di all players (fallback)...");
            // Fallback jika player belum aktif (misal saat scene loading additif baru selesai)
            PlayerMovement[] allPlayers = Resources.FindObjectsOfTypeAll<PlayerMovement>();
            foreach (PlayerMovement p in allPlayers)
            {
                if (string.IsNullOrEmpty(p.gameObject.scene.name)) continue;

                // Jika di scene Hutan, prioritaskan Orang Utan
                if (gameObject.scene.name == "Hutan" && p.CompareTag("Player-Orang Utan"))
                {
                    playerTransform = p.transform;
                    Debug.Log($"[EnemySpawner] Fallback: Menemukan Player-Orang Utan '{p.gameObject.name}'");
                    break;
                }
                else if (p.CompareTag("Player"))
                {
                    playerTransform = p.transform;
                    Debug.Log($"[EnemySpawner] Fallback: Menemukan Player biasa '{p.gameObject.name}'");
                }
            }
        }

        if (playerTransform == null)
        {
            Debug.LogError($"[EnemySpawner] ERROR: Player tidak ditemukan di scene memori! Musuh akan di-spawn tanpa target player.");
        }

        // Cari ArenaManager di parent atau di scene
        ArenaManager arena = GetComponentInParent<ArenaManager>();
        if (arena == null)
        {
            arena = FindFirstObjectByType<ArenaManager>();
        }

        Debug.Log($"[EnemySpawner] Mulai melakukan Instantiate sebanyak {spawnCount} musuh dari prefab '{enemyPrefab.name}' sebagai child dari '{gameObject.name}'.");

        for (int i = 0; i < spawnCount; i++)
        {
            // Menentukan posisi random di sekitar spawner
            Vector2 randomPos = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;

            // Memunculkan prefab musuh sebagai child dari spawner (transform)
            GameObject enemy = Instantiate(
                enemyPrefab,
                new Vector3(randomPos.x, randomPos.y, 1f), // Z = 1f agar sesuai layer rendering
                Quaternion.identity,
                transform // Set parent agar rapi di Hierarchy di bawah spawner ini
            );

            Debug.Log($"[EnemySpawner] Musuh ke-{i+1} '{enemy.name}' berhasil di-spawn di {enemy.transform.position} sebagai child.");

            // Daftarkan ke ArenaManager jika ada
            if (arena != null)
            {
                if (arena.enemies == null)
                {
                    arena.enemies = new GameObject[] { enemy };
                }
                else
                {
                    List<GameObject> enemyList = new List<GameObject>(arena.enemies);
                    enemyList.Add(enemy);
                    arena.enemies = enemyList.ToArray();
                }
            }

            // Suntikkan player transform ke AI musuh jika ada
            if (playerTransform != null)
            {
                bool targetSet = false;
                if (enemy.TryGetComponent<EnemyAI>(out var enemyAI))
                {
                    enemyAI.player = playerTransform;
                    targetSet = true;
                }
                if (enemy.TryGetComponent<UlarAI>(out var ularAI))
                {
                    ularAI.player = playerTransform;
                    targetSet = true;
                }
                if (enemy.TryGetComponent<BuayaAI>(out var buayaAI))
                {
                    buayaAI.player = playerTransform;
                    targetSet = true;
                }
                if (enemy.TryGetComponent<BabiHutanAI>(out var babiAI))
                {
                    babiAI.player = playerTransform;
                    targetSet = true;
                }

                if (targetSet)
                {
                    Debug.Log($"[EnemySpawner] Referensi player '{playerTransform.name}' disuntikkan ke AI '{enemy.name}'.");
                }
                else
                {
                    Debug.LogWarning($"[EnemySpawner] Peringatan: Tidak ditemukan komponen AI pada musuh '{enemy.name}'.");
                }
            }
        }

        if (arena != null)
        {
            Debug.Log($"[EnemySpawner] Selesai memunculkan {spawnCount} musuh dan mendaftarkannya ke ArenaManager '{arena.gameObject.name}'.");
        }
        else
        {
            Debug.Log($"[EnemySpawner] Selesai memunculkan {spawnCount} musuh.");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
