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
                waitForInput = false,   // langsung next setelah video selesai
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

        // Load bad ending images
        Sprite img1 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Object/UI/BadEnding/Bad Ending 1.png");
        Sprite img2 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Object/UI/BadEnding/BadEnding2.png");

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
                image = img1,
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
                image = img2,
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

    // ───────────────────────────────────────────────────────────────────────
    //  6. Setup Ending Characters (Harimau and Manusia Setup)
    // ───────────────────────────────────────────────────────────────────────
    [MenuItem("Araloka/6 - Setup Ending Characters")]
    public static void SetupEndingCharacters()
    {
        // 1. Manusia Setup
        GameObject manusia = GameObject.Find("Manusia");
        if (manusia != null)
        {
            Undo.RecordObject(manusia, "Setup Manusia Ending Components");

            // Tambahkan BoxCollider2D sebagai trigger area interaksi
            BoxCollider2D boxCol = manusia.GetComponent<BoxCollider2D>();
            if (boxCol == null)
            {
                boxCol = manusia.AddComponent<BoxCollider2D>();
            }
            boxCol.isTrigger = true;
            boxCol.size = new Vector2(2.7f, 2.25f);
            boxCol.offset = new Vector2(0.04f, 0.69f);

            // Tambahkan EndingTrigger
            EndingTrigger et = manusia.GetComponent<EndingTrigger>();
            if (et == null)
            {
                et = manusia.AddComponent<EndingTrigger>();
            }
            et.isGoodEnding = false; // Manusia = Bad Ending
            EditorUtility.SetDirty(et);

            // Tambahkan InteractableObject
            InteractableObject io = manusia.GetComponent<InteractableObject>();
            if (io == null)
            {
                io = manusia.AddComponent<InteractableObject>();
            }
            io.useQuest = false;
            io.requireSpawnerCleared = false;
            io.isRescueInteraction = false;
            io.dialogueLines = new DialogueLine[0];

            // Setup Event OnInteractComplete untuk memanggil EndingTrigger.Trigger
            // Gunakan UnityEventTools agar permanen & tersimpan di Scene
            io.onInteractComplete = new UnityEngine.Events.UnityEvent();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(io.onInteractComplete, et.Trigger);
            
            EditorUtility.SetDirty(io);
            EditorUtility.SetDirty(manusia);

            Debug.Log("[GoodEndingSetup] Manusia (Bad Ending) setup selesai.");
        }
        else
        {
            Debug.LogError("[GoodEndingSetup] Manusia GameObject tidak ditemukan di Scene!");
        }

        // 2. Harimau Setup
        GameObject harimau = GameObject.Find("Harimau");
        if (harimau != null)
        {
            Undo.RecordObject(harimau, "Setup Harimau Ending Components");

            BoxCollider2D boxCol = harimau.GetComponent<BoxCollider2D>();
            if (boxCol == null)
            {
                boxCol = harimau.AddComponent<BoxCollider2D>();
            }
            boxCol.isTrigger = true;
            boxCol.size = new Vector2(2.7f, 2.25f);
            boxCol.offset = new Vector2(0.04f, 0.69f);

            EndingTrigger et = harimau.GetComponent<EndingTrigger>();
            if (et == null)
            {
                et = harimau.AddComponent<EndingTrigger>();
            }
            et.isGoodEnding = true; // Harimau = Good Ending
            EditorUtility.SetDirty(et);

            InteractableObject io = harimau.GetComponent<InteractableObject>();
            if (io == null)
            {
                io = harimau.AddComponent<InteractableObject>();
            }
            io.useQuest = false;
            io.requireSpawnerCleared = false;
            io.isRescueInteraction = false;
            io.dialogueLines = new DialogueLine[0];

            io.onInteractComplete = new UnityEngine.Events.UnityEvent();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(io.onInteractComplete, et.Trigger);

            EditorUtility.SetDirty(io);
            EditorUtility.SetDirty(harimau);

            Debug.Log("[GoodEndingSetup] Harimau (Good Ending) setup selesai.");
        }
        else
        {
            Debug.LogError("[GoodEndingSetup] Harimau GameObject tidak ditemukan di Scene!");
        }
    }

    // ───────────────────────────────────────────────────────────────────────
    //  7. Setup Player Interact Prompts
    // ───────────────────────────────────────────────────────────────────────
    [MenuItem("Araloka/7 - Setup Player Interact Prompts")]
    public static void SetupPlayerInteractPrompts()
    {
        GameObject[] allGOs = Resources.FindObjectsOfTypeAll<GameObject>();
        int count = 0;
        foreach (var go in allGOs)
        {
            if (go.name == "InteractPrompt" && !string.IsNullOrEmpty(go.scene.name))
            {
                Undo.RecordObject(go, "Setup Player Interact Prompt");

                // 1. Dapatkan atau tambahkan script controller
                var ipc = go.GetComponent<InteractPromptController>();
                if (ipc == null)
                {
                    ipc = go.AddComponent<InteractPromptController>();
                }

                // 2. Load slice sprite
                Sprite spr = null;
                object[] assets = AssetDatabase.LoadAllAssetsAtPath("Assets/Object/UI/Core/ChatGPT Image 18 Jun 2026, 16.20.58 2.png");
                foreach (var asset in assets)
                {
                    if (asset is Sprite && ((UnityEngine.Object)asset).name.Contains("_0"))
                    {
                        spr = (Sprite)asset;
                        break;
                    }
                }
                Sprite targetSprite = spr != null ? spr : AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Object/UI/Core/ChatGPT Image 18 Jun 2026, 16.20.58 2.png");

                if (ipc != null)
                {
                    ipc.interactSprite = targetSprite;
                }

                // 3. Hapus TextMeshPro dan MeshRenderer agar tidak bentrok dengan SpriteRenderer
                var tmp = go.GetComponent<TMPro.TextMeshPro>();
                if (tmp != null) Undo.DestroyObjectImmediate(tmp);

                var mr = go.GetComponent<MeshRenderer>();
                if (mr != null) Undo.DestroyObjectImmediate(mr);

                // 4. Tambahkan SpriteRenderer
                var sr = go.GetComponent<SpriteRenderer>();
                if (sr == null)
                {
                    sr = go.AddComponent<SpriteRenderer>();
                }
                if (sr != null)
                {
                    sr.sprite = targetSprite;
                    sr.enabled = true;

                    var parentSR = go.transform.parent != null ? go.transform.parent.GetComponent<SpriteRenderer>() : null;
                    if (parentSR != null)
                    {
                        sr.sortingLayerID = parentSR.sortingLayerID;
                        sr.sortingLayerName = parentSR.sortingLayerName;
                        sr.sortingOrder = parentSR.sortingOrder + 10;
                    }
                    else
                    {
                        sr.sortingOrder = 20;
                    }
                }

                var col = go.GetComponent<BoxCollider2D>();
                if (col == null)
                {
                    col = go.AddComponent<BoxCollider2D>();
                }
                col.isTrigger = true;
                col.size = new Vector2(3f, 3f);

                EditorUtility.SetDirty(go);
                count++;
            }
        }
    }

    // ───────────────────────────────────────────────────────────────────────
    //  8. Setup Canvas Interact Prompts (Hibrida UI Image)
    // ───────────────────────────────────────────────────────────────────────
    [MenuItem("Araloka/8 - Setup Canvas Interact Prompts")]
    public static void SetupCanvasInteractPrompts()
    {
        // 1. Cari Canvas di active scenes
        Canvas canvas = null;
        Canvas[] canvases = Resources.FindObjectsOfTypeAll<Canvas>();
        foreach (var c in canvases)
        {
            if (!string.IsNullOrEmpty(c.gameObject.scene.name) && c.name == "Canvas")
            {
                canvas = c;
                break;
            }
        }

        if (canvas == null)
        {
            Debug.LogError("[GoodEndingSetup] Canvas dengan nama 'Canvas' tidak ditemukan di scene!");
            return;
        }

        Undo.RecordObject(canvas.gameObject, "Setup Canvas Interact Prompts");

        // 2. Nonaktifkan InteractPrompt bawaan TMP yang nempel di Player
        GameObject[] allGOs = Resources.FindObjectsOfTypeAll<GameObject>();
        GameObject playerBaraObj = null;
        GameObject playerOrangUtanObj = null;

        foreach (var go in allGOs)
        {
            if (string.IsNullOrEmpty(go.scene.name)) continue;

            if (go.name == "InteractPrompt")
            {
                // Nonaktifkan GameObject TMP bawaan
                Undo.RecordObject(go, "Disable TMP Interact Prompt");
                go.SetActive(false);
                EditorUtility.SetDirty(go);
            }
            else if (go.name == "Player")
            {
                playerBaraObj = go;
            }
            else if (go.name == "Player-Orang Utan")
            {
                playerOrangUtanObj = go;
            }
        }

        // 3. Cari atau buat GameObject CanvasInteractPrompt tunggal di Canvas
        string promptName = "CanvasInteractPrompt";
        Transform promptTrans = canvas.transform.Find(promptName);
        GameObject promptGO = promptTrans != null ? promptTrans.gameObject : null;

        Sprite spr = null;
        object[] assets = AssetDatabase.LoadAllAssetsAtPath("Assets/Object/UI/Core/ChatGPT Image 18 Jun 2026, 16.20.58 2.png");
        foreach (var asset in assets)
        {
            if (asset is Sprite && ((UnityEngine.Object)asset).name.Contains("_0"))
            {
                spr = (Sprite)asset;
                break;
            }
        }
        Sprite targetSprite = spr != null ? spr : AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Object/UI/Core/ChatGPT Image 18 Jun 2026, 16.20.58 2.png");

        if (promptGO == null)
        {
            promptGO = new GameObject(promptName, typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(InteractPromptFollower));
            Undo.RegisterCreatedObjectUndo(promptGO, "Create Canvas UI Prompt");
            promptGO.transform.SetParent(canvas.transform, false);
        }
        
        var img = promptGO.GetComponent<UnityEngine.UI.Image>();
        img.sprite = targetSprite;
        var rTrans = promptGO.GetComponent<RectTransform>();
        rTrans.sizeDelta = new Vector2(67f, 84f); // Sesuai resolusi sprite original

        var follower = promptGO.GetComponent<InteractPromptFollower>();
        follower.interactSprite = targetSprite;
        // Target follow dinamis (akan dideteksi otomatis saat runtime oleh PlayerMovement di script follower)
        follower.playerTransform = null; 
        promptGO.SetActive(false); // Sembunyikan default awal

        // Hapus prompt lama jika ada sisa duplikasi di scene
        Transform oldOrangUtan = canvas.transform.Find("CanvasInteractPrompt_OrangUtan");
        if (oldOrangUtan != null) Undo.DestroyObjectImmediate(oldOrangUtan.gameObject);
        Transform oldBara = canvas.transform.Find("CanvasInteractPrompt_Bara");
        if (oldBara != null) Undo.DestroyObjectImmediate(oldBara.gameObject);

        // 4. Hubungkan ke DialogueManager
        DialogueManager dm = GameObject.FindFirstObjectByType<DialogueManager>();
        if (dm != null)
        {
            Undo.RecordObject(dm, "Link Prompt to DialogueManager");
            dm.interactPromptBara = promptGO;
            dm.interactPromptOrangUtan = null;
            EditorUtility.SetDirty(dm);
            Debug.Log("[GoodEndingSetup] Berhasil menghubungkan Canvas Interact Prompt ke DialogueManager.");
        }
        else
        {
            Debug.LogWarning("[GoodEndingSetup] DialogueManager tidak ditemukan di scene untuk menghubungkan link reference.");
        }

        EditorUtility.SetDirty(canvas.gameObject);
        Debug.Log("[GoodEndingSetup] Setup Canvas Interact Prompt selesai otomatis (Single mode)!");
    }
}

