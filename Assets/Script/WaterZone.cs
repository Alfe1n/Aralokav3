using UnityEngine;

public class WaterZone : MonoBehaviour
{
    public static bool playerIsInWater = false;

    [Header("Settings")]
    public float slowMultiplier = 0.5f;

    [Tooltip("Seberapa jauh Y turun saat masuk air")]
    public float sinkAmount = 0.2f;

    [Tooltip("Seberapa smooth transisi Y — lebih kecil = lebih cepat (0.1–0.25)")]
    public float sinkSmoothTime = 0.15f;

    [Header("Collider di Air")]
    public Vector2 waterColliderOffset = new Vector2(0f, 0.8f);
    public Vector2 waterColliderSize = new Vector2(0.76f, 0.5f);

    [Header("SFX")]
    public AudioSource sfxSource;
    public AudioClip splashEnterClip;
    public AudioClip splashExitClip;

    private void OnTriggerEnter2D(Collider2D other)
    {
        WaterInteractor interactor = other.GetComponent<WaterInteractor>();
        if (interactor != null)
        {
            interactor.EnterWater(this);
            if (other.CompareTag("Player") || other.CompareTag("Player-Orang Utan"))
            {
                playerIsInWater = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        WaterInteractor interactor = other.GetComponent<WaterInteractor>();
        if (interactor != null)
        {
            interactor.ExitWater();
            if (other.CompareTag("Player") || other.CompareTag("Player-Orang Utan"))
            {
                playerIsInWater = false;
            }
        }
    }
}
