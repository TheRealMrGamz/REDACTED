using UnityEngine;
using System.Collections;

public class TriggerAnimation : MonoBehaviour
{
    public Animator targetAnimator;
    public string animationTriggerName = "PlayAnimation";
    public string animationStateName = "YourAnimationState"; // The actual state name in Animator
    public GameObject objectToDisable;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Trigger the animation
            targetAnimator.SetTrigger(animationTriggerName);
            
            // Start coroutine to wait for animation completion
            StartCoroutine(DisableAfterAnimation());
        }
    }
    
    private IEnumerator DisableAfterAnimation()
    {
        // Wait a small amount to ensure animation starts
        yield return new WaitForSeconds(0.1f);
        
        // Wait until we're in the animation state
        while (!targetAnimator.GetCurrentAnimatorStateInfo(0).IsName(animationStateName))
        {
            yield return null;
        }
        
        // Get info about current state
        AnimatorStateInfo stateInfo = targetAnimator.GetCurrentAnimatorStateInfo(0);
        
        // Wait until animation is done (normalizedTime >= 1 means completed)
        while (stateInfo.normalizedTime < 1)
        {
            stateInfo = targetAnimator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }
        
        // Extra small delay to be safe
        yield return new WaitForSeconds(0.05f);
        
        // Now disable the object
        if (objectToDisable != null)
        {
            objectToDisable.SetActive(false);
        }
        else
        {
            targetAnimator.gameObject.SetActive(false);
        }
    }
}