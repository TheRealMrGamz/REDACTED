using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunniDoor : InteractableBase
{
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip lockSound; // Optional sound for when door locks
    [SerializeField] public AudioSource audioSource;

    private bool isOpen;
    private bool isLocked = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    private void Start()
    {
        closedRotation = transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(0f, 0f, openAngle);
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
    }

    public override void OnInteract(PSXFirstPersonController player)
    {
        // If the door is locked, don't allow interaction
        if (isLocked)
        {
            // Optional: Play a locked sound or display a message
            return;
        }

        isOpen = !isOpen;
        interactionPrompt = isOpen ? "Close" : "Open";

        audioSource.PlayOneShot(isOpen ? openSound : closeSound);

        base.OnInteract(player);
    }

    /// <summary>
    /// Closes the door and locks it permanently
    /// </summary>
    public void CloseAndLock()
    {
        // Only do something if the door isn't already closed and locked
        if (isOpen || !isLocked)
        {
            // Set the door to closed state
            isOpen = false;
            
            // Update the interaction prompt (optional)
            interactionPrompt = "Locked";
            
            // Lock the door
            isLocked = true;
            
            // Play close sound
            if (audioSource != null && closeSound != null)
            {
                audioSource.PlayOneShot(closeSound);
                
                // Optional: Play lock sound if available
                if (lockSound != null)
                {
                    // Wait a moment then play lock sound
                    StartCoroutine(PlayLockSoundDelayed());
                }
            }
        }
    }

    private IEnumerator PlayLockSoundDelayed()
    {
        // Wait a short time for the close sound to finish
        yield return new WaitForSeconds(0.3f);
        
        // Play the lock sound
        if (audioSource != null && lockSound != null)
        {
            audioSource.PlayOneShot(lockSound);
        }
    }
}