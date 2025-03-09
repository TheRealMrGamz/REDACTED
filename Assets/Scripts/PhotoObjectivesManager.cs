using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.Events;

[System.Serializable]
public class PhotoObjective
{
    public string objectName;
    public GameObject targetObject;
    public bool isCompleted;
    public Material secretMaterial;
    public bool isPictureRequired = true;
    
    // Add trigger event support
    public UnityEvent onPhotoTaken;
    public bool triggerOnlyOnce = true;
    public bool hasTriggeredEvent = false;

    [TextArea(3, 5)]
    public string description;
}

[System.Serializable]
public class LevelObjectives
{
    public string levelName;
    public List<PhotoObjective> objectives;
}

public class PhotoObjectivesManager : MonoBehaviour
{
    public static PhotoObjectivesManager Instance { get; private set; }
    
    [SerializeField] private List<LevelObjectives> levels;
    [SerializeField] private AudioClip objectiveCompleteSound;
    
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI levelTitleText;
    [SerializeField] private TextMeshProUGUI objectivesListText;
    
    [Header("Notepad Reference")]
    [SerializeField] private SimpleNotepadController notepad; // Reference to the notepad
    
    private PhotoObjective lastCompletedObjective;
    private LevelObjectives currentLevel;
    private AudioSource audioSource;
    private int currentLevelIndex = 0;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
            
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        // Find notepad if not assigned
        if (notepad == null)
        {
            notepad = FindObjectOfType<SimpleNotepadController>();
            if (notepad == null)
            {
                Debug.LogWarning("Notepad controller not found in the scene. Objectives won't be displayed on notepad.");
            }
        }
        
