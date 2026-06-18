using UnityEngine;

public class QuestObjectToggle : MonoBehaviour
{
    [Header("Quest Settings")]
    public int minQuest = -1;
    public int maxQuest = -1;
    public bool disableWhenOutOfRange = true;

    [Header("Objects to Enable when in range")]
    public GameObject[] enableObjects;

    [Header("Objects to Disable when in range")]
    public GameObject[] disableObjects;

    private int lastQuest = -1;

    void Start()
    {
        UpdateObjects();
    }

    void OnEnable()
    {
        QuestManager.OnQuestChanged += OnQuestChangedHandler;
        UpdateObjects();
    }

    void OnDisable()
    {
        QuestManager.OnQuestChanged -= OnQuestChangedHandler;
    }

    void OnQuestChangedHandler(int _) => UpdateObjects();

    public void UpdateObjects()
    {
        if (QuestManager.Instance == null) return;

        lastQuest = QuestManager.Instance.CurrentQuest;
        bool inRange = true;

        if (minQuest >= 0 && lastQuest < minQuest) inRange = false;
        if (maxQuest >= 0 && lastQuest > maxQuest) inRange = false;

        Debug.Log($"[QuestObjectToggle] '{gameObject.name}' updating objects. Current Quest: {lastQuest}, In Range ({minQuest}-{maxQuest}): {inRange}");

        if (inRange)
        {
            foreach (var go in enableObjects)
            {
                if (go != null)
                {
                    go.SetActive(true);
                    Debug.Log($"[QuestObjectToggle] Activated: {go.name}");
                }
            }
            foreach (var go in disableObjects)
            {
                if (go != null)
                {
                    go.SetActive(false);
                    Debug.Log($"[QuestObjectToggle] Deactivated: {go.name}");
                }
            }
        }
        else if (disableWhenOutOfRange)
        {
            foreach (var go in enableObjects)
            {
                if (go != null)
                {
                    go.SetActive(false);
                    Debug.Log($"[QuestObjectToggle] Deactivated (out of range): {go.name}");
                }
            }
            foreach (var go in disableObjects)
            {
                if (go != null)
                {
                    go.SetActive(true);
                    Debug.Log($"[QuestObjectToggle] Activated (out of range): {go.name}");
                }
            }
        }
    }
}
