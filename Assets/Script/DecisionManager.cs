using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Rendering;
using TMPro;
using System.Collections;

/// <summary>
/// Alur ending Araloka:
///   1. DecisionTrigger → dialog pra-keputusan → ShowDecision()
///   2. Player pilih (SelectChoice) → dialog reaksi → fade balik ke game
///   3. Player interaksi Harimau/Manusia → EndingTrigger → TriggerGoodEnding/TriggerBadEnding
///   4. Dialog singkat (preEndingLines) → fade → RunEndingSequence
///   5. Tekan E/Space untuk lanjut tiap step (gambar/narator/video/teks)
/// </summary>
public class DecisionManager : MonoBehaviour
{
    public static DecisionManager instance;

    // ──────────────────────────────────────────────────────
    //  Data types
    // ──────────────────────────────────────────────────────

    [System.Serializable]
    public class SequenceStep
    {
        [Header("Konten step")]
        [Tooltip("Gambar CG yang ditampilkan (opsional)")]
        public Sprite image;
        [Tooltip("Video yang diputar (opsional)")]
        public VideoClip videoClip;

        [Header("Teks Narator (panel gelap, klik E/Space tiap baris)")]
        public string[] narratorLines;

        [Header("Teks Putih di Background Hitam")]
        public bool showBlackText = false;
        [TextArea(1, 4)]
        public string blackText;

        [Header("Navigasi")]
        [Tooltip("Tekan E/Space untuk lanjut ke step berikutnya (default true). Jika false, pakai displayDuration.")]
        public bool waitForInput = true;
        [Tooltip("Dipakai jika waitForInput = false")]
        public float displayDuration = 3f;

        [Header("Fade")]
        public bool fadeIn = false;
        public bool fadeOut = false;
    }

    [System.Serializable]
    public class EndingSequence
    {
        [Header("Dialog singkat saat interaksi (sebelum fade ke ending)")]
        public DialogueLine[] preEndingLines;

        [Header("Steps ending (gambar, narator, video, teks)")]
        public SequenceStep[] steps = new SequenceStep[0];

        [Header("Setelah sequence selesai")]
        public string nextScene = "MainMenu";
        public string nextSpawn = "";
        public int nextQuest = -1;
        public bool useFade = true;
    }

    // ──────────────────────────────────────────────────────
    //  Inspector fields
    // ──────────────────────────────────────────────────────

    [Header("Canvas Keputusan")]
    public GameObject decisionCanvas;

    [Header("Fade")]
    public FadeUI fader;

    [Header("Dialog reaksi setelah pilih (kembali ke game)")]
    public DialogueLine[] manusiaLines;
    public DialogueLine[] harimauLines;

    [Header("Posisi player setelah pilih")]
    public Transform spawnDekatManusia;
    public Transform spawnDekatHarimau;

    [Header("Animator karakter")]
    public Animator harimauAnimator;
    public Animator manusiaAnimator;
    public string harimauBerdiriTrigger = "Berdiri";

    [Header("Ending Sequences")]
    public EndingSequence badEnding;
    public EndingSequence goodEnding;

    [Header("Sequence UI")]
    public GameObject sequencePanel;
    public Image sequenceImage;
    public RawImage sequenceRawImage;
    public VideoPlayer sequenceVideoPlayer;
    public TMP_Text blackText;

    [Header("Narator Panel")]
    public NarratorDialogue narratorDialogue;

    [Header("Continue Hint (tampil saat tunggu E/Space di gambar/teks)")]
    [Tooltip("Objek UI berisi teks 'Tekan E / Space untuk melanjutkan'")]
    public GameObject sequenceContinueHint;

    // ──────────────────────────────────────────────────────
    //  Private state
    // ──────────────────────────────────────────────────────

    private bool choiceMade = false;
    private bool endingStarted = false;