        LoadLevel(currentLevelIndex);
    }
    
    public List<LevelObjectives> GetLevels()
    {
        return levels;
    }

    public int GetCurrentLevelIndex()
    {
        return currentLevelIndex;
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Count)
        {
            Debug.LogError($"Invalid level index: {levelIndex}");
            return;
        }
        
        currentLevel = levels[levelIndex];
        currentLevelIndex = levelIndex;
        
        foreach (var objective in currentLevel.objectives)
        {
            objective.isCompleted = false;
            objective.hasTriggeredEvent = false;
        }
        
        UpdateObjectivesDisplay();
        
        // Update objectives on the notepad
        if (notepad != null)
        {
            notepad.UpdateObjectivesOnNotepad();
        }
    }
    
    public bool ValidatePhoto(Camera playerCamera, PhotoMetadata metadata)
    {
        if (currentLevel == null)
        {
            return false;
        }
        bool flag = false;
        lastCompletedObjective = null;
        
        foreach (PhotoObjective photoObjective in currentLevel.objectives)
        {
            if (photoObjective.targetObject == null) continue;
            
            bool objectInView = IsObjectInCameraView(playerCamera, photoObjective.targetObject, 10f);
            
            // Handle triggers for objects in view
            if (objectInView)
            {
                // Fire trigger event even if the objective is already completed
                if (!photoObjective.hasTriggeredEvent || !photoObjective.triggerOnlyOnce)
                {
                    photoObjective.onPhotoTaken?.Invoke();
                    photoObjective.hasTriggeredEvent = true;
                }
                
                // Only mark as completed if it wasn't already completed
                if (!photoObjective.isCompleted)
                {
                    // Handle non-picture required objectives
                    if (!photoObjective.isPictureRequired)
                    {
                        photoObjective.isCompleted = true;
                        flag = true;
                        lastCompletedObjective = photoObjective;
                        RevealSecret(photoObjective.targetObject, photoObjective);
                    }
                    // Handle normal photo objectives
                    else
                    {
                        photoObjective.isCompleted = true;
                        flag = true;
                        lastCompletedObjective = photoObjective;
                        RevealSecret(photoObjective.targetObject, photoObjective);
                    }
                }
            }
        }
        
        if (flag)
        {
            if (objectiveCompleteSound != null)
            {
                audioSource.PlayOneShot(objectiveCompleteSound);
            }
            UpdateObjectivesDisplay();
            
            // Update objectives on the notepad when an objective is completed
            if (notepad != null)
            {
                notepad.UpdateObjectivesOnNotepad();
            }
            
            CheckLevelCompletion();
            return true;
        }
        return false;
    }
    
    // Take photo of a specific object by code/script (not from player camera)
    public bool TakePhotoOfObject(string objectName)
    {
        if (currentLevel == null) return false;
        
        foreach (PhotoObjective objective in currentLevel.objectives)
        {
            if (objective.objectName == objectName && objective.targetObject != null)
            {
                // Fire trigger event
                if (!objective.hasTriggeredEvent || !objective.triggerOnlyOnce)
                {
                    objective.onPhotoTaken?.Invoke();
                    objective.hasTriggeredEvent = true;
                }
                
                if (!objective.isCompleted)
                {
                    objective.isCompleted = true;
                    lastCompletedObjective = objective;
                    RevealSecret(objective.targetObject, objective);
                    
                    if (objectiveCompleteSound != null)
                    {
                        audioSource.PlayOneShot(objectiveCompleteSound);
                    }
                    
                    UpdateObjectivesDisplay();
                    
                    if (notepad != null)
                    {
                        notepad.UpdateObjectivesOnNotepad();
                    }
                    
                    CheckLevelCompletion();
                    return true;
                }
                
                return true; // Event was triggered even if already completed
            }
        }
        
        return false;
    }
    
    public PhotoObjective GetLastCompletedObjective()
    {
        return lastCompletedObjective;
    }

    private bool IsObjectInCameraView(Camera camera, GameObject targetObject, float maxDistance = 10f)
    {
        if (targetObject == null) return false;

        Renderer targetRenderer = targetObject.GetComponent<Renderer>();
        if (targetRenderer == null) return false;

        Vector3 objectCenter = targetRenderer.bounds.center;
        float distance = Vector3.Distance(camera.transform.position, objectCenter);

        if (distance > maxDistance) return false;

        Vector3 viewportPoint = camera.WorldToViewportPoint(objectCenter);
    
        int layerMask = ~LayerMask.GetMask("Player", "Camera");
        RaycastHit hit;
        if (Physics.Linecast(camera.transform.position, objectCenter, out hit, layerMask))
        {
            if (hit.collider.gameObject != targetObject)
            {
                return false;
            }
        }

        return viewportPoint.z > 0 && 
               viewportPoint.x >= 0 && viewportPoint.x <= 1 && 
               viewportPoint.y >= 0 && viewportPoint.y <= 1;
    }
    
    private void RevealSecret(GameObject hitObject, PhotoObjective objective)
    {
        Transform transform = hitObject.transform.Find("Secret");
        if (transform != null && objective.secretMaterial != null)
        {
            MeshRenderer component = transform.GetComponent<MeshRenderer>();
            if (component != null)
            {
                component.material = objective.secretMaterial;
            }
        }
    }

    private void UpdateObjectivesDisplay()
    {
        // Update level title
        if (levelTitleText != null)
        {
            levelTitleText.text = $"<size=28>{currentLevel.levelName}</size>";
        }

        // Update objectives list
        if (objectivesListText != null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            // Add "Evidence:" header with larger font size
            sb.AppendLine("<size=24><b>Evidence:</b></size>");
            sb.AppendLine();  // Add a blank line for spacing

            foreach (var objective in currentLevel.objectives)
            {
                if (!objective.isPictureRequired)
                    continue;

                string objectiveText = objective.isCompleted ?
                    $"<size=20><color=#95a5a6><s>{objective.description}</s></color></size>" :
                    $"<size=20><color=#34495e>{objective.description}</color></size>";

                sb.AppendLine(objectiveText);
            }

            objectivesListText.text = sb.ToString();
        }
    }

    private void CheckLevelCompletion()
    {
        bool allRequiredCompleted = currentLevel.objectives
            .Where(obj => obj.isPictureRequired)
            .All(obj => obj.isCompleted);

        if (allRequiredCompleted)
        {
            Debug.Log($"Level {currentLevel.levelName} completed!");
        }
    }
}