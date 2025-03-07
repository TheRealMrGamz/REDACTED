using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

[System.Serializable]
public class PhotoObjective
{
    public string objectName;
    public GameObject targetObject;
    public bool isCompleted;
    public Material secretMaterial;
    public bool isPictureRequired = true;

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
    [SerializeField] private TextMeshProUGUI objectivesList;
    
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
            if (!photoObjective.isCompleted && !(photoObjective.targetObject == null))
            {
                if (!photoObjective.isPictureRequired)
                {
                    if (IsObjectInCameraView(playerCamera, photoObjective.targetObject, 10f))
                    {
                        photoObjective.isCompleted = true;
                        flag = true;
                        lastCompletedObjective = photoObjective;
                        this.RevealSecret(photoObjective.targetObject, photoObjective);
                    }
                }
                else if (IsObjectInCameraView(playerCamera, photoObjective.targetObject, 10f))
                {
                    photoObjective.isCompleted = true;
                    flag = true;
                    lastCompletedObjective = photoObjective;
                    this.RevealSecret(photoObjective.targetObject, photoObjective);
                }
            }
        }
        if (flag)
        {
            if (objectiveCompleteSound != null)
            {
                audioSource.PlayOneShot(this.objectiveCompleteSound);
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
        if (levelTitleText == null || objectivesList == null) return;

        // Update level title
        levelTitleText.text = $"<size=150%><b><color=black>{currentLevel.levelName}</color></b></size>";

        // Update objectives list
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (var objective in currentLevel.objectives)
        {
            if (!objective.isPictureRequired)
                continue;

            string statusIndicator = objective.isCompleted ? 
                "<color=#2ecc71>[X]</color>" : 
                "<color=#e74c3c>[ ]</color>";  

            string objectiveText = objective.isCompleted ?
                $"<color=#95a5a6><s>{objective.description}</s></color>" :
                $"<color=#34495e>{objective.description}</color>"; 

            sb.AppendLine($"{statusIndicator} {objectiveText}");
        }

        objectivesList.text = sb.ToString();
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