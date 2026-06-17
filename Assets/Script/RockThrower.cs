using UnityEngine;

// ================================================================
// RockThrower — ADAPTASI untuk Aralokav3
// ================================================================
// PERUBAHAN dari versi teman:
// 1. Input diubah dari KeyCode.E → KeyCode.Q
//    Alasan: E sudah dipakai oleh InteractableObject (Aralokav3)
//    Q adalah tombol yang aman untuk skill/item lempar
// ================================================================

public class RockThrower : MonoBehaviour
{
    [Header("Settings")]
    public float maxThrowDistance = 5f;
    public int rockCount = 3;
    public GameObject rockPrefab;

    [Header("Aiming Indicator")]
    public LineRenderer trajectoryLine;
    public GameObject landingCircle;
    public int arcPoints = 20;
    public float arcHeight = 1.5f;

    [Header("Input")]
    [Tooltip("Tombol untuk melempar batu. Default Q (E dipakai InteractableObject)")]
    public KeyCode throwKey = KeyCode.Q;

    private bool isAiming = false;
    private Vector2 targetPos;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = false;
            trajectoryLine.positionCount = arcPoints;
        }

        if (landingCircle != null)
            landingCircle.SetActive(false);
    }

    void Update()
    {
        // Jangan bisa aim saat player tidak bisa bergerak
        PlayerMovement player = GetComponent<PlayerMovement>();
        if (player != null && !player.canMove) return;

        // Mulai aiming dengan KeyCode.Q (bukan E agar tidak konflik)
        if (Input.GetKeyDown(throwKey) && rockCount > 0)
            isAiming = true;

        if (isAiming)
        {
            Vector2 mouseWorld =
                cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 origin = transform.position;
            Vector2 dir = mouseWorld - origin;

            // Clamp jarak maksimal
            if (dir.magnitude > maxThrowDistance)
                dir = dir.normalized * maxThrowDistance;

            targetPos = origin + dir;

            // Tampilkan indicator
            DrawArc(origin, targetPos);

            if (landingCircle != null)
            {
                landingCircle.SetActive(true);
                landingCircle.transform.position = targetPos;
            }

            // Lepas throwKey → lempar
            if (Input.GetKeyUp(throwKey))
                Throw();

            // Batal dengan klik kanan
            if (Input.GetMouseButtonDown(1))
                CancelAim();
        }
    }

    void DrawArc(Vector2 start, Vector2 end)
    {
        if (trajectoryLine == null) return;

        trajectoryLine.enabled = true;
        for (int i = 0; i < arcPoints; i++)
        {
            float t = (float)i / (arcPoints - 1);
            Vector2 point = Vector2.Lerp(start, end, t);
            point.y += arcHeight * Mathf.Sin(t * Mathf.PI);
            trajectoryLine.SetPosition(i, point);
        }
    }

    void Throw()
    {
        isAiming = false;

        if (trajectoryLine != null)
            trajectoryLine.enabled = false;

        if (landingCircle != null)
            landingCircle.SetActive(false);

        if (rockPrefab != null)
        {
            GameObject rock = Instantiate(
                rockPrefab, transform.position, Quaternion.identity
            );

            RockProjectile proj = rock.GetComponent<RockProjectile>();
            if (proj != null)
                proj.Init(transform.position, targetPos, arcHeight);

            rockCount--;
        }
    }

    void CancelAim()
    {
        isAiming = false;

        if (trajectoryLine != null)
            trajectoryLine.enabled = false;

        if (landingCircle != null)
            landingCircle.SetActive(false);
    }
}
