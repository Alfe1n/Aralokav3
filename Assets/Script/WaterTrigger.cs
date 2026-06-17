using UnityEngine;

// ================================================================
// WaterTrigger — ADAPTASI untuk Aralokav3
// ================================================================
// PERUBAHAN dari versi teman:
// 1. Movement → PlayerMovement (Aralokav3 standard)
// ================================================================

public class WaterTrigger : MonoBehaviour
{
    [Header("Water Settings")]
    [Tooltip("Seberapa jauh Y player turun saat masuk air")]
    public float sinkAmount = 0.2f;

    [Tooltip("Multiplier kecepatan di dalam air (misal: 0.5f = setengah kecepatan)")]
    [Range(0.1f, 0.9f)]
    public float speedMultiplier = 0.5f;

    private float originalSpeed = -1f;
    private bool isInWater = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (isInWater) return;

        isInWater = true;

        // Pakai PlayerMovement (Aralokav3) bukan Movement
        PlayerMovement mov = collision.GetComponent<PlayerMovement>();
        if (mov != null)
        {
            originalSpeed = mov.moveSpeed;
            mov.moveSpeed *= speedMultiplier;
        }

        Vector3 pos = collision.transform.position;
        pos.y -= sinkAmount;
        collision.transform.position = pos;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (!isInWater) return;

        isInWater = false;

        // Pakai PlayerMovement (Aralokav3)
        PlayerMovement mov = collision.GetComponent<PlayerMovement>();
        if (mov != null && originalSpeed > 0)
        {
            mov.moveSpeed = originalSpeed;
            originalSpeed = -1f;
        }

        Vector3 pos = collision.transform.position;
        pos.y += sinkAmount;
        collision.transform.position = pos;
    }
}
