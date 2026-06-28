using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

/// <summary>
/// Editor helper: creates a ready-to-use `sequencePanel` GameObject under a Canvas.
/// Menu: Araloka/Create Sequence Panel
/// After running, assign created objects to `DecisionManager` fields in Inspector.
/// </summary>
public class SequencePanelCreator
{
    [MenuItem("Araloka/Create Sequence Panel")]
    public static void CreateSequencePanel()
    {
        // Find or create Canvas
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        GameObject canvasGO = null;
        if (canvas == null)
        {
            canvasGO = new GameObject("Canvas", typeof(Canvas));
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(canvasGO, "Create Canvas");
        }
        else
        {
            canvasGO = canvas.gameObject;
        }

        // Create sequencePanel
        GameObject panel = new GameObject("SequencePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(canvasGO.transform, false);
        RectTransform panelRt = panel.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = Color.black;

        // Sequence Image
        GameObject seqImageGO = new GameObject("SequenceImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        seqImageGO.transform.SetParent(panel.transform, false);
        RectTransform seqImageRt = seqImageGO.GetComponent<RectTransform>();
        seqImageRt.anchorMin = new Vector2(0.5f, 0.5f);
        seqImageRt.anchorMax = new Vector2(0.5f, 0.5f);
        seqImageRt.sizeDelta = new Vector2(800, 450);
        seqImageRt.anchoredPosition = Vector2.zero;

        // RawImage for video
        GameObject rawImageGO = new GameObject("SequenceRawImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        rawImageGO.transform.SetParent(panel.transform, false);
        RectTransform rawRt = rawImageGO.GetComponent<RectTransform>();
        rawRt.anchorMin = new Vector2(0.5f, 0.5f);
        rawRt.anchorMax = new Vector2(0.5f, 0.5f);
        rawRt.sizeDelta = new Vector2(800, 450);
        rawRt.anchoredPosition = Vector2.zero;

        // VideoPlayer object
        GameObject videoGO = new GameObject("SequenceVideoPlayer", typeof(VideoPlayer));
        videoGO.transform.SetParent(panel.transform, false);
        var vp = videoGO.GetComponent<VideoPlayer>();
        vp.playOnAwake = false;
        vp.isLooping = false;
        vp.renderMode = VideoRenderMode.RenderTexture;

        // Create a RenderTexture asset under Assets/RenderTextures if not exists
        string rtPath = "Assets/RenderTextures";
        if (!AssetDatabase.IsValidFolder(rtPath))
        {
            AssetDatabase.CreateFolder("Assets", "RenderTextures");
        }
        string assetPath = rtPath + "/SequenceRT.rendertexture";
        RenderTexture rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(assetPath);
        if (rt == null)
        {
            rt = new RenderTexture(1920, 1080, 0);
            AssetDatabase.CreateAsset(rt, assetPath);
            AssetDatabase.SaveAssets();
        }

        vp.targetTexture = rt;

        // Assign render texture to RawImage
        var rawImage = rawImageGO.GetComponent<RawImage>();
        rawImage.texture = rt;

        // Black text (TextMeshProUGUI)
        GameObject textGO = new GameObject("BlackText", typeof(RectTransform));
        textGO.transform.SetParent(panel.transform, false);
        RectTransform textRt = textGO.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0.5f, 0.2f);
        textRt.anchorMax = new Vector2(0.5f, 0.2f);
        textRt.anchoredPosition = Vector2.zero;
        textRt.sizeDelta = new Vector2(1200, 200);

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "";
        tmp.fontSize = 36;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        // Deactivate by default
        panel.SetActive(false);

        // Select created object
        Selection.activeGameObject = panel;

        Debug.Log("SequencePanel created. Assign SequencePanel, SequenceImage, SequenceRawImage, SequenceVideoPlayer (VideoPlayer) and BlackText (TMP) to DecisionManager in Inspector.");
    }
}
