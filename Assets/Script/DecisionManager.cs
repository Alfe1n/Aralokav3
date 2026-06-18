using UnityEngine;
using System.Collections;

public class DecisionManager : MonoBehaviour
{
    public static DecisionManager instance;

    [Header("Canvas")]
    public GameObject decisionCanvas;

    [Header("Fade")]
    public FadeUI fader;

    [Header("Dialog setelah pilih manusia (index 0)")]
    public DialogueLine[] manusiaLines;

    [Header("Dialog setelah pilih harimau (index 1)")]
    public DialogueLine[] harimauLines;

    [Header("Posisi player setelah memilih")]
    public Transform spawnDekatManusia;
    public Transform spawnDekatHarimau;

    [Header("Animator karakter (objek di scene)")]
    public Animator harimauAnimator;
    public Animator manusiaAnimator;
    public string harimauBerdiriTrigger = "Berdiri";

    private bool choiceMade = false;

    void Awake()
    {
        instance = this;
        if (decisionCanvas != null) decisionCanvas.SetActive(false);
    }

    private FadeUI GetActiveFader()
    {
        if (fader != null) return fader;
        if (FadeUI.instance != null) return FadeUI.instance;
        FadeUI[] faders = Resources.FindObjectsOfTypeAll<FadeUI>();
        foreach (FadeUI f in faders)
        {
            if (f.gameObject.scene.name != null)
            {
                FadeUI.instance = f;
                return f;
            }
        }
        return null;
    }

    public IEnumerator ShowDecision()
    {
        // Sembunyikan GUI OrangUtan selama cutscene pilihan berlangsung
        OrangUtanUIVisibility.Instance?.ForceHide();
        if (QuestManager.Instance != null) QuestManager.Instance.HideObjective();

        // 1. Fade ke hitam
        FadeUI activeFader = GetActiveFader();
        if (activeFader != null)
            yield return StartCoroutine(activeFader.FadeOut());
        else
            Debug.LogWarning("[DecisionManager] No active fader found for FadeOut!");

        // 2. Aktifkan canvas saat layar masih hitam
        if (decisionCanvas != null) decisionCanvas.SetActive(true);
        Cursor.visible = true;

        // 3. Tunggu video siap (prepare + play selesai)
        DecisionVideoBackground videoBg = decisionCanvas.GetComponentInChildren<DecisionVideoBackground>();
        if (videoBg != null)
            yield return new WaitUntil(() => videoBg.IsReady);

        // tunggu 2 frame ekstra supaya RenderTexture sudah ada isinya
        yield return null;
        yield return null;

        // 4. Baru fade in — semua sudah siap, tidak ada blink
        if (activeFader != null)
            yield return StartCoroutine(activeFader.FadeIn());
        else
            Debug.LogWarning("[DecisionManager] No active fader found for FadeIn!");

        // 5. Tunggu player pilih
        choiceMade = false;
        yield return new WaitUntil(() => choiceMade);
    }

    public void SelectChoice(int index)
    {
        if (choiceMade) return;
        choiceMade = true;
        StartCoroutine(ProcessChoice(index));
    }

    IEnumerator ProcessChoice(int index)
    {
        PlayerPrefs.SetInt("PlayerChoice", index);
        PlayerPrefs.Save();

        // 6. Dialog hasil pilihan — canvas MASIH TERLIHAT
        DialogueLine[] lines = index == 0 ? manusiaLines : harimauLines;
        if (lines != null && lines.Length > 0 && DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(lines);
            yield return null; // tunggu 1 frame biar dialogue aktif
            yield return new WaitUntil(() => !DialogueManager.instance.IsDialogueActive());
        }

        // 7. Teleport player saat canvas masih menutupi game scene
        GameObject player = null;
        if (PlayerMovement.ActivePlayerInstance != null)
        {
            player = PlayerMovement.ActivePlayerInstance.gameObject;
        }
        else
        {
            player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                player = GameObject.FindWithTag("Player-Orang Utan");
            }
        }

        if (player != null)
        {
            Transform target = index == 0 ? spawnDekatManusia : spawnDekatHarimau;
            if (target != null)
            {
                Vector3 pos = target.position;
                pos.z = 1f;
                player.transform.position = pos;
                Debug.Log($"[DecisionManager] Teleported player '{player.name}' to position: {pos}");
            }
            else
            {
                Debug.LogWarning($"[DecisionManager] Teleport target transform is null for choice index {index}!");
            }
        }
        else
        {
            Debug.LogWarning("[DecisionManager] Player GameObject not found for teleportation!");
        }

        // 8. Baru fade ke hitam dan sembunyikan canvas
        FadeUI activeFader = GetActiveFader();
        if (activeFader != null)
            yield return StartCoroutine(activeFader.FadeOut());

        if (decisionCanvas != null) decisionCanvas.SetActive(false);
        Cursor.visible = false;

        // 8b. Ganti animasi karakter saat layar masih hitam
        if (index == 1)
        {
            // Pilih harimau -> harimau selamat (berdiri), manusia mati (animasi berhenti)
            if (harimauAnimator != null) harimauAnimator.SetTrigger(harimauBerdiriTrigger);
            if (manusiaAnimator != null) manusiaAnimator.enabled = false;
        }
        else
        {
            // Pilih manusia -> manusia tetap mati, harimau juga mati (animasi berhenti)
            if (manusiaAnimator != null) manusiaAnimator.enabled = false;
            if (harimauAnimator != null) harimauAnimator.enabled = false;
        }

        // 9. Fade balik ke game — player sudah di posisi baru
        if (activeFader != null)
            yield return StartCoroutine(activeFader.FadeIn());

        // Restore GUI OrangUtan dan Quest UI setelah cutscene selesai
        OrangUtanUIVisibility.Instance?.ForceRefresh();
        if (QuestManager.Instance != null) QuestManager.Instance.ShowObjective();
    }
}
