using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class InteractableTV : InteractableBase
{
    [SerializeField] private GameObject videoPlane;           // The plane that displays the video
    [SerializeField] private VideoPlayer videoPlayer;         // Reference to the VideoPlayer component
    [SerializeField] private bool isOn = false;               // Starts off by default
    [SerializeField] private string onPrompt = "Press E to turn OFF TV";
    [SerializeField] private string offPrompt = "Press E to turn ON TV";
    
    [Header("Optional Effects")]
    [SerializeField] private GameObject[] additionalOnObjects;  // Other objects to show when TV is on (like power LEDs)
    [SerializeField] private AudioSource tvAudioSource;         // Optional audio source for TV sounds
    [SerializeField] private AudioClip powerOnSound;            // Sound when turning on
    [SerializeField] private AudioClip powerOffSound;           // Sound when turning off
    
    private void Start()
    {
        // Set initial state
        SetTVState();
        interactionPrompt = isOn ? onPrompt : offPrompt;
    }

    public override void OnInteract(PSXFirstPersonController player)
    {
        // Toggle TV state
        isOn = !isOn;
        SetTVState();
        interactionPrompt = isOn ? onPrompt : offPrompt;
        
        // Play appropriate sound effect if audio source is assigned
        if (tvAudioSource != null)
        {
            if (isOn && powerOnSound != null)
            {
                tvAudioSource.PlayOneShot(powerOnSound);
            }
            else if (!isOn && powerOffSound != null)
            {
                tvAudioSource.PlayOneShot(powerOffSound);
            }
        }
        
        base.OnInteract(player);
    }

    /// <summary>
    /// Updates the TV state (on/off) including video playback and related objects
    /// </summary>
    private void SetTVState()
    {
        // Enable/disable the video plane
        if (videoPlane != null)
        {
            videoPlane.SetActive(isOn);
        }
        
        // Handle video playback
        if (videoPlayer != null)
        {
            if (isOn)
            {
                // Start video if it's not already playing
                if (!videoPlayer.isPlaying)
                {
                    videoPlayer.Play();
                }
            }
            else
            {
                // Stop video when turning off
                videoPlayer.Stop();
            }
        }
        
        // Handle additional objects (like power LEDs, etc.)
        if (additionalOnObjects != null)
        {
            foreach (GameObject obj in additionalOnObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(isOn);
                }
            }
        }
    }
    
    /// <summary>
    /// Public method to directly control the TV state
    /// </summary>
    /// <param name="turnOn">Whether to turn the TV on (true) or off (false)</param>
    public void SetTVPower(bool turnOn)
    {
        if (isOn != turnOn)
        {
            isOn = turnOn;
            SetTVState();
            interactionPrompt = isOn ? onPrompt : offPrompt;
        }
    }
}