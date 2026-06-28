using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class DebugPrefsClear
{
    [MenuItem("Araloka/Reset Save Data (Clear PlayerPrefs)")]
    public static void ClearPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[Araloka Debug] PlayerPrefs (Save Data & Triggers) has been cleared successfully!");
    }

    [MenuItem("Araloka/Setup Minimap in Core Scene")]
    public static void SetupMinimap()
    {
        // 1. Dapatkan atau Buat Render Texture
        string rtPath = "Assets/MinimapRenderTexture.renderTexture";
        RenderTexture rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(rtPath);
        if (rt == null)
        {
            rt = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
            AssetDatabase.CreateAsset(rt, rtPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Minimap Setup] Created Render Texture at: {rtPath}");
        }

        // 2. Cari GUI_OrangUtan di scene aktif (Core Scene)
        GameObject guiOrangUtan = GameObject.Find("GUI_OrangUtan");
        if (guiOrangUtan == null)
        {
            Debug.LogError("[Minimap Setup] Gagal menemukan GameObject 'GUI_OrangUtan' di scene aktif! Buka 'Core Scene' terlebih dahulu.");
            EditorUtility.DisplayDialog("Error", "Gagal menemukan GameObject 'GUI_OrangUtan' di scene aktif! Pastikan Anda sedang membuka 'Core Scene'.", "OK");
            return;
        }

        // 3. Buat MinimapGroup sebagai Container utama
        GameObject minimapGroupObj = GameObject.Find("MinimapGroup");
        if (minimapGroupObj == null)
        {
            minimapGroupObj = new GameObject("MinimapGroup");
            minimapGroupObj.transform.SetParent(guiOrangUtan.transform, false);

            RectTransform rect = minimapGroupObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-20f, -20f); // margin dari pojok kanan atas
            rect.sizeDelta = new Vector2(180f, 180f);       // Ukuran UI minimap

            Undo.RegisterCreatedObjectUndo(minimapGroupObj, "Create Minimap Group");
            Debug.Log("[Minimap Setup] Created MinimapGroup Container under GUI_OrangUtan.");
        }

        // 4. Buat RawImage UI di dalam MinimapGroup
        GameObject minimapUIObj = GameObject.Find("MinimapGroup/MinimapUI");
        if (minimapUIObj == null)
        {
            minimapUIObj = new GameObject("MinimapUI");
            minimapUIObj.transform.SetParent(minimapGroupObj.transform, false);

            RawImage rawImage = minimapUIObj.AddComponent<RawImage>();
            rawImage.texture = rt;

            // Atur agar stretch memenuhi Container dengan sedikit padding di dalam border (misal 5px)
            RectTransform rect = minimapUIObj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.offsetMin = new Vector2(5f, 5f);   // Padding kiri, bawah
                rect.offsetMax = new Vector2(-5f, -5f); // Padding kanan, atas
            }

            Undo.RegisterCreatedObjectUndo(minimapUIObj, "Create Minimap UI");
            Debug.Log("[Minimap Setup] Created MinimapUI RawImage under MinimapGroup.");
        }
        else
        {
            RawImage rawImage = minimapUIObj.GetComponent<RawImage>();
            if (rawImage != null)
            {
                rawImage.texture = rt;
            }
        }

        // 5. Buat Image Border di atas MinimapUI
        GameObject minimapBorderObj = GameObject.Find("MinimapGroup/MinimapBorder");
        if (minimapBorderObj == null)
        {
            minimapBorderObj = new GameObject("MinimapBorder");
            minimapBorderObj.transform.SetParent(minimapGroupObj.transform, false);

            // Tambahkan komponen Image untuk Border
            Image borderImage = minimapBorderObj.AddComponent<Image>();
            borderImage.color = Color.white; // Default white color, user can assign custom sprite
            borderImage.raycastTarget = false;

            // Stretch memenuhi Container agar menutupi tepi minimap
            RectTransform rect = minimapBorderObj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            Undo.RegisterCreatedObjectUndo(minimapBorderObj, "Create Minimap Border");
            Debug.Log("[Minimap Setup] Created MinimapBorder Image under MinimapGroup.");
        }

        // 6. Buat FullMapGroup sebagai Container Map Besar (Tengah layar)
        GameObject fullMapGroupObj = GameObject.Find("FullMapGroup");
        if (fullMapGroupObj == null)
        {
            fullMapGroupObj = new GameObject("FullMapGroup");
            fullMapGroupObj.transform.SetParent(guiOrangUtan.transform, false);

            RectTransform rect = fullMapGroupObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero; // Tengah layar
            rect.sizeDelta = new Vector2(500f, 500f); // Ukuran map besar

            Undo.RegisterCreatedObjectUndo(fullMapGroupObj, "Create FullMap Group");
            Debug.Log("[Minimap Setup] Created FullMapGroup Container under GUI_OrangUtan.");
        }

        // 7. Buat RawImage UI di dalam FullMapGroup
        GameObject fullMapUIObj = GameObject.Find("FullMapGroup/FullMapUI");
        if (fullMapUIObj == null)
        {
            fullMapUIObj = new GameObject("FullMapUI");
            fullMapUIObj.transform.SetParent(fullMapGroupObj.transform, false);

            RawImage rawImage = fullMapUIObj.AddComponent<RawImage>();
            rawImage.texture = rt;

            RectTransform rect = fullMapUIObj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.offsetMin = new Vector2(15f, 15f);   // Padding kiri, bawah
                rect.offsetMax = new Vector2(-15f, -15f); // Padding kanan, atas
            }

            Undo.RegisterCreatedObjectUndo(fullMapUIObj, "Create FullMap UI");
            Debug.Log("[Minimap Setup] Created FullMapUI RawImage.");
        }

        // 8. Buat Image Border di dalam FullMapGroup (sebagai border map besar)
        GameObject fullMapBorderObj = GameObject.Find("FullMapGroup/FullMapBorder");
        if (fullMapBorderObj == null)
        {
            fullMapBorderObj = new GameObject("FullMapBorder");
            fullMapBorderObj.transform.SetParent(fullMapGroupObj.transform, false);

            Image borderImage = fullMapBorderObj.AddComponent<Image>();
            borderImage.color = Color.white;
            borderImage.raycastTarget = false;

            RectTransform rect = fullMapBorderObj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            Undo.RegisterCreatedObjectUndo(fullMapBorderObj, "Create FullMap Border");
            Debug.Log("[Minimap Setup] Created FullMapBorder Image.");
        }

        // Deaktifkan map besar secara default
        fullMapGroupObj.SetActive(false);

        // 9. Buat MinimapCamera di scene jika belum ada
        GameObject minimapCamObj = GameObject.Find("MinimapCamera");
        if (minimapCamObj == null)
        {
            minimapCamObj = new GameObject("MinimapCamera");
            
            // Tambahkan komponen Camera
            Camera cam = minimapCamObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 15f;
            cam.targetTexture = rt;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            
            // Matikan AudioListener bawaan agar tidak konflik
            AudioListener listener = minimapCamObj.GetComponent<AudioListener>();
            if (listener != null) Object.DestroyImmediate(listener);

            // Tambahkan pelacak MinimapFollower
            minimapCamObj.AddComponent<MinimapFollower>();

            Undo.RegisterCreatedObjectUndo(minimapCamObj, "Create Minimap Camera");
            Debug.Log("[Minimap Setup] Created MinimapCamera with MinimapFollower.");
        }
        else
        {
            Camera cam = minimapCamObj.GetComponent<Camera>();
            if (cam != null)
            {
                cam.targetTexture = rt;
            }
        }

        // 10. Tambahkan & Konfigurasi komponen MinimapToggle pada GUI_OrangUtan
        MinimapToggle toggle = guiOrangUtan.GetComponent<MinimapToggle>();
        if (toggle == null)
        {
            toggle = guiOrangUtan.AddComponent<MinimapToggle>();
        }

        if (toggle != null)
        {
            toggle.minimapGroup = minimapGroupObj;
            toggle.fullMapGroup = fullMapGroupObj;
            toggle.minimapCamera = minimapCamObj.GetComponent<Camera>();
            toggle.normalOrthoSize = 15f;
            toggle.fullMapOrthoSize = 40f;
            toggle.toggleKey = KeyCode.M;
            EditorUtility.SetDirty(toggle);
        }

        EditorUtility.DisplayDialog("Sukses", "Sistem Minimap & Full Map berhasil disetup secara otomatis di Core Scene!", "OK");
    }

    [MenuItem("Araloka/Setup Pause Button in Core Scene")]
    public static void SetupPauseButton()
    {
        // 1. Cari Canvas di Core Scene
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("[Pause Button Setup] Gagal menemukan GameObject 'Canvas' di scene aktif!");
            EditorUtility.DisplayDialog("Error", "Gagal menemukan GameObject 'Canvas' di scene aktif!", "OK");
            return;
        }

        // 2. Load sprite kayu Name_Plate
        Sprite woodSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Asset/Name_Plate.png");
        if (woodSprite == null)
        {
            Debug.LogWarning("[Pause Button Setup] Sprite Name_Plate.png tidak ditemukan di Assets/Asset/.");
        }

        // 3. Cari atau buat PauseButton
        GameObject pauseBtnObj = GameObject.Find("Canvas/PauseButton");
        if (pauseBtnObj == null)
        {
            pauseBtnObj = new GameObject("PauseButton");
            pauseBtnObj.transform.SetParent(canvas.transform, false);
            Undo.RegisterCreatedObjectUndo(pauseBtnObj, "Create Pause Button");
        }

        // 4. Atur RectTransform
        RectTransform rect = pauseBtnObj.GetComponent<RectTransform>();
        if (rect == null) rect = pauseBtnObj.AddComponent<RectTransform>();
        
        rect.anchorMin = new Vector2(1f, 1f); // Pojok kanan atas
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        
        // Posisikan di atas Pancarona (Pancarona biasanya berada agak ke bawah sudut kanan atas)
        rect.anchoredPosition = new Vector2(-20f, -20f); 
        rect.sizeDelta = new Vector2(70f, 70f); // Ukuran tombol kayu pas

        // 5. Tambahkan Image dengan sprite kayu
        Image img = pauseBtnObj.GetComponent<Image>();
        if (img == null) img = pauseBtnObj.AddComponent<Image>();
        if (woodSprite != null)
        {
            img.sprite = woodSprite;
            img.type = Image.Type.Sliced;
        }
        img.color = Color.white;

        // 6. Tambahkan Text (||) menggunakan TextMeshProUGUI
        GameObject textObj = null;
        Transform textTrans = pauseBtnObj.transform.Find("Text");
        if (textTrans != null)
        {
            textObj = textTrans.gameObject;
        }
        else
        {
            textObj = new GameObject("Text");
            textObj.transform.SetParent(pauseBtnObj.transform, false);
        }

        TMPro.TextMeshProUGUI tmp = textObj.GetComponent<TMPro.TextMeshProUGUI>();
        if (tmp == null) tmp = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        
        tmp.text = "<b>||</b>";
        tmp.fontSize = 32f;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = new Color(0.22f, 0.15f, 0.08f); // Warna cokelat kayu tua menyatu tema

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = new Vector2(0f, -5f); // Sedikit offset ke bawah biar center visual kayu

        // 7. Tambahkan Button dan event onClick ke SettingsManager
        Button btn = pauseBtnObj.GetComponent<Button>();
        if (btn == null) btn = pauseBtnObj.AddComponent<Button>();

        // Bersihkan listener lama agar tidak berlipat ganda
        while (btn.onClick.GetPersistentEventCount() > 0)
        {
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(btn.onClick, 0);
        }

        SettingsManager settings = Object.FindFirstObjectByType<SettingsManager>();
        if (settings != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, settings.ToggleOptions);
            Debug.Log("[Pause Button Setup] Successfully linked button onClick to SettingsManager.ToggleOptions.");
        }
        else
        {
            Debug.LogWarning("[Pause Button Setup] SettingsManager tidak ditemukan di scene! Klik event harus dihubungkan secara manual nanti.");
        }

        EditorUtility.SetDirty(pauseBtnObj);
        EditorUtility.DisplayDialog("Sukses", "Tombol Pause Kayu (||) berhasil dibuat di pojok kanan atas!", "OK");
    }
}
