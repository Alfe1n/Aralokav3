using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

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
    }

    void HandleInput()
    {
        // Raw biar responsif (no smoothing input)
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");
    }

    void Move()
    {
        Vector3 move = new Vector3(h, v, 0f);

        // 🔥 Normalize biar diagonal ga lebih cepat
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
        else
        {
            // 🔥 Idle default: HADAP BAWAH
            animator.SetFloat("moveX", 0);
            animator.SetFloat("moveY", -1);
        }
    }
}