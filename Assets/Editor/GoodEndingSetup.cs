using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

/// <summary>
/// Menu: Araloka/
///   1 - Create Narrator Panel   → buat NarratorPanel + ContinueHint di SequencePanel
///   2 - Fill Good Ending Data   → isi data goodEnding di DecisionManager
///   3 - Wire Sequence References → auto-assign field-field di DecisionManager
///
/// Jalankan di Hutan5 scene yang sudah terbuka.
/// Portrait / Sprite / Video harus di-assign manual di Inspector setelahnya.
/// </summary>
public static class GoodEndingSetup
{
    // ───────────────────────────────────────────────────────────────────────
    //  1. Create Narrator Panel
    // ───────────────────────────────────────────────────────────────────────

    [MenuItem("Araloka/1 - Create Narrator Panel")]
    public static void CreateNarratorPanel()
    {
        GameObject sequencePanel = GameObject.Find("SequencePanel");
        if (sequencePanel == null)
        {
            EditorUtility.DisplayDialog("Info",
                "SequencePanel belum ada di scene!", "OK");
            return;
        }

        Transform existing = sequencePanel.transform.Find("NarratorPanel");
        if (existing != null)
        {
            Debug.Log("[GoodEndingSetup] NarratorPanel sudah ada.");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        // NarratorPanel — panel bawah layar
        GameObject narratorPanel = new GameObject("NarratorPanel",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        narratorPanel.transform.SetParent(sequencePanel.transform, false);
        RectTransform npRt = narratorPanel.GetComponent<RectTransform>();
        npRt.anchorMin = new Vector2(0f, 0f);
        npRt.anchorMax = new Vector2(1f, 0.32f);
        npRt.offsetMin = Vector2.zero;
        npRt.offsetMax = Vector2.zero;
        narratorPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);

        // NarratorText
        GameObject textGO = new GameObject("NarratorText", typeof(RectTransform));
        textGO.transform.SetParent(narratorPanel.transform, false);
        RectTransform textRt = textGO.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0.05f, 0.15f);
        textRt.anchorMax = new Vector2(0.95f, 0.88f);
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        TextMeshProUGUI narratorTmp = textGO.AddComponent<TextMeshProUGUI>();
        narratorTmp.text = "";
        narratorTmp.fontSize = 28;
        narratorTmp.alignment = TextAlignmentOptions.MidlineLeft;
        narratorTmp.color = Color.white;
        narratorTmp.fontStyle = FontStyles.Italic;

        // ContinueHint
        GameObject hintGO = new GameObject("ContinueHint", typeof(RectTransform));
        hintGO.transform.SetParent(narratorPanel.transform, false);
        RectTransform hintRt = hintGO.GetComponent<RectTransform>();
        hintRt.anchorMin = new Vector2(0.65f, 0.02f);
        hintRt.anchorMax = new Vector2(0.98f, 0.18f);
        hintRt.offsetMin = Vector2.zero;
        hintRt.offsetMax = Vector2.zero;
        TextMeshProUGUI hintTmp = hintGO.AddComponent<TextMeshProUGUI>();
        hintTmp.text = "Tekan E / Space ▶";
        hintTmp.fontSize = 18;
        hintTmp.alignment = TextAlignmentOptions.MidlineRight;
        hintTmp.color = new Color(1f, 1f, 1f, 0.6f);

        // NarratorDialogue component
        NarratorDialogue nd = narratorPanel.AddComponent<NarratorDialogue>();
        nd.panel = narratorPanel;
        nd.lineText = narratorTmp;
        nd.continueHint = hintGO;
        nd.typingSpeed = 0.04f;
        nd.useTypewriter = true;

        narratorPanel.SetActive(false);

        Undo.RegisterCreatedObjectUndo(narratorPanel, "Create NarratorPanel");
        Selection.activeGameObject = narratorPanel;
        Debug.Log("[GoodEndingSetup] NarratorPanel dibuat di bawah SequencePanel.");
    }

    // ───────────────────────────────────────────────────────────────────────
    //  2. Fill Good Ending Data
    // ───────────────────────────────────────────────────────────────────────

