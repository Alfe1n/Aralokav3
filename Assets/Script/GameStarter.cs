using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameStarter : MonoBehaviour
{
    IEnumerator Start()
    {
        // load core
        yield return SceneManager.LoadSceneAsync(
            "Core Scene",
            LoadSceneMode.Additive
        );

        // load map pertama
        yield return SceneManager.LoadSceneAsync(
            "Kamar Bara",
            LoadSceneMode.Additive
        );

        // set active scene
        SceneManager.SetActiveScene(
            SceneManager.GetSceneByName("Kamar Bara")
        );
    }
}