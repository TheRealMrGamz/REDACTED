using UnityEngine;
using System.Collections;

public class HallwayTeleporter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform originalHallway;
    [SerializeField] private Transform infiniteHallway;
    [SerializeField] private AudioSource breathingSound;
    
    [Header("Settings")]
    [SerializeField] private float lookAwayAngleThreshold = 45f;
    [SerializeField] private float lookBackAngleThreshold = -45f;
    [SerializeField] private Vector3 teleportOffset; // Offset to place player in the infinite hallway
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    private bool isInOriginalHallway = true;
    private bool isTeleporting = false;
    private Vector3 entranceDoorPosition; // Position of entrance door in original hallway
    private Vector3 lookDirection; // Direction player is looking
    private float angleToEntrance; // Angle between player's view and entrance door
    
    private void Start()
    {
        if (player == null)
            player = transform;
            
        if (playerCamera == null)
            playerCamera = Camera.main;
            
        // Store the entrance door position - adjust as needed for your level
        entranceDoorPosition = originalHallway.position - new Vector3(0, 0, 10); // Assuming door is 10 units behind hallway center
    }
    
    private void Update()
    {
        if (isTeleporting)
            return;
            
        // Calculate the direction to the entrance door
        Vector3 directionToEntrance = entranceDoorPosition - player.position;
        
        // Get player's forward direction (where they're looking)
        lookDirection = playerCamera.transform.forward;
        
        // Calculate angle between player's view and the entrance door
        angleToEntrance = Vector3.SignedAngle(lookDirection, directionToEntrance, Vector3.up);
        
        if (showDebugInfo)
            Debug.Log($"Angle to entrance: {angleToEntrance}, In original hallway: {isInOriginalHallway}");
        
        // If in original hallway and player is facing away from entrance (looking forward)
        if (isInOriginalHallway && Mathf.Abs(angleToEntrance) > lookAwayAngleThreshold)
        {
            // Start playing breathing sound when player is facing away
            if (!breathingSound.isPlaying)
                breathingSound.Play();
        }
        
        // If in original hallway, player was facing away, now looking back
        if (isInOriginalHallway && Mathf.Abs(angleToEntrance) < lookBackAngleThreshold)
        {
            StartCoroutine(TeleportToInfiniteHallway());
        }
        // If in infinite hallway, player is facing forward (away from where they came from)
        else if (!isInOriginalHallway && Mathf.Abs(angleToEntrance) > lookAwayAngleThreshold)
        {
            // We could add additional logic here if needed
        }
    }
    
    private IEnumerator TeleportToInfiniteHallway()
    {
        isTeleporting = true;
        
        // Wait one frame to ensure the player isn't looking
        yield return null;
        
        // Calculate the teleport position in the infinite hallway
        Vector3 relativePosition = player.position - originalHallway.position;
        Vector3 newPosition = infiniteHallway.position + relativePosition + teleportOffset;
        
        // Teleport the player
        player.position = newPosition;
        
        // Update state
        isInOriginalHallway = false;
        
        // Deactivate original hallway to save resources
        originalHallway.gameObject.SetActive(false);
        infiniteHallway.gameObject.SetActive(true);
        
        isTeleporting = false;
        
        if (showDebugInfo)
            Debug.Log("Teleported to infinite hallway");
    }
    
    // Call this method to reset the player back to the original hallway (for testing)
    public void ResetToOriginalHallway()
    {
        if (!isInOriginalHallway)
        {
            // Reactivate original hallway
            originalHallway.gameObject.SetActive(true);
            
            // Calculate the teleport position in the original hallway
            Vector3 relativePosition = player.position - infiniteHallway.position - teleportOffset;
            Vector3 newPosition = originalHallway.position + relativePosition;
            
            // Teleport the player
            player.position = newPosition;
            
            // Update state
            isInOriginalHallway = true;
            
            // Deactivate infinite hallway
            infiniteHallway.gameObject.SetActive(false);
            
            if (showDebugInfo)
                Debug.Log("Reset to original hallway");
        }
    }
}