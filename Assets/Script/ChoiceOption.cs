using UnityEngine;

/// <summary>
/// Attach to UI Button GameObject; set `choiceIndex` and wire button OnClick to `OnSelected()`
/// </summary>
public class ChoiceOption : MonoBehaviour
{
    [Tooltip("0 = choose manusia (bad), 1 = choose harimau (good) — align with DecisionManager")]
    public int choiceIndex = 0;

    public void OnSelected()
    {
        if (DecisionManager.instance != null)
        {
            DecisionManager.instance.SelectChoice(choiceIndex);
        }
        else
        {
            Debug.LogWarning("[ChoiceOption] DecisionManager.instance is null.");
        }
    }
}
