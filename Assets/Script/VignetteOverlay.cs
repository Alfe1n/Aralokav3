using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menampilkan vignette (tepi layar gelap) menggunakan UI Image dengan texture procedural.
/// Pasang ke GameObject kosong di Canvas scene Void.
/// </summary>
[RequireComponent(typeof(Image))]
public class VignetteOverlay : MonoBehaviour
{
    [Header("Tampilan")]
    [Range(0f, 1f)] public float intensity = 0.85f;
    [Range(0f, 1f)] public float smoothness = 0.4f;
    public Color vignetteColor = Color.black;

    [Header("Animasi (opsional)")]
    public bool animate = true;
    [Range(0f, 0.3f)] public float pulseAmount = 0.05f;
    public float pulseSpeed = 1.2f;

    private Image image;
    private Texture2D tex;
    private float baseIntensity;

    void Awake()
    {
        image = GetComponent<Image>();
        baseIntensity = intensity;
        BuildTexture();
        SetupRectTransform();
    }

    void Update()
    {
        if (!animate) return;
        float t = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        float current = Mathf.Clamp01(baseIntensity + t);
        if (Mathf.Abs(current - intensity) > 0.005f)
        {
            intensity = current;
            BuildTexture();
        }
    }

    void BuildTexture()
    {
        int size = 256;
        if (tex == null)
        {
            tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
        }

        Color[] pixels = new Color[size * size];
        float half = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - half) / half; // -1 .. 1
                float ny = (y - half) / half;
                float dist = Mathf.Sqrt(nx * nx + ny * ny); // 0 (center) .. ~1.41 (corner)

                // Normalkan ke 0..1 agar pojok = 1
                float t = Mathf.Clamp01(dist / 1.414f);

                // Smooth step agar transisi halus
                float vignette = Mathf.SmoothStep(1f - smoothness, 1f, t) * intensity;

                Color c = vignetteColor;
                c.a = vignette;
                pixels[y * size + x] = c;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        image.sprite = Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f);
    }

    void SetupRectTransform()
    {
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void OnDestroy()
    {
        if (tex != null) Destroy(tex);
    }

    // Panggil ini dari script lain untuk mengubah intensitas secara runtime
    public void SetIntensity(float value)
    {
        baseIntensity = Mathf.Clamp01(value);
        intensity = baseIntensity;
        BuildTexture();
    }
}
