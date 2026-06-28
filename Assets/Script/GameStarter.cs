using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;

public class GameStarter : MonoBehaviour
{
    [Header("Splash Screen Settings")]
    [Tooltip("Tarik asset gambar logo HALVA ke sini di Inspector")]
    public Sprite halvaLogo;
    
    [Tooltip("Durasi fade in logo (detik)")]
    public float fadeInDuration = 1.0f;
    
    [Tooltip("Durasi logo tampil diam di layar (detik)")]
    public float showDuration = 2.0f;
    
    [Tooltip("Durasi fade out logo (detik)")]
    public float fadeOutDuration = 1.0f;

    IEnumerator Start()
    {
        // 1. Buat Kamera Temp untuk Boot agar layar hitam bersih awal
        GameObject tempCamObj = new GameObject("BootCamera");
        Camera cam = tempCamObj.AddComponent<Camera>();
        cam.backgroundColor = Color.black;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.depth = 999;

        // 2. Tampilkan logo HALVA jika di-assign di Inspector
        if (halvaLogo != null)
        {
            // Buat Canvas temporer secara programmatis agar tidak mengotori scene
            GameObject canvasObj = new GameObject("SplashCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // Buat latar belakang hitam solid
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvasObj.transform, false);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = Color.black;
            
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // Buat Image untuk logo HALVA
            GameObject logoObj = new GameObject("LogoImage");
            logoObj.transform.SetParent(canvasObj.transform, false);
            Image logoImage = logoObj.AddComponent<Image>();
            logoImage.sprite = halvaLogo;
            logoImage.preserveAspect = true;
            logoImage.color = new Color(1f, 1f, 1f, 0f); // Mulai transparan

            RectTransform logoRect = logoObj.GetComponent<RectTransform>();
            logoRect.anchorMin = new Vector2(0.5f, 0.5f);
            logoRect.anchorMax = new Vector2(0.5f, 0.5f);
            logoRect.pivot = new Vector2(0.5f, 0.5f);
            logoRect.anchoredPosition = Vector2.zero;
            logoRect.sizeDelta = new Vector2(600f, 337f); // Ukuran rasio pas untuk logo 16:9

            // FADE IN
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsed / fadeInDuration);
                logoImage.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }
            logoImage.color = Color.white;

            // TAMPIL DIAM
            yield return new WaitForSeconds(showDuration);

            // FADE OUT
            elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(1f - (elapsed / fadeOutDuration));
                logoImage.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }
            logoImage.color = new Color(1f, 1f, 1f, 0f);

            // Bersihkan Canvas Splash
            Destroy(canvasObj);
        }

        // 3. Load Core Scene (managers, player, FadeUI, dll)
        if (!SceneManager.GetSceneByName("Core Scene").isLoaded)
        {
            yield return SceneManager.LoadSceneAsync("Core Scene", LoadSceneMode.Additive);
        }

        // Segera tutup layar dengan FadeUI agar kamera Core Scene tidak kelihatan
        FadeUI.BlackInstant();

        // 4. Load MainMenu secara Additive
        yield return SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("MainMenu"));

        // Tunggu VideoPlayer MainMenu siap sebelum fade in
        VideoPlayer vp = Object.FindFirstObjectByType<VideoPlayer>();
        if (vp != null && vp.clip != null)
        {
            vp.Prepare();
            float timeout = 3f;
            while (!vp.isPrepared && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }
            if (vp.isPrepared) vp.Play();
        }

        // Hapus temp camera — FadeUI sudah handle blackness
        Destroy(tempCamObj);

        // Fade in masuk ke MainMenu
        yield return StartCoroutine(FadeUI.In());
    }
}
