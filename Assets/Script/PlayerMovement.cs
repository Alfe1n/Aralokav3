using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Animator animator;

    float h;
    float v;

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
        h = Input.GetAxisRaw("Horizontal"); // A D
        v = Input.GetAxisRaw("Vertical");   // W S
    }

    void Move()
    {
        Vector3 move = new Vector3(h, v, 0f).normalized;

        transform.position += move * moveSpeed * Time.deltaTime;
    }

    void Animate()
    {
        // 🔥 cek lagi gerak atau nggak
        bool isMoving = (h != 0 || v != 0);

        animator.SetBool("IsMoving", isMoving);

        if (isMoving)
        {
            // 🔥 PRIORITAS arah dominan (biar ga diagonal glitch)
            if (Mathf.Abs(h) > Mathf.Abs(v))
            {
                // Kiri / kanan
                animator.SetFloat("moveX", h);
                animator.SetFloat("moveY", 0);
            }
            else
            {
                // Atas / bawah
                animator.SetFloat("moveX", 0);
                animator.SetFloat("moveY", v);
            }
        }
        else
        {
            // 🔥 IDLE → paksa ke depan (bawah)
            animator.SetFloat("moveX", 0);
            animator.SetFloat("moveY", -1);
        }
    }
}