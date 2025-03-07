using UnityEngine;

public class Max : InteractableBase
{
    [SerializeField] private string interactionMessage = "Interact";
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip interactSound;
    private bool hasInteracted = false;

    private void Start()
    {
        interactionPrompt = interactionMessage;
    }

    public override void OnInteract(PSXFirstPersonController player)
    {
        if (hasInteracted) return;

        hasInteracted = true;

        if (audioSource && interactSound)
        {
            audioSource.PlayOneShot(interactSound);
        }

        interactionPrompt = "";
        GetComponent<Collider>().enabled = false;
    }
}