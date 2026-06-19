using UnityEngine;
using UnityEngine.Video;
using System.Collections;

/// <summary>
/// Helper component to play a VideoClip on a VideoPlayer then notify when finished.
/// Attach to same GameObject as a VideoPlayer and call PlayClipAndWait.
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class DecisionVideo : MonoBehaviour
{
    private VideoPlayer vp;

    void Awake()
    {
        vp = GetComponent<VideoPlayer>();
        vp.playOnAwake = false;
        vp.isLooping = false;
    }

    public IEnumerator PlayClipAndWait(VideoClip clip)
    {
        if (vp == null || clip == null)
            yield break;

        vp.clip = clip;
        vp.Prepare();
        yield return new WaitUntil(() => vp.isPrepared);
        vp.Play();
        while (vp.isPlaying)
            yield return null;
    }
}