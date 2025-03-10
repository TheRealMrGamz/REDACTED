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
    [SerializeField] private bool showDebugLogs = true;
    
    private bool hasBeenTriggered = false;
    
    private void Start()
    {
        // Ensure animations don't auto-play on start
        if (firstAnimator != null)
        {
            firstAnimator.enabled = true;
            firstAnimator.speed = 0;  // Pause animator
            DebugLog("First animator initialized and paused");
        }
        
        if (secondAnimator != null)
        {
            secondAnimator.enabled = true;
            secondAnimator.speed = 0;  // Pause animator
            DebugLog("Second animator initialized and paused");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        DebugLog("Trigger entered by: " + other.name + " with tag: " + other.tag);
        
        // Check if this is the player and trigger hasn't been activated yet
        if (other.CompareTag(playerTag) && !hasBeenTriggered)
        {
            DebugLog("Player detected, starting animation sequence");
            
            // Set flag to prevent multiple activations
            hasBeenTriggered = true;
            
            // Start the first animation
            if (firstAnimator != null)
            {
                firstAnimator.speed = 1;  // Resume animation speed
                firstAnimator.Play(firstAnimationName, 0, 0f);  // Play from beginning
                DebugLog("Playing first animation: " + firstAnimationName);
                
                // Register a callback to detect when first animation ends
                StartCoroutine(WaitForAnimationToEnd());
            }
            else
            {
                DebugLog("ERROR: First animator is null!");
            }
        }
    }
    
    private System.Collections.IEnumerator WaitForAnimationToEnd()
    {
        // Wait until the current animation is done
        float animationLength = GetAnimationLength(firstAnimator, firstAnimationName);
        DebugLog($"Waiting for {firstAnimationName} to complete. Duration: {animationLength} seconds");
        
        yield return new WaitForSeconds(animationLength);
        
        // Play the second animation
        if (secondAnimator != null)
        {
            secondAnimator.speed = 1;  // Resume animation speed
            secondAnimator.Play(secondAnimationName, 0, 0f);  // Play from beginning
            DebugLog("Playing second animation: " + secondAnimationName);
        }
        else
        {
            DebugLog("ERROR: Second animator is null!");
        }
    }
    
    private float GetAnimationLength(Animator animator, string animName)
    {
        // Get animation length from animator
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in clips)
            {
                if (clip.name == animName)
                {
                    return clip.length;
                }
            }
            
            DebugLog("Warning: Animation clip not found: " + animName);
        }
        else
        {
            DebugLog("ERROR: Animator or controller is null!");
        }
        
        return 1.0f; // Default fallback duration
    }
    
    private void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[SequentialAnimationTrigger] {message}");
        }
    }
    
    // Visual debug in scene view
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Collider col = GetComponent<Collider>();
        if (col != null && col.isTrigger)
        {
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}