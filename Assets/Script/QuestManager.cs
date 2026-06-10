using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("UI")]
    public TMP_Text objectiveText;
    public GameObject objectivePanel;

    [Header("Quest List")]
    public List<QuestData> quests = new();

    [SerializeField]
    private int currentQuest = 0;

    public int CurrentQuest => currentQuest;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        HideObjective();

        UpdateObjective();
    }

    public void ShowObjective()
    {
        Debug.Log("SHOW QUEST UI");

        if (objectivePanel == null)
        {
            Debug.LogError("OBJECTIVE PANEL NULL");
            return;
        }

        objectivePanel.SetActive(true);
    }

    public void HideObjective()
    {
        Debug.Log("HIDE QUEST UI");

        if (objectivePanel == null)
        {
            Debug.LogError("OBJECTIVE PANEL NULL");
            return;
        }

        objectivePanel.SetActive(false);
    }

    public void SetQuest(int id)
    {
        if (id < 0 || id >= quests.Count)
            return;

        currentQuest = id;

        Debug.Log(
            $"QUEST CHANGED -> {currentQuest}"
        );

        UpdateObjective();
    }

    public void NextQuest()
    {
        SetQuest(currentQuest + 1);
    }

    private void UpdateObjective()
    {
        if (objectiveText == null)
            return;

        if (quests.Count == 0)
            return;

        if (currentQuest >= quests.Count)
            return;

        Debug.Log(
            "Quest Update: " + currentQuest
        );

        objectiveText.text =
            quests[currentQuest].objectiveText;
    }
}