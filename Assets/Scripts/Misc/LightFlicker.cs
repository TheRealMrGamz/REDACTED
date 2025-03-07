using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
[RequireComponent(typeof(AudioSource))]
public class LightFlicker : MonoBehaviour
{
    [Header("Flicker Settings")]
    [SerializeField]
    private float minIntensity = 0.0f;
    [SerializeField]
    private float maxIntensity = 1.0f;
    
    [SerializeField]
    [Range(0.01f, 0.5f)]
    private float flickerInterval = 0.05f;
    
    [SerializeField]
    private float flickerDuration = 2.0f;

    [Header("Audio Settings")]
    [SerializeField]
    private AudioClip flickerSound;
    
    [SerializeField]
    [Range(0f, 1f)]
    private float soundVolume = 0.5f;
    
    [SerializeField]
    private bool playOnEveryFlicker = false;

    private bool isFlickering = false;
    private Light lightComponent;
    private AudioSource audioSource;
    private float originalIntensity;
    private Coroutine flickerCoroutine;

    private void Awake()
    {
        lightComponent = GetComponent<Light>();
        audioSource = GetComponent<AudioSource>();
        originalIntensity = lightComponent.intensity;
        
        // Configure audio source
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = soundVolume;
    }

    public void StartFlicker()
    {
        if (isFlickering) return;
        
        isFlickering = true;
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
        }
        flickerCoroutine = StartCoroutine(FlickerRoutine());

        // Play initial sound if not playing on every flicker
        if (!playOnEveryFlicker && flickerSound != null)
        {
            audioSource.PlayOneShot(flickerSound, soundVolume);
        }
    }

    public void StopFlicker()
    {
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
        }
        isFlickering = false;
        lightComponent.intensity = originalIntensity;
    }

    private IEnumerator FlickerRoutine()
    {
        float endTime = Time.time + flickerDuration;
        
        while (Time.time < endTime)
        {
            // Randomly choose between min and max intensity
            bool isMax = Random.value > 0.5f;
            lightComponent.intensity = isMax ? maxIntensity : minIntensity;
            
            // Play sound on every flicker if enabled
            if (playOnEveryFlicker && flickerSound != null && isMax)
            {
                audioSource.PlayOneShot(flickerSound, soundVolume);
            }
            
            yield return new WaitForSeconds(flickerInterval);
        }
        
        // Return to original state
        lightComponent.intensity = originalIntensity;
        isFlickering = false;
    }

    // Optional: Method to update settings at runtime
    public void UpdateSettings(float duration, float interval, float min, float max, float volume)
    {
        flickerDuration = duration;
        flickerInterval = interval;
        minIntensity = min;
        maxIntensity = max;
        soundVolume = volume;
        audioSource.volume = volume;
    }
}