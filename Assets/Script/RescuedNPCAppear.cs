using UnityEngine;

// Pasang script ini ke NPC di scene tujuan (misal Hutan).
// NPC mulai inactive, muncul kalau sudah diselamatkan di scene lain.
public class RescuedNPCAppear : MonoBehaviour
{
    public string rescueId;

    void Awake()
    {
        bool rescued = RescueManager.instance != null && RescueManager.instance.IsRescued(rescueId);
        if (!rescued) gameObject.SetActive(false);
    }
}
