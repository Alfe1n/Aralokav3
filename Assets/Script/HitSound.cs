using UnityEngine;

public class HitSound : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip hitClip;

    public void PlayHitSound()
    {
        if (audioSource == null || hitClip == null)
            return;

        // Pastikan GameObject audioSource aktif di scene
        if (!audioSource.gameObject.activeInHierarchy)
            return;

        // Jika komponen AudioSource di-disable, aktifkan secara otomatis
        if (!audioSource.enabled)
            audioSource.enabled = true;

        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(hitClip);
    }
}
