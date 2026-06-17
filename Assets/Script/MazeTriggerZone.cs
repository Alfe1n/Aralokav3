using UnityEngine;

public class MazeTriggerZone : MonoBehaviour
{
    [Tooltip("Centang untuk pintu KELUAR labirin")]
    public bool isExit = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Player-Orang Utan")) return;
        if (MazeDarkness.instance == null) return;

        if (isExit)
            MazeDarkness.instance.ExitMaze();
        else
            MazeDarkness.instance.EnterMaze();
    }
}
