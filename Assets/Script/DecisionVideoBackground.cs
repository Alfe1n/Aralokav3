using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(RawImage))]
[RequireComponent(typeof(VideoPlayer))]
public class DecisionVideoBackground : MonoBehaviour
{
    public VideoClip videoClip;

    private RawImage rawImage;
    private VideoPlayer videoPlayer;
    private RenderTexture renderTexture;
    public bool IsReady { get; private set; }

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
        videoPlayer = GetComponent<VideoPlayer>();

        renderTexture = new RenderTexture(1920, 1080, 0);
        rawImage.texture = renderTexture;

        videoPlayer.clip = videoClip;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.isLooping = true;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.Prepare();
    }

    void OnPrepared(VideoPlayer vp)
    {
        vp.Play();
        IsReady = true;
    }

    void OnDestroy()
    {
        if (renderTexture != null) renderTexture.Release();
    }
}
