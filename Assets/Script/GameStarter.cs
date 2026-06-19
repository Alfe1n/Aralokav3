using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

public class GameStarter : MonoBehaviour
{
    IEnumerator Start()
    {
        // Temp camera: depth sangat tinggi agar menutupi semua kamera lain
        GameObject tempCam = new GameObject("BootCamera");
        Camera cam = tempCam.AddComponent<Camera>();
        cam.backgroundColor = Color.black;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.depth = 999;

        // Load Core Scene (managers, player, FadeUI, dll)
        if (!SceneManager.GetSceneByName("Core Scene").isLoaded)
        {
            yield return SceneManager.LoadSceneAsync("Core Scene", LoadSceneMode.Additive);
        }

        // Segera tutup layar dengan FadeUI agar kamera Core Scene tidak kelihatan
        FadeUI.BlackInstant();

        // Load MainMenu
        yield return SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("MainMenu"));

        // Tunggu VideoPlayer MainMenu siap sebelum fade in
        VideoPlayer vp = Object.FindFirstObjectByType<VideoPlayer>();
        if (vp != null && vp.clip != null)
        {
            vp.Prepare();
            float timeout = 3f;
            while (!vp.isPrepared && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }
            if (vp.isPrepared) vp.Play();
        }

        // Hapus temp camera — FadeUI sudah handle blackness
        Destroy(tempCam);

        // Fade in untuk tampilkan MainMenu
        yield return StartCoroutine(FadeUI.In());
    }
}
