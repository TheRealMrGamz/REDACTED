using UnityEngine;
using UnityEngine.Events;

public class TriggerEventHandler : MonoBehaviour
{
    [SerializeField]
    private UnityEvent onTriggerEnter;

    [SerializeField]
    private string targetTag = "Player";
    
    [SerializeField]
    private bool useTagFilter = true;

    [SerializeField]
    private bool disableAfterTriggered = true;

    [SerializeField]
    private bool hasBeenTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenTriggered && disableAfterTriggered)
        {
            return;
        }

        if (useTagFilter && !other.CompareTag(targetTag))
        {
            return;
        }

        onTriggerEnter.Invoke();
        
        hasBeenTriggered = true;
        
        if (disableAfterTriggered)
        {
            Collider triggerCollider = GetComponent<Collider>();
            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
            }
        }
    }

    public void ResetTrigger()
    {
        hasBeenTriggered = false;
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
        }
    }
}