    [MenuItem("Araloka/2 - Fill Good Ending Data")]
    public static void FillGoodEndingData()
    {
        DecisionManager dm = Object.FindFirstObjectByType<DecisionManager>();
        if (dm == null)
        {
            EditorUtility.DisplayDialog("Error",
                "DecisionManager tidak ditemukan di scene!\nBuka dulu scene Hutan5.", "OK");
            return;
        }

        Undo.RecordObject(dm, "Fill Good Ending Data");
        dm.goodEnding = BuildGoodEnding();
        EditorUtility.SetDirty(dm);

        Debug.Log("[GoodEndingSetup] Good Ending data selesai diisi.\n" +
                  "TODO (manual di Inspector):\n" +
                  "  - preEndingLines: portrait Harimau\n" +
                  "  - Step 0 image: sprite CG hewan berkumpul\n" +
                  "  - Step 1 image: sprite CG hutan pulih\n" +
                  "  - Step 2 videoClip: GoodEnding_Pancarona.mp4");
    }

    static DecisionManager.EndingSequence BuildGoodEnding()
    {
        var seq = new DecisionManager.EndingSequence();
        seq.nextScene = "MainMenu";
        seq.nextSpawn = "";
        seq.nextQuest = -1;
        seq.useFade = true;

        // Dialog singkat saat player interaksi dengan Harimau (sebelum fade ke ending)
        seq.preEndingLines = new DialogueLine[]
        {
            new DialogueLine
            {
                speaker = "Harimau",
                text = "Terima kasih, Bara. Karena pilihanmu, hutan ini masih memiliki harapan.",
                portrait = null  // assign portrait Harimau di Inspector
            },
            new DialogueLine
            {
                speaker = "Bara",
                text = "Aku hanya melakukan apa yang menurutku benar.",
                portrait = null  // assign portrait Bara di Inspector
            },
        };

        seq.steps = new DecisionManager.SequenceStep[]
        {
            // ── STEP 0: Gambar hewan berkumpul + narator ──
            new DecisionManager.SequenceStep
            {
                image = null,           // assign sprite CG hewan berkumpul
                narratorLines = new string[]
                {
                    "Rusa: Kalau kamu tidak datang, mungkin kami tidak akan pernah bisa bertahan.",
                    "Monyet: Kami tidak akan melupakan apa yang sudah kamu lakukan.",
                    "Harimau: Kadang satu keputusan kecil bisa mengubah banyak hal.",
                },
                waitForInput = true,
                fadeIn = true,
                fadeOut = true,
            },

            // ── STEP 1: Gambar hutan pulih + narator ──
            new DecisionManager.SequenceStep
            {
                image = null,           // assign sprite CG hutan pulih
                narratorLines = new string[]
                {
                    "Perlahan, ancaman terhadap hutan mulai berhenti.",
                    "Suara alat berat menghilang dari dalam rimba.",
                    "Satwa-satwa kembali hidup dengan tenang di rumah mereka.",
                },
                waitForInput = true,
                fadeIn = true,
                fadeOut = true,
            },

            // ── STEP 2: Video GoodEnding_Pancarona ──
            new DecisionManager.SequenceStep
            {
                videoClip = null,       // assign GoodEnding_Pancarona.mp4
                waitForInput = true,    // tunggu E/Space setelah video selesai
                fadeIn = true,
                fadeOut = true,
            },

            // ── STEP 3: Teks "ARALOKA" di layar hitam ──
            new DecisionManager.SequenceStep
            {
                showBlackText = true,
                blackText = "ARALOKA",
                waitForInput = true,
                fadeIn = true,
                fadeOut = true,
            },
        };

        return seq;
    }

    // ───────────────────────────────────────────────────────────────────────
    //  5. Fill Bad Ending Data
    // ───────────────────────────────────────────────────────────────────────

