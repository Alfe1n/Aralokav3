using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using TMPro;
using System.Collections;

public class OpeningCutscene : MonoBehaviour
{
    [Header("Narrative Opening")]
    public GameObject narrativePanel;
    public TMP_Text narrativeText;

    [TextArea(2, 4)]
    public string[] dialogLines;

    [Header("Typewriter")]
    public float typingSpeed = 0.03f;
    public float inputDelay = 0.15f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip dialogueBlip;
    public int blipEveryLetters = 4;
    [Range(0f, 1f)] public float blipVolume = 0.05f;
    public Vector2 pitchRange = new Vector2(0.96f, 1.04f);

    [Header("Opening Video")]
    public VideoPlayer openingVideo;
    public float cutsceneDuration = 7f;

    private int currentDialogIndex = 0;
    private bool isTyping = false;
    private bool canPressNext = false;
    private Coroutine typingCoroutine;

    IEnumerator Start()
    {
        SpawnManager.spawnPointName = "Spawn_Utama";

        if (openingVideo != null)
        {
            openingVideo.playOnAwake = false;
            openingVideo.Stop();
        }

        yield return StartCoroutine(PlayNarrativeOpening());
        yield return StartCoroutine(PlayOpeningVideo());
        yield return StartCoroutine(LoadKamarBaraScene());
    }

    private IEnumerator PlayNarrativeOpening()
    {
        if (dialogLines == null || dialogLines.Length == 0)
        {
            if (narrativePanel != null)
                narrativePanel.SetActive(false);

            yield break;
        }

        if (narrativePanel != null)
            narrativePanel.SetActive(true);

        currentDialogIndex = 0;
        ShowCurrentDialog();

        yield return new WaitForSeconds(inputDelay);
        canPressNext = true;

        while (currentDialogIndex < dialogLines.Length)
        {
            if (canPressNext && Input.GetKeyDown(KeyCode.Space))
            {
                if (isTyping)
                {
                    SkipTyping();
                }
                else
                {
                    currentDialogIndex++;

                    if (currentDialogIndex < dialogLines.Length)
                    {
                        ShowCurrentDialog();
                    }
                }
            }

            yield return null;
        }

        if (narrativeText != null)
            narrativeText.text = "";

        if (narrativePanel != null)
            narrativePanel.SetActive(false);
    }

    private void ShowCurrentDialog()
    {
        if (narrativeText == null)
            return;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText(dialogLines[currentDialogIndex]));
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        narrativeText.text = "";
        int letterCount = 0;

        foreach (char c in text)
        {
            narrativeText.text += c;

            if (!char.IsWhiteSpace(c))
            {
                letterCount++;

                if (blipEveryLetters > 0 && letterCount % blipEveryLetters == 0)
                {
                    PlayBlip();
                }
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        narrativeText.text = dialogLines[currentDialogIndex];
        isTyping = false;
    }

    private void PlayBlip()
    {
        if (audioSource == null || dialogueBlip == null)
            return;

        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(dialogueBlip, blipVolume);
    }

    private IEnumerator PlayOpeningVideo()
    {
        if (openingVideo != null)
        {
            openingVideo.Stop();
            openingVideo.frame = 0;
            openingVideo.Play();

            yield return null;

            while (openingVideo.isPlaying || openingVideo.frame < (long)openingVideo.frameCount - 1)
            {
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(cutsceneDuration);
        }
    }

    private IEnumerator LoadKamarBaraScene()
    {
        SpawnManager.spawnPointName = "Spawn_Utama";

        if (!SceneManager.GetSceneByName("Core Scene").isLoaded)
        {
            yield return SceneManager.LoadSceneAsync(
                "Core Scene",
                LoadSceneMode.Additive
            );
        }

        yield return SceneManager.LoadSceneAsync(
            "Kamar Bara",
            LoadSceneMode.Additive
        );

        Scene kamarBaraScene = SceneManager.GetSceneByName(
            "Kamar Bara"
        );

        SceneManager.SetActiveScene(
            kamarBaraScene
        );

        yield return null;
        yield return null;

        TeleportBaraToSpawn(kamarBaraScene, "Spawn_Utama");

        yield return SceneManager.UnloadSceneAsync(
            "OpeningScene"
        );
    }

    private void TeleportBaraToSpawn(Scene gameplayScene, string spawnName)
    {
        GameObject spawnObj = FindObjectInScene(gameplayScene, spawnName);
        if (spawnObj == null)
        {
            spawnObj = GameObject.Find(spawnName);
        }

        PlayerMovement targetPlayer = null;
        PlayerMovement[] allPlayers = Resources.FindObjectsOfTypeAll<PlayerMovement>();

        foreach (PlayerMovement player in allPlayers)
        {
            if (player == null || string.IsNullOrEmpty(player.gameObject.scene.name))
                continue;

            if (player.CompareTag("Player"))
            {
                player.gameObject.SetActive(true);
                targetPlayer = player;
            }
            else if (player.CompareTag("Player-Orang Utan"))
            {
                player.gameObject.SetActive(false);
            }
        }

        if (spawnObj != null && targetPlayer != null)
        {
            targetPlayer.transform.position = spawnObj.transform.position;
            targetPlayer.canMove = true;
            SetupCameraTarget(targetPlayer.transform);
            Debug.Log($"[OpeningCutscene] Bara spawned at {spawnName}: {spawnObj.transform.position}");
        }
        else
        {
            Debug.LogWarning($"[OpeningCutscene] Failed to spawn Bara. Spawn found: {spawnObj != null}, Player found: {targetPlayer != null}");
        }
    }

    private GameObject FindObjectInScene(Scene scene, string objectName)
    {
        if (!scene.IsValid() || string.IsNullOrEmpty(objectName))
            return null;

        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (GameObject rootObject in rootObjects)
        {
            if (rootObject.name == objectName)
                return rootObject;

            Transform[] children = rootObject.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.name == objectName)
                    return child.gameObject;
            }
        }

        return null;
    }

    private void SetupCameraTarget(Transform target)
    {
        if (target == null)
            return;

        MonoBehaviour[] activeScripts = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (MonoBehaviour script in activeScripts)
        {
            if (script == null)
                continue;

            string scriptName = script.GetType().Name;
            if (scriptName == "CinemachineVirtualCamera" || scriptName == "CinemachineCamera")
            {
                var followProp = script.GetType().GetProperty("Follow");
                if (followProp != null)
                {
                    followProp.SetValue(script, target);
                }

                var targetProp = script.GetType().GetProperty("Target");
                if (targetProp != null)
                {
                    object targetStruct = targetProp.GetValue(script);
                    var trackingField = targetStruct.GetType().GetField("TrackingTarget");
                    if (trackingField != null)
                    {
                        trackingField.SetValue(targetStruct, target);
                        targetProp.SetValue(script, targetStruct);
                    }
                }
            }
        }
    }
}
