using UnityEngine;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple UI cursor that follows mouse position.
/// Enable the GameObject to show the custom cursor. If you want to hide the OS cursor
/// set `useNativeCursor` to false and enable this GameObject; call SetVisible(true/false)
/// from other scripts (e.g., DecisionManager) to control visibility.
/// </summary>
[RequireComponent(typeof(Image))]
public class CustomCursor : MonoBehaviour
{
    public Image cursorImage;
    [Tooltip("Offset so the visual tip aligns with actual pointer position (in pixels).")]
    public Vector2 hotspot = Vector2.zero;

    [Tooltip("If true, native OS cursor will be hidden when this custom cursor is shown.")]
    public bool hideNativeWhenShown = true;

    void Awake()
    {
        if (cursorImage == null)
            cursorImage = GetComponent<Image>();

        // Cursor UI tidak boleh memblokir klik ke sprite di bawahnya
        if (cursorImage != null)
            cursorImage.raycastTarget = false;

        // start hidden
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        // Input.mousePosition is a Vector3. Use Vector3 to avoid ambiguous operator overloads.
        Vector3 pos = Input.mousePosition;
        pos -= (Vector3)hotspot;
        transform.position = pos;
    }

    public void SetSprite(Sprite s)
    {
        if (cursorImage != null) cursorImage.sprite = s;
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
        if (hideNativeWhenShown)
        {
            Cursor.visible = !visible ? true : false;
        }
    }
}
