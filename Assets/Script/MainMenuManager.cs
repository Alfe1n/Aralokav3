using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    public void NewGame()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ResetAllData();
        }
        else
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        if (Inventory.instance != null)
        {
            Inventory.instance.Clear();
        }
        StartCoroutine(StartGame());
    }

    IEnumerator StartGame()
    {
        // load opening cutscene
        yield return SceneManager.LoadSceneAsync(
            "OpeningScene",
            LoadSceneMode.Additive
        );

        // set active scene
        SceneManager.SetActiveScene(
            SceneManager.GetSceneByName(
                "OpeningScene"
            )
        );

        // unload menu
        yield return SceneManager.UnloadSceneAsync(
            "MainMenu"
        );
    }

    public void ContinueGame()
    {
        if (QuestManager.HasSaveData())
        {
            StartCoroutine(LoadContinueGame());
        }
        else
        {
            Debug.LogWarning("[MainMenuManager] No save data found! Starting New Game instead.");
            NewGame();
        }
    }

    IEnumerator LoadContinueGame()
    {
        // load loading scene
        yield return SceneManager.LoadSceneAsync(
            "LoadingScene",
            LoadSceneMode.Additive
        );

        // set loading scene active
        Scene loadingScene = SceneManager.GetSceneByName("LoadingScene");
        SceneManager.SetActiveScene(loadingScene);

        yield return null;

        // unload menu
        yield return SceneManager.UnloadSceneAsync(
            "MainMenu"
        );
    }

    public void Settings()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.ToggleOptions();
        }
        else
        {
            // Fallback cari manual
            SettingsManager sm = FindFirstObjectByType<SettingsManager>();
            if (sm != null)
            {
                sm.ToggleOptions();
            }
            else
            {
                Debug.LogWarning("[MainMenuManager] SettingsManager not found!");
            }
        }
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}