using UnityEngine;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections;

/// <summary>
/// DecisionManager: handles choice UI (existing behavior) and then runs a configurable
/// ending sequence (images, dialogues, videos, final black text) per ending choice.
/// Uses the existing `DialogueManager` for all dialogue and `TransitionManager` for scene transitions.
/// Configure sequences from the Inspector.
/// </summary>
public class DecisionManager : MonoBehaviour
{
    public static DecisionManager instance;

    [Header("Canvas")]
    public GameObject decisionCanvas;

    [Header("Fade")]
    public FadeUI fader;

    [Header("Dialog setelah pilih manusia (index 0)")]
    public DialogueLine[] manusiaLines;

    [Header("Dialog setelah pilih harimau (index 1)")]
    public DialogueLine[] harimauLines;

    [Header("Posisi player setelah memilih")]
    public Transform spawnDekatManusia;
    public Transform spawnDekatHarimau;

    [Header("Animator karakter (objek di scene)")]
    public Animator harimauAnimator;
    public Animator manusiaAnimator;
    public string harimauBerdiriTrigger = "Berdiri";



    // --- Ending sequence configuration (set these in Inspector) ---
    [System.Serializable]
    public class SequenceStep
    {
        public Sprite image;                 // optional: shown on `sequenceImage`
        public DialogueLine[] lines;         // optional: DialogueManager will run these
        public VideoClip videoClip;          // optional: played by a VideoPlayer on sequenceRawImage
        public float displayDuration = 2f;   // optional: after image+dialogue/video, wait this long (if > 0)
        public bool showBlackText = false;   // special step to show a black fullscreen text
        [TextArea(1, 4)]
        public string blackText;             // used if showBlackText == true
        public float blackTextDuration = 3f;
    }

    [System.Serializable]
    public class EndingSequence
    {
        public SequenceStep[] steps = new SequenceStep[0];
        [Header("After sequence -> transition")]
        public string nextScene = "";
        public string nextSpawn = "";
        public int nextQuest = -1;
        public bool useFade = true;
    }

    [Header("Ending Sequences")]
    public EndingSequence badEnding;
    public EndingSequence goodEnding;

    // UI components for sequence playback (assign in Inspector)
    [Header("Sequence UI (assign from Inspector)")]
    public GameObject sequencePanel;       // fullscreen panel to show image/video/text
    public Image sequenceImage;           // for showing images
    public RawImage sequenceRawImage;     // for showing video render texture
    public VideoPlayer sequenceVideoPlayer; // VideoPlayer used for playback
    public TMP_Text blackText;            // for final black text (set color background as needed)

    private bool choiceMade = false;

    void Awake()
    {
        instance = this;
        if (decisionCanvas != null) decisionCanvas.SetActive(false);
        if (sequencePanel != null) sequencePanel.SetActive(false);

        // Make sure VideoPlayer doesn't auto-play
        if (sequenceVideoPlayer != null)
        {
            sequenceVideoPlayer.playOnAwake = false;
            sequenceVideoPlayer.isLooping = false;
            sequenceVideoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        }
    }

    private FadeUI GetActiveFader()
    {
        if (fader != null) return fader;
        if (FadeUI.instance != null) return FadeUI.instance;
        FadeUI[] faders = Resources.FindObjectsOfTypeAll<FadeUI>();
        foreach (FadeUI f in faders)
        {
            if (f.gameObject.scene.name != null)
            {
                FadeUI.instance = f;
                return f;
            }
        }
        return null;
    }

