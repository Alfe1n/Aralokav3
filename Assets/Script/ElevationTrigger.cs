using UnityEngine;

public class ElevationTrigger : MonoBehaviour
{
    [Header("Elevation Settings")]
    [Tooltip("Target layer fisika saat Bara menyentuh trigger ini (misal: 'Level_Middle')")]
    public string targetPhysicsLayer = "Level_Middle";

    [Tooltip("Nilai Z target (Sesuai dengan Z pada Grid Isometric kamu, misal: 1)")]
    public float targetZPosition = 1f;

    [Tooltip("Dorongan visual ke atas/bawah pada sumbu Y agar Bara terlihat naik blok. Sesuaikan dengan tinggi tile kamu (misal: 0.25f atau 0.5f)")]
    public float visualYOffset = 0.25f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Pastikan objek Bara memiliki Tag "Player"
        if (collision.CompareTag("Player"))
        {
            // 1. Ubah Physics Layer agar tidak menabrak blok di layer yang salah
            collision.gameObject.layer = LayerMask.NameToLayer(targetPhysicsLayer);

            // 2. Sesuaikan posisi Y (Visual Naik/Turun) dan Z (Sorting Isometric)
            Vector3 newPos = collision.transform.position;
            newPos.z = targetZPosition;
            newPos.y += visualYOffset;

            collision.transform.position = newPos;
        }
    }
}