    void Awake()
    {
        instance = this;
        if (decisionCanvas != null) decisionCanvas.SetActive(false);
        if (sequencePanel != null) sequencePanel.SetActive(false);
        if (sequenceContinueHint != null) sequenceContinueHint.SetActive(false);

        if (sequenceVideoPlayer != null)
        {
            sequenceVideoPlayer.playOnAwake = false;
            sequenceVideoPlayer.isLooping = false;
            
            // Cek jika ada AudioSource di objek VideoPlayer
            AudioSource videoAudio = sequenceVideoPlayer.GetComponent<AudioSource>();
            if (videoAudio != null)
            {
                sequenceVideoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                sequenceVideoPlayer.SetTargetAudioSource(0, videoAudio);
            }
            else
            {
                // Jika tidak ada AudioSource, keluarkan suara langsung ke speaker (Direct) agar tidak bisu
                sequenceVideoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            }
        }
    }

    // ──────────────────────────────────────────────────────
    //  1. Decision Screen
    // ──────────────────────────────────────────────────────

    /// <summary>Dipanggil oleh DecisionTrigger setelah dialog pra-keputusan.</summary>
    public IEnumerator ShowDecision()
    {
        OrangUtanUIVisibility.Instance?.ForceHide();
        if (QuestManager.Instance != null) QuestManager.Instance.HideObjective();

        FadeUI activeFader = GetActiveFader();
        if (activeFader != null)
            yield return StartCoroutine(activeFader.FadeOut());

        if (decisionCanvas != null) decisionCanvas.SetActive(true);

        // Pastikan cursor bisa bergerak bebas
        Cursor.lockState = CursorLockMode.None;

        CustomCursor custom = FindFirstObjectByType<CustomCursor>();
        bool hasCursorSprite = custom != null && custom.cursorImage != null && custom.cursorImage.sprite != null;
        if (hasCursorSprite)
        {
            custom.SetVisible(true);
            Cursor.visible = false;
        }
        else
        {
            if (custom != null) custom.SetVisible(false);
            Cursor.visible = true;
        }

        DecisionVideoBackground videoBg = decisionCanvas?.GetComponentInChildren<DecisionVideoBackground>();
        if (videoBg != null)
            yield return new WaitUntil(() => videoBg.IsReady);

        yield return null;

        if (activeFader != null)
            yield return StartCoroutine(activeFader.FadeIn());

        choiceMade = false;
        yield return new WaitUntil(() => choiceMade);
    }

    public void SelectChoice(int index)
    {
        if (choiceMade) return;
        choiceMade = true;
        StartCoroutine(ProcessChoice(index));
    }

    /// <summary>
    /// Setelah pilih: dialog reaksi → fade → kembali ke game.
    /// Player lanjut jalan ke karakter yang dipilih dan berinteraksi.
    /// </summary>
    IEnumerator ProcessChoice(int index)
    {
        PlayerPrefs.SetInt("PlayerChoice", index);
        PlayerPrefs.Save();

        // Dialog reaksi (masih di decision canvas)
        DialogueLine[] lines = index == 0 ? manusiaLines : harimauLines;
        if (lines != null && lines.Length > 0 && DialogueManager.instance != null)
        {
            // Sembunyikan cursor dulu saat dialog
            CustomCursor custom = FindFirstObjectByType<CustomCursor>();
            if (custom != null) custom.SetVisible(false);
            Cursor.visible = false;

            DialogueManager.instance.StartDialogue(lines);
            yield return null;
            yield return new WaitUntil(() => !DialogueManager.instance.IsDialogueActive());
        }

        FadeUI activeFader = GetActiveFader();
        if (activeFader != null)
            yield return StartCoroutine(activeFader.FadeOut());

        // Sembunyikan decision canvas
        if (decisionCanvas != null) decisionCanvas.SetActive(false);

        // Kursor game normal (tersembunyi)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        // Teleport player dekat karakter yang dipilih
        GameObject playerObj = null;
        if (PlayerMovement.ActivePlayerInstance != null)
            playerObj = PlayerMovement.ActivePlayerInstance.gameObject;
        else
        {
            playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null) playerObj = GameObject.FindWithTag("Player-Orang Utan");
        }

