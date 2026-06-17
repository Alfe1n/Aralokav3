using UnityEngine;
using System.Collections;

public class Knockback : MonoBehaviour
{
    public Rigidbody2D rb;
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    public bool isKnockedBack { get; private set; } = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void ApplyKnockback(Vector2 sourcePosition)
    {
        if (isKnockedBack) return;
        StartCoroutine(KnockbackRoutine(sourcePosition));
    }

    IEnumerator KnockbackRoutine(Vector2 sourcePosition)
    {
        isKnockedBack = true;

        Vector2 direction = (
            (Vector2)transform.position - sourcePosition
        ).normalized;

        rb.AddForce(
            direction * knockbackForce,
            ForceMode2D.Impulse
        );

        yield return new WaitForSeconds(knockbackDuration);

        // reset velocity supaya tidak terus bergerak
        rb.linearVelocity = Vector2.zero;

        isKnockedBack = false;
    }
}
