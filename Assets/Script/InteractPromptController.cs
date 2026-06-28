using UnityEngine;
using System.Collections;

public class InteractPromptController : MonoBehaviour
{
    [Header("Sprite Settings")]
    public Sprite interactSprite;
    public Vector3 customScale = new Vector3(0.5f, 0.5f, 1f); // Adjust scale for the sprite

    [Header("Floating Animation")]
    public float floatSpeed = 4f;
    public float floatAmplitude = 0.15f;

    [Header("Zoom Settings")]
    public float zoomScaleFactor = 1.35f;
    public float zoomDuration = 0.12f;

    private SpriteRenderer spriteRenderer;
    private TMPro.TextMeshPro tmpText;
    private MeshRenderer meshRenderer;

    private Vector3 originalLocalPos;
    private Vector3 baseScale;
    private Coroutine zoomCoroutine;

    void Awake()
    {
        // 1. Destroy TextMeshPro text and MeshRenderer to keep it as pure image (Hanya jika bukan UI Canvas / tidak punya RectTransform)
        bool isUI = GetComponent<RectTransform>() != null;
        if (!isUI)
        {
            tmpText = GetComponent<TMPro.TextMeshPro>();
            if (tmpText != null) DestroyImmediate(tmpText);

            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null) DestroyImmediate(meshRenderer);

            // 2. Add SpriteRenderer if missing
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            if (spriteRenderer != null) spriteRenderer.enabled = true;
        }
        else
        {
            // Jika UI Canvas, sembunyikan text TMP bawaan jika ada
            var tmpUI = GetComponent<TMPro.TextMeshProUGUI>();
            if (tmpUI != null) tmpUI.enabled = false;
        }

        // 3. Load sprite if not assigned
        if (interactSprite == null)
        {
#if UNITY_EDITOR
            object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Object/UI/Core/ChatGPT Image 18 Jun 2026, 16.20.58 2.png");
            foreach (var asset in assets)
            {
                if (asset is Sprite && ((UnityEngine.Object)asset).name.Contains("_0"))
                {
                    interactSprite = (Sprite)asset;
                    break;
                }
            }
            if (interactSprite == null)
            {
                interactSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Object/UI/Core/ChatGPT Image 18 Jun 2026, 16.20.58 2.png");
            }
#endif
        }

        if (spriteRenderer != null && interactSprite != null)
        {
            spriteRenderer.sprite = interactSprite;

            // Salin Sorting Layer dari parent (Player) agar tidak terhalang rendering player atau tilemap
            SpriteRenderer parentRenderer = GetComponentInParent<SpriteRenderer>();
            if (parentRenderer != null)
            {
                spriteRenderer.sortingLayerID = parentRenderer.sortingLayerID;
                spriteRenderer.sortingLayerName = parentRenderer.sortingLayerName;
                spriteRenderer.sortingOrder = parentRenderer.sortingOrder + 10;
            }
            else
            {
                spriteRenderer.sortingOrder = 20; // Fallback
            }
        }

        // 4. Jika berada di dalam Canvas UI (menggunakan Image UI), pasang spritenya juga
        UnityEngine.UI.Image uiImage = GetComponent<UnityEngine.UI.Image>();
        if (uiImage != null)
        {
            uiImage.sprite = interactSprite;
            uiImage.enabled = true;
        }

        originalLocalPos = transform.localPosition;
        baseScale = customScale;
        transform.localScale = baseScale;
    }

    private bool isOriginalPosSet = false;

    void OnEnable()
    {
        if (!isOriginalPosSet)
        {
            originalLocalPos = transform.localPosition;
            isOriginalPosSet = true;
        }
        // Reset scale/position on show
        transform.localPosition = originalLocalPos;
        transform.localScale = baseScale;
    }

    private Transform playerTransform;

    void Update()
    {
        // Sembunyikan prompt jika menu Options sedang aktif
        if (SettingsManager.Instance != null && SettingsManager.Instance.optionsPanel != null && SettingsManager.Instance.optionsPanel.activeSelf)
        {
            if (spriteRenderer != null && spriteRenderer.enabled) spriteRenderer.enabled = false;
            UnityEngine.UI.Image uiImg = GetComponent<UnityEngine.UI.Image>();
            if (uiImg != null && uiImg.enabled) uiImg.enabled = false;
            return;
        }
        else
        {
            if (spriteRenderer != null && !spriteRenderer.enabled) spriteRenderer.enabled = true;
            UnityEngine.UI.Image uiImg = GetComponent<UnityEngine.UI.Image>();
            if (uiImg != null && !uiImg.enabled) uiImg.enabled = true;
        }

        // 1. Jika ini elemen UI Canvas, ikuti koordinat player di layar (Camera Space to Screen Space)
        if (GetComponent<RectTransform>() != null)
        {
            if (PlayerMovement.ActivePlayerInstance != null)
            {
                playerTransform = PlayerMovement.ActivePlayerInstance.transform;
            }
            else if (playerTransform == null || !playerTransform.gameObject.activeInHierarchy)
            {
                var pm = FindFirstObjectByType<PlayerMovement>();
                if (pm != null) playerTransform = pm.transform;
            }

            if (playerTransform != null)
            {
                // Ambil koordinat dunia player + offset ke atas kepalanya
                Vector3 worldPos = playerTransform.position + new Vector3(0f, 1.8f, 0f);
                Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);

                // Tambahkan efek floating animation di sumbu Y layar
                float floatOffset = Mathf.Sin(Time.time * floatSpeed) * (floatAmplitude * 30f); // Skala float disesuaikan pixel screen
                GetComponent<RectTransform>().position = new Vector3(screenPos.x, screenPos.y + floatOffset, 0f);
            }
        }
        else
        {
            // 2. Floating Animation biasa jika menggunakan SpriteRenderer (World Space)
            float newY = originalLocalPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.localPosition = new Vector3(originalLocalPos.x, newY, originalLocalPos.z);
        }

        // 3. Zoom trigger on E press
        if (Input.GetKeyDown(KeyCode.E))
        {
            TriggerZoom();
        }
    }

    private void OnMouseDown()
    {
        TriggerZoom();
    }

    public void TriggerZoom()
    {
        if (!gameObject.activeInHierarchy) return;
        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(ZoomRoutine());
    }

    private IEnumerator ZoomRoutine()
    {
        Vector3 targetScale = baseScale * zoomScaleFactor;

        // Zoom In
        float elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / zoomDuration;
            transform.localScale = Vector3.Lerp(baseScale, targetScale, percent);
            yield return null;
        }
        transform.localScale = targetScale;

        // Zoom Out
        elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / zoomDuration;
            transform.localScale = Vector3.Lerp(targetScale, baseScale, percent);
            yield return null;
        }
        transform.localScale = baseScale;
    }
}
