using UnityEngine;

public class UIFloatMotion : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform keyBox;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Float")]
    [SerializeField] private float floatAmount = 4f;
    [SerializeField] private float floatSpeed = 1.4f;

    [Header("Pulse")]
    [SerializeField] private float scalePulseAmount = 0.04f;
    [SerializeField] private float alphaMin = 0.65f;
    [SerializeField] private float alphaMax = 1f;

    [Header("Press Feedback")]
    [SerializeField] private KeyCode inputKey = KeyCode.Space;
    [SerializeField] private float pressScale = 0.92f;
    [SerializeField] private float pressReturnSpeed = 14f;

    private RectTransform rectTransform;
    private Vector2 startAnchoredPosition;
    private Vector3 startScale;
    private Vector3 keyBoxStartScale;
    private float pressFeedback;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup ??= GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (rectTransform != null)
        {
            startAnchoredPosition = rectTransform.anchoredPosition;
            startScale = rectTransform.localScale;
        }

        if (keyBox != null)
        {
            keyBoxStartScale = keyBox.localScale;
        }
    }

    private void OnEnable()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if (rectTransform != null)
        {
            startAnchoredPosition = rectTransform.anchoredPosition;
            startScale = rectTransform.localScale;
        }

        if (keyBox != null)
        {
            keyBoxStartScale = keyBox.localScale;
        }

        pressFeedback = 0f;
    }

    private void Update()
    {
        if (rectTransform == null)
        {
            return;
        }

        float wave = (Mathf.Sin(Time.unscaledTime * floatSpeed) + 1f) * 0.5f;
        float yOffset = Mathf.Lerp(-floatAmount, floatAmount, wave);
        float scaleOffset = 1f + (wave * scalePulseAmount);

        if (Input.GetKeyDown(inputKey))
        {
            pressFeedback = 1f;
        }

        pressFeedback = Mathf.MoveTowards(
            pressFeedback,
            0f,
            Time.unscaledDeltaTime * pressReturnSpeed
        );

        float pressedScale = Mathf.Lerp(1f, pressScale, pressFeedback);
        rectTransform.anchoredPosition = startAnchoredPosition + new Vector2(0f, yOffset);
        rectTransform.localScale = startScale * scaleOffset * pressedScale;

        if (keyBox != null)
        {
            keyBox.localScale = keyBoxStartScale * (1f + wave * scalePulseAmount * 1.5f) * pressedScale;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = Mathf.Lerp(alphaMin, alphaMax, wave);
        }
    }
}
