using UnityEngine;
using System;
using System.Collections;

public class Health : MonoBehaviour
{
    [Header("HP")]
    public int maxHealth = 3;
    public int currentHealth;

    public bool isPlayer = false;

    public Action onDeath;
    public Action onHit;

    void Awake()
    {
        currentHealth = maxHealth;

        onDeath = () =>
        {
            Debug.Log($"[Health] onDeath triggered for '{gameObject.name}'. isPlayer: {isPlayer}");
            if (isPlayer)
            {
                Debug.Log($"[Health] isPlayer is true. GameOverManager.Instance is null? {GameOverManager.Instance == null}");
                // Trigger Game Over Panel jika tersedia
                if (GameOverManager.Instance != null)
                {
                    SpriteRenderer sr = GetComponent<SpriteRenderer>();
                    PlayerMovement pm = GetComponent<PlayerMovement>();
                    if (sr != null) sr.enabled = false;
                    if (pm != null) pm.canMove = false;

                    GameOverManager.Instance.ShowGameOver();
                }
                else
                {
                    // Fallback jika dimainkan terpisah di Editor tanpa Core Scene
                    StartCoroutine(PlayerRespawnRoutine());
                }
            }
            else
            {
                // Enemy: langsung destroy
                Destroy(gameObject);
            }
        };
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return; // Sudah mati / sedang respawn, abaikan damage tambahan

        currentHealth -= amount;
        Debug.Log($"[Health] '{gameObject.name}' took {amount} damage. Current HP: {currentHealth}. isPlayer: {isPlayer}");
        onHit?.Invoke();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log($"[Health] '{gameObject.name}' HP is <= 0! Invoking onDeath. onDeath is null? {onDeath == null}");
            onDeath?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(
            currentHealth + amount,
            maxHealth
        );
    }

    IEnumerator PlayerRespawnRoutine()
    {
        // Jangan gunakan SetActive(false) karena akan menghentikan Coroutine!
        // Kita matikan visual dan pergerakannya saja.
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        PlayerMovement pm = GetComponent<PlayerMovement>();

        if (sr != null) sr.enabled = false;
        if (pm != null) pm.canMove = false;

        // Tunggu 2 detik
        yield return new WaitForSeconds(2f);

        // Isi ulang HP
        currentHealth = maxHealth;

        // Hidupkan kembali visual dan pergerakan
        if (sr != null) sr.enabled = true;
        if (pm != null) pm.canMove = true;

        Debug.Log("Player Respawned!");
    }
}
