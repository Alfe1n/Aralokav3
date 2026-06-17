using UnityEngine;
using UnityEngine.EventSystems;

public class ChoiceOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public int choiceIndex; // 0 = manusia, 1 = harimau

    [Header("Cursor")]
    public Texture2D handCursor;
    public Vector2 hotspot = Vector2.zero;

    private DecisionManager decisionManager;

    void Start()
    {
        decisionManager = FindFirstObjectByType<DecisionManager>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Cursor.SetCursor(handCursor, hotspot, CursorMode.Auto);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        decisionManager?.SelectChoice(choiceIndex);
    }
}
