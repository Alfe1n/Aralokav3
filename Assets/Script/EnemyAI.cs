using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public static bool globalFreeze = false;

    [Header("References")]
    public Transform player;
    public GameObject attackHitbox;
    public Animator animator;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float followRange = 4f;
    public float attackRange = 0.7f;

    [Header("Patrol")]
    public float patrolRadius = 3f;
    public float patrolWaitMin = 1f;
    public float patrolWaitMax = 3f;

    private Rigidbody2D rb;
    private bool isAttacking;
    private Vector2 patrolCenter;

    void Awake()
    {
        if (GetComponent<WaterInteractor>() == null)
        {
            gameObject.AddComponent<WaterInteractor>();
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        patrolCenter = transform.position;

        Vector3 pos = transform.position;
        pos.z = 1f;
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

        float distance = Vector2.Distance(
            transform.position, player.position
        );

        if (distance <= attackRange)
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

        float distance = Vector2.Distance(rb.position, player.position);

        // Hanya kejar jika dalam range deteksi tetapi di luar range serang
        if (distance <= followRange && distance > attackRange)
        {
            Vector2 dir = ((Vector2)player.position - rb.position).normalized;
            rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);

            if (dir.x > 0.01f) transform.localScale = new Vector3(1, 1, 1);
            else if (dir.x < -0.01f) transform.localScale = new Vector3(-1, 1, 1);
        }
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

    // ─── PATROL ────────────────────────────────────────────
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

            float dist = Vector2.Distance(
                transform.position, player.position
            );

            if (dist <= followRange)
            {
                yield return null;
                continue;
            }

            Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;
            Vector2 target = patrolCenter + randomOffset;

            float maxPatrolTime = 3f;
            float patrolTimer = 0f;

            while (Vector2.Distance(transform.position, target) > 0.1f)
            {
                if (player == null || !player.gameObject.activeInHierarchy)
                    break;

                if (Vector2.Distance(
                    transform.position, player.position) <= followRange)
                    break;

                patrolTimer += Time.fixedDeltaTime;
                if (patrolTimer >= maxPatrolTime)
                    break;

                Vector2 dir = (
                    target - (Vector2)transform.position
                ).normalized;

                rb.MovePosition(
                    rb.position + dir * (moveSpeed * 0.6f) * Time.fixedDeltaTime
                );

                if (dir.x > 0) transform.localScale = new Vector3(1, 1, 1);
                else if (dir.x < 0) transform.localScale = new Vector3(-1, 1, 1);

                yield return new WaitForFixedUpdate();
            }

            float wait = Random.Range(patrolWaitMin, patrolWaitMax);
            yield return new WaitForSeconds(wait);
        }
    }

    // ─── ATTACK ────────────────────────────────────────────
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
                player.position - transform.position
            ).normalized;

            if (animator != null)
            {
                animator.SetTrigger("Attack");
                yield return new WaitForSeconds(0.12f);
                animator.speed = 0f;
                yield return new WaitForSeconds(0.6f);
                animator.speed = 1f;
            }

            if (attackHitbox != null)
            {
                float frontOffset = Mathf.Max(0.8f, attackRange * 0.5f);
                attackHitbox.transform.localPosition = new Vector3(frontOffset, 0f, 0f);
                attackHitbox.SetActive(true);
            }

            float lungeSpeed    = 10f;
            float lungeDuration = 0.15f;
            float lungeTimer    = 0f;

            while (lungeTimer < lungeDuration)
            {
                rb.MovePosition(
                    rb.position + lungeDir * lungeSpeed * Time.fixedDeltaTime
                );
                lungeTimer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            if (attackHitbox != null)
                attackHitbox.SetActive(false);

            yield return new WaitForSeconds(0.6f);
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
