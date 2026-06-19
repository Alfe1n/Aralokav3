using UnityEngine;

// ================================================================
// ArenaTrigger — ADAPTASI untuk Aralokav3
// ================================================================
// PERUBAHAN dari versi teman:
// 1. Hapus dependency Unity.Cinemachine
// 2. Diganti dengan aktivasi GameObject langsung (opsional)
// 3. Jika kamu pakai Cinemachine, tambahkan manual setelah install
// ================================================================

public class ArenaTrigger : MonoBehaviour
{
    [Header("Camera (Opsional)")]
    [Tooltip("Assign GameObject VCam_Locked jika pakai Cinemachine")]
    public GameObject vcamLocked;

    [Header("Arena")]
    public ArenaManager arenaManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Player-Orang Utan")) return;

        // Cek Quest jika ada restriction
        if (arenaManager != null && arenaManager.useQuest && QuestManager.Instance != null)
        {
            if (QuestManager.Instance.CurrentQuest != arenaManager.requiredQuest)
                return;
        }

        // Aktifkan camera lock (jika pakai Cinemachine)
        GameObject lockedCam = vcamLocked != null ? vcamLocked : CameraRegister.lockedCamera;
        GameObject normalCam = CameraRegister.normalCamera;

        if (lockedCam != null)
            lockedCam.SetActive(true);
        if (normalCam != null)
            normalCam.SetActive(false);

        // Jika ada ArenaManager, aktifkan wallNorth dan aktifkan ArenaManager itu sendiri
        if (arenaManager != null)
        {
            if (arenaManager.wallNorth != null)
                arenaManager.wallNorth.SetActive(true);

            arenaManager.enabled = true;

            // Picu EnemySpawner
            EnemySpawner[] spawners = arenaManager.GetComponentsInChildren<EnemySpawner>(true);
            foreach (EnemySpawner spawner in spawners)
            {
                if (!spawner.spawnOnStart)
                    spawner.SpawnEnemies();
            }

            // Picu HutanSpawner (ular, buaya, dll)
            foreach (HutanSpawner hs in arenaManager.hutanSpawners)
            {
                if (hs != null && !hs.spawnOnStart)
                    hs.SpawnAll();
            }
        }

        // Matikan trigger agar tidak trigger ulang
        gameObject.SetActive(false);
    }
}
