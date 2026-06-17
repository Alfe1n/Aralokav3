using UnityEngine;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    [Header("Transition Target")]
    [Tooltip("Nama Scene tujuan (misal: Kamar Bara)")]
    public string targetScene;
    [Tooltip("Nama titik spawn di scene tujuan (misal: Spawn_Utama)")]
    public string targetSpawnPoint;
    [Tooltip("Centang jika ingin mengubah quest saat transisi berjalan")]
    public bool useTransitionQuest = true;
    [Tooltip("Setel quest ke angka ini setelah pindah scene")]
    public int transitionQuest = -1;

    [Header("Quest Requirements (Opsional)")]
    public bool useQuest;
    [Tooltip("Pemain hanya bisa pindah jika quest saat ini SAMA DENGAN angka di bawah")]
    public int requiredQuest = -1;
    [Tooltip("Pemain hanya bisa pindah jika quest saat ini LEBIH ATAU SAMA DENGAN angka di bawah")]
    public int requiredMinQuest = -1;
    
    [Header("Blocked Event (Jika syarat quest tidak terpenuhi)")]
    [Tooltip("Pesan dialog yang muncul jika pemain dilarang lewat (Misal: Aku harus menyelesaikan tugas di sini dulu)")]
    public DialogueLine[] blockedDialogueLines;

    private bool isTransitioning = false;

    private void Awake()
    {
        // Fitur Anti-Lupa: Otomatis menambahkan BoxCollider2D jika belum ada
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            BoxCollider2D box = gameObject.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            Debug.LogWarning($"[SceneTransition] BoxCollider2D otomatis ditambahkan di '{gameObject.name}'. Jangan lupa atur ukurannya!");
        }
        else if (!col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"[SceneTransition] Collider di '{gameObject.name}' belum dicentang 'Is Trigger'. Dicentang otomatis!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTransitioning) return;
        if (TransitionManager.Instance != null && TransitionManager.Instance.isTransitioning) return;

        if (other.CompareTag("Player") || other.CompareTag("Player-Orang Utan"))
        {
            // Cek Syarat Quest
            if (useQuest && QuestManager.Instance != null)
            {
                bool questMet = true;
                
                if (requiredQuest >= 0 && QuestManager.Instance.CurrentQuest != requiredQuest)
                    questMet = false;
                    
                if (requiredMinQuest >= 0 && QuestManager.Instance.CurrentQuest < requiredMinQuest)
                    questMet = false;

                if (!questMet)
                {
                    // Tampilkan dialog pemblokiran jika ada
                    if (blockedDialogueLines != null && blockedDialogueLines.Length > 0 && DialogueManager.instance != null)
                    {
                        if (!DialogueManager.instance.IsDialogueActive())
                        {
                            DialogueManager.instance.StartDialogue(blockedDialogueLines);
                            
                            // Dorong player sedikit ke belakang agar tidak terjebak terus menerus menabrak trigger
                            PlayerMovement pm = other.GetComponent<PlayerMovement>();
                            if (pm != null)
                            {
                                Vector3 pushDirection = (pm.transform.position - transform.position).normalized;
                                if (pushDirection.sqrMagnitude < 0.01f) pushDirection = Vector3.down; // fallback
                                pm.transform.position += pushDirection * 0.5f;
                            }
                        }
                    }
                    return;
                }
            }

            // Mulai Transisi
            isTransitioning = true;
            
            // Kunci pergerakan player
            PlayerMovement activePlayer = other.GetComponent<PlayerMovement>();
            if (activePlayer != null)
            {
                activePlayer.canMove = false;
            }

            Debug.Log($"[SceneTransition] Memulai transisi dari {gameObject.name} menuju {targetScene} di {targetSpawnPoint}");

            if (TransitionManager.Instance != null)
            {
                int questToSend = useTransitionQuest ? transitionQuest : -1;
                TransitionManager.Instance.StartTransition(targetScene, targetSpawnPoint, questToSend);
            }
            else
            {
                Debug.LogError("[SceneTransition] TransitionManager.Instance tidak ditemukan di scene!");
            }
        }
    }
}
