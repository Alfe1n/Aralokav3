using UnityEngine;
using TMPro;
using System.Collections;

public class NarratorDialogue : MonoBehaviour
{
    public static NarratorDialogue instance;

    [Header("UI References")]
    public GameObject panel;
    public TMP_Text lineText;
    public GameObject continueHint;

    [Header("Typewriter")]
    public float typingSpeed = 0.04f;
    public bool useTypewriter = true;

    private bool advancePressed;
    private bool isTyping;
    private string currentFullLine;

    void Awake()
    {
        instance = this;
        // Jangan SetActive(false) di sini — Awake() dipanggil saat panel pertama kali diaktifkan
        // dari Show(). Memanggil SetActive(false) di sini akan langsung mematikan panel sebelum
        // teks muncul. Panel sudah di-set inactive di scene, jadi tidak perlu dilakukan di sini.
        if (continueHint != null) continueHint.SetActive(false);
    }

    void Update()
    {
        if (panel == null || !panel.activeInHierarchy) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E) ||
            Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (isTyping) SkipTyping();
            else advancePressed = true;
        }
    }

    public IEnumerator Show(string[] lines)
    {
        if (panel == null || lines == null || lines.Length == 0) yield break;

        panel.SetActive(true);
        if (continueHint != null) continueHint.SetActive(false);

        foreach (string line in lines)
        {
            advancePressed = false;
            currentFullLine = line;

            if (useTypewriter && lineText != null)
            {
                // Inline typewriter — tidak pakai nested StartCoroutine supaya tidak error saat GameObject baru aktif
                isTyping = true;
                lineText.text = "";
                foreach (char c in line)
                {
                    if (!isTyping) { lineText.text = currentFullLine; break; }
                    lineText.text += c;
                    yield return new WaitForSeconds(typingSpeed);
                }
                isTyping = false;
            }
            else
            {
                if (lineText != null) lineText.text = line;
            }

            if (continueHint != null) continueHint.SetActive(true);
            yield return new WaitUntil(() => advancePressed);
            if (continueHint != null) continueHint.SetActive(false);
            yield return null;
        }

        panel.SetActive(false);
    }

    private void SkipTyping()
    {
        isTyping = false;
        if (lineText != null) lineText.text = currentFullLine;
    }

    public bool IsShowing() => panel != null && panel.activeInHierarchy;
}
