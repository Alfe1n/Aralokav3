using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

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

    public void StartTransition(
        string targetScene,
        string targetSpawn,
        int nextQuest = -1
    )
    {
        StartCoroutine(
            TransitionRoutine(
                targetScene,
                targetSpawn,
                nextQuest
            )
        );
    }

    IEnumerator TransitionRoutine(
        string targetScene,
        string targetSpawn,
        int nextQuest
    )
    {
        Debug.Log(
            $"TRANSITION -> {targetScene}"
        );

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.HideObjective();
        }

        SpawnManager.spawnPointName =
            targetSpawn;

        Scene currentScene =
            SceneManager.GetActiveScene();

        yield return SceneManager
            .LoadSceneAsync(
                targetScene,
                LoadSceneMode.Additive
            );

        Scene newScene =
            SceneManager.GetSceneByName(
                targetScene
            );

        SceneManager.SetActiveScene(
            newScene
        );

        yield return null;
        yield return null;

        if (
            currentScene.IsValid()
            && currentScene.name != "Core Scene"
        )
        {
            yield return SceneManager
                .UnloadSceneAsync(
                    currentScene.name
                );
        }

        yield return null;
        yield return null;

        if (
            QuestManager.Instance != null
            && nextQuest >= 0
        )
        {
            QuestManager.Instance.SetQuest(
                nextQuest
            );
        }

        PlayerMovement player =
            FindFirstObjectByType<PlayerMovement>();

        if (player != null)
        {
            player.canMove = true;
        }

        yield return new WaitForSeconds(
            0.5f
        );

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ShowObjective();
        }

        Debug.Log(
            "TRANSITION FINISHED"
        );
    }
}