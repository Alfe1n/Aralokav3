using UnityEngine;

public class QuestTargetMarker : MonoBehaviour
{
    [Header("Quest Range")]
    [Tooltip("Quest ID index minimum di mana target ini aktif")]
    public int minQuest = 0;

    [Tooltip("Quest ID index maksimum di mana target ini aktif")]
    public int maxQuest = 0;
}
