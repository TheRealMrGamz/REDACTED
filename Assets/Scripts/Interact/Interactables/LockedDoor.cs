using UnityEngine;

public class LockedDoor : Door
{
    [SerializeField] private bool isLocked = true;
    [SerializeField] private string requiredKeyID;
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip lockedSound;
    
    public override void OnInteract(PSXFirstPersonController player)
    {
        Key playerKey = player.GetComponent<InventoryController>()?.FindKey(requiredKeyID);

        if (isLocked && playerKey != null)
        {
            isLocked = false;
            interactionPrompt = "Unlock";
            playerKey.Use();

            audioSource.PlayOneShot(unlockSound);
        }

        if (!isLocked)
        {
            base.OnInteract(player);
        }
        else
        {
            interactionPrompt = "Locked";
            audioSource.PlayOneShot(lockedSound);
        }
    }

    public override string GetInteractionPrompt()
    {
        return isLocked ? "Locked" : base.GetInteractionPrompt();
    }
}