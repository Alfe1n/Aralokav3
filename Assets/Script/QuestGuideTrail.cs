using UnityEngine;

public class QuestGuideTrail : MonoBehaviour
{
    private Transform target;
    private float speed = 6f;
    private float waveFrequency = 6f;
    private float waveAmplitude = 0.6f;

    private Vector3 direction;
    private float timeActive = 0f;
    private float maxLifetime = 5f;

    public void Initialize(Transform targetTransform, float movementSpeed)
    {
        target = targetTransform;
        speed = movementSpeed;
        timeActive = 0f;

        if (target != null)
        {
            direction = (target.position - transform.position).normalized;
        }
        else
        {
            direction = Vector3.up;
        }

        SetupVisuals();
    }

    private void SetupVisuals()
    {
        // Tambahkan TrailRenderer untuk membuat efek ekor debu berkilau
        TrailRenderer trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.6f;
        trail.startWidth = 0.2f;
        trail.endWidth = 0.0f;
        
        // Gunakan shader default sprite agar terlihat solid dan glowing tanpa pencahayaan
        Material material = new Material(Shader.Find("Sprites/Default"));
        trail.material = material;

        // Gradient emas berkilau (Pancarona style)
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(1f, 0.9f, 0.4f), 0f), // Emas terang di depan
                new GradientColorKey(new Color(1f, 0.5f, 0f), 1f)    // Oranye hangat di ekor
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f), 
                new GradientAlphaKey(0f, 1f) 
            }
        );
        trail.colorGradient = gradient;
        
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trail.receiveShadows = false;

        // Salin Sorting Layer dan Order dari Player agar tidak tertutup objek background/black square
        if (PlayerMovement.ActivePlayerInstance != null)
        {
            SpriteRenderer playerSr = PlayerMovement.ActivePlayerInstance.GetComponent<SpriteRenderer>();
            if (playerSr != null)
            {
                trail.sortingLayerID = playerSr.sortingLayerID;
                trail.sortingLayerName = playerSr.sortingLayerName;
                trail.sortingOrder = playerSr.sortingOrder + 5; // Di depan player sedikit
            }
        }
        else
        {
            trail.sortingOrder = 100;
        }
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        timeActive += Time.deltaTime;

        Vector3 targetPos = target.position;
        Vector3 currentPos = transform.position;

        Vector3 toTarget = targetPos - currentPos;
        float distance = toTarget.magnitude;

        // Hancurkan jika sudah sangat dekat dengan target
        if (distance < 0.4f || timeActive > maxLifetime)
        {
            DetachAndDestroy();
            return;
        }

        direction = toTarget.normalized;

        // Pergerakan maju dasar
        Vector3 movement = direction * speed * Time.deltaTime;

        // Meliuk-liuk (sine wave) tegak lurus dengan arah pergerakan agar terkesan alami ditiup angin
        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f);
        float wave = Mathf.Sin(timeActive * waveFrequency) * waveAmplitude;
        
        // Redam liukan saat mendekati target agar pas masuk ke pusat target
        if (distance < 2.0f)
        {
            wave *= (distance / 2.0f);
        }

        transform.position = currentPos + movement + (perpendicular * wave * Time.deltaTime);
    }

    private void DetachAndDestroy()
    {
        // Lepas trail agar tidak langsung lenyap seketika (biar fade-out natural)
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.transform.parent = null;
            trail.autodestruct = true;
        }
        Destroy(gameObject);
    }
}
