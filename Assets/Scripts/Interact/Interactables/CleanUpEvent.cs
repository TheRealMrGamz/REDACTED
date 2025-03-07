using UnityEngine;
using UnityEngine.UI; // For UI elements like Image

public class CleanupInteractable : InteractableBase
{
    [SerializeField] private GameObject objectToEnable;  // Object to enable after cleanup
    [SerializeField] private string interactionText = "Hold E to clean up";
    [SerializeField] private bool destroyInsteadOfDisable = false; // Option to destroy instead of disable
    
    [Header("Hold Interaction Settings")]
    [SerializeField] private float cleanupDuration = 2.0f; // How long to hold the key in seconds
    [SerializeField] private GameObject progressBarCanvas; // Canvas containing the progress UI
    [SerializeField] private Image progressBarFill; // Image component to display progress

    [Header("Audio Settings")]
    [SerializeField] private AudioClip cleaningSound; // Sound to play during cleanup
    [SerializeField] private float volume = 1.0f; // Volume for the cleaning sound
    [SerializeField] private bool loopAudio = true; // Whether the audio should loop

    private bool isBeingCleaned = false;
    private float cleanupProgress = 0f;
    private AudioSource audioSource; // Component to play the audio

    private void Start()
    {
        // Set the initial interaction prompt
        interactionPrompt = interactionText;
        
        // Make sure the object to enable is initially disabled
        if (objectToEnable != null)
        {
            objectToEnable.SetActive(false);
        }

        // Make sure progress bar is initially hidden
        if (progressBarCanvas != null)
        {
            progressBarCanvas.SetActive(false);
        }

        // Set up audio source component
        SetupAudioSource();
    }

    private void SetupAudioSource()
    {
        // Check if we already have an AudioSource component
        audioSource = GetComponent<AudioSource>();
        
        // If not, add one
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure the audio source
        audioSource.playOnAwake = false;
        audioSource.loop = loopAudio;
        audioSource.volume = volume;
        audioSource.clip = cleaningSound;
    }

    private void Update()
    {
        // If player is currently cleaning
        if (isBeingCleaned)
        {
            // Check if the interaction key is still being held
            if (Input.GetKey(KeyCode.E))
            {
                // Update cleanup progress
                cleanupProgress += Time.deltaTime;
                
                // Update the progress bar
                if (progressBarFill != null)
                {
                    progressBarFill.fillAmount = cleanupProgress / cleanupDuration;
                }
                
                // If cleaning is complete
                if (cleanupProgress >= cleanupDuration)
                {
                    CompleteCleanup();
                }
            }
            else
            {
                // Player released the key before cleanup was complete
                CancelCleanup();
            }
        }
    }

    public override void OnInteract(PSXFirstPersonController player)
    {
        // Start the cleanup process instead of immediately completing it
        StartCleanup();
    }

    private void StartCleanup()
    {
        isBeingCleaned = true;
        cleanupProgress = 0f;
        
        // Show the progress bar
        if (progressBarCanvas != null)
        {
            progressBarCanvas.SetActive(true);
        }
        
        // Reset progress bar fill
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = 0f;
        }

        // Play the cleaning sound
        if (cleaningSound != null && audioSource != null)
        {
            audioSource.Play();
        }
    }

    private void CancelCleanup()
    {
        isBeingCleaned = false;
        cleanupProgress = 0f;
        
        // Hide the progress bar
        if (progressBarCanvas != null)
        {
            progressBarCanvas.SetActive(false);
        }

        // Stop the cleaning sound
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private void CompleteCleanup()
    {
        // Enable the replacement object
        if (objectToEnable != null)
        {
            objectToEnable.SetActive(true);
        }
        
        // Hide the progress bar
        if (progressBarCanvas != null)
        {
            progressBarCanvas.SetActive(false);
        }

        // Stop the cleaning sound
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        // Either destroy or disable this object
        if (destroyInsteadOfDisable)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
        
        isBeingCleaned = false;
        
        // Call the base interaction method
        base.OnInteract(null);
    }
}