        if (playerObj != null)
        {
            Transform spawnTarget = index == 0 ? spawnDekatManusia : spawnDekatHarimau;
            if (spawnTarget != null)
            {
                Vector3 pos = spawnTarget.position;
                pos.z = 1f;
                playerObj.transform.position = pos;
            }
        }

        // Sembunyikan karakter yang tidak dipilih agar tidak bisa diinteraksi lagi
        GameObject manusiaObj = GameObject.Find("Manusia");
        GameObject harimauObj = GameObject.Find("Harimau");
        if (index == 0) // Pilih Manusia
        {
            if (harimauObj != null) harimauObj.SetActive(false);
        }
        else if (index == 1) // Pilih Harimau
        {
            if (manusiaObj != null) manusiaObj.SetActive(false);
        }

        // Animator karakter
        if (index == 1 && harimauAnimator != null)
            harimauAnimator.SetTrigger(harimauBerdiriTrigger);
        else if (index == 0 && manusiaAnimator != null)
            manusiaAnimator.enabled = false;

        yield return null;

        if (activeFader != null)
            yield return StartCoroutine(activeFader.FadeIn());

        // Kembalikan UI dan gerakan player
        OrangUtanUIVisibility.Instance?.ForceRefresh();
        if (QuestManager.Instance != null)
        {
            if (index == 0)
                QuestManager.Instance.SetCustomObjectiveText("Berbicara dengan Manusia");
            else
                QuestManager.Instance.SetCustomObjectiveText("Berbicara dengan Harimau");
        }

