using UnityEngine;
using UnityEngine.UI;

public class UIAnimatedSprite : MonoBehaviour
{
    public SpriteRenderer source;
    private Image image;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    void Update()
    {
        if (source != null && image != null)
            image.sprite = source.sprite;
    }
}
