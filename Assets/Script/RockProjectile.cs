using UnityEngine;
using System.Collections;

// RockProjectile — copy langsung, tidak ada dependency konflik
public class RockProjectile : MonoBehaviour
{
    public float speed = 8f;
    public float stunRadius = 1f;
    public LayerMask enemyLayer;

    [Header("Damage (Opsional)")]
    public int damage = 1;

    private Vector2 startPos;
    private Vector2 endPos;
    private float arcHeight;
    private float t = 0f;
    private bool flying = false;

    public void Init(Vector2 from, Vector2 to, float arc)
    {
        startPos  = from;
        endPos    = to;
        arcHeight = arc;
        flying    = true;
    }

    void Update()
    {
        if (!flying) return;

        t += Time.deltaTime * speed /
            Mathf.Max(0.01f, Vector2.Distance(startPos, endPos));
        t = Mathf.Clamp01(t);

        // Ikuti jalur arc
        Vector2 pos = Vector2.Lerp(startPos, endPos, t);
        pos.y += arcHeight * Mathf.Sin(t * Mathf.PI);
        transform.position = pos;

        // Scale: besar di tengah, kecil di ujung (efek 3D)
        float scale = 0.5f + Mathf.Sin(t * Mathf.PI) * 0.5f;
        transform.localScale = Vector3.one * scale;

        if (t >= 1f) Land();
    }

    void Land()
    {
        flying = false;

        // Cek enemy dalam radius dan terapkan damage
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            endPos, stunRadius, enemyLayer
        );

        foreach (var hit in hits)
        {
            Health hp = hit.GetComponent<Health>();
            if (hp != null) hp.TakeDamage(damage);

            Knockback knockback = hit.GetComponent<Knockback>();
            if (knockback != null)
                knockback.ApplyKnockback(endPos);

            Debug.Log("Batu kena: " + hit.name);
        }

        Destroy(gameObject, 1f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(endPos, stunRadius);
    }
}
