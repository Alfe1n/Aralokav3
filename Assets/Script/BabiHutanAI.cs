using UnityEngine;
using System.Collections;

public class BabiHutanAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject attackHitbox;
    public Animator animator;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float followRange = 8f;
    public float attackRange = 3f;

    [Header("Chase")]
    public float chaseUpdateInterval = 2f;

    [Header("Patrol")]
    public float patrolRadius = 3f;
    public float patrolWaitMin = 1f;
    public float patrolWaitMax = 3f;

    [Header("Charge Attack")]
    public float chargeUpTime    = 1.5f;
    public float lungeStartSpeed = 1f;
    public float lungeEndSpeed   = 18f;
    public float lungeDuration   = 1.2f;
    public float decelerateTime  = 0.4f;
    public float recoverTime     = 1.5f;

    private Rigidbody2D rb;
    private bool isAttacking;
    private bool isChasing;
    private Vector2 patrolCenter;
    private Vector2 chaseTarget;
    private float chaseTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        patrolCenter = transform.position;

        if (player == null || player == transform || player.GetComponent<PlayerMovement>() == null || !player.gameObject.activeInHierarchy)
            FindActivePlayer();

        if (player != null)
            chaseTarget = player.position;

        StartCoroutine(PatrolRoutine());
        StartCoroutine(ChaseRoutine());
    }

    void Update()
    {
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            FindActivePlayer();
            if (player == null) return;
        }

        if (isAttacking) return;

        float distance = Vector2.Distance(
            transform.position, player.position
        );

        if (distance <= attackRange && !isAttacking)
        {
            isChasing = false;
            StartCoroutine(ChargeAttack());
        }
        else if (distance <= followRange)
        {
            isChasing = true;
        }
        else
        {
            isChasing = false;
        }
    }

    IEnumerator ChaseRoutine()
    {
        while (true)
        {
            if (player == null || !player.gameObject.activeInHierarchy)
            {
                FindActivePlayer();
            }

            if (isChasing && !isAttacking && player != null && player.gameObject.activeInHierarchy)
            {
                chaseTimer += Time.deltaTime;
                if (chaseTimer >= chaseUpdateInterval)
                {
                    chaseTarget = player.position;
                    chaseTimer = 0f;
                }

                Vector2 dir = (
                    chaseTarget - (Vector2)transform.position
                ).normalized;

                if ((chaseTarget - (Vector2)transform.position).magnitude > 0.1f)
                {
                    rb.MovePosition(
                        rb.position + dir * moveSpeed * Time.fixedDeltaTime
                    );
                }

                if (dir.x > 0)
                    transform.localScale = new Vector3(1, 1, 1);
                else if (dir.x < 0)
                    transform.localScale = new Vector3(-1, 1, 1);
            }

            yield return new WaitForFixedUpdate();
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

    IEnumerator PatrolRoutine()
    {
        while (true)
        {
            if (isChasing || isAttacking)
            {
                yield return null;
                continue;
            }

            Vector2 randomOffset =
                Random.insideUnitCircle * patrolRadius;
            Vector2 target = patrolCenter + randomOffset;

            float maxPatrolTime = 3f;
            float patrolTimer = 0f;

            while (Vector2.Distance(transform.position, target) > 0.1f)
            {
                if (isChasing || isAttacking) break;

                patrolTimer += Time.fixedDeltaTime;
                if (patrolTimer >= maxPatrolTime)
                    break;

                Vector2 dir = (
                    target - (Vector2)transform.position
                ).normalized;

                rb.MovePosition(
                    rb.position + dir * (moveSpeed * 0.6f) * Time.fixedDeltaTime
                );

                if (dir.x > 0)
                    transform.localScale = new Vector3(1, 1, 1);
                else if (dir.x < -0.01f)
                    transform.localScale = new Vector3(-1, 1, 1);

                yield return new WaitForFixedUpdate();
            }

            float wait = Random.Range(patrolWaitMin, patrolWaitMax);
            yield return new WaitForSeconds(wait);
        }
    }

    IEnumerator ChargeAttack()
    {
        isAttacking = true;

        try
        {
            if (player == null || !player.gameObject.activeInHierarchy)
                FindActivePlayer();

            Vector2 lockedTarget = (player != null) ? player.position : (Vector2)transform.position;

            if (animator != null)
            {
                animator.SetTrigger("Attack");
                yield return new WaitForSeconds(0.05f);
                animator.speed = 0f;
            }

            // FASE CHARGE UP
            float chargeTimer = 0f;
            while (chargeTimer < chargeUpTime)
            {
                chargeTimer += Time.deltaTime;

                Vector2 dir = lockedTarget - (Vector2)transform.position;
                if (dir.x > 0)
                    transform.localScale = new Vector3(1, 1, 1);
                else if (dir.x < 0)
                    transform.localScale = new Vector3(-1, 1, 1);

                yield return null;
            }

            // FASE LUNGE
            if (animator != null)
                animator.speed = 1f;

            Vector2 lungeDir = (
                lockedTarget - (Vector2)transform.position
            ).normalized;

            if (attackHitbox != null)
            {
                float frontOffset = Mathf.Max(1.0f, attackRange * 0.5f);
                attackHitbox.transform.localPosition = new Vector3(frontOffset, 0f, 0f);
                attackHitbox.SetActive(true);
            }

            float lungeTimer = 0f;
            while (lungeTimer < lungeDuration)
            {
                float t = lungeTimer / lungeDuration;
                float currentSpeed = Mathf.Lerp(lungeStartSpeed, lungeEndSpeed, t);

                rb.MovePosition(
                    rb.position + lungeDir * currentSpeed * Time.fixedDeltaTime
                );

                lungeTimer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            if (attackHitbox != null)
                attackHitbox.SetActive(false);

            // FASE DECELERATE
            float decelerateTimer = 0f;
            while (decelerateTimer < decelerateTime)
            {
                float t = decelerateTimer / decelerateTime;
                float currentSpeed = Mathf.Lerp(lungeEndSpeed, 0f, t);

                rb.MovePosition(
                    rb.position + lungeDir * currentSpeed * Time.fixedDeltaTime
                );

                decelerateTimer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            // FASE RECOVER
            if (animator != null)
            {
                animator.speed = 0f;
                yield return new WaitForSeconds(recoverTime);
                animator.speed = 1f;
            }
            else
            {
                yield return new WaitForSeconds(recoverTime);
            }
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
