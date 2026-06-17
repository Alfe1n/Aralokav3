using UnityEngine;
using System.Collections;

public class BuayaAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject attackHitbox;
    public Animator animator;

    [Header("Detection")]
    public float detectRange = 6f;     // jarak buaya mulai lurk / mengintai
    public float attackRange = 1.2f;   // jarak gigit

    [Header("Movement")]
    public float patrolSpeed = 1f;
    public float chargeSpeed = 7f;

    [Header("Patrol")]
    public float patrolRadius = 4f;
    public float patrolWaitMin = 2f;
    public float patrolWaitMax = 4f;

    [Header("Freeze")]
    public bool ignoreFreeze = false;

    [Header("Timing")]
    public float chargeWindup = 0.4f;  // jeda sebelum charge (tension)
    public float chargeDuration = 0.8f;
    public float recoverTime = 1.2f;
    public float attackDuration = 0.5f;

    enum State { Patrol, Lurk, Charge, Attack, Recover }
    State state = State.Patrol;

    private Rigidbody2D rb;
    private Vector2 patrolCenter;
    private PlayerMovement playerMovement;

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
        animator = animator != null ? animator : GetComponent<Animator>();
        patrolCenter = transform.position;

        if (player == null || player == transform || player.GetComponent<PlayerMovement>() == null || !player.gameObject.activeInHierarchy)
        {
            FindActivePlayer();
        }
        else
        {
            playerMovement = player.GetComponent<PlayerMovement>();
        }

        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        StartCoroutine(PatrolRoutine());
    }

    void Update()
    {
        if (EnemyAI.globalFreeze && !ignoreFreeze) return;

        // Auto-update player reference if it's null or inactive (e.g. character switched)
        if (player == null || !player.gameObject.activeInHierarchy || playerMovement == null)
        {
            FindActivePlayer();
            if (player == null || playerMovement == null) return;
        }

        float dist = Vector2.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Patrol:
                if (dist <= detectRange)
                    state = State.Lurk;
                break;

            case State.Lurk:
                rb.linearVelocity = Vector2.zero;
                FacePlayer();

                if (dist > detectRange)
                {
                    state = State.Patrol;
                }
                else if (playerMovement != null && playerMovement.isInWater)
                {
                    StartCoroutine(ChargeRoutine());
                }
                break;
        }
        // State Charge, Attack, Recover dihandle coroutine
    }

    // ── PATROL ──────────────────────────────────────────────────
    IEnumerator PatrolRoutine()
    {
        while (true)
        {
            if (EnemyAI.globalFreeze && !ignoreFreeze) { yield return null; continue; }
            if (state != State.Patrol)
            {
                yield return null;
                continue;
            }

            Vector2 target = patrolCenter + Random.insideUnitCircle * patrolRadius;
            float wait = Random.Range(patrolWaitMin, patrolWaitMax);

            float maxPatrolTime = 4f;
            float patrolTimer = 0f;

            while (Vector2.Distance(transform.position, target) > 0.2f)
            {
                if (state != State.Patrol) break;

                patrolTimer += Time.fixedDeltaTime;
                if (patrolTimer >= maxPatrolTime)
                    break;

                Vector2 dir = (target - (Vector2)transform.position).normalized;
                rb.MovePosition(rb.position + dir * patrolSpeed * Time.fixedDeltaTime);
                Flip(dir.x);
                yield return new WaitForFixedUpdate();
            }

            yield return new WaitForSeconds(wait);
        }
    }

    // ── CHARGE ──────────────────────────────────────────────────
    IEnumerator ChargeRoutine()
    {
        state = State.Charge;

        // mulai animasi attack sejak windup supaya visual sinkron saat kena
        animator.SetBool("IsAttack", true);

        // windup — diam sebentar sebelum meluncur
        yield return new WaitForSeconds(chargeWindup);

        // arah charge dikunci saat mulai (tidak tracking)
        Vector2 chargeDir = (player.position - transform.position).normalized;
        float timer = 0f;

        while (timer < chargeDuration)
        {
            // kalau player keluar air saat charge → berhenti
            if (playerMovement == null || !playerMovement.isInWater)
            {
                rb.linearVelocity = Vector2.zero;
                animator.SetBool("IsAttack", false);
                state = State.Lurk;
                yield break;
            }

            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= attackRange)
            {
                StartCoroutine(AttackRoutine());
                yield break;
            }

            rb.MovePosition(rb.position + chargeDir * chargeSpeed * Time.fixedDeltaTime);
            Flip(chargeDir.x);

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // charge meleset → recover
        animator.SetBool("IsAttack", false);
        StartCoroutine(RecoverRoutine());
    }

    // ── ATTACK ──────────────────────────────────────────────────
    IEnumerator AttackRoutine()
    {
        state = State.Attack;
        rb.linearVelocity = Vector2.zero;

        // animasi sudah jalan sejak charge, langsung aktifkan hitbox
        if (attackHitbox != null)
        {
            float frontOffset = Mathf.Max(1.0f, attackRange * 0.5f);
            attackHitbox.transform.localPosition = new Vector3(frontOffset, 0f, 0f);
            attackHitbox.SetActive(true);
        }

        yield return new WaitForSeconds(attackDuration);

        if (attackHitbox != null) attackHitbox.SetActive(false);
        animator.SetBool("IsAttack", false);

        StartCoroutine(RecoverRoutine());
    }

    // ── RECOVER ─────────────────────────────────────────────────
    IEnumerator RecoverRoutine()
    {
        state = State.Recover;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(recoverTime);

        // setelah recover, lurk kalau player masih dekat
        float dist = Vector2.Distance(transform.position, player.position);
        state = dist <= detectRange ? State.Lurk : State.Patrol;
    }

    // ── HELPERS ─────────────────────────────────────────────────
    void Flip(float dirX)
    {
        if (dirX > 0.01f) transform.localScale = new Vector3(1, 1, 1);
        else if (dirX < -0.01f) transform.localScale = new Vector3(-1, 1, 1);
    }

    void FacePlayer()
    {
        if (player == null) return;
        float dirX = player.position.x - transform.position.x;
        Flip(dirX);
    }

    private void FindActivePlayer()
    {
        // Cari object aktif dengan tag Player-Orang Utan
        GameObject playerObj = GameObject.FindWithTag("Player-Orang Utan");
        if (playerObj != null && playerObj.activeInHierarchy)
        {
            player = playerObj.transform;
            playerMovement = playerObj.GetComponent<PlayerMovement>();
            return;
        }

        // Fallback ke tag Player
        playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null && playerObj.activeInHierarchy)
        {
            player = playerObj.transform;
            playerMovement = playerObj.GetComponent<PlayerMovement>();
            return;
        }

        // Fallback terakhir ke komponen PlayerMovement
        PlayerMovement pm = Object.FindFirstObjectByType<PlayerMovement>();
        if (pm != null && pm.gameObject.activeInHierarchy)
        {
            player = pm.transform;
            playerMovement = pm;
        }
        else
        {
            player = null;
            playerMovement = null;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
