using UnityEngine;
using System.Collections;

public class DecisionTrigger : MonoBehaviour
{
    [Header("Dialog sebelum decision")]
    public DialogueLine[] preDecisionLines;

    [Header("Fade")]
    public FadeUI fader;

    private bool hasTriggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("Player-Orang Utan")) && !hasTriggered)
        {
            Debug.Log($"[DecisionTrigger] Player ('{other.tag}') entered decision trigger zone. Starting sequence.");
            hasTriggered = true;
            StartCoroutine(StartSequence());
        }
    }

    IEnumerator StartSequence()
    {
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null) player.canMove = false;

        // dialog dulu sebelum decision
        if (preDecisionLines != null && preDecisionLines.Length > 0 && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(preDecisionLines);
            yield return new WaitUntil(() => !DialogueManager.instance.IsDialogueActive());
        }

        // mulai decision scene
        if (DecisionManager.instance != null)
            yield return StartCoroutine(DecisionManager.instance.ShowDecision());

        if (player != null) player.canMove = true;

        // hapus trigger agar tidak muncul lagi
        Destroy(gameObject);
    }
}