    public IEnumerator ShowDecision()
    {
        // Sembunyikan GUI OrangUtan selama cutscene pilihan berlangsung
        OrangUtanUIVisibility.Instance?.ForceHide();
        if (QuestManager.Instance != null) QuestManager.Instance.HideObjective();

        // 1. Fade ke hitam
        FadeUI activeFader = GetActiveFader();
        if (activeFader != null)
            yield return StartCoroutine(activeFader.FadeOut());
        else
            Debug.LogWarning("[DecisionManager] No active fader found for FadeOut!");

        // 2. Aktifkan canvas saat layar masih hitam
        if (decisionCanvas != null) decisionCanvas.SetActive(true);

        // If a CustomCursor exists, enable it and hide the native cursor. Otherwise keep native cursor visible.
        CustomCursor custom = FindObjectOfType<CustomCursor>();
        if (custom != null)
        {
            custom.SetVisible(true);
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = true;
        }

        // 3. Tunggu video siap (prepare + play selesai) on decision background (optional)
        DecisionVideoBackground videoBg = decisionCanvas.GetComponentInChildren<DecisionVideoBackground>();
        if (videoBg != null)
            yield return new WaitUntil(() => videoBg.IsReady);

        // tunggu 2 frame ekstra supaya RenderTexture sudah ada isinya
        yield return null;
        yield return null;

        // 4. Baru fade in — semua sudah siap, tidak ada blink
        if (activeFader != null)
            yield return StartCoroutine(activeFader.FadeIn());
        else
            Debug.LogWarning("[DecisionManager] No active fader found for FadeIn!");

        // 5. Tunggu player pilih
        choiceMade = false;
        yield return new WaitUntil(() => choiceMade);
    }

    public void SelectChoice(int index)
    {
        if (choiceMade) return;
        choiceMade = true;
        StartCoroutine(ProcessChoice(index));
    }

    IEnumerator ProcessChoice(int index)
    {
        // Save choice so other systems / continue logic can read it
        PlayerPrefs.SetInt("PlayerChoice", index);
        PlayerPrefs.Save();

        // 6. Dialog hasil pilihan — canvas MASIH TERLIHAT
        DialogueLine[] lines = index == 0 ? manusiaLines : harimauLines;
        if (lines != null && lines.Length > 0 && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(lines);
            yield return null; // wait 1 frame for dialogue to initialize
            yield return new WaitUntil(() => !DialogueManager.instance.IsDialogueActive());
        }

        // 7. Teleport player saat canvas masih menutupi game scene
        GameObject player = null;
        if (PlayerMovement.ActivePlayerInstance != null)
        {
            player = PlayerMovement.ActivePlayerInstance.gameObject;
        }
        else
        {
            player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                player = GameObject.FindWithTag("Player-Orang Utan");
            }
        }

        if (player != null)
        {
            Transform target = index == 0 ? spawnDekatManusia : spawnDekatHarimau;
            if (target != null)
            {
                Vector3 pos = target.position;
                pos.z = 1f;
                player.transform.position = pos;
                Debug.Log($"[DecisionManager] Teleported player '{player.name}' to position: {pos}");
            }
            else
            {
                Debug.LogWarning($"[DecisionManager] Teleport target transform is null for choice index {index}!");
            }
        }
        else
        {
            Debug.LogWarning("[DecisionManager] Player GameObject not found for teleportation!");
        }

        // 8. Baru fade ke hitam dan sembunyikan canvas
        FadeUI activeFader = GetActiveFader();
        if (activeFader != null)
            yield return StartCoroutine(activeFader.FadeOut());

        if (decisionCanvas != null) decisionCanvas.SetActive(false);

        // Disable custom cursor if present (we'll control native cursor below)
        CustomCursor custom = FindObjectOfType<CustomCursor>();
        if (custom != null)
        {
            custom.SetVisible(false);
        }

        // Only hide native cursor if a sequence panel will be shown immediately
        if (sequencePanel != null)
        {
            sequencePanel.SetActive(true);
            Cursor.visible = false;
        }
        else
        {
            // keep cursor visible so player sees pointer after making choice
            Cursor.visible = true;
        }

        // 8b. Ganti animasi karakter saat layar masih hitam
        if (index == 1)
        {
            // Pilih harimau -> harimau selamat (berdiri), manusia mati (animasi berhenti)
            if (harimauAnimator != null) harimauAnimator.SetTrigger(harimauBerdiriTrigger);
            if (manusiaAnimator != null) manusiaAnimator.enabled = false;
        }
        else
        {
            // Pilih manusia -> manusia tetap mati, harimau juga mati (animasi berhenti)
            if (manusiaAnimator != null) manusiaAnimator.enabled = false;
            if (harimauAnimator != null) harimauAnimator.enabled = false;
        }

        // 9. Fade balik ke game — player sudah di posisi baru
        if (activeFader != null)
            yield return StartCoroutine(activeFader.FadeIn());

        // Restore GUI OrangUtan dan Quest UI after this initial choice sequence (we will hide again if sequence needs it)
        OrangUtanUIVisibility.Instance?.ForceRefresh();
        if (QuestManager.Instance != null) QuestManager.Instance.ShowObjective();

