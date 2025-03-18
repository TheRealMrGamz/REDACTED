using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedLightAndEmissionTrigger : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private List<Light> lightsToControl = new List<Light>();
    [SerializeField] private Color emergencyColor = Color.red;
    [SerializeField] private float delayBeforeTurningOn = 1.0f;
    
    [Header("Emission Settings")]
    [SerializeField] private List<GameObject> emissionObjects = new List<GameObject>();
    [SerializeField] private Color emissionColor = Color.red;
    [SerializeField] private float emissionIntensity = 1.0f;
    
    [Header("Optional Settings")]
    [SerializeField] private bool deactivateOnExit = true;
    [SerializeField] private float deactivationDelay = 2.0f;
    [SerializeField] private AudioClip alarmSound;
    
    // Keep track of original light colors
    private Dictionary<Light, Color> originalLightColors = new Dictionary<Light, Color>();
    // Keep track of original emission colors and states
    private Dictionary<Material, Color> originalEmissionColors = new Dictionary<Material, Color>();
    private Dictionary<Material, bool> originalEmissionStates = new Dictionary<Material, bool>();
    
    private AudioSource audioSource;
    private bool isActive = false;
    private Coroutine sequenceCoroutine;
    private Coroutine deactivationCoroutine;
    
    void Start()
    {
        // Store original colors of all lights
        foreach (Light light in lightsToControl)
        {
            if (light != null)
            {
                originalLightColors.Add(light, light.color);
            }
        }
        
        // Store original emission states and colors
        foreach (GameObject obj in emissionObjects)
        {
            if (obj != null)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null && renderer.materials.Length > 1)
                {
                    Material emissionMaterial = renderer.materials[1];
                    originalEmissionColors.Add(emissionMaterial, emissionMaterial.GetColor("_EmissionColor"));
                    originalEmissionStates.Add(emissionMaterial, emissionMaterial.IsKeywordEnabled("_EMISSION"));
                }
            }
        }
        
        // Setup audio if provided
        if (alarmSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = alarmSound;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f; // Make sound fully 3D
        }
        
        // Ensure everything is off at start
        SetAllLightsState(false);
        SetAllEmissionState(false);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered is the player
        if (other.CompareTag("Player"))
        {
            // Cancel any pending deactivation
            if (deactivationCoroutine != null)
            {
                StopCoroutine(deactivationCoroutine);
                deactivationCoroutine = null;
            }
            
            // Only start the sequence if not already active
            if (!isActive)
            {
                sequenceCoroutine = StartCoroutine(TriggerLightSequence());
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        // Check if the object that exited is the player
        if (other.CompareTag("Player") && deactivateOnExit)
        {
            // Start deactivation with delay
            deactivationCoroutine = StartCoroutine(DeactivateWithDelay());
        }
    }
    
    // Light sequence coroutine
    private IEnumerator TriggerLightSequence()
    {
        // Turn off all lights and emission
        SetAllLightsState(false);
        SetAllEmissionState(false);
        
        // Wait for specified delay
        yield return new WaitForSeconds(delayBeforeTurningOn);
        
        // Turn lights back on with red color
        foreach (Light light in lightsToControl)
        {
            if (light != null)
            {
                light.color = emergencyColor;
                light.enabled = true;
            }
        }
        
        // Set emission to red and enable
        foreach (GameObject obj in emissionObjects)
        {
            if (obj != null)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null && renderer.materials.Length > 1)
                {
                    Material emissionMaterial = renderer.materials[1];
                    emissionMaterial.EnableKeyword("_EMISSION");
                    emissionMaterial.SetColor("_EmissionColor", emissionColor * emissionIntensity);
                }
            }
        }
        
        // Play alarm sound if available
        if (audioSource != null)
        {
            audioSource.Play();
        }
        
        isActive = true;
    }
    
    // Deactivate with delay coroutine
    private IEnumerator DeactivateWithDelay()
    {
        yield return new WaitForSeconds(deactivationDelay);
        RestoreOriginalState();
    }
    
    // Set all lights to specified state
    private void SetAllLightsState(bool state)
    {
        foreach (Light light in lightsToControl)
        {
            if (light != null)
            {
                light.enabled = state;
            }
        }
    }
    
    // Set all emission objects to specified state
    private void SetAllEmissionState(bool state)
    {
        foreach (GameObject obj in emissionObjects)
        {
            if (obj != null)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null && renderer.materials.Length > 1)
                {
                    Material emissionMaterial = renderer.materials[1];
                    
                    if (state)
                    {
                        emissionMaterial.EnableKeyword("_EMISSION");
                    }
                    else
                    {
                        emissionMaterial.DisableKeyword("_EMISSION");
                    }
                }
            }
        }
    }
    
    // Restore lights and emission to their original state
    public void RestoreOriginalState()
    {
        // Stop any active sequences
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }
        
        // Restore original light colors
        foreach (Light light in lightsToControl)
        {
            if (light != null && originalLightColors.ContainsKey(light))
            {
                light.color = originalLightColors[light];
                light.enabled = true;
            }
        }
        
        // Restore original emission colors and states
        foreach (GameObject obj in emissionObjects)
        {
            if (obj != null)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null && renderer.materials.Length > 1)
                {
                    Material emissionMaterial = renderer.materials[1];
                    
                    if (originalEmissionColors.ContainsKey(emissionMaterial))
                    {
                        emissionMaterial.SetColor("_EmissionColor", originalEmissionColors[emissionMaterial]);
                    }
                    
                    if (originalEmissionStates.ContainsKey(emissionMaterial))
                    {
                        if (originalEmissionStates[emissionMaterial])
                        {
                            emissionMaterial.EnableKeyword("_EMISSION");
                        }
                        else
                        {
                            emissionMaterial.DisableKeyword("_EMISSION");
                        }
                    }
                }
            }
        }
        
        // Stop alarm sound if playing
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        isActive = false;
    }
    
    // Public method to manually activate the trigger
    public void ActivateTrigger()
    {
        if (!isActive)
        {
            sequenceCoroutine = StartCoroutine(TriggerLightSequence());
        }
    }
    
    // Public method to manually deactivate the trigger
    public void DeactivateTrigger()
    {
        RestoreOriginalState();
    }
}