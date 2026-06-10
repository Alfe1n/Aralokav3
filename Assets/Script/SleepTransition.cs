using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SleepTransition : MonoBehaviour
{
    [Header("Scene")]
    public string targetScene = "Void";

    [Header("Quest")]
    public int nextQuest = 2;

    public void StartSleep()
    {
        StartCoroutine(
            SleepRoutine()
        );
    }

    IEnumerator SleepRoutine()
    {
        Debug.Log(
            "START SLEEP TRANSITION"
        );

        // =========================
        // HIDE QUEST UI
        // =========================

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.HideObjective();
        }

        // =========================
        // SET SPAWN TUJUAN
        // =========================

        SpawnManager.spawnPointName =
            "Spawn_Utama";

        // =========================
        // LOAD VOID
        // =========================

        yield return SceneManager
            .LoadSceneAsync(
                targetScene,
                LoadSceneMode.Additive
            );

        Scene voidScene =
            SceneManager.GetSceneByName(
                targetScene
            );

        SceneManager.SetActiveScene(
            voidScene
        );

        // kasih waktu scene init
        yield return null;
        yield return null;

        // =========================
        // UNLOAD KAMAR BARA
        // =========================

        if (
            SceneManager
            .GetSceneByName("Kamar Bara")
            .isLoaded
        )
        {
            yield return SceneManager
                .UnloadSceneAsync(
                    "Kamar Bara"
                );
        }

        // tunggu PlayerSpawn jalan
        yield return null;
        yield return null;

        // =========================
        // UPDATE QUEST
        // =========================

        if (
            QuestManager.Instance != null
            && nextQuest >= 0
        )
        {
            Debug.Log(
                $"QUEST UPDATE -> {nextQuest}"
            );

            QuestManager.Instance.SetQuest(
                nextQuest
            );
        }


        PlayerMovement player =
            FindFirstObjectByType<PlayerMovement>();

        if (player != null)
        {
            Debug.Log(
                "PLAYER FOUND : " +
                player.name
            );

            Debug.Log(
                "canMove sebelum = " +
                player.canMove
            );

            player.canMove = true;

            Debug.Log(
                "canMove sesudah = " +
                player.canMove
            );
        }
        else
        {
            Debug.LogError(
                "PLAYER NOT FOUND"
            );
        }

        // =========================
        // SHOW QUEST UI
        // =========================

        yield return new WaitForSeconds(
            0.5f
        );

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ShowObjective();
        }

        Debug.Log(
            "SHOW QUEST UI"
        );
    }
}