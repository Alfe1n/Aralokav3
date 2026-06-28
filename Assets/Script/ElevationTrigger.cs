using UnityEngine;

// ElevationTrigger — copy langsung, tidak ada dependency konflik
public class ElevationTrigger : MonoBehaviour
{
    [Header("Elevation Settings")]
    [Tooltip("Target layer fisika saat Bara menyentuh trigger ini (misal: 'Level_Middle')")]
    public string targetPhysicsLayer  = "Level_Middle";
    public string defaultPhysicsLayer = "Level_Ground";

    [Tooltip("Nilai Z target (misal: 1)")]
    public float targetZPosition = 1f;

    [Tooltip("Dorongan visual ke atas/bawah pada sumbu Y agar Bara terlihat naik blok")]
    public float visualYOffset = 0.25f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") && !collision.CompareTag("Player-Orang Utan")) return;

        collision.gameObject.layer =
            LayerMask.NameToLayer(targetPhysicsLayer);

        Vector3 newPos = collision.transform.position;
        newPos.z = targetZPosition;
        newPos.y += visualYOffset;
        collision.transform.position = newPos;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") && !collision.CompareTag("Player-Orang Utan")) return;

        collision.gameObject.layer =
            LayerMask.NameToLayer(defaultPhysicsLayer);

        Vector3 newPos = collision.transform.position;
        newPos.y -= visualYOffset;
        collision.transform.position = newPos;
    }
}
