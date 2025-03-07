using UnityEngine;

public class PhoneTrigger : MonoBehaviour
{
    [SerializeField] private Phone targetPhone;

    private void OnTriggerEnter(Collider other)
    {
        PSXFirstPersonController player = other.GetComponent<PSXFirstPersonController>();
        
        if (player != null && targetPhone != null)
        {
            targetPhone.ActivatePhone();
        }
    }
}