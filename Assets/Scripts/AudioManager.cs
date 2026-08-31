using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public AudioClip fishMinigameHit;
    public AudioClip fishMinigameMiss;
    public AudioSource audioSource;

    // coin sounds
    public AudioClip coinSound;
    private float coinPitch = 1.0f;
    private float pitchResetTimer = 0f;
    public float sfxVolume = 1;

    void Update()
    {
        // Reset coin audio pitch step-up after 0.4s of no coins landing
        if (Time.time > pitchResetTimer)
        {
            coinPitch = 1.0f;
        }
    }

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
    public void PlayCoinPickupAudio()
    {
        if (audioSource != null && coinSound != null)
        {
            audioSource.pitch = coinPitch;
            audioSource.PlayOneShot(coinSound, 0.7f);

            coinPitch = Mathf.Min(coinPitch + 0.04f, 1.8f);
            pitchResetTimer = Time.time + 0.4f;
        }
    }
}
