using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;

public class InteractableObject : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueLine[] dialogueLines;

    [Header("Quest")]
    public bool useQuest;
    public int requiredQuest = -1;
    public int nextQuest = -1;

    [Header("Cutscene")]
    public bool useCutscene;
    public GameObject cutscenePanel;
    public VideoPlayer videoPlayer;
    public RawImage rawImageVideo;
    public GameObject cgImage;

    [Header("Scene Transition")]
    public bool useSceneTransition;

    public string targetScene;
    public string targetSpawnPoint;

    public int transitionQuest = -1;

    private bool playerInside;
    private bool isInteracting;

    private PlayerMovement playerMovement;

    private void Start()
    {
        playerMovement =
            FindFirstObjectByType<PlayerMovement>();

        if (cutscenePanel != null)
            cutscenePanel.SetActive(false);

        if (cgImage != null)
            cgImage.SetActive(false);

        if (rawImageVideo != null)
            rawImageVideo.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!playerInside)
            return;

        if (isInteracting)
            return;

        if (
            DialogueManager.instance != null &&
            DialogueManager.instance.IsDialogueActive()
        )
            return;

        if (useQuest)
        {
            if (QuestManager.Instance == null)
                return;

            if (
                QuestManager.Instance.CurrentQuest
                != requiredQuest
            )
            {
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(BeginInteraction());
        }
    }

    IEnumerator BeginInteraction()
    {
        isInteracting = true;

        Debug.Log(
            $"INTERACT : {gameObject.name}"
        );

        DialogueManager.instance?.HidePrompt();

        if (playerMovement != null)
            playerMovement.canMove = false;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.HideObjective();
        }

        // ====================
        // CUTSCENE DULU
        // ====================

        if (useCutscene)
        {
            Debug.Log("PLAY CUTSCENE");

            if (cutscenePanel != null)
                cutscenePanel.SetActive(true);

            if (cgImage != null)
                cgImage.SetActive(false);

            if (rawImageVideo != null)
                rawImageVideo.gameObject.SetActive(true);

            if (videoPlayer != null)
            {
                videoPlayer.Stop();

                videoPlayer.frame = 0;

                videoPlayer.Play();

                yield return new WaitUntil(
                    () => videoPlayer.isPlaying
                );

                yield return new WaitUntil(
                    () => !videoPlayer.isPlaying
                );

                videoPlayer.Stop();
            }

            if (rawImageVideo != null)
                rawImageVideo.gameObject.SetActive(false);

            // freeze frame
            if (cgImage != null)
                cgImage.SetActive(true);
        }

        // ====================
        // DIALOGUE
        // ====================

        if (
            dialogueLines != null &&
            dialogueLines.Length > 0
        )
        {
            Debug.Log("START DIALOGUE");

            DialogueManager.instance.StartDialogue(
                dialogueLines
            );

            while (
                DialogueManager.instance != null &&
                DialogueManager.instance
                    .IsDialogueActive()
            )
            {
                yield return null;
            }

            Debug.Log("DIALOGUE FINISHED");
        }

        // ====================
        // TUTUP FREEZE FRAME
        // ====================

        if (cgImage != null)
            cgImage.SetActive(false);

        if (cutscenePanel != null)
            cutscenePanel.SetActive(false);

        // ====================
        // QUEST UPDATE
        // ====================

        if (useQuest)
        {
            if (
                QuestManager.Instance != null &&
                nextQuest >= 0
            )
            {
                Debug.Log(
                    $"QUEST UPDATE {requiredQuest} -> {nextQuest}"
                );

                QuestManager.Instance.SetQuest(
                    nextQuest
                );
            }
        }

        // ====================
        // SCENE TRANSITION
        // ====================

        if (useSceneTransition)
        {
            TransitionManager.Instance
                .StartTransition(
                    targetScene,
                    targetSpawnPoint,
                    transitionQuest
                );

            yield break;
        }

        // ====================
        // CLEANUP
        // ====================

        if (playerMovement != null)
            playerMovement.canMove = true;

        isInteracting = false;
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ShowObjective();
        }
    }

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        if (useQuest)
        {
            if (QuestManager.Instance == null)
                return;

            if (
                QuestManager.Instance.CurrentQuest
                != requiredQuest
            )
            {
                return;
            }
        }

        if (
            DialogueManager.instance != null &&
            !DialogueManager.instance
                .IsDialogueActive()
        )
        {
            DialogueManager.instance.ShowPrompt();
        }
    }

    private void OnTriggerExit2D(
        Collider2D other
    )
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;

        DialogueManager.instance?.HidePrompt();
    }
}