using UnityEngine;

public class SequentialAnimationTrigger : MonoBehaviour
{
    [Header("First Animation")]
    [SerializeField] private Animator firstAnimator;
    [SerializeField] private string firstAnimationName = "FirstAnimation";
    
    [Header("Second Animation")]
    [SerializeField] private Animator secondAnimator;
    [SerializeField] private string secondAnimationName = "SecondAnimation";
    
    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool showDebugLogs = false;
    
    private bool hasBeenTriggered = false;
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if this is the player and trigger hasn't been activated yet
        if (other.CompareTag(playerTag) && !hasBeenTriggered)
        {
            // Set flag to prevent multiple activations
            hasBeenTriggered = true;
            
            // Start the first animation
            DebugLog("Playing first animation: " + firstAnimationName);
            firstAnimator.Play(firstAnimationName);
            
            // Register a callback to detect when first animation ends
            StartCoroutine(WaitForAnimationToEnd());
        }
    }
    
    private System.Collections.IEnumerator WaitForAnimationToEnd()
    {
        // Wait until the current animation is done
        float animationLength = GetAnimationLength(firstAnimator, firstAnimationName);
        DebugLog($"Waiting for {firstAnimationName} to complete. Duration: {animationLength} seconds");
        
        yield return new WaitForSeconds(animationLength);
        
        // Play the second animation
        DebugLog("Playing second animation: " + secondAnimationName);
        secondAnimator.Play(secondAnimationName);
    }
    
    private float GetAnimationLength(Animator animator, string animName)
    {
        // Get animation length from animator
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == animName)
            {
                return clip.length;
            }
        }
        
        DebugLog("Warning: Animation clip not found: " + animName);
        return 1.0f; // Default fallback duration
    }
    
    private void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[SequentialAnimationTrigger] {message}");
        }
    }
}