using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbienceAudio : MonoBehaviour
{
    [SerializeField] private float fadeInDuration = 2f;
    [SerializeField] private float fadeOutDuration = 2f;
    [SerializeField] private float maxVolume = 0.15f;
    [SerializeField] private float minVolume = 0.05f;
    
    // Dictionary to track active audio sources and their trigger transforms
    private Dictionary<Transform, AudioSource> activeAudioSources = new Dictionary<Transform, AudioSource>();
    // Dictionary to track active coroutines to stop them if needed
    private Dictionary<Transform, Coroutine> fadeCoroutines = new Dictionary<Transform, Coroutine>();
    
    // Singleton instance for easy access
    public static AmbienceAudio Instance { get; private set; }
    
    private void Awake()
    {
        // Set up singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        if (activeAudioSources.Count == 0) return;
        
        // Find the player
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) return;
        
        // Create a temporary list to avoid modifying during iteration
        List<Transform> keysToRemove = new List<Transform>();
        
        // Check for null triggers and mark them for removal
        foreach (var kvp in activeAudioSources)
        {
            if (kvp.Key == null || kvp.Value == null)
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        
        // Remove any invalid entries
        foreach (Transform key in keysToRemove)
        {
            CleanupAudioSource(key);
        }
        
        if (activeAudioSources.Count == 0) return;
        
        // Get the closest trigger
        Transform closestTrigger = null;
        float closestDistance = float.MaxValue;
        
        foreach (Transform trigger in activeAudioSources.Keys)
        {
            if (trigger == null) continue;
            
            float distance = Vector3.Distance(player.position, trigger.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTrigger = trigger;
            }
        }
        
        // Adjust volumes based on proximity
        foreach (var kvp in activeAudioSources)
        {
            Transform trigger = kvp.Key;
            AudioSource audioSource = kvp.Value;
            
            if (audioSource == null) continue;
            
            if (trigger == closestTrigger)
            {
                // The closest trigger gets max volume
                audioSource.volume = Mathf.Lerp(audioSource.volume, maxVolume, Time.deltaTime * 2f);
            }
            else
            {
                // Other active triggers get reduced volume
                audioSource.volume = Mathf.Lerp(audioSource.volume, minVolume, Time.deltaTime * 2f);
            }
        }
    }
    
    // Call this when entering a trigger zone
    public void PlayAmbienceAudio(Transform triggerTransform, AudioClip audioClip)
    {
        if (triggerTransform == null || audioClip == null) return;
        
        // Stop any ongoing fade coroutines for this trigger
        StopFadeCoroutine(triggerTransform);
        
        // If we're already playing this trigger's audio, do nothing
        if (activeAudioSources.ContainsKey(triggerTransform) && 
            activeAudioSources[triggerTransform] != null &&
            activeAudioSources[triggerTransform].clip == audioClip &&
            activeAudioSources[triggerTransform].isPlaying)
        {
            return;
        }
        
        // Create a new audio source if needed or if the previous one was destroyed
        if (!activeAudioSources.ContainsKey(triggerTransform) || activeAudioSources[triggerTransform] == null)
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.spatialBlend = 0f; // 2D sound
            newSource.loop = true;
            newSource.volume = 0f; // Start silent
            
            if (activeAudioSources.ContainsKey(triggerTransform))
            {
                activeAudioSources[triggerTransform] = newSource;
            }
            else
            {
                activeAudioSources.Add(triggerTransform, newSource);
            }
        }
        
        AudioSource audioSource = activeAudioSources[triggerTransform];
        
        // If the clip is different, change it
        if (audioSource.clip != audioClip)
        {
            audioSource.clip = audioClip;
        }
        
        // Start playing with fade in
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
            fadeCoroutines[triggerTransform] = StartCoroutine(FadeAudio(triggerTransform, true, fadeInDuration, maxVolume));
        }
    }
    
    // Call this when exiting a trigger zone
    public void StopAmbienceAudio(Transform triggerTransform)
    {
        if (triggerTransform == null) return;
        
        if (activeAudioSources.ContainsKey(triggerTransform) && activeAudioSources[triggerTransform] != null)
        {
            // Stop any ongoing fade coroutines for this trigger
            StopFadeCoroutine(triggerTransform);
            
            // Start the fade out coroutine
            fadeCoroutines[triggerTransform] = StartCoroutine(FadeOutAndRemove(triggerTransform));
        }
        else
        {
            // If the audio source is already gone, just clean up any references
            CleanupAudioSource(triggerTransform);
        }
    }
    
    private void StopFadeCoroutine(Transform triggerTransform)
    {
        if (fadeCoroutines.ContainsKey(triggerTransform) && fadeCoroutines[triggerTransform] != null)
        {
            StopCoroutine(fadeCoroutines[triggerTransform]);
            fadeCoroutines[triggerTransform] = null;
        }
    }
    
    private void CleanupAudioSource(Transform triggerTransform)
    {
        if (activeAudioSources.ContainsKey(triggerTransform))
        {
            AudioSource source = activeAudioSources[triggerTransform];
            if (source != null)
            {
                Destroy(source);
            }
            activeAudioSources.Remove(triggerTransform);
        }
        
        if (fadeCoroutines.ContainsKey(triggerTransform))
        {
            fadeCoroutines.Remove(triggerTransform);
        }
    }
    
    private IEnumerator FadeOutAndRemove(Transform triggerTransform)
    {
        yield return StartCoroutine(FadeAudio(triggerTransform, false, fadeOutDuration));
        
        // After fading out, remove the audio source and destroy it
        CleanupAudioSource(triggerTransform);
    }
    
    private IEnumerator FadeAudio(Transform triggerTransform, bool fadeIn, float duration, float targetVolume = 0.15f)
    {
        if (!activeAudioSources.ContainsKey(triggerTransform) || activeAudioSources[triggerTransform] == null)
            yield break;
            
        AudioSource audioSource = activeAudioSources[triggerTransform];
        
        float startVolume = audioSource.volume;
        float endVolume = fadeIn ? targetVolume : 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Check if audio source still exists
            if (audioSource == null || triggerTransform == null)
            {
                yield break;
            }
            
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, endVolume, elapsed / duration);
            yield return null;
        }

        // Final check before setting end volume
        if (audioSource != null)
        {
            audioSource.volume = endVolume;
            
            // If fading out, stop the audio completely
            if (!fadeIn)
            {
                audioSource.Stop();
            }
        }
    }
    
    private void OnDestroy()
    {
        // Clean up all audio sources when the manager is destroyed
        foreach (var source in activeAudioSources.Values)
        {
            if (source != null)
            {
                Destroy(source);
            }
        }
        
        activeAudioSources.Clear();
        fadeCoroutines.Clear();
    }
}