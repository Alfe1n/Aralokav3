using UnityEngine;
using UnityEngine.Rendering.Universal;
using Unity.Cinemachine;

public class MazeDarkness : MonoBehaviour
{
    public static MazeDarkness instance;

    [Header("Lights")]
    public Light2D globalLight;
    public Light2D playerLight;

    [Header("Darkness Settings")]
    public float normalIntensity = 1f;
    public float darkIntensity = 0.05f;
    public float transitionSpeed = 2f;

    [Header("Camera Zoom")]
    public CinemachineCamera virtualCamera;
    public float normalOrthoSize = 6f;
    public float mazeOrthoSize = 3.5f;

    [Header("Wall Arena (Opsional)")]
    public GameObject[] wallArena;

    private float targetIntensity;
    private float targetOrthoSize;

    void Awake() { instance = this; }

    void Start()
    {
        EnsureGlobalLightRef();
        EnsurePlayerLightRef();

        targetIntensity = normalIntensity;
        targetOrthoSize = normalOrthoSize;
        
        if (virtualCamera != null)
        {
            var lens = virtualCamera.Lens;
            lens.OrthographicSize = normalOrthoSize;
            virtualCamera.Lens = lens;
        }
        
        if (globalLight) globalLight.intensity = normalIntensity;
        if (playerLight) playerLight.enabled = false;
    }

    void EnsureGlobalLightRef()
    {
        if (globalLight == null || !globalLight.gameObject.scene.IsValid())
        {
            GameObject go = GameObject.Find("Global Light 2D");
            if (go != null)
            {
                globalLight = go.GetComponent<Light2D>();
            }
        }
    }

    void EnsurePlayerLightRef()
    {
        if (playerLight == null || !playerLight.gameObject.scene.IsValid())
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                player = GameObject.FindWithTag("Player-Orang Utan");
            }
            if (player != null)
            {
                playerLight = player.GetComponentInChildren<Light2D>(true);
            }
        }
    }

    void Update()
    {
        EnsureGlobalLightRef();

        if (globalLight)
            globalLight.intensity = Mathf.Lerp(globalLight.intensity, targetIntensity, transitionSpeed * Time.deltaTime);

        if (virtualCamera != null)
        {
            var lens = virtualCamera.Lens;
            if (Mathf.Abs(lens.OrthographicSize - targetOrthoSize) > 0.01f)
            {
                lens.OrthographicSize = Mathf.Lerp(lens.OrthographicSize, targetOrthoSize, transitionSpeed * Time.deltaTime);
                virtualCamera.Lens = lens;
            }
        }
    }

    public bool isInMaze { get; private set; }

    public void EnterMaze()
    {
        isInMaze = true;
        EnsurePlayerLightRef();
        targetIntensity = darkIntensity;
        targetOrthoSize = mazeOrthoSize;
        if (playerLight) playerLight.enabled = true;

        if (wallArena != null)
            foreach (var wall in wallArena) if (wall != null) wall.SetActive(true);
    }

    public void ExitMaze()
    {
        isInMaze = false;
        EnsurePlayerLightRef();
        targetIntensity = normalIntensity;
        targetOrthoSize = normalOrthoSize;
        if (playerLight) playerLight.enabled = false;

        if (wallArena != null)
            foreach (var wall in wallArena) if (wall != null) wall.SetActive(false);
    }

    void OnDestroy()
    {
        EnsureGlobalLightRef();
        if (globalLight)
        {
            globalLight.intensity = normalIntensity;
        }
    }
}
