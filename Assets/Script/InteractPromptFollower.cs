using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InteractPromptFollower : MonoBehaviour
{
    [Header("Target settings")]
    [Tooltip("Target player to follow (Bara / OrangUtan)")]
    public Transform playerTransform;
    public Vector3 offset = new Vector3(0f, 1.8f, 0f);

    [Header("Sprite Settings")]
    public Sprite interactSprite;
    public Vector3 customScale = new Vector3(0.5f, 0.5f, 1f);

    [Header("Floating Animation")]
    public float floatSpeed = 4f;
    public float floatAmplitude = 15f; // Pixel offset in screen space

    [Header("Zoom Settings")]
    public float zoomScaleFactor = 1.35f;
    public float zoomDuration = 0.12f;

    private RectTransform rectTransform;
    private Image uiImage;
    private Vector3 baseScale;
    private Coroutine zoomCoroutine;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        uiImage = GetComponent<Image>();

        if (uiImage == null)
        {
            uiImage = gameObject.AddComponent<Image>();
        }

        // Load sprite if missing
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

        if (uiImage != null && interactSprite != null)
        {
            uiImage.sprite = interactSprite;
        }

        baseScale = customScale;
        rectTransform.localScale = baseScale;
    }

    void OnEnable()
    {
        rectTransform.localScale = baseScale;
        UpdatePosition();
    }

    void Update()
    {
        UpdatePosition();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TriggerZoom();
        }
    }

    void UpdatePosition()
    {
        // Selalu prioritaskan ActivePlayerInstance yang sedang aktif
        if (PlayerMovement.ActivePlayerInstance != null)
        {
            playerTransform = PlayerMovement.ActivePlayerInstance.transform;
        }
        else if (playerTransform == null || !playerTransform.gameObject.activeInHierarchy)
        {
            // Deteksi berdasarkan nama controller atau prefab yang aktif
            var pm = FindFirstObjectByType<PlayerMovement>();
            if (pm != null)
            {
                playerTransform = pm.transform;
            }
        }

        if (playerTransform != null && rectTransform != null && Camera.main != null)
        {
            Vector3 worldPos = playerTransform.position + offset;
            Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            float floatOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            rectTransform.position = new Vector3(screenPos.x, screenPos.y + floatOffset, 0f);
        }
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
            rectTransform.localScale = Vector3.Lerp(baseScale, targetScale, percent);
            yield return null;
        }
        rectTransform.localScale = targetScale;

        // Zoom Out
        elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / zoomDuration;
            rectTransform.localScale = Vector3.Lerp(targetScale, baseScale, percent);
            yield return null;
        }
        rectTransform.localScale = baseScale;
    }
}