        // ---- NEW: Run configured ending sequence for chosen ending ----
        EndingSequence seq = (index == 0) ? badEnding : goodEnding;
        if (seq != null)
        {
            // hide normal UI so sequence panel takes full attention
            OrangUtanUIVisibility.Instance?.ForceHide();
            if (QuestManager.Instance != null) QuestManager.Instance.HideObjective();

            yield return StartCoroutine(RunEndingSequence(seq));
        }
    }

    IEnumerator RunEndingSequence(EndingSequence seq)
    {
        if (sequencePanel == null)
        {
            Debug.LogWarning("[DecisionManager] sequencePanel not assigned in Inspector. Skipping ending sequence and running transition.");
            // fallback to transition even if sequence panel missing
            InvokeTransition(seq);
            yield break;
        }

        // Ensure sequence UI state
        sequencePanel.SetActive(true);
        if (sequenceImage != null) sequenceImage.gameObject.SetActive(false);
        if (sequenceRawImage != null) sequenceRawImage.gameObject.SetActive(false);
        if (blackText != null) { blackText.gameObject.SetActive(false); blackText.text = ""; }
        if (sequenceVideoPlayer != null)
        {
            sequenceVideoPlayer.Stop();
            sequenceVideoPlayer.clip = null;
        }

        // Play every step in order
        foreach (var step in seq.steps)
        {
            // 1) Image (with optional dialogue)
            if (step.image != null && sequenceImage != null)
            {
                sequenceRawImage?.gameObject.SetActive(false);
                sequenceImage.gameObject.SetActive(true);
                sequenceImage.sprite = step.image;
                sequenceImage.SetNativeSize();
            }

            // 2) Video (mutually exclusive with image for clarity)
            if (step.videoClip != null && sequenceVideoPlayer != null && sequenceRawImage != null)
            {
                sequenceImage?.gameObject.SetActive(false);
                sequenceRawImage.gameObject.SetActive(true);

                // Assign and play
                sequenceVideoPlayer.Stop();
                sequenceVideoPlayer.clip = step.videoClip;

                // If VideoPlayer has AudioSource, ensure it is playing (assign inspector if needed)
                sequenceVideoPlayer.Prepare();
                yield return new WaitUntil(() => sequenceVideoPlayer.isPrepared);
                sequenceVideoPlayer.Play();

                // Wait until finishes
                while (sequenceVideoPlayer.isPlaying)
                    yield return null;
            }

            // 3) Dialogue attached to this step
            if (step.lines != null && step.lines.Length > 0 && DialogueManager.instance != null)
            {
                DialogueManager.instance.StartDialogue(step.lines);
                yield return null;
                yield return new WaitUntil(() => !DialogueManager.instance.IsDialogueActive());
            }

            // 4) Black-screen text special step
            if (step.showBlackText && blackText != null)
            {
                // hide images/video
                sequenceImage?.gameObject.SetActive(false);
                sequenceRawImage?.gameObject.SetActive(false);

                blackText.gameObject.SetActive(true);
                blackText.text = step.blackText ?? "";
                yield return new WaitForSeconds(Mathf.Max(0.01f, step.blackTextDuration));
                blackText.gameObject.SetActive(false);
            }

            // 5) Optional display duration to let static image linger
            if (step.displayDuration > 0f)
            {
                yield return new WaitForSeconds(step.displayDuration);
            }
        }

        // Final black text if configured at sequence level
        if (blackText != null && !string.IsNullOrEmpty( (seq.steps != null && seq.steps.Length>0) ? "" : "" )) { /* keep for flexibility */ }

        // Hide sequence UI before transition (optionally keep black screen by using FadeUI)
        if (sequencePanel != null) sequencePanel.SetActive(false);

        // Perform transition using TransitionManager
        InvokeTransition(seq);
        yield break;
    }

    private void InvokeTransition(EndingSequence seq)
    {
        if (seq == null)
            return;

        if (!string.IsNullOrEmpty(seq.nextScene) && TransitionManager.Instance != null)
        {
            // Use TransitionManager to move to next scene, preserving quest if configured
            TransitionManager.Instance.StartTransition(
                seq.nextScene,
                seq.nextSpawn ?? "",
                seq.nextQuest,
                seq.useFade
            );
        }
    }
}
