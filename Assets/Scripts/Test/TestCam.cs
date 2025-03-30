using UnityEngine;

public class CameraVisionController : MonoBehaviour
{
    [Header("Vision Settings")]
    [SerializeField] private GameObject darkOverlay;
    [SerializeField] private float normalLightIntensity = 0.3f;
    [SerializeField] private float cameraLightIntensity = 1.0f;
    
    [Header("Game Objects")]
    [SerializeField] private GameObject[] onlyVisibleWithCamera;
    
    private EnhancedCamera playerCamera;
    private Light[] sceneLights;
    
    private void Start()
    {
        // Find all scene lights
        sceneLights = FindObjectsOfType<Light>();
        
        // Store original light intensities
        foreach (Light light in sceneLights)
        {
            light.intensity = normalLightIntensity;
        }
        
        // Hide camera-only objects by default
        foreach (GameObject obj in onlyVisibleWithCamera)
        {
            if (obj != null)
                obj.SetActive(false);
        }
        
        // Find the camera script
        playerCamera = FindObjectOfType<EnhancedCamera>();
        
        if (playerCamera == null)
            Debug.LogError("No EnhancedCamera found in scene!");
            
        if (darkOverlay != null)
            darkOverlay.SetActive(true);
    }
    
    private void Update()
    {
        if (playerCamera == null) return;
        
        // Check if player is using the camera viewfinder
        bool isUsingCamera = Input.GetButton("Fire2");
        
        // Set scene lighting based on camera usage
        foreach (Light light in sceneLights)
        {
            // Don't modify the flash light from the camera
            if (light.name == "CameraFlash") continue;
            
            light.intensity = isUsingCamera ? cameraLightIntensity : normalLightIntensity;
        }
        
        // Show/hide camera-only objects
        foreach (GameObject obj in onlyVisibleWithCamera)
        {
            if (obj != null)
                obj.SetActive(isUsingCamera);
        }
        
        // Show/hide dark overlay
        if (darkOverlay != null)
            darkOverlay.SetActive(!isUsingCamera);
    }
}