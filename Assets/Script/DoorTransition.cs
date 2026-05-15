using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoorTransition : MonoBehaviour
{
    [Header("Scene")]
    public string targetScene;
    public string spawnPointName;

    [Header("Audio")]
    public AudioClip doorSound;

    private bool playerInside = false;
    private bool isTransitioning = false;

    void Update()
    {
        if (playerInside &&
            !isTransitioning &&
            Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(Transition());
        }
    }

    IEnumerator Transition()
    {
        isTransitioning = true;

        if (doorSound != null)
        {
            AudioSource.PlayClipAtPoint(
                doorSound,
                Camera.main.transform.position,
                0.2f
            );
        }

        yield return new WaitForSeconds(0.2f);

        // save spawn
        SpawnManager.spawnPointName =
            spawnPointName;

        // ambil current scene
        Scene currentScene =
            SceneManager.GetActiveScene();

        // load target additive
        yield return SceneManager.LoadSceneAsync(
            targetScene,
            LoadSceneMode.Additive
        );

        // set active
        Scene target =
            SceneManager.GetSceneByName(targetScene);

        SceneManager.SetActiveScene(target);

        // unload scene lama
        yield return SceneManager.UnloadSceneAsync(
            currentScene
        );

        isTransitioning = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;

            if (!DialogueManager.instance
                .IsDialogueActive())
            {
                DialogueManager.instance
                    .ShowPrompt();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;

            DialogueManager.instance
                .HidePrompt();
        }
    }
}