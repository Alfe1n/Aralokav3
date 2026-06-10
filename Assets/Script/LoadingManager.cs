using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;

    [Header("Fade")]
    public SceneFader sceneFader;

    [Header("Loading")]
    public float minimumLoadTime = 3f;

    IEnumerator Start()
    {
        // =========================
        // HIDE QUEST UI
        // =========================

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.HideObjective();
        }

        // =========================
        // FADE IN
        // =========================

        if (sceneFader != null)
        {
            yield return StartCoroutine(
                sceneFader.FadeIn()
            );
        }

        // =========================
        // PLAY VIDEO
        // =========================

        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }

        float timer = 0f;

        // =========================
        // LOAD CORE SCENE
        // =========================

        if (
            !SceneManager
            .GetSceneByName("Core Scene")
            .isLoaded
        )
        {
            yield return SceneManager
                .LoadSceneAsync(
                    "Core Scene",
                    LoadSceneMode.Additive
                );
        }

        // =========================
        // LOAD GAMEPLAY SCENE
        // =========================

        AsyncOperation gameplayLoad =
            SceneManager.LoadSceneAsync(
                "Kamar Bara",
                LoadSceneMode.Additive
            );

        while (!gameplayLoad.isDone)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // =========================
        // MINIMUM LOAD TIME
        // =========================

        while (timer < minimumLoadTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // =========================
        // SET ACTIVE SCENE
        // =========================

        Scene gameplay =
            SceneManager.GetSceneByName(
                "Kamar Bara"
            );

        SceneManager.SetActiveScene(
            gameplay
        );

        yield return null;
        yield return null;

        // =========================
        // STOP VIDEO
        // =========================

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        // =========================
        // FADE OUT
        // =========================

        if (sceneFader != null)
        {
            yield return StartCoroutine(
                sceneFader.FadeOut()
            );
        }

        // =========================
        // TUNGGU SEBENTAR
        // =========================

        yield return new WaitForSeconds(
            0.5f
        );

        // =========================
        // SHOW QUEST UI
        // =========================

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ShowObjective();
        }

        // kasih 1 frame biar aman
        yield return null;

        // =========================
        // UNLOAD LOADING SCENE
        // =========================

        yield return SceneManager
            .UnloadSceneAsync(
                "LoadingScene"
            );
    }
}