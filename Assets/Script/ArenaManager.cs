using UnityEngine;
using System.Collections;

// ================================================================
// ArenaManager — ADAPTASI untuk Aralokav3
// ================================================================
// PERUBAHAN dari versi teman:
// 1. Movement → PlayerMovement (Aralokav3 standard)
// 2. Hapus dependency Unity.Cinemachine — diganti dengan flag
//    sederhana (vcamNormal/vcamLocked tetap ada tapi opsional)
// 3. DialogueManager.instance sudah kompatibel (versi Aralokav3)
// ================================================================

public class ArenaManager : MonoBehaviour
{
    [Header("Setup")]
    public GameObject[] enemies;
    public HutanSpawner[] hutanSpawners;
    public GameObject wallNorth;

    [Header("Dialog Setelah Arena")]
    public DialogueLine[] afterArenaLines;

    // ── Cinemachine (opsional — assign jika pakai Cinemachine) ──
    // Jika tidak pakai Cinemachine, biarkan kosong.
    // Install Cinemachine via Package Manager jika ingin digunakan.
    [Header("Camera (Opsional — butuh Cinemachine)")]
    public GameObject vcamNormal;
    public GameObject vcamLocked;

    [Header("Quest Update (Optional)")]
    public bool useQuest = false;
    public int requiredQuest = -1;
    public int nextQuest = -1;

    private bool arenaCleared = false;

    void Start()
    {
        // Cari apakah ada ArenaTrigger yang mengarah ke kita
        ArenaTrigger[] triggers = Object.FindObjectsByType<ArenaTrigger>(FindObjectsSortMode.None);
        foreach (ArenaTrigger trigger in triggers)
        {
            if (trigger != null && trigger.arenaManager == this)
            {
                enabled = false;
                break;
            }
        }
    }

    void Update()
    {
        if (arenaCleared) return;

        bool hasEnemies = (enemies != null && enemies.Length > 0) ||
                          (hutanSpawners != null && hutanSpawners.Length > 0);
        if (!hasEnemies) return;

        if (AllEnemiesDead())
        {
            arenaCleared = true;
            enabled = false;
            StartCoroutine(OnArenaCleared());
        }
    }

    bool AllEnemiesDead()
    {
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null && enemy.activeInHierarchy)
                return false;
        }
        foreach (HutanSpawner spawner in hutanSpawners)
        {
            if (spawner != null && !spawner.IsAllDead())
                return false;
        }
        return true;
    }

    IEnumerator OnArenaCleared()
    {
        yield return new WaitForSeconds(0.5f);

        // Lock player via PlayerMovement (Aralokav3)
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null) player.canMove = false;

        // Sembunyikan objective quest selama dialog akhir
        if (QuestManager.Instance != null)
            QuestManager.Instance.HideObjective();

        // Kembalikan ke kamera normal jika pakai Cinemachine
        GameObject normalCam = vcamNormal != null ? vcamNormal : CameraRegister.normalCamera;
        GameObject lockedCam = vcamLocked != null ? vcamLocked : CameraRegister.lockedCamera;

        if (normalCam != null) normalCam.SetActive(true);
        if (lockedCam != null) lockedCam.SetActive(false);

        // Tunggu kamera settle
        yield return new WaitForSeconds(1f);

        // Buka WallNorth
        if (wallNorth != null)
            wallNorth.SetActive(false);

        // Dialogue otomatis via DialogueManager Aralokav3
        if (afterArenaLines != null && afterArenaLines.Length > 0)
        {
            if (DialogueManager.instance != null)
            {
                DialogueManager.instance.StartDialogue(afterArenaLines);

                while (
                    DialogueManager.instance != null &&
                    DialogueManager.instance.IsDialogueActive()
                )
                    yield return null;
            }
        }

        // Unlock player
        if (player != null) player.canMove = true;

        // Update atau Tampilkan kembali objective quest
        if (useQuest && QuestManager.Instance != null && nextQuest >= 0)
        {
            QuestManager.Instance.SetQuest(nextQuest);
        }
        
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ShowObjective();
        }

        Debug.Log("ARENA CLEARED");
    }
}
