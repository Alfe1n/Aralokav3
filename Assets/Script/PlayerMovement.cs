using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [HideInInspector]
    public bool canMove = true;

    [Header("Footstep Audio")]
    public AudioSource footstepSource;
    public AudioClip[] footstepClips;

    public float footstepDelay = 0.4f;

    private float footstepTimer;

    private Animator animator;

    private float h;
    private float v;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleInput();
        Move();
        Animate();
        HandleFootstep();
    }

    void HandleInput()
    {
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
        Vector3 move = new Vector3(h, v, 0f);

        // biar diagonal ga lebih cepat
        if (move.magnitude > 1f)
            move.Normalize();

        transform.position += move * moveSpeed * Time.deltaTime;
    }

    void Animate()
    {
        bool isMoving = (h != 0 || v != 0);

        animator.SetBool("IsMoving", isMoving);

        if (isMoving)
        {
            Vector2 dir = new Vector2(h, v).normalized;

            animator.SetFloat("moveX", dir.x);
            animator.SetFloat("moveY", dir.y);
        }
    }

    void HandleFootstep()
    {
        // kalau ga bisa gerak → ga ada footstep
        if (!canMove) return;

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
}