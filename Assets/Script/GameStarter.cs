using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameStarter : MonoBehaviour
{
    IEnumerator Start()
    {
        // Kamera sementara: layar hitam solid agar tidak ada "No cameras rendering"
        // selama jeda antara Boot Scene aktif dan scene lain belum dimuat
        GameObject tempCam = new GameObject("BootCamera");
        Camera cam = tempCam.AddComponent<Camera>();
        cam.backgroundColor = Color.black;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.depth = 99; // Depth TINGGI agar menimpa kamera Core Scene (mencegah flash)

        // Load Core Scene first so managers (QuestManager, etc.) are always available
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

        // Beri 1 frame agar MainMenu sepenuhnya tampil sebelum hapus kamera temp
        yield return null;

        // Hapus kamera sementara — MainMenu sudah siap dan punya kamera sendiri
        Destroy(tempCam);
    }
}