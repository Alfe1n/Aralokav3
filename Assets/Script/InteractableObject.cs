using UnityEngine;
using System.Collections;

public class InteractableObject : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueLine[] dialogueLines;

    private bool playerInside = false;


    void Update()
    {
        if (!playerInside) return;

        // CEGAH SPAM
        if (DialogueManager.instance.IsDialogueActive())
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            DialogueManager.instance.StartDialogue(
                dialogueLines
            );

            DialogueManager.instance.HidePrompt();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;

            if (!DialogueManager.instance.IsDialogueActive())
            {
                DialogueManager.instance.ShowPrompt();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;

            DialogueManager.instance.HidePrompt();
        }
    }
}