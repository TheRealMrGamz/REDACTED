using UnityEngine;
using System.Collections;

public class Phone : InteractableBase
{
    [Header("Phone Audio Settings")]
    [SerializeField] private AudioSource loopAudioSource;
    [SerializeField] private AudioClip loopClip;
    [SerializeField] private AudioClip interactClip;
    [SerializeField] private AudioClip timeoutClip;
    
    [Header("3D Audio Configuration")]
    [SerializeField] private float spatialBlend = 1f; // 0 = 2D, 1 = 3D
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
    [SerializeField] private AnimationCurve spatialCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    [Header("Timeout Settings")]
    [SerializeField] private float timeoutDuration = 20f;

    private bool hasBeenInteracted = false;
    private bool hasPlayedTimeoutClip = false;
    private bool isPhoneActive = false;
    private Coroutine timeoutCoroutine;

    private void Awake()
    {
        ConfigureAudioSource(loopAudioSource);
    }

    private void ConfigureAudioSource(AudioSource audioSource)
    {
        if (audioSource == null) return;

        // 3D Audio Configuration
        audioSource.spatialBlend = spatialBlend;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.rolloffMode = rolloffMode;
        audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, spatialCurve);

        // Additional audio source optimizations
        audioSource.dopplerLevel = 0f; // Disable doppler effect for phone
        audioSource.spread = 0f; // Narrow sound cone
        audioSource.bypassEffects = false;
        audioSource.bypassListenerEffects = false;
        audioSource.bypassReverbZones = false;
    }

    public void ActivatePhone()
    {
        if (!isPhoneActive)
        {
            isPhoneActive = true;

            // Ensure loop audio source is set up and starts playing
            if (loopAudioSource != null && loopClip != null)
            {
                loopAudioSource.clip = loopClip;
                loopAudioSource.loop = true;
                loopAudioSource.Play();

                // Start timeout tracking
                timeoutCoroutine = StartCoroutine(TimeoutTracking());
            }
        }
    }

    public override void OnInteract(PSXFirstPersonController player)
    {
        // Only allow interaction if phone is active
        if (!isPhoneActive) return;

        // Stop the timeout tracking
        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
        }

        // Stop loop audio
        if (loopAudioSource != null)
        {
            loopAudioSource.Stop();
        }

        // Play interaction clip with 3D positioning
        if (interactClip != null)
        {
            // Create a temporary audio source for the interaction clip
            AudioSource tempSource = gameObject.AddComponent<AudioSource>();
            ConfigureAudioSource(tempSource);
            tempSource.clip = interactClip;
            tempSource.Play();

            // Destroy the temporary audio source after the clip finishes
            Destroy(tempSource, interactClip.length);
        }

        hasBeenInteracted = true;

        // Call base class interaction (will trigger any Unity events)
        base.OnInteract(player);
    }

    private IEnumerator TimeoutTracking()
    {
        yield return new WaitForSeconds(timeoutDuration);

        // Only play timeout clip if not already interacted
        if (!hasBeenInteracted && !hasPlayedTimeoutClip)
        {
            // Stop loop audio
            if (loopAudioSource != null)
            {
                loopAudioSource.Stop();
            }

            // Play timeout clip with 3D positioning
            if (timeoutClip != null)
            {
                // Create a temporary audio source for the timeout clip
                AudioSource tempSource = gameObject.AddComponent<AudioSource>();
                ConfigureAudioSource(tempSource);
                tempSource.clip = timeoutClip;
                tempSource.Play();

                // Destroy the temporary audio source after the clip finishes
                Destroy(tempSource, timeoutClip.length);

                hasPlayedTimeoutClip = true;
            }
        }
    }

    // Optional: Visualization of audio settings in scene view
    private void OnDrawGizmosSelected()
    {
        // Visualize min and max distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
    }
}