using UnityEngine;
using UnityEngine.UI;

public class OrangUtanHealthUI : MonoBehaviour
{
    [Header("GUI References")]
    public GameObject uiRoot;
    public Image[] heartImages;

    [Header("Sprites")]
    public Sprite healthFullSprite;
    public Sprite healthEmptySprite;

    private int lastHealth = -1;
    private int lastMaxHealth = -1;

    void Start()
    {
        if (uiRoot == null || heartImages == null || heartImages.Length < 3 || healthFullSprite == null || healthEmptySprite == null)
        {
            Debug.LogError("[OrangUtanHealthUI] Missing references in inspector!");
        }
        else
        {
            // Initially hide the GUI
            uiRoot.SetActive(false);
        }
    }

    void Update()
    {
        if (uiRoot == null) return;

        PlayerMovement player = PlayerMovement.ActivePlayerInstance;
        if (player != null && player.gameObject.activeInHierarchy && player.CompareTag("Player-Orang Utan"))
        {
            Health health = player.GetComponent<Health>();
            if (health != null)
            {
                if (!uiRoot.activeSelf)
                {
                    uiRoot.SetActive(true);
                }

                if (health.currentHealth != lastHealth || health.maxHealth != lastMaxHealth)
                {
                    UpdateHearts(health.currentHealth, health.maxHealth);
                }
            }
            else
            {
                if (uiRoot.activeSelf) uiRoot.SetActive(false);
            }
        }
        else
        {
            if (uiRoot.activeSelf) uiRoot.SetActive(false);
        }
    }

    void UpdateHearts(int currentHealth, int maxHealth)
    {
        lastHealth = currentHealth;
        lastMaxHealth = maxHealth;

        for (int i = 0; i < 3; i++)
        {
            if (heartImages[i] != null)
            {
                if (currentHealth >= i + 1)
                {
                    heartImages[i].sprite = healthFullSprite;
                }
                else
                {
                    heartImages[i].sprite = healthEmptySprite;
                }
            }
        }
    }
}
