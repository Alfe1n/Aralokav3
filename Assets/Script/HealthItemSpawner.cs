using UnityEngine;

public class HealthItemSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject healthItemPrefab;
    public int spawnCount = 3;

    [Header("Area (ukuran dari titik tengah spawner)")]
    public float areaWidth = 10f;
    public float areaHeight = 10f;

    void Start()
    {
        Random.InitState(gameObject.scene.name.GetHashCode()); // posisi konsisten per scene

        for (int i = 0; i < spawnCount; i++)
        {
            float x = transform.position.x + Random.Range(-areaWidth / 2f, areaWidth / 2f);
            float y = transform.position.y + Random.Range(-areaHeight / 2f, areaHeight / 2f);

            string itemName = $"HealthItem_{gameObject.scene.name}_{i}";
            string key = $"healthitem_{gameObject.scene.name}_{itemName}";

            // Jangan spawn item yang sudah pernah diambil
            if (PlayerPrefs.GetInt(key, 0) == 1) continue;

            GameObject item = Instantiate(healthItemPrefab, new Vector3(x, y, 1f), Quaternion.identity);
            item.name = itemName;

            // Pastikan persistent key sama dengan yang dicek di atas
            HealthItem hi = item.GetComponent<HealthItem>();
            if (hi != null) hi.persistentKey = key;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position, new Vector3(areaWidth, areaHeight, 0.1f));
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(areaWidth, areaHeight, 0.1f));
    }
}
