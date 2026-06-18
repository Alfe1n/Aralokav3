using UnityEngine;
using System.Collections;

public class EventAreaTrigger : MonoBehaviour
{
    [Header("Event Settings")]
    [Tooltip("Pilih jika event ini hanya boleh terjadi satu kali")]
    public bool triggerOnce = true;
    [Tooltip("Centang jika butuh waktu tunggu sebelum event dimulai (misal untuk loading screen)")]
    public bool useInitialDelay = false;
    [Tooltip("Berapa detik menunggu sebelum event (kamera/dialog) dimulai")]
    public float initialDelayDuration = 2f;

    [Header("Fade")]
    [Tooltip("Fade ke hitam sebelum event dimulai")]
    public bool useFadeIn = false;
    [Tooltip("Fade balik terang setelah event selesai")]
    public bool useFadeOut = false;

    [Header("Camera Panning")]
    [Tooltip("Target objek yang akan disorot kamera saat event terjadi")]
    public Transform cameraTarget;
    [Tooltip("Berapa lama kamera menetap menyorot target (jika tidak ada dialog)")]
    public float cameraStayDuration = 2f;
    [Tooltip("Gunakan pergerakan kamera yang halus (Smooth Pan)")]
    public bool useSmoothPan = true;
    [Tooltip("Kecepatan pergerakan kamera saat bergeser (semakin kecil semakin lambat)")]
    public float panSpeed = 5f;

    [Header("Player Movement (Optional)")]
    [Tooltip("Target lokasi tempat player akan berjalan setelah kamera selesai menyorot")]
    public Transform playerWalkTarget;
    [Tooltip("Jarak toleransi henti saat player berjalan ke target")]
    public float playerWalkStopDistance = 0.5f;
    [Tooltip("Di baris dialog ke-berapa player berjalan ke target? (Isi -1 jika ingin player berjalan sebelum dialog dimulai)")]
    public int movePlayerAtDialogIndex = -1;

    [Header("Dialogue (Optional)")]
    public bool useDialogue = false;
    public DialogueLine[] dialogueLines;

    [Header("Camera Sync With Dialogue")]
    [Tooltip("Di baris dialog ke-berapa (mulai dari 0) kamera bergeser ke target? (0 = dari awal)")]
    public int moveCameraAtDialogIndex = 0;
    [Tooltip("Di baris dialog ke-berapa kamera kembali ke player? (Isi angka besar misal 99 jika ingin kembali otomatis saat dialog tamat)")]
    public int returnCameraAtDialogIndex = 99;

    [Header("Quest (Optional)")]
    public bool useQuest = false;
    public int requiredQuest = -1;
    public int nextQuest = -1;

    [Header("Persistence")]
    [Tooltip("Centang jika event ini hanya boleh terjadi satu kali seumur hidup (disimpan permanen)")]
    public bool triggerOncePersistent = false;
    [Tooltip("ID unik untuk menyimpan status trigger ini. Kosongkan untuk menggunakan nama scene + nama GameObject secara otomatis.")]
    public string customPersistentKey;

    private bool hasTriggered = false;
    private bool isEventRunning = false;

    private Coroutine activePanCoroutine;
    private GameObject activeCameraRig;

