using UnityEngine;

public class Jude : InteractableBase
{
    [SerializeField] private string interactionMessage = "Interact";
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] private float[] clipChances; // Matching chance values for each clip (sum should be ≤ 1)

    private void Start()
    {
        interactionPrompt = interactionMessage;
    }

    public override void OnInteract(PSXFirstPersonController player)
    {
        if (audioSource && audioClips.Length > 0 && clipChances.Length == audioClips.Length)
        {
            float randomValue = Random.value; // Generates a value between 0 and 1
            float cumulativeChance = 0f;

            for (int i = 0; i < audioClips.Length; i++)
            {
                cumulativeChance += clipChances[i];

                if (randomValue <= cumulativeChance)
                {
                    audioSource.PlayOneShot(audioClips[i]);
                    break;
                }
            }
        }
    }
}