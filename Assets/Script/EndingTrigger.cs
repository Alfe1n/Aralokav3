using UnityEngine;

/// <summary>
/// Pasang ke InteractableObject (Harimau/Manusia) di Hutan5.
/// Drag ke field "On Interact Complete" di InteractableObject, pilih method Trigger().
/// </summary>
public class EndingTrigger : MonoBehaviour
{
    [Tooltip("true = Good Ending (Harimau), false = Bad Ending (Manusia)")]
    public bool isGoodEnding = true;

    public void Trigger()
    {
        if (DecisionManager.instance == null)
        {
            Debug.LogWarning("[EndingTrigger] DecisionManager.instance is null!");
            return;
        }

        if (isGoodEnding)
            DecisionManager.instance.TriggerGoodEnding();
        else
            DecisionManager.instance.TriggerBadEnding();
    }
}
