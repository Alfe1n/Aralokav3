using UnityEngine;
using System.Collections;

public class UlarAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject attackHitbox;
    public Animator animator;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float followRange = 5f;
    public float attackRange = 0.8f;

    [Header("Patrol")]
    public float patrolRadius   = 3f;
    public float patrolWaitMin  = 1f;
    public float patrolWaitMax  = 3f;

    [Header("Attack")]
    [Tooltip("Jeda diam sebelum menggigit — kayak ular coil")]
    public float coilTime      = 0.5f;
    public float lungeSpeed    = 12f;
    public float lungeDuration = 0.15f;
    public float recoverTime   = 0.8f;

    private Rigidbody2D rb;
    private bool isAttacking;
    private Vector2 patrolCenter;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        patrolCenter = transform.position;

        Vector3 pos = transform.position;
        pos.z = 0f; // PERBAIKAN: Set ke 0 agar tidak tertutup map (sebelumnya 1f)
        transform.position = pos;

        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
            sr.sortingLayerName = "Foreground";

        if (player == null || player == transform || player.GetComponent<PlayerMovement>() == null || !player.gameObject.activeInHierarchy)
            FindActivePlayer();

        StartCoroutine(PatrolRoutine());
    }

    void Update()
    {
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            FindActivePlayer();
            if (player == null) return;
        }

        float dist = Vector2.Distance(
            transform.position, player.position
        );

        if (dist <= attackRange)
        {
            if (!isAttacking)
                StartCoroutine(Attack());
        }
    }

    void FixedUpdate()
    {
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            FindActivePlayer();
            if (player == null) return;
        }

        if (isAttacking) return;

        float dist = Vector2.Distance(rb.position, player.position);

        if (dist <= followRange && dist > attackRange)
        {
            Vector2 dir = ((Vector2)player.position - rb.position).normalized;
            rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
            RotateToward(dir);
        }
    }

    // Gunakan Flip Kiri/Kanan agar sprite 2D tidak rusak (bukan di-rotate)
    void RotateToward(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.001f) return;

        // Pastikan rotasi dinetralkan agar tidak nyangkut / miring
        transform.rotation = Quaternion.identity;

        Vector3 scale = transform.localScale;
        if (dir.x > 0) scale.x = Mathf.Abs(scale.x); // Hadap Kanan
        else if (dir.x < 0) scale.x = -Mathf.Abs(scale.x); // Hadap Kiri
        transform.localScale = scale;
    }

    private void FindActivePlayer()
    {
        // Cari object aktif dengan tag Player-Orang Utan
        GameObject playerObj = GameObject.FindWithTag("Player-Orang Utan");
        if (playerObj != null && playerObj.activeInHierarchy)
        {
            player = playerObj.transform;
            return;
        }

        // Fallback ke tag Player
        playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null && playerObj.activeInHierarchy)
        {
            player = playerObj.transform;
            return;
        }

        // Fallback terakhir ke komponen PlayerMovement
        PlayerMovement pm = Object.FindFirstObjectByType<PlayerMovement>();
        if (pm != null && pm.gameObject.activeInHierarchy)
        {
            player = pm.transform;
        }
    }

    // ─── PATROL ─────────────────────────────────────────────────
    IEnumerator PatrolRoutine()
    {
        while (true)
        {
            if (player == null || !player.gameObject.activeInHierarchy)
            {
                FindActivePlayer();
                yield return null;
                continue;
            }

            if (Vector2.Distance(transform.position, player.position) <= followRange)
            {
                yield return null;
                continue;
            }

            Vector2 target = patrolCenter + Random.insideUnitCircle * patrolRadius;

            float maxPatrolTime = 3f;
            float patrolTimer = 0f;

            while (Vector2.Distance(transform.position, target) > 0.1f)
            {
                if (player == null || !player.gameObject.activeInHierarchy)
                    break;

                if (Vector2.Distance(transform.position, player.position) <= followRange)
                    break;
                if (isAttacking) break;

                patrolTimer += Time.fixedDeltaTime;
                if (patrolTimer >= maxPatrolTime)
                    break;

                Vector2 dir = (target - (Vector2)transform.position).normalized;
                rb.MovePosition(
                    rb.position + dir * (moveSpeed * 0.5f) * Time.fixedDeltaTime
                );
                RotateToward(dir);

                yield return new WaitForFixedUpdate();
            }

            yield return new WaitForSeconds(
                Random.Range(patrolWaitMin, patrolWaitMax)
            );
        }
    }

    // ─── ATTACK ─────────────────────────────────────────────────
    IEnumerator Attack()
    {
        isAttacking = true;

        try
        {
            if (player == null || !player.gameObject.activeInHierarchy)
                FindActivePlayer();

            if (player == null)
            {
                yield break;
            }

            Vector2 lungeDir = (
                (Vector2)player.position - rb.position
            ).normalized;
            RotateToward(lungeDir);

            if (animator != null)
            {
                animator.SetTrigger("Attack");
                yield return new WaitForSeconds(0.1f);
                animator.speed = 0f;
                yield return new WaitForSeconds(coilTime);
                animator.speed = 1f;
            }
            else
            {
                yield return new WaitForSeconds(coilTime);
            }

            if (attackHitbox != null)
            {
                float frontOffset = Mathf.Max(0.8f, attackRange * 0.5f);
                attackHitbox.transform.localPosition = new Vector3(frontOffset, 0f, 0f);
                attackHitbox.SetActive(true);
            }

            float timer = 0f;
            while (timer < lungeDuration)
            {
                rb.MovePosition(
                    rb.position + lungeDir * lungeSpeed * Time.fixedDeltaTime
                );
                timer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            if (attackHitbox != null)
                attackHitbox.SetActive(false);

            yield return new WaitForSeconds(recoverTime);
        }
        finally
        {
            if (animator != null)
                animator.speed = 1f;
            if (attackHitbox != null)
                attackHitbox.SetActive(false);
            isAttacking = false;
        }
    }
}
