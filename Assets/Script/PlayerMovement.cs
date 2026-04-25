using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Tooltip("Centang jika Bara hanya boleh bergerak Atas/Bawah/Kiri/Kanan (tidak bisa diagonal)")]
    public bool strict4WayMovement = false;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Mengambil komponen Animator jika nanti sudah ditambahkan
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // GetAxisRaw menghasilkan nilai -1, 0, atau 1 secara instan
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Logika Strict 4-Directional
        // Memprioritaskan sumbu X jika tombol ditekan bersamaan agar tidak jalan diagonal
        if (strict4WayMovement && movement.x != 0)
        {
            movement.y = 0;
        }

        // Normalisasi agar pergerakan diagonal tidak lebih cepat dari pergerakan lurus
        movement = movement.normalized;

        UpdateAnimation();
    }

    void FixedUpdate()
    {
        // Mengaplikasikan pergerakan ke Rigidbody2D
        rb.linearVelocity = movement * moveSpeed;
    }

    void UpdateAnimation()
    {
        if (anim != null)
        {
            if (movement != Vector2.zero)
            {
                // Menyimpan arah gerak terakhir untuk mengatur arah menghadap (idle)
                anim.SetFloat("MoveX", movement.x);
                anim.SetFloat("MoveY", movement.y);
                anim.SetBool("IsMoving", true);
            }
            else
            {
                anim.SetBool("IsMoving", false);
            }
        }
    }
}