    private void Awake()
    {
        // Fitur Anti-Lupa: Otomatis menambahkan BoxCollider2D jika belum ada
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            BoxCollider2D box = gameObject.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            Debug.LogWarning($"[EventAreaTrigger] Halo! Saya menambahkan BoxCollider2D secara otomatis di '{gameObject.name}' karena kamu lupa memasangnya. Jangan lupa atur ukurannya ya!");
        }
        else if (!col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"[EventAreaTrigger] Collider di '{gameObject.name}' belum dicentang 'Is Trigger'. Saya centangkan otomatis ya!");
        }
    }

    private void Start()
    {
        if (triggerOncePersistent)
        {
            string key = string.IsNullOrEmpty(customPersistentKey) ? ("event_triggered_" + gameObject.scene.name + "_" + gameObject.name) : customPersistentKey;
            if (PlayerPrefs.GetInt(key, 0) == 1)
            {
                hasTriggered = true;
                Collider2D col = GetComponent<Collider2D>();
                if (col != null) col.enabled = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[EventAreaTrigger] '{gameObject.name}' collided with '{other.gameObject.name}' (Tag: {other.tag}). hasTriggered: {hasTriggered}, isEventRunning: {isEventRunning}");

        if (!other.CompareTag("Player") && !other.CompareTag("Player-Orang Utan")) return;
        if (hasTriggered && (triggerOnce || triggerOncePersistent)) return;
        if (isEventRunning) return;

        // Cek Quest jika ada restriction
        if (useQuest && QuestManager.Instance != null)
        {
            Debug.Log($"[EventAreaTrigger] '{gameObject.name}' checking quest: Current: {QuestManager.Instance.CurrentQuest}, Required: {requiredQuest}");
            if (QuestManager.Instance.CurrentQuest != requiredQuest)
            {
                Debug.Log($"[EventAreaTrigger] '{gameObject.name}' quest check failed: current {QuestManager.Instance.CurrentQuest} != required {requiredQuest}");
                return;
            }
        }

        Debug.Log($"[EventAreaTrigger] '{gameObject.name}' starting event coroutine!");
        PlayerMovement pm = other.GetComponent<PlayerMovement>();
        StartCoroutine(RunEventRoutine(pm));
    }

    private IEnumerator RunEventRoutine(PlayerMovement activePlayer)
    {
        isEventRunning = true;

        // Sembunyikan objective quest selama event berlangsung
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.HideObjective();
        }

        // 1. Kunci pergerakan player
        if (activePlayer != null)
        {
            activePlayer.canMove = false;
        }

        // Fade ke hitam sebelum event
        if (useFadeIn)
            yield return StartCoroutine(FadeUI.Out());

        // --- TAMBAHAN BARU: INITIAL DELAY ---
        if (useInitialDelay && initialDelayDuration > 0f)
        {
            yield return new WaitForSeconds(initialDelayDuration);
        }

        // 2. Simpan target kamera original (yaitu si player)
        Transform originalCameraTarget = activePlayer != null ? activePlayer.transform : null;

        // 3. Geser kamera ke target DULU (jika cameraTarget ditentukan)
        if (cameraTarget != null && originalCameraTarget != null)
        {
            yield return MoveCameraTo(originalCameraTarget, cameraTarget);
            // Beri jeda kecil setelah kamera selesai mem-pan sebelum langkah berikutnya
            yield return new WaitForSeconds(0.2f);
        }

        // 3.5. Jalankan pergerakan player ke tempat yang ditentukan SEBELUM DIALOG (jika indeks < 0)
        bool playerWalkStarted = false;
        if (playerWalkTarget != null && activePlayer != null && movePlayerAtDialogIndex < 0)
        {
            playerWalkStarted = true;
            yield return activePlayer.WalkToTargetRoutine(playerWalkTarget, playerWalkStopDistance);
            yield return new WaitForSeconds(0.2f);
        }

        // 4. Jalankan Dialog (jika ada)
        if (useDialogue && dialogueLines != null && dialogueLines.Length > 0 && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(dialogueLines);

            // Tunggu sampai dialog beres
            while (DialogueManager.instance.IsDialogueActive())
            {
                int currentIndex = DialogueManager.instance.GetCurrentIndex();

                // Cek apakah sudah saatnya player berjalan (jika indeks >= 0)
                if (!playerWalkStarted && playerWalkTarget != null && activePlayer != null && movePlayerAtDialogIndex >= 0 && currentIndex >= movePlayerAtDialogIndex)
                {
                    playerWalkStarted = true;
                    // Jalankan pergerakan secara async (tidak menghambat dialog)
                    StartCoroutine(activePlayer.WalkToTargetRoutine(playerWalkTarget, playerWalkStopDistance));
                }

                yield return null;
            }
        }
        else if (cameraTarget != null)
        {
            // Jika tidak ada dialog dan player belum jalan, jalankan sekarang
            if (!playerWalkStarted && playerWalkTarget != null && activePlayer != null)
            {
                playerWalkStarted = true;
                yield return activePlayer.WalkToTargetRoutine(playerWalkTarget, playerWalkStopDistance);
            }

            // Jika tidak ada dialog tapi ada cameraTarget, sorot selama durasi tertentu
            yield return new WaitForSeconds(cameraStayDuration);
        }

        // 4.5. Pastikan player sudah selesai berjalan sepenuhnya sebelum kamera kembali
        if (activePlayer != null && activePlayer.IsAutoMoving)
        {
            while (activePlayer.IsAutoMoving)
            {
                yield return null;
            }
            yield return new WaitForSeconds(0.2f);
        }

        // 5. Kembalikan kamera ke player dan tunggu sampai selesai
        if (originalCameraTarget != null && cameraTarget != null)
        {
            yield return MoveCameraTo(cameraTarget, originalCameraTarget);
            // Beri jeda kecil setelah kamera kembali agar transisi terasa alami sebelum dilepas
            yield return new WaitForSeconds(0.2f);
        }

        // Fade balik terang setelah event
        if (useFadeOut)
            yield return StartCoroutine(FadeUI.In());

        // 6. Lepaskan kunci pergerakan
        if (activePlayer != null)
        {
            activePlayer.canMove = true;
        }

        // Update atau Tampilkan kembali objective quest
        if (useQuest && QuestManager.Instance != null && nextQuest >= 0)
        {
            QuestManager.Instance.SetQuest(nextQuest);
        }
        
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ShowObjective();
        }

        isEventRunning = false;
        if (triggerOnce)
        {
            hasTriggered = true;
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }

        if (triggerOncePersistent)
        {
            hasTriggered = true;
            string key = string.IsNullOrEmpty(customPersistentKey) ? ("event_triggered_" + gameObject.scene.name + "_" + gameObject.name) : customPersistentKey;
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }

    private Coroutine MoveCameraTo(Transform fromTarget, Transform toTarget)
    {
        Vector3 currentPos = fromTarget != null ? fromTarget.position : transform.position;

        if (activePanCoroutine != null)
        {
            StopCoroutine(activePanCoroutine);
        }
        if (activeCameraRig != null)
        {
            currentPos = activeCameraRig.transform.position; // Lanjutkan dari posisi jika terputus di tengah jalan
            Destroy(activeCameraRig);
        }

        if (useSmoothPan)
        {
            activePanCoroutine = StartCoroutine(SmoothPanRoutine(currentPos, toTarget));
            return activePanCoroutine;
        }
        else
        {
            SetCinemachineTarget(toTarget);
            return null;
        }
    }

    private IEnumerator SmoothPanRoutine(Vector3 startPos, Transform toTarget)
    {
        activeCameraRig = new GameObject("TempCameraRig");
        activeCameraRig.transform.position = startPos;
        SetCinemachineTarget(activeCameraRig.transform);

        float distance = Vector3.Distance(startPos, toTarget.position);
        float duration = distance > 0.1f ? distance / panSpeed : 0.1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Vector3 endPos = toTarget.position; // Update terus jika targetnya bergerak (seperti player)
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            activeCameraRig.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        SetCinemachineTarget(toTarget);
        Destroy(activeCameraRig);
        activeCameraRig = null;
    }

    // Fungsi canggih untuk mengubah target Cinemachine tanpa perlu import namespace Cinemachine
    private void SetCinemachineTarget(Transform newTarget)
    {
        if (newTarget == null) return;

        MonoBehaviour[] allScripts = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
        foreach (var script in allScripts)
        {
            string scriptName = script.GetType().Name;
            // Pastikan kita hanya mengubah kamera utama yang sedang aktif di scene ini
            if (script.gameObject.activeInHierarchy && (scriptName == "CinemachineVirtualCamera" || scriptName == "CinemachineCamera"))
            {
                var followProp = script.GetType().GetProperty("Follow");
                if (followProp != null)
                {
                    followProp.SetValue(script, newTarget);
                }
                else
                {
                    // Untuk Cinemachine v3 ke atas
                    var targetProp = script.GetType().GetProperty("Target");
                    if (targetProp != null)
                    {
                        object targetStruct = targetProp.GetValue(script);
                        var trackingField = targetStruct.GetType().GetField("TrackingTarget");
                        if (trackingField != null)
                        {
                            trackingField.SetValue(targetStruct, newTarget);
                            targetProp.SetValue(script, targetStruct);
                        }
                    }
                }
            }
        }
    }
}