        PlayerMovement player = PlayerMovement.ActivePlayerInstance
            ?? FindFirstObjectByType<PlayerMovement>();
        if (player != null) player.canMove = true;
    }

    // ──────────────────────────────────────────────────────
    //  2. Ending Trigger (dari EndingTrigger / InteractableObject)
    // ──────────────────────────────────────────────────────

    public void TriggerGoodEnding()
    {
        if (endingStarted) return;
        if (goodEnding != null)
        {
            endingStarted = true;
            StartCoroutine(StartEndingSequence(goodEnding));
        }
        else
            Debug.LogWarning("[DecisionManager] goodEnding belum di-assign!");
    }

    public void TriggerBadEnding()
    {
        if (endingStarted) return;
        if (badEnding != null)
        {
            endingStarted = true;
            StartCoroutine(StartEndingSequence(badEnding));
        }
        else
            Debug.LogWarning("[DecisionManager] badEnding belum di-assign!");
    }

    IEnumerator StartEndingSequence(EndingSequence seq)
    {
        // Kunci player
        PlayerMovement player = PlayerMovement.ActivePlayerInstance
            ?? FindFirstObjectByType<PlayerMovement>();
        if (player != null) player.canMove = false;

        // Sembunyikan UI game
        OrangUtanUIVisibility.Instance?.ForceHide();
        if (QuestManager.Instance != null) QuestManager.Instance.HideObjective();

        // Dialog singkat sebelum fade (di scene, dengan karakter)
        if (seq.preEndingLines != null && seq.preEndingLines.Length > 0
            && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(seq.preEndingLines);
            yield return null;
            yield return new WaitUntil(() => !DialogueManager.instance.IsDialogueActive());
        }

        // Tunggu 2 frame agar InteractableObject selesai cleanup
        yield return null;
        yield return null;

        // Fade ke hitam sebelum sequence
        FadeUI activeFader = GetActiveFader();
        if (activeFader != null)
            yield return StartCoroutine(activeFader.FadeOut());

        yield return StartCoroutine(RunEndingSequence(seq));
    }

    // ──────────────────────────────────────────────────────
    //  3. Ending Sequence Runner
    // ──────────────────────────────────────────────────────

    IEnumerator RunEndingSequence(EndingSequence seq)
    {
        FadeUI activeFader = GetActiveFader();

        if (sequencePanel == null)
        {
            Debug.LogWarning("[DecisionManager] sequencePanel belum di-assign. Skip sequence.");
            InvokeTransition(seq);
            yield break;
        }

        // Aktifkan panel
        sequencePanel.SetActive(true);

        // Auto-find NarratorDialogue dari children SequencePanel
        if (narratorDialogue == null)
            narratorDialogue = sequencePanel.GetComponentInChildren<NarratorDialogue>(true);

        // Pastikan SequencePanel punya Image hitam sebagai background
        Image panelBg = sequencePanel.GetComponent<Image>();
        if (panelBg == null)
        {
            panelBg = sequencePanel.AddComponent<Image>();
            panelBg.raycastTarget = false;
        }
        panelBg.color = Color.black;

        // Fix SequenceBlackText agar full screen
        if (blackText != null)
        {
            RectTransform btRt = blackText.rectTransform;
            if (btRt.anchorMin != Vector2.zero || btRt.anchorMax != Vector2.one)
            {
                btRt.anchorMin = Vector2.zero;
                btRt.anchorMax = Vector2.one;
                btRt.offsetMin = new Vector2(80f, 80f);
                btRt.offsetMax = new Vector2(-80f, -80f);
            }
            var sg = blackText.gameObject.GetComponent<SortingGroup>();
            if (sg != null) Destroy(sg);
        }

        // Reset semua elemen
        if (sequenceImage != null) sequenceImage.gameObject.SetActive(false);
        if (sequenceRawImage != null) sequenceRawImage.gameObject.SetActive(false);
        if (blackText != null) { blackText.gameObject.SetActive(false); blackText.text = ""; }
        if (sequenceVideoPlayer != null) { sequenceVideoPlayer.Stop(); sequenceVideoPlayer.clip = null; }
        if (sequenceContinueHint != null) sequenceContinueHint.SetActive(false);

        foreach (var step in seq.steps)
        {
            // ── FADE IN sebelum konten ──
            if (step.fadeIn && activeFader != null)
                yield return StartCoroutine(activeFader.FadeIn());

            // ── Gambar CG ──
            if (step.image != null && sequenceImage != null)
            {
                sequenceRawImage?.gameObject.SetActive(false);
                if (blackText != null) blackText.gameObject.SetActive(false);
                sequenceImage.gameObject.SetActive(true);
                sequenceImage.sprite = step.image;
            }

            // ── Video ──
            if (step.videoClip != null && sequenceVideoPlayer != null && sequenceRawImage != null)
            {
                sequenceImage?.gameObject.SetActive(false);
                if (blackText != null) blackText.gameObject.SetActive(false);
                sequenceRawImage.gameObject.SetActive(true);

                sequenceVideoPlayer.Stop();
                sequenceVideoPlayer.clip = step.videoClip;
                sequenceVideoPlayer.Prepare();
                yield return new WaitUntil(() => sequenceVideoPlayer.isPrepared);
                sequenceVideoPlayer.Play();

                while (sequenceVideoPlayer.isPlaying)
                    yield return null;

                // Tunggu E/Space setelah video selesai (lewati jika ini adalah video)
                if (step.waitForInput && step.videoClip == null)
                {
                    ShowContinueHint(true);
                    yield return StartCoroutine(WaitForAdvanceInput());
                    ShowContinueHint(false);
                }
            }

            // ── Teks Narator (klik E/Space tiap baris, panel gelap sendiri) ──
            if (step.narratorLines != null && step.narratorLines.Length > 0)
            {
                if (narratorDialogue != null)
                {
                    yield return StartCoroutine(narratorDialogue.Show(step.narratorLines));
                }
                else
                {
                    // Fallback jika NarratorDialogue tidak ada
                    if (blackText != null)
                    {
                        sequenceImage?.gameObject.SetActive(false);
                        sequenceRawImage?.gameObject.SetActive(false);
                        blackText.gameObject.SetActive(true);
                        foreach (string line in step.narratorLines)
                        {
                            blackText.text = line;
                            ShowContinueHint(true);
                            yield return StartCoroutine(WaitForAdvanceInput());
                            ShowContinueHint(false);
                            yield return null;
                        }
                        blackText.gameObject.SetActive(false);
                    }
                }
            }

            // ── Teks putih di background hitam ──
            if (step.showBlackText && blackText != null)
            {
                sequenceImage?.gameObject.SetActive(false);
                sequenceRawImage?.gameObject.SetActive(false);
                blackText.gameObject.SetActive(true);
                blackText.text = step.blackText ?? "";

                if (step.waitForInput)
                {
                    ShowContinueHint(true);
                    yield return StartCoroutine(WaitForAdvanceInput());
                    ShowContinueHint(false);
                }
                else
                {
                    yield return new WaitForSeconds(Mathf.Max(0.5f, step.displayDuration));
                }

                blackText.gameObject.SetActive(false);
            }

            // ── Gambar saja (tanpa narator/teks): tunggu E/Space ──
            bool hasWaitableContent = (step.narratorLines != null && step.narratorLines.Length > 0)
                                      || step.showBlackText
                                      || step.videoClip != null;
            if (step.image != null && !hasWaitableContent)
            {
                if (step.waitForInput)
                {
                    ShowContinueHint(true);
                    yield return StartCoroutine(WaitForAdvanceInput());
                    ShowContinueHint(false);
                }
                else if (step.displayDuration > 0f)
                {
                    yield return new WaitForSeconds(step.displayDuration);
                }
            }

            // ── FADE OUT setelah step ──
            if (step.fadeOut && activeFader != null)
                yield return StartCoroutine(activeFader.FadeOut());

            // Reset tampilan setelah step
            if (sequenceImage != null) sequenceImage.gameObject.SetActive(false);
            if (sequenceRawImage != null) sequenceRawImage.gameObject.SetActive(false);
            if (blackText != null) blackText.gameObject.SetActive(false);
        }

        // Selesai — bersihkan
        if (sequenceContinueHint != null) sequenceContinueHint.SetActive(false);
        if (sequencePanel != null) sequencePanel.SetActive(false);

        // Pulihkan player (sebelum transisi)
        PlayerMovement player = PlayerMovement.ActivePlayerInstance
            ?? FindFirstObjectByType<PlayerMovement>();
        if (player != null) player.canMove = true;

        InvokeTransition(seq);
    }

    // ──────────────────────────────────────────────────────
    //  Helpers
    // ──────────────────────────────────────────────────────

    /// <summary>Tunggu sampai player tekan E, Space, atau Enter.</summary>
    IEnumerator WaitForAdvanceInput()
    {
        // Skip frame saat ini agar tidak langsung ter-trigger oleh input yang memulai step ini
        yield return null;
        while (!Input.GetKeyDown(KeyCode.E) &&
               !Input.GetKeyDown(KeyCode.Space) &&
               !Input.GetKeyDown(KeyCode.Return))
        {
            yield return null;
        }
    }

    void ShowContinueHint(bool show)
    {
        if (sequenceContinueHint != null)
            sequenceContinueHint.SetActive(show);
    }

    FadeUI GetActiveFader()
    {
        if (fader != null) return fader;
        if (FadeUI.instance != null) return FadeUI.instance;
        FadeUI[] faders = Resources.FindObjectsOfTypeAll<FadeUI>();
        foreach (FadeUI f in faders)
        {
            if (!string.IsNullOrEmpty(f.gameObject.scene.name))
            {
                FadeUI.instance = f;
                return f;
            }
        }
        return null;
    }

    void InvokeTransition(EndingSequence seq)
    {
        if (seq == null || string.IsNullOrEmpty(seq.nextScene)) return;

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.StartTransition(
                seq.nextScene,
                seq.nextSpawn ?? "",
                seq.nextQuest,
                seq.useFade
            );
        }
        else
        {
            // Fallback langsung load scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(seq.nextScene);
        }
    }
}
