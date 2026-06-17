using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameStarter : MonoBehaviour
{
    IEnumerator Start()
    {
        // Load Core Scene first so SettingsManager & other managers are always available
        if (!SceneManager.GetSceneByName("Core Scene").isLoaded)
        {
            yield return SceneManager.LoadSceneAsync(
                "Core Scene",
                LoadSceneMode.Additive
            );
        }

        // Then load Main Menu on top
        yield return SceneManager.LoadSceneAsync(
            "MainMenu",
            LoadSceneMode.Additive
        );

        SceneManager.SetActiveScene(
            SceneManager.GetSceneByName("MainMenu")
        );
    }
}