    [MenuItem("Araloka/5 - Fill Bad Ending Data")]
    public static void FillBadEndingData()
    {
        DecisionManager dm = Object.FindFirstObjectByType<DecisionManager>();
        if (dm == null)
        {
            EditorUtility.DisplayDialog("Error",
                "DecisionManager tidak ditemukan di scene!\nBuka dulu scene Hutan5.", "OK");
            return;
        }

        Undo.RecordObject(dm, "Fill Bad Ending Data");
        dm.badEnding = BuildBadEnding();
        EditorUtility.SetDirty(dm);

        Debug.Log("[GoodEndingSetup] Bad Ending data selesai diisi.\n" +
                  "TODO (manual di Inspector):\n" +
                  "  - preEndingLines: portrait Manusia dan Bara\n" +
                  "  - Step 0 image: sprite Panel 1 (Bara menolong Manusia)\n" +
                  "  - Step 1 image: sprite Panel 2 (hutan hancur)");
    }

    static DecisionManager.EndingSequence BuildBadEnding()
    {
        var seq = new DecisionManager.EndingSequence();
        seq.nextScene = "MainMenu";
        seq.nextSpawn = "";
        seq.nextQuest = -1;
        seq.useFade = true;

        // Dialog singkat saat player interaksi dengan Manusia (sebelum fade ke ending)
        seq.preEndingLines = new DialogueLine[]
        {
            new DialogueLine
            {
                speaker = "Manusia",
                text = "Tolong... aku tidak bisa bergerak...",
                portrait = null   // assign portrait Manusia di Inspector
            },
            new DialogueLine
            {
                speaker = "Bara",
                text = "Aku di sini. Jangan menyerah!",
                portrait = null   // assign portrait Bara di Inspector
            },
        };

        seq.steps = new DecisionManager.SequenceStep[]
        {
            // ── STEP 0: Panel 1 — Bara menolong Manusia ──
            new DecisionManager.SequenceStep
            {
                image = null,   // assign sprite Panel 1 di Inspector
                narratorLines = new string[]
                {
                    "Bara mencoba mendekati manusia itu...",
                    "Namun kondisinya sudah terlalu parah.",
                    "\"Bertahanlah... aku akan membantumu.\"",
                    "Tapi semua usaha Bara tidak cukup.",
                    "Nyawa itu sudah tidak bisa diselamatkan.",
                },
                waitForInput = true,
                fadeIn  = true,
                fadeOut = true,
            },

            // ── STEP 1: Panel 2 — Hutan hancur ──
            new DecisionManager.SequenceStep
            {
                image = null,   // assign sprite Panel 2 di Inspector
                narratorLines = new string[]
                {
                    "Hari berganti hari...",
                    "Hutan yang menjadi rumah bagi banyak makhluk mulai menghilang.",
                    "Tanpa kekuatan Pancarona yang lengkap, keseimbangan hutan perlahan runtuh.",
                    "Bara hanya bisa melihat tempat yang ia lindungi perlahan hancur.",
                },
                waitForInput = true,
                fadeIn  = true,
                fadeOut = true,
            },

            // ── STEP 2: Teks kutipan di layar hitam ──
            new DecisionManager.SequenceStep
            {
                showBlackText = true,
                blackText     = "\"Mungkin... ini adalah kenyataan yang tidak pernah bisa diperbaiki.\"",
                waitForInput  = true,
                fadeIn        = true,
                fadeOut       = true,
            },

            // ── STEP 3: Judul "Bad Ending" ──
            new DecisionManager.SequenceStep
            {
                showBlackText = true,
                blackText     = "Bad Ending",
                waitForInput  = true,
                fadeIn        = true,
                fadeOut       = true,
            },
        };

        return seq;
    }

    // ───────────────────────────────────────────────────────────────────────
    //  3. Create SequenceContinueHint
    // ───────────────────────────────────────────────────────────────────────

    [MenuItem("Araloka/3 - Create Sequence Continue Hint")]
    public static void CreateSequenceContinueHint()
    {
        GameObject sequencePanel = GameObject.Find("SequencePanel");
        if (sequencePanel == null)
        {
            EditorUtility.DisplayDialog("Info", "SequencePanel tidak ditemukan!", "OK");
            return;
        }

        Transform existing = sequencePanel.transform.Find("SequenceContinueHint");
        if (existing != null)
        {
            Debug.Log("[GoodEndingSetup] SequenceContinueHint sudah ada.");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        GameObject hintGO = new GameObject("SequenceContinueHint", typeof(RectTransform));
        hintGO.transform.SetParent(sequencePanel.transform, false);
        RectTransform rt = hintGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.6f, 0.02f);
        rt.anchorMax = new Vector2(0.98f, 0.12f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = hintGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "Tekan E / Space ▶";
        tmp.fontSize = 22;
        tmp.alignment = TextAlignmentOptions.MidlineRight;
        tmp.color = new Color(1f, 1f, 1f, 0.7f);
        tmp.fontStyle = FontStyles.Italic;

        hintGO.SetActive(false);

        Undo.RegisterCreatedObjectUndo(hintGO, "Create SequenceContinueHint");
        Selection.activeGameObject = hintGO;
        Debug.Log("[GoodEndingSetup] SequenceContinueHint dibuat. Drag ke field 'Sequence Continue Hint' di DecisionManager.");
    }

    // ───────────────────────────────────────────────────────────────────────
    //  4. Wire Sequence References
    // ───────────────────────────────────────────────────────────────────────

    [MenuItem("Araloka/4 - Wire Sequence References")]
    public static void WireSequenceReferences()
    {
        DecisionManager dm = Object.FindFirstObjectByType<DecisionManager>();
        if (dm == null)
        {
            EditorUtility.DisplayDialog("Error", "DecisionManager tidak ditemukan!", "OK");
            return;
        }

        Undo.RecordObject(dm, "Wire Sequence References");

        if (dm.sequencePanel == null)
        {
            var sp = GameObject.Find("SequencePanel");
            if (sp != null) dm.sequencePanel = sp;
        }

        if (dm.sequenceImage == null)
        {
            var si = GameObject.Find("SequenceImage");
            if (si != null) dm.sequenceImage = si.GetComponent<Image>();
        }

        if (dm.sequenceRawImage == null)
        {
            var sri = GameObject.Find("SequenceRawImage");
            if (sri != null) dm.sequenceRawImage = sri.GetComponent<RawImage>();
        }

        if (dm.sequenceVideoPlayer == null)
        {
            var svp = GameObject.Find("SequenceVideoPlayer");
            if (svp != null) dm.sequenceVideoPlayer = svp.GetComponent<VideoPlayer>();
        }

        if (dm.blackText == null)
        {
            var bt = GameObject.Find("SequenceBlackText");
            if (bt != null) dm.blackText = bt.GetComponent<TMP_Text>();
        }

        if (dm.narratorDialogue == null)
        {
            var nd = Object.FindFirstObjectByType<NarratorDialogue>();
            if (nd != null) dm.narratorDialogue = nd;
        }

        // Cari SequenceContinueHint di NarratorPanel atau langsung di SequencePanel
        if (dm.sequenceContinueHint == null)
        {
            var hint = GameObject.Find("SequenceContinueHint");
            if (hint == null) hint = GameObject.Find("ContinueHint");
            if (hint != null) dm.sequenceContinueHint = hint;
        }

        EditorUtility.SetDirty(dm);

        Debug.Log("[GoodEndingSetup] Wire selesai:\n" +
            $"  sequencePanel: {(dm.sequencePanel != null ? "OK" : "MISSING")}\n" +
            $"  sequenceImage: {(dm.sequenceImage != null ? "OK" : "MISSING")}\n" +
            $"  sequenceRawImage: {(dm.sequenceRawImage != null ? "OK" : "MISSING")}\n" +
            $"  sequenceVideoPlayer: {(dm.sequenceVideoPlayer != null ? "OK" : "MISSING")}\n" +
            $"  blackText: {(dm.blackText != null ? "OK" : "MISSING")}\n" +
            $"  narratorDialogue: {(dm.narratorDialogue != null ? "OK" : "MISSING")}\n" +
            $"  sequenceContinueHint: {(dm.sequenceContinueHint != null ? "OK" : "MISSING")}");
    }
}
