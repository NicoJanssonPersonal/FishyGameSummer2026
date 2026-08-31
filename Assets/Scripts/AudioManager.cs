using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public AudioClip fishMinigameHit;
    public AudioClip fishMinigameMiss;
    public AudioSource audioSource;

    public float sfxVolume = 1;
    
    public void PlayHitSound(float currentFishMult)
    {
        if (audioSource == null || fishMinigameHit == null) return;

        float t = Mathf.InverseLerp(1f, 10f, currentFishMult);

        audioSource.pitch = Mathf.Lerp(0.8f, 1.8f, t);

        float volume = Mathf.Lerp(0.6f, 0.7f, t);

        audioSource.PlayOneShot(fishMinigameHit, volume * sfxVolume);
    }
    public void PlayMissSound()
    {
        if (audioSource == null || fishMinigameMiss == null) return;
        float volume = 0.4f;
        audioSource.PlayOneShot(fishMinigameMiss, volume * sfxVolume);
    }
}
