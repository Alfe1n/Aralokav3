using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement ActivePlayerInstance { get; private set; }

    [Header("Movement")]
    public float moveSpeed = 5f;

    [HideInInspector]
    public bool canMove = true;

    [HideInInspector]
    public bool isInWater = false;

    [Header("Footstep Audio")]
    public AudioSource footstepSource;
    public AudioClip[] footstepClips;

    public float footstepDelay = 0.4f;

    private float footstepTimer;

    private Animator animator;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Knockback knockback;

    private float h;
    private float v;

    void Awake()
    {
        // Auto-attach WaterInteractor for decentralized water handling
        if (GetComponent<WaterInteractor>() == null)
        {
            gameObject.AddComponent<WaterInteractor>();
        }
    }

    void OnEnable()
    {
        ActivePlayerInstance = this;
    }

    void OnDisable()
    {
        if (ActivePlayerInstance == this)
        {
            ActivePlayerInstance = null;
        }
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        knockback = GetComponent<Knockback>();
    }

    void Update()
    {
        HandleInput();
        Move();
        Animate();
        HandleFootstep();

        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log(
                "canMove = " + canMove
            );
        }
    }

    void HandleInput()
    {
        if (autoMoveTarget != null)
        {
            Vector2 currentPos = (Vector2)transform.position;
            Vector2 direction = (Vector2)autoMoveTarget.position - currentPos;
            float distance = direction.magnitude;

            // Tambahkan timer timeout
            autoMoveTimeoutTimer += Time.deltaTime;

            // Periksa apakah posisi tidak berubah (stagnan)
            if (Vector2.Distance(currentPos, lastPosition) < STAGNATION_THRESHOLD)
            {
                stagnationTimer += Time.deltaTime;
            }
            else
            {
                stagnationTimer = 0f;
                lastPosition = currentPos;
            }

            // Jika sampai, terhalang collider (stagnan), atau mencapai batas waktu (timeout)
            if (distance <= autoMoveStopDistance || stagnationTimer >= STAGNATION_DURATION || autoMoveTimeoutTimer >= AUTOMOVE_TIMEOUT)
            {
                if (stagnationTimer >= STAGNATION_DURATION)
                {
                    Debug.Log($"[PlayerMovement] Auto-walk berhenti karena terhalang collider/stagnan. Sisa jarak: {distance}");
                }
                else if (autoMoveTimeoutTimer >= AUTOMOVE_TIMEOUT)
                {
                    Debug.LogWarning("[PlayerMovement] Auto-walk berhenti karena melebihi batas waktu (timeout).");
                }

                autoMoveTarget = null;
                h = 0;
                v = 0;
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                }
                var callback = onAutoMoveComplete;
                onAutoMoveComplete = null;
                callback?.Invoke();
            }
            else
            {
                direction.Normalize();
                h = direction.x;
                v = direction.y;
            }
            return;
        }

        // LOCK MOVEMENT SAAT DIALOGUE
        if (!canMove)
        {
            h = 0;
            v = 0;
            return;
        }

        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");
    }

    void Move()
    {
        // JANGAN override pergerakan jika sedang terkena efek knockback dari musuh
        if (knockback != null && knockback.isKnockedBack)
            return;

        Vector2 move = new Vector2(h, v);

        // biar diagonal ga lebih cepat
        if (move.magnitude > 1f)
            move.Normalize();

        if (rb != null)
        {
            rb.linearVelocity = move * moveSpeed;
        }
        else
        {
            transform.position += (Vector3)move * moveSpeed * Time.deltaTime;
        }
    }

    void Animate()
    {
        bool isMoving = (h != 0 || v != 0);

        animator.SetBool("IsMoving", isMoving);

        if (isMoving)
        {
            Vector2 dir = new Vector2(h, v).normalized;

            // Flip visual character ke Kiri atau Kanan (Sama seperti project Haerul)
            if (sr != null)
            {
                if (dir.x < 0) sr.flipX = true;
                else if (dir.x > 0) sr.flipX = false;
            }

            // Karena menggunakan flipX, kita harus memaksa moveX selalu positif (seperti project Haerul)
            // agar Blend Tree selalu membaca animasi hadap kanan, lalu SpriteRenderer yang membaliknya.
            animator.SetFloat("moveX", Mathf.Abs(dir.x));
            animator.SetFloat("moveY", dir.y);
        }
    }

    void HandleFootstep()
    {
        // kalau ga bisa gerak dan tidak sedang auto-walk → ga ada footstep
        if (!canMove && autoMoveTarget == null) return;

        bool isMoving = (h != 0 || v != 0);

        if (!isMoving) return;

        // countdown timer
        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0f)
        {
            if (footstepClips.Length > 0)
            {
                int randomIndex =
                    Random.Range(0, footstepClips.Length);

                footstepSource.PlayOneShot(
                    footstepClips[randomIndex]
                );
            }

            footstepTimer = footstepDelay;
        }
    }

    public void SetInWater(bool value)
    {
        isInWater = value;
        if (animator != null)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == "IsInWater")
                {
                    animator.SetBool("IsInWater", value);
                    break;
                }
            }
        }
    }

    // --- AUTO-MOVE (SCRIPTED WALK) METHODS ---
    public bool IsAutoMoving => autoMoveTarget != null;
    private Transform autoMoveTarget;
    private float autoMoveStopDistance = 0.1f;
    private System.Action onAutoMoveComplete;
    private Vector2 lastPosition;
    private float stagnationTimer = 0f;
    private float autoMoveTimeoutTimer = 0f;
    private const float STAGNATION_THRESHOLD = 0.01f;
    private const float STAGNATION_DURATION = 0.3f;
    private const float AUTOMOVE_TIMEOUT = 6.0f;

    public void WalkToTarget(Transform target, float stopDistance, System.Action onComplete)
    {
        autoMoveTarget = target;
        autoMoveStopDistance = stopDistance;
        onAutoMoveComplete = onComplete;
        canMove = false; // Kunci input manual

        // Inisialisasi pengecekan stagnasi & timeout
        lastPosition = transform.position;
        stagnationTimer = 0f;
        autoMoveTimeoutTimer = 0f;
    }

    public void StopAutoWalk()
    {
        autoMoveTarget = null;
        onAutoMoveComplete = null;
        h = 0;
        v = 0;
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    public IEnumerator WalkToTargetRoutine(Transform target, float stopDistance = 0.15f)
    {
        if (target == null) yield break;

        bool done = false;
        WalkToTarget(target, stopDistance, () => {
            done = true;
        });

        while (!done && autoMoveTarget == target)
        {
            yield return null;
        }
    }
}