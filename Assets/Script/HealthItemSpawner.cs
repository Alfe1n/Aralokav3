using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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
        StartCoroutine(SpawnDelayed());
    }

    IEnumerator SpawnDelayed()
    {
        // Tunggu 1 frame agar scene sudah benar-benar aktif sebelum Instantiate
        yield return null;

        Scene myScene = gameObject.scene;
        string sceneName = myScene.name;

        Random.InitState(sceneName.GetHashCode());

        for (int i = 0; i < spawnCount; i++)
        {
            float x = transform.position.x + Random.Range(-areaWidth / 2f, areaWidth / 2f);
            float y = transform.position.y + Random.Range(-areaHeight / 2f, areaHeight / 2f);

            string itemName = $"HealthItem_{sceneName}_{i}";
            string key = $"healthitem_{sceneName}_{itemName}";

            if (PlayerPrefs.GetInt(key, 0) == 1) continue;

            GameObject item = Instantiate(healthItemPrefab, new Vector3(x, y, 1f), Quaternion.identity);
            item.name = itemName;

            // Paksa item masuk ke scene yang sama dengan spawner, bukan active scene
            if (item.scene != myScene)
                SceneManager.MoveGameObjectToScene(item, myScene);

            HealthItem hi = item.GetComponent<HealthItem>();
            if (hi != null) hi.persistentKey = key;
        }
    }

    // Dipanggil oleh GameOverManager saat retry agar items bisa spawn ulang
    public void ClearPrefs()
    {
        string sceneName = gameObject.scene.name;
        for (int i = 0; i < spawnCount; i++)
        {
            string itemName = $"HealthItem_{sceneName}_{i}";
            string key = $"healthitem_{sceneName}_{itemName}";
            PlayerPrefs.DeleteKey(key);
        }
        PlayerPrefs.Save();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position, new Vector3(areaWidth, areaHeight, 0.1f));
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(areaWidth, areaHeight, 0.1f));
    }
}
