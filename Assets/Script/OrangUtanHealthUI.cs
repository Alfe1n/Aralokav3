using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OrangUtanHealthUI : MonoBehaviour
{
    public static OrangUtanHealthUI instance;

    [Header("Heart Images (urutan: Heart_0, Heart_1, Heart_2)")]
    public Image[] heartImages;

    [Header("Sprites")]
    public Sprite fullSprite;
    public Sprite emptySprite;

    private Health playerHealth;

    void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        FindPlayer();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Unsubscribe();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-find player setiap kali scene Hutan dimuat
        if (scene.name.Contains("Hutan"))
            FindPlayer();
    }

    void FindPlayer()
    {
        // Cari semua PlayerMovement termasuk yang inactive
        PlayerMovement[] allPlayers = Resources.FindObjectsOfTypeAll<PlayerMovement>();
        foreach (PlayerMovement pm in allPlayers)
        {
            if (!pm.CompareTag("Player-Orang Utan")) continue;
            if (string.IsNullOrEmpty(pm.gameObject.scene.name)) continue; // skip prefab di Project

            Health h = pm.GetComponent<Health>();
            if (h == null) continue;

            if (playerHealth == h) return; // sudah subscribe, skip

            Unsubscribe();
            playerHealth = h;
            playerHealth.onHit += RefreshUI;
            playerHealth.onDeath += RefreshUI;
            RefreshUI();
            return;
        }
    }

    void Unsubscribe()
    {
        if (playerHealth != null)
        {
            playerHealth.onHit -= RefreshUI;
            playerHealth.onDeath -= RefreshUI;
            playerHealth = null;
        }
    }

    public void RefreshUI()
    {
        if (playerHealth == null) { FindPlayer(); return; }

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null) continue;
            heartImages[i].sprite = (i < playerHealth.currentHealth) ? fullSprite : emptySprite;
        }
    }
}
