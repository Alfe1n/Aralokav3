using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PancaronaCrystalUI : MonoBehaviour
{
    public static PancaronaCrystalUI instance;

    [Header("Sprites (index 0 = 0/5, index 5 = 5/5)")]
    public Sprite[] crystalSprites;

    [Header("UI References")]
    public Image crystalImage;
    public CanvasGroup canvasGroup;

    [Header("Animation")]
    public int blinkCount = 4;
    public float blinkInterval = 0.2f;
    public float holdDuration = 1.5f;
    public float fadeDuration = 0.4f;

    private static readonly string PREFS_KEY = "pancarona_crystal_count";
    private int currentCount;
    private Coroutine activeRoutine;

    void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }

        currentCount = PlayerPrefs.GetInt(PREFS_KEY, 0);
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        gameObject.SetActive(true);
    }

    public void CollectCrystal()
    {
        currentCount = Mathf.Min(currentCount + 1, crystalSprites.Length);
        PlayerPrefs.SetInt(PREFS_KEY, currentCount);
        PlayerPrefs.Save();

        // Refresh semua ConditionWall yang bergantung pada crystal
        foreach (var wall in FindObjectsByType<ConditionWall>(FindObjectsSortMode.None))
            wall.RefreshState();

        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(ShowRoutine());
    }

    // Panggil ini untuk reset (misal new game)
    public void ResetCount()
    {
        currentCount = 0;
        PlayerPrefs.SetInt(PREFS_KEY, 0);
        PlayerPrefs.Save();
    }

    private IEnumerator ShowRoutine()
    {
        int spriteIndex = currentCount - 1;
        if (crystalImage != null && spriteIndex >= 0 && spriteIndex < crystalSprites.Length)
            crystalImage.sprite = crystalSprites[spriteIndex];

        // Blink
        for (int i = 0; i < blinkCount; i++)
        {
            SetAlpha(1f);
            yield return new WaitForSeconds(blinkInterval);
            SetAlpha(0f);
            yield return new WaitForSeconds(blinkInterval);
        }

        // Tahan tampil
        SetAlpha(1f);
        yield return new WaitForSeconds(holdDuration);

        // Fade out
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(1f - (elapsed / fadeDuration));
            yield return null;
        }

        SetAlpha(0f);
        activeRoutine = null;
    }

    private void SetAlpha(float alpha)
    {
        if (canvasGroup != null) canvasGroup.alpha = alpha;
    }
}
