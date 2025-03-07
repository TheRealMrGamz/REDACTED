using UnityEngine;

public class OneTimeInteractable : InteractableBase
{
    [SerializeField] private string interactionMessage = "Interact";
    private bool hasInteracted = false;

    private void Start()
    {
        interactionPrompt = interactionMessage;
    }

    public override void OnInteract(PSXFirstPersonController player)
    {
        if (hasInteracted) return;

        hasInteracted = true;
    }
}