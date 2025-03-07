using UnityEngine;

public class Maxwell : InteractableBase
{
    [SerializeField] private AudioClip soundToPlay;
    [SerializeField] private string interactionText = "Press E to interact with Maxwell";
    [SerializeField] private float volume = 1.0f;
    [SerializeField] private bool playOnce = false;
    
    private AudioSource audioSource;
    private bool hasPlayed = false;

    private void Start()
    {
        // Set the interaction prompt
        interactionPrompt = interactionText;
        
        // Get or add an AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure the AudioSource
        audioSource.clip = soundToPlay;
        audioSource.volume = volume;
        audioSource.playOnAwake = false;
    }

    public override void OnInteract(PSXFirstPersonController player)
    {
        // Check if we should only play the sound once
        if (playOnce && hasPlayed)
        {
            return;
        }
        
        // Play the sound
        if (audioSource != null && soundToPlay != null)
        {
            audioSource.Play();
            hasPlayed = true;
        }
        else
        {
            Debug.LogWarning("Maxwell: Missing AudioSource or AudioClip!");
        }
        
        // Call the base interaction method
        base.OnInteract(player);
    }
}