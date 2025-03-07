using UnityEngine;

public class AmbienceTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip ambienceClip;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Tell the ambience system to play this clip
            AmbienceAudio.Instance.PlayAmbienceAudio(transform, ambienceClip);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Tell the ambience system to fade out this clip
            AmbienceAudio.Instance.StopAmbienceAudio(transform);
        }
    }
}