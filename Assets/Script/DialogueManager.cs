using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;
    public bool isDialogueActive;

    [Header("UI")]
    public GameObject dialoguePanel;
    public GameObject interactPromptBara; // Kita simpan agar tidak break, tapi logic menyederhanakannya
    [HideInInspector] public GameObject interactPromptOrangUtan; // Sembunyikan dan satukan targetnya

    public TMP_Text dialogueText;
    public TMP_Text nameText;

    public Image portraitImage;

    [Header("Typewriter")]
    public float typingSpeed = 0.03f;

    [Header("Audio")]
    public AudioSource audioSource;

    public AudioClip dialogueBlip;
    public AudioClip interactSound;

    private DialogueLine[] currentLines;
    private int currentIndex;

    private bool isTyping;
    private bool dialogueActive;

    // FIX SKIP BUG

    private Coroutine typingCoroutine;

    private bool canPressNext = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        HideDialogue();
        HidePrompt();
    }

    void Update()
    {
        if (!dialogueActive) return;

        // CEGAH INPUT DOUBLE
        if (!canPressNext) return;

        if (Input.GetKeyDown(KeyCode.E) ||
            Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                SkipTyping();
            }
            else
            {
                NextDialogue();
            }
        }
    }

    public void StartDialogue(DialogueLine[] lines)
    {
        currentLines = lines;
        currentIndex = 0;

        dialogueActive = true;

        // BLOCK INPUT NEXT
        canPressNext = false;

        // LOCK PLAYER
        PlayerMovement activePlayer = FindFirstObjectByType<PlayerMovement>();
        if (activePlayer != null)
        {
            activePlayer.canMove = false;
        }

        dialoguePanel.SetActive(true);

        HidePrompt();

        ShowCurrentDialogue();

        // AKTIFKAN INPUT SETELAH SEDIKIT DELAY
        StartCoroutine(EnableNextInput());
    }

    IEnumerator EnableNextInput()
    {
        yield return new WaitForSeconds(0.15f);

        canPressNext = true;
    }

    IEnumerator BeginDialogue(DialogueLine[] lines)
    {
        currentLines = lines;
        currentIndex = 0;

        dialogueActive = true;

        // LOCK PLAYER
        PlayerMovement activePlayer = FindFirstObjectByType<PlayerMovement>();
        if (activePlayer != null)
        {
            activePlayer.canMove = false;
        }

        HidePrompt();

        // INTERACT SOUND
        if (audioSource != null &&
            interactSound != null)
        {
            audioSource.PlayOneShot(
                interactSound,
                0.2f
            );
        }

        // DELAY BIAR GA TABRAKAN
        yield return new WaitForSeconds(0.2f);

        dialoguePanel.SetActive(true);

        // FIX AUTO SKIP

        ShowCurrentDialogue();
    }

    void ShowCurrentDialogue()
    {
        // FIX ERROR ARRAY
        if (currentLines == null ||
            currentIndex >= currentLines.Length)
        {
            HideDialogue();
            return;
        }

        DialogueLine line =
            currentLines[currentIndex];

        nameText.text = line.speaker;

        if (portraitImage != null)
        {
            portraitImage.sprite =
                line.portrait;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine =
            StartCoroutine(TypeText(line.text));
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;

        dialogueText.text = "";

        int letterCount = 0;

        foreach (char c in text)
        {
            dialogueText.text += c;

            // BLIP SOUND
            if (c != ' ')
            {
                letterCount++;

                // tiap 4 huruf
                if (letterCount % 4 == 0)
                {
                    if (audioSource != null &&
                        dialogueBlip != null)
                    {
                        audioSource.pitch =
                            Random.Range(0.96f, 1.04f);

                        audioSource.PlayOneShot(
                            dialogueBlip,
                            0.05f
                        );
                    }
                }
            }

            yield return new WaitForSeconds(
                typingSpeed
            );
        }

        isTyping = false;
    }

    void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        dialogueText.text =
            currentLines[currentIndex].text;

        isTyping = false;
    }

    void NextDialogue()
    {
        currentIndex++;

        // FIX INDEX OVERFLOW
        if (currentLines == null ||
            currentIndex >= currentLines.Length)
        {
            HideDialogue();
            return;
        }
        
        ShowCurrentDialogue();
    }

    public void HideDialogue()
    {
        dialoguePanel.SetActive(false);

        dialogueActive = false;

        currentLines = null;
        currentIndex = 0;

        isTyping = false;

        // UNLOCK PLAYER
        PlayerMovement activePlayer = FindFirstObjectByType<PlayerMovement>();
        if (activePlayer != null)
        {
            activePlayer.canMove = true;
        }
    }

    public bool IsDialogueActive()
    {
        return dialogueActive;
    }

    // Jalankan dialog dengan fade hitam sebelum dan sesudah
    public IEnumerator StartDialogueWithFade(DialogueLine[] lines)
    {
        yield return StartCoroutine(FadeUI.Out());
        StartDialogue(lines);
        yield return StartCoroutine(FadeUI.In());
        while (IsDialogueActive()) yield return null;
    }

    public int GetCurrentIndex()
    {
        return currentIndex;
    }

    public void ShowPrompt()
    {
        if (!dialogueActive)
        {
            if (interactPromptBara != null) interactPromptBara.SetActive(true);
        }
    }

    public void HidePrompt()
    {
        if (interactPromptBara != null) interactPromptBara.SetActive(false);
    }
}