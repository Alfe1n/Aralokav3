using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Pasang ke SpriteManusia / SpriteHarimau di DecisionCanvas.
/// Otomatis detect klik pointer (tidak perlu Button component).
/// Pastikan Image.raycastTarget = true pada GameObject ini.
/// </summary>
public class ChoiceOption : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("0 = pilih manusia (bad ending), 1 = pilih harimau (good ending)")]
    public int choiceIndex = 0;

    [Header("Hover Effect")]
    [Tooltip("Scale saat kursor di atas sprite (1 = tidak ada efek)")]
    public float hoverScale = 1.08f;

    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnSelected();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
    }

    public void OnSelected()
    {
        if (DecisionManager.instance != null)
            DecisionManager.instance.SelectChoice(choiceIndex);
        else
            Debug.LogWarning("[ChoiceOption] DecisionManager.instance is null!");
    }
}
