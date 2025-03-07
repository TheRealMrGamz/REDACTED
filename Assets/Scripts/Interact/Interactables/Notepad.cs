using UnityEngine;
using TMPro;

public class SimpleNotepadController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject notepadModel;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private TextMeshPro objectivesText; // Changed to TextMeshPro for a physical text component
    
    [Header("Position Settings")]
    [SerializeField] private Vector3 defaultPosition = new Vector3(0.4f, -0.3f, 0.5f);
    [SerializeField] private Vector3 viewingPosition = new Vector3(0f, -0.2f, 0.3f);
    [SerializeField] private float moveSpeed = 10f;
    
    [Header("Bobbing Settings")]
    [SerializeField] private float bobbingAmount = 0.01f;
    [SerializeField] private float bobbingSpeed = 12f;
    [SerializeField] private float runningBobbingMultiplier = 1.5f;
    
    private bool isViewing = false;
    private float bobbingTimer = 0f;
    private Vector3 lastPlayerPosition;
    private float currentBobbingAmount = 0f;
    private PhotoObjectivesManager objectivesManager; // Reference to the objectives manager

    private void Start()
    {
        // Get reference to the objectives manager
        objectivesManager = PhotoObjectivesManager.Instance;
        if (objectivesManager == null)
        {
            Debug.LogError("PhotoObjectivesManager not found in the scene!");
        }
        
        // If playerCamera wasn't assigned in inspector, try to find the main camera
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                Debug.LogError("No camera found! Please assign a camera in the inspector.");
                enabled = false;
                return;
            }
        }
        
        // If notepadModel wasn't assigned, use this GameObject
        if (notepadModel == null)
        {
            notepadModel = this.gameObject;
        }
        
        // Set up the notepad initially
        notepadModel.transform.SetParent(playerCamera.transform);
        notepadModel.transform.localPosition = defaultPosition;
        notepadModel.transform.localRotation = Quaternion.identity;
        
        // Store initial player position for movement detection
        lastPlayerPosition = playerCamera.transform.position;
        
        // Check if objectives text component exists
        if (objectivesText == null)
        {
            Debug.LogWarning("No TextMeshPro component assigned for objectives display on notepad!");
        }
        else
        {
            // Initialize objectives text on the notepad
            UpdateObjectivesOnNotepad();
        }
    }
    
    private void Update()
    {
        // Check for right mouse button input
        if (Input.GetButton("Fire2")) // Right mouse button held down
        {
            isViewing = true;
        }
        else
        {
            isViewing = false;
        }
        
        // Move the notepad based on viewing state
        Vector3 targetPosition = isViewing ? viewingPosition : defaultPosition;
        
        // Calculate the basic movement (without bobbing)
        Vector3 newPosition = Vector3.Lerp(
            notepadModel.transform.localPosition,
            targetPosition,
            Time.deltaTime * moveSpeed
        );
        
        // Apply bobbing effect
        Vector3 finalPosition = ApplyBobbing(newPosition);
        
        // Apply the final position
        notepadModel.transform.localPosition = finalPosition;
        
        // Update last player position for next frame
        lastPlayerPosition = playerCamera.transform.position;
    }
    
    private Vector3 ApplyBobbing(Vector3 basePosition)
    {
        // Check if player is moving by comparing current position to last frame's position
        Vector3 movement = playerCamera.transform.position - lastPlayerPosition;
        bool isMoving = movement.magnitude > 0.001f;
        
        // Determine if player is running (you may need to adjust this based on your input system)
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        
        // Calculate target bobbing amount
        float targetBobbing = 0f;
        if (isMoving)
        {
            targetBobbing = isRunning ? bobbingAmount * runningBobbingMultiplier : bobbingAmount;
            // Increment the timer when moving
            bobbingTimer += Time.deltaTime * bobbingSpeed;
        }
        
        // Smoothly transition to the target bobbing amount
        currentBobbingAmount = Mathf.Lerp(currentBobbingAmount, targetBobbing, Time.deltaTime * 5f);
        
        // Calculate vertical and horizontal offsets using sine and cosine
        float verticalOffset = Mathf.Sin(bobbingTimer) * currentBobbingAmount;
        float horizontalOffset = Mathf.Cos(bobbingTimer * 0.5f) * (currentBobbingAmount * 0.5f);
        
        // Create the bobbing effect
        Vector3 bobbingOffset = new Vector3(horizontalOffset, verticalOffset, 0f);
        
        // Return the position with bobbing applied
        return basePosition + bobbingOffset;
    }
    
    // New method to update objectives on the notepad
    public void UpdateObjectivesOnNotepad()
    {
        if (objectivesText == null || objectivesManager == null) return;
        
        // Get current level data
        LevelObjectives currentLevel = objectivesManager.GetLevels()[objectivesManager.GetCurrentLevelIndex()];
        
        // Format the objectives text for the notepad
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        // Add the level title
        sb.AppendLine($"<size=10><b>{currentLevel.levelName}</b></size>");
        sb.AppendLine();
        
        // Add each objective
        foreach (var objective in currentLevel.objectives)
        {
            if (!objective.isPictureRequired)
                continue;
                
            string statusMark = objective.isCompleted ? "✓" : "□";
            string objectiveText = objective.isCompleted ?
                $"<color=#666666><s>{objective.description}</s></color>" :
                objective.description;
                
            sb.AppendLine($"{statusMark} {objectiveText}");
        }
        
        // Set the text on the notepad
        objectivesText.text = sb.ToString();
    }
    
    // Public method that can be called from other scripts when objectives change
    public void RefreshObjectives()
    {
        UpdateObjectivesOnNotepad();
    }
}