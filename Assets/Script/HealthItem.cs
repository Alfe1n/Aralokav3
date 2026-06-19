using UnityEngine;

public class HealthItem : MonoBehaviour
{
    [Header("Heal")]
    public int healAmount = 1;

    [Header("SFX")]
    public AudioClip healSFX;

    [Header("Persistence")]
    [Tooltip("Centang agar item tidak muncul lagi setelah diambil (permanen)")]
    public bool persistent = true;
    [Tooltip("ID unik item ini. Kosongkan untuk auto-generate dari nama scene + nama object.")]
    public string persistentKey;

    private bool playerInside = false;
    private bool picked = false;

    void Start()
    {
        if (persistent)
        {
            string key = GetKey();
            if (PlayerPrefs.GetInt(key, 0) == 1)
            {
                Destroy(gameObject);
                return;
            }
        }
    }

    void Update()
    {
        if (picked || !playerInside) return;
        if (Input.GetKeyDown(KeyCode.E))
            Pickup();
    }

    void Pickup()
    {
        picked = true;

        // Heal player aktif
        PlayerMovement pm = FindFirstObjectByType<PlayerMovement>();
        if (pm != null)
        {
            Health hp = pm.GetComponent<Health>();
            if (hp != null)
            {
                hp.Heal(healAmount);
                OrangUtanHealthUI.instance?.RefreshUI();
            }
        }

        if (healSFX != null)
            AudioSource.PlayClipAtPoint(healSFX, transform.position);

        if (DialogueManager.instance != null)
            DialogueManager.instance.HidePrompt();

        if (persistent)
        {
            PlayerPrefs.SetInt(GetKey(), 1);
            PlayerPrefs.Save();
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Player-Orang Utan")) return;
        playerInside = true;
        if (DialogueManager.instance != null)
            DialogueManager.instance.ShowPrompt();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Player-Orang Utan")) return;
        playerInside = false;
        if (DialogueManager.instance != null)
            DialogueManager.instance.HidePrompt();
    }

    string GetKey()
    {
        if (!string.IsNullOrEmpty(persistentKey)) return persistentKey;
        return "healthitem_" + gameObject.scene.name + "_" + gameObject.name;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
}
