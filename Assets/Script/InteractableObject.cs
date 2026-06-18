using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class InteractableObject : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueLine[] dialogueLines;

    [Header("Quest")]
    public bool useQuest;
    public int requiredQuest = -1;
    public int nextQuest = -1;
    public bool useFadeBeforeQuestAdvance = false;

    [Header("Quest Range (Optional)")]
    public bool useQuestRange = false;
    public int minQuest = -1;
    public int maxQuest = -1;

    [Header("Cutscene")]
    public bool useCutscene;
    public GameObject cutscenePanel;
    public VideoPlayer videoPlayer;
    public RawImage rawImageVideo;
    public GameObject cgImage;

    [Header("Scene Transition")]
    public bool useSceneTransition;
    public string targetScene;
    public string targetSpawnPoint;
    public int transitionQuest = -1;
    public bool transitionFade = true;

    [Header("Condition - Spawner Cleared")]
    public bool requireSpawnerCleared = false;
    public HutanSpawner[] requiredSpawners;

    [Header("Item Pickup")]
    public bool giveItem = false;
    public string itemId;
    public bool destroyAfterPickup = true;

    [Header("Item Requirement")]
    public bool requireItem = false;
    public string requiredItemId;
    public bool removeItemOnUse = false;

    [Header("Additional Objects")]
    public GameObject additionalObjectToDestroy;

    [Header("Custom Action")]
    public UnityEvent onInteractComplete;

    [Header("Rescue NPC")]
    public bool isRescueInteraction = false;
    public string rescueId;
    public GameObject npcVisual;        // visual NPC yang hilang (null = sembunyikan renderer di object ini)
    public FadeUI fader;
    public GameObject crystalToActivate; // crystal yang muncul setelah hewan diselamatkan

    [Header("Crystal Cutscene")]
    public bool isCrystalCutscene = false;
    public DialogueLine[] postCutsceneLines; // monolog setelah video selesai

    [Header("Persistence")]
    [Tooltip("Centang jika objek/dialog ini hanya boleh dipicu satu kali saja seumur hidup (disimpan permanen)")]
    public bool triggerOnce = false;
    public bool triggerOncePersistent = false;
    [Tooltip("ID unik untuk menyimpan status trigger ini. Kosongkan untuk menggunakan nama scene + nama GameObject secara otomatis.")]
    public string customPersistentKey;

    private PlayerMovement playerMovement;
    private bool playerInside = false;
    private bool isInteracting = false;
    private Collider2D col;
    private bool hasTriggered = false;

    void Start()
    {
        if (triggerOncePersistent)
        {
            string key = string.IsNullOrEmpty(customPersistentKey) ? ("interact_triggered_" + gameObject.scene.name + "_" + gameObject.name) : customPersistentKey;
            if (PlayerPrefs.GetInt(key, 0) == 1)
            {
                hasTriggered = true;
                this.enabled = false;

                // Only disable collider if no other active and untriggered interactables exist on this GameObject
                bool anyActiveLeft = false;
                InteractableObject[] allInteractables = GetComponents<InteractableObject>();
                foreach (var io in allInteractables)
                {
                    if (io != this && io.enabled && !io.hasTriggered)
                    {
                        anyActiveLeft = true;
                        break;
                    }
                }
                if (!anyActiveLeft)
                {
                    Collider2D c = GetComponent<Collider2D>();
                    if (c != null) c.enabled = false;
                }
                return;
            }
        }

        playerMovement = FindFirstObjectByType<PlayerMovement>();
        col = GetComponent<Collider2D>();

        if (cutscenePanel != null) cutscenePanel.SetActive(false);
        if (rawImageVideo != null) rawImageVideo.gameObject.SetActive(false);
        if (cgImage != null) cgImage.SetActive(false);
    }

    void Update()
    {
        if (hasTriggered) return;

        // toggle collider berdasarkan kondisi spawner
        if (requireSpawnerCleared && col != null)
        {
            bool cleared = ConditionMet();
            if (col.enabled != cleared)
            {
                col.enabled = cleared;
                // kalau collider baru dimatikan saat player sudah di dalam
                if (!cleared && playerInside)
                {
                    playerInside = false;
                    if (DialogueManager.instance != null)
                        DialogueManager.instance.HidePrompt();
                }
            }
        }

        if (!playerInside) return;
        if (!IsInteractionAllowed()) return;
        if (DialogueManager.instance != null && DialogueManager.instance.IsDialogueActive()) return;
        if (isInteracting) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"[InteractableObject] E pressed on {gameObject.name}");
            StartCoroutine(BeginInteraction());
        }
    }

    public bool IsInteractionAllowed()
    {
        if (useQuest && QuestManager.Instance != null && QuestManager.Instance.CurrentQuest != requiredQuest)
            return false;

        if (useQuestRange && QuestManager.Instance != null)
        {
            if (minQuest >= 0 && QuestManager.Instance.CurrentQuest < minQuest)
                return false;
            if (maxQuest >= 0 && QuestManager.Instance.CurrentQuest > maxQuest)
                return false;
        }

        if (requireItem && (Inventory.instance == null || !Inventory.instance.HasItem(requiredItemId)))
            return false;

        if (requireSpawnerCleared)
        {
            foreach (HutanSpawner spawner in requiredSpawners)
            {
                if (spawner != null && !spawner.IsAllDead())
                    return false;
            }
        }

        return true;
    }

    bool ConditionMet()
    {
        if (useQuest && QuestManager.Instance != null && QuestManager.Instance.CurrentQuest != requiredQuest)
            return false;

        if (requireSpawnerCleared)
        {
            foreach (HutanSpawner spawner in requiredSpawners)
            {
                if (spawner != null && !spawner.IsAllDead())
                    return false;
            }
        }

        return true;
    }

    private FadeUI GetActiveFader()
    {
        if (fader != null) return fader;
        if (FadeUI.instance != null) return FadeUI.instance;

        FadeUI[] faders = Resources.FindObjectsOfTypeAll<FadeUI>();
        if (faders != null && faders.Length > 0)
        {
            foreach (FadeUI f in faders)
            {
                if (!string.IsNullOrEmpty(f.gameObject.scene.name))
                {
                    FadeUI.instance = f;
                    return f;
                }
            }
        }
        return null;
    }

    IEnumerator BeginInteraction()
    {
        isInteracting = true;

        Debug.Log($"INTERACT : {gameObject.name}");

        if (DialogueManager.instance != null) DialogueManager.instance.HidePrompt();
        
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null) playerMovement.canMove = false;

        if (QuestManager.Instance != null && !useSceneTransition)
        {
            QuestManager.Instance.HideObjective();
        }

        // === CRYSTAL CUTSCENE MODE ===
        if (isCrystalCutscene)
        {
            yield return StartCoroutine(CrystalCutsceneSequence());
            if (playerMovement != null) playerMovement.canMove = true;
            isInteracting = false;
            Destroy(gameObject);
            yield break;
        }

        // === CUTSCENE (mode biasa — langsung tanpa fade) ===
        if (useCutscene)
        {
            if (cutscenePanel != null) cutscenePanel.SetActive(true);
            if (cgImage != null) cgImage.SetActive(false);
            if (rawImageVideo != null) rawImageVideo.gameObject.SetActive(false); // Jangan aktifkan dulu untuk cegah glitch

            if (videoPlayer != null)
            {
                videoPlayer.enabled = true;
                videoPlayer.Prepare();

                // Tunggu sampai video siap
                while (!videoPlayer.isPrepared)
                {
                    yield return null;
                }

                // Setelah siap, baru aktifkan rawImageVideo
                if (rawImageVideo != null) rawImageVideo.gameObject.SetActive(true);
                videoPlayer.Play();

                yield return new WaitUntil(() => videoPlayer.isPlaying);
                yield return new WaitUntil(() => !videoPlayer.isPlaying);

                videoPlayer.Stop();
            }

            if (rawImageVideo != null) rawImageVideo.gameObject.SetActive(false);
            if (cgImage != null) cgImage.SetActive(true);
        }

        // === DIALOGUE ===
        if (dialogueLines != null && dialogueLines.Length > 0 && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(dialogueLines);
            while (DialogueManager.instance.IsDialogueActive())
                yield return null;
        }

        // === ITEM PICKUP ===
        if (giveItem && !string.IsNullOrEmpty(itemId))
        {
            if (Inventory.instance != null)
                Inventory.instance.AddItem(itemId);
        }

        // === ITEM CONSUMPTION ===
        if (requireItem && removeItemOnUse && !string.IsNullOrEmpty(requiredItemId))
        {
            if (Inventory.instance != null)
                Inventory.instance.RemoveItem(requiredItemId);
        }

        // === QUEST ADVANCE ===
        if (useQuest && nextQuest >= 0)
        {
            if (useFadeBeforeQuestAdvance)
            {
                FadeUI activeFader = FadeUI.instance;
                if (activeFader == null) activeFader = FindFirstObjectByType<FadeUI>();

                if (activeFader != null)
                {
                    yield return StartCoroutine(activeFader.FadeOut());
                    yield return new WaitForSeconds(0.5f);
                }

                if (QuestManager.Instance != null)
                    QuestManager.Instance.SetQuest(nextQuest);

                yield return new WaitForSeconds(0.5f);

                if (activeFader != null)
                {
                    yield return StartCoroutine(activeFader.FadeIn());
                }
            }
            else
            {
                if (QuestManager.Instance != null)
                    QuestManager.Instance.SetQuest(nextQuest);
            }
        }

        // === RESCUE STATE REGISTER (BEFORE TRANSITION) ===
        if (useSceneTransition && isRescueInteraction && !string.IsNullOrEmpty(rescueId))
        {
            if (RescueManager.instance != null)
                RescueManager.instance.Rescue(rescueId);
        }

        // === SCENE TRANSITION ===
        if (useSceneTransition)
        {
            if (additionalObjectToDestroy != null)
            {
                Destroy(additionalObjectToDestroy);
            }
            onInteractComplete?.Invoke();

            TransitionManager.Instance.StartTransition(
                targetScene,
                targetSpawnPoint,
                transitionQuest,
                transitionFade
            );
            yield break;
        }

        // === CUSTOM ACTION ===
        onInteractComplete?.Invoke();

        // === ADDITIONAL OBJECT DESTRUCTION ===
        if (additionalObjectToDestroy != null)
        {
            Destroy(additionalObjectToDestroy);
        }

        // === RESCUE SEQUENCE ===
        if (isRescueInteraction && !string.IsNullOrEmpty(rescueId))
        {
            if (RescueManager.instance != null)
                RescueManager.instance.Rescue(rescueId);

            FadeUI activeFader = GetActiveFader();

            // fade ke hitam
            if (activeFader != null)
                yield return StartCoroutine(activeFader.FadeOut());

            // sembunyikan NPC saat layar hitam — jangan SetActive karena bisa matiin coroutine
            GameObject hideTarget = npcVisual != null ? npcVisual : gameObject;
            foreach (var r in hideTarget.GetComponentsInChildren<Renderer>())
                r.enabled = false;

            yield return new WaitForSeconds(0.5f);

            // aktifkan crystal saat layar masih hitam — muncul natural saat fade in
            if (crystalToActivate != null)
                crystalToActivate.SetActive(true);

            // fade balik ke game
            if (activeFader != null)
                yield return StartCoroutine(activeFader.FadeIn());
        }

        // === CLEANUP ===
        if (cgImage != null) cgImage.SetActive(false);
        if (cutscenePanel != null) cutscenePanel.SetActive(false);
 
        if (playerMovement != null) playerMovement.canMove = true;
 
        isInteracting = false;
 
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ShowObjective();
        }
 
        if (triggerOnce)
        {
            hasTriggered = true;
            this.enabled = false;
            if (DialogueManager.instance != null) DialogueManager.instance.HidePrompt();

            // Only disable collider if no other active and untriggered interactables exist on this GameObject
            bool anyActiveLeft = false;
            InteractableObject[] allInteractables = GetComponents<InteractableObject>();
            foreach (var io in allInteractables)
            {
                if (io != this && io.enabled && !io.hasTriggered)
                {
                    anyActiveLeft = true;
                    break;
                }
            }
            if (!anyActiveLeft)
            {
                Collider2D c = GetComponent<Collider2D>();
                if (c != null) c.enabled = false;
            }
        }
        else if (triggerOncePersistent)
        {
            hasTriggered = true;
            string key = string.IsNullOrEmpty(customPersistentKey) ? ("interact_triggered_" + gameObject.scene.name + "_" + gameObject.name) : customPersistentKey;
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
            
            this.enabled = false;
            if (DialogueManager.instance != null) DialogueManager.instance.HidePrompt();

            // Only disable collider if no other active and untriggered interactables exist on this GameObject
            bool anyActiveLeft = false;
            InteractableObject[] allInteractables = GetComponents<InteractableObject>();
            foreach (var io in allInteractables)
            {
                if (io != this && io.enabled && !io.hasTriggered)
                {
                    anyActiveLeft = true;
                    break;
                }
            }
            if (!anyActiveLeft)
            {
                Collider2D c = GetComponent<Collider2D>();
                if (c != null) c.enabled = false;
            }
        }
        else if (isRescueInteraction || (giveItem && destroyAfterPickup))
        {
            Destroy(gameObject);
        }
    }

    // Flow: pre-dialogue → fade hitam → video muncul → fade terang →
    //        video selesai → fade hitam → game kembali → fade terang → post-dialogue
    IEnumerator CrystalCutsceneSequence()
    {
        if (isRescueInteraction && !string.IsNullOrEmpty(rescueId))
        {
            if (RescueManager.instance != null)
                RescueManager.instance.Rescue(rescueId);
        }

        // 1. Monolog sebelum cutscene (opsional)
        if (dialogueLines != null && dialogueLines.Length > 0 && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(dialogueLines);
            while (DialogueManager.instance.IsDialogueActive())
                yield return null;
        }

        FadeUI activeFader = GetActiveFader();

        // 2. Fade ke hitam
        if (activeFader != null)
            yield return StartCoroutine(activeFader.FadeOut());

        // 3. Tampilkan panel video saat layar masih hitam
        if (cutscenePanel != null) cutscenePanel.SetActive(true);
        if (rawImageVideo != null) rawImageVideo.gameObject.SetActive(true);

        if (videoPlayer != null)
        {
            videoPlayer.enabled = true;
            videoPlayer.Stop();
            videoPlayer.frame = 0;
            videoPlayer.Play();
            yield return new WaitUntil(() => videoPlayer.isPlaying);
        }

        // 4. Fade in — video terlihat
        if (activeFader != null)
            yield return StartCoroutine(activeFader.FadeIn());

        // 5. Tunggu video selesai
        if (videoPlayer != null)
            yield return new WaitUntil(() => !videoPlayer.isPlaying);

        // 6. Fade ke hitam lagi
        if (activeFader != null)
            yield return StartCoroutine(activeFader.FadeOut());

        // 7. Sembunyikan panel + crystal saat layar hitam
        if (videoPlayer != null) videoPlayer.Stop();
        if (rawImageVideo != null) rawImageVideo.gameObject.SetActive(false);
        if (cutscenePanel != null) cutscenePanel.SetActive(false);

        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        // 8. Fade balik ke game
        if (activeFader != null)
            yield return StartCoroutine(activeFader.FadeIn());

        // 9. Monolog setelah cutscene (opsional)
        if (postCutsceneLines != null && postCutsceneLines.Length > 0 && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(postCutsceneLines);
            while (DialogueManager.instance.IsDialogueActive())
                yield return null;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        if (!enabled) return;

        Debug.Log($"[InteractableObject] OnTriggerEnter2D by: {other.name} with tag {other.tag}");

        if (!other.CompareTag("Player") && !other.CompareTag("Player-Orang Utan"))
        {
            Debug.Log($"[InteractableObject] Tag rejected: {other.tag}");
            return;
        }

        playerInside = true;
        Debug.Log($"[InteractableObject] playerInside = true for {gameObject.name}");

        if (!IsInteractionAllowed())
            return;

        if (DialogueManager.instance != null && !DialogueManager.instance.IsDialogueActive())
            DialogueManager.instance.ShowPrompt();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!enabled) return;
        if (!other.CompareTag("Player") && !other.CompareTag("Player-Orang Utan"))
            return;

        playerInside = false;

        if (DialogueManager.instance != null)
            DialogueManager.instance.HidePrompt();
    }
}