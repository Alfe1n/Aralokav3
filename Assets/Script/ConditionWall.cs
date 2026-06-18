using UnityEngine;

// Pasang script ini ke WallArena yang perlu kondisi untuk aktif/nonaktif.
// Wall aktif jika kondisi AKTIF terpenuhi, dan nonaktif jika kondisi NONAKTIF terpenuhi.
public class ConditionWall : MonoBehaviour
{
    [Header("Aktif setelah rescue ini selesai")]
    public string activateAfterRescueId;

    [Header("Nonaktif setelah crystal Pancarona dikumpulkan")]
    public bool deactivateWhenCrystalCollected = false;
    public int requiredCrystalCount = 1;

    void Start()
    {
        RefreshState();
    }

    public void RefreshState()
    {
        gameObject.SetActive(ShouldBeActive());
    }

    bool ShouldBeActive()
    {
        // Belum rescue → wall belum perlu aktif
        if (!string.IsNullOrEmpty(activateAfterRescueId))
        {
            bool rescued = RescueManager.instance != null &&
                           RescueManager.instance.IsRescued(activateAfterRescueId);
            if (!rescued) return false;
        }

        // Sudah ambil crystal → wall buka
        if (deactivateWhenCrystalCollected)
        {
            int count = PlayerPrefs.GetInt("pancarona_crystal_count", 0);
            if (count >= requiredCrystalCount) return false;
        }

        return true;
    }
}
