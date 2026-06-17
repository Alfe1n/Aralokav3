using UnityEngine;
using System.Collections;

// ================================================================
// PlayerAttack — ADAPTASI untuk Aralokav3
// ================================================================
// PERUBAHAN dari versi teman:
// 1. Cek canMove dari PlayerMovement agar tidak attack saat dialogue
// 2. Hitbox posisi otomatis berdasarkan arah animasi terakhir
// 3. Input: Left Mouse Button (tidak konflik dengan E/Interactable)
// ================================================================

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public GameObject attackHitbox;

    [Header("Settings")]
    public float attackCooldown = 0.5f;

    private bool isAttacking;
    private SpriteRenderer spriteRenderer;
    private PlayerMovement playerMovement;

    private Vector3 hitboxRightPos = new Vector3(0.8f, 0f, 0f);
    private Vector3 hitboxLeftPos  = new Vector3(-0.8f, 0f, 0f);

    void Start()
    {
        spriteRenderer  = GetComponent<SpriteRenderer>();
        playerMovement  = GetComponent<PlayerMovement>();

        if (attackHitbox != null)
            attackHitbox.SetActive(false);
    }

    void Update()
    {
        // Jangan attack saat player tidak bisa bergerak (dialogue, cutscene, dsb)
        if (playerMovement != null && !playerMovement.canMove)
            return;

        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        isAttacking = true;

        if (animator != null)
            animator.SetTrigger("Attack");

        // Posisikan hitbox berdasarkan arah hadap dari SpriteRenderer.flipX
        if (attackHitbox != null)
        {
            // Kiri atau Kanan (jika spriteRenderer di-flip berarti kiri, selain itu kanan)
            bool isFacingLeft = (spriteRenderer != null) && spriteRenderer.flipX;
            Vector3 targetPos = isFacingLeft ? hitboxLeftPos : hitboxRightPos;

            attackHitbox.transform.localPosition = targetPos;
            attackHitbox.SetActive(true);
        }

        yield return new WaitForSeconds(0.2f);

        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        yield return new WaitForSeconds(0.3f);

        isAttacking = false;
    }
}
