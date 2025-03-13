using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SequentialLightController : MonoBehaviour
{
    [Header("Light Sequence Settings")]
    [SerializeField] private List<GameObject> lightObjects;          // The light objects to activate in sequence
    [SerializeField] private List<GameObject> emissionObjects;       // Objects that will have emission enabled
    [SerializeField] private float initialDelay = 0.5f;              // Initial time delay between lights
    [SerializeField] private float exponentialFactor = 0.8f;         // Factor for exponential speedup (lower = faster)
    [SerializeField] private float minimumDelay = 0.01f;             // Minimum delay between lights
    [SerializeField] private bool deactivateOnExit = true;           // Whether to turn off lights when player exits
    [SerializeField] private float deactivationDelay = 2.0f;         // Delay before turning off lights when player exits
    
    [Header("Sound Effects")]
    [SerializeField] private AudioClip activationSound;              // Sound played during sequential activation
    [SerializeField] private AudioSource audioSource;                // Reference to the AudioSource component
    
    private bool lightsActive = false;
    private Coroutine sequenceCoroutine;
    private Coroutine deactivationCoroutine;

    private void Start()
    {
        // Ensure all lights are initially off
        SetAllLightsState(false);
        
        // Create AudioSource if not assigned
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f; // Make sound fully 3D
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering object is the player
        if (other.CompareTag("Player"))
        {
            // Cancel any pending deactivation
            if (deactivationCoroutine != null)
            {
                StopCoroutine(deactivationCoroutine);
                deactivationCoroutine = null;
            }
            
            // Only start the sequence if the lights aren't already on
            if (!lightsActive)
            {
                sequenceCoroutine = StartCoroutine(ActivateLightsSequentially());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the exiting object is the player
        if (other.CompareTag("Player") && deactivateOnExit)
        {
            // Start deactivation with delay
            deactivationCoroutine = StartCoroutine(DeactivateLightsWithDelay());
        }
    }

    /// <summary>
    /// Activates all lights in sequence with an exponentially decreasing delay between each
    /// </summary>
    private IEnumerator ActivateLightsSequentially()
    {
        // Play activation sound
        if (activationSound != null)
        {
            audioSource.clip = activationSound;
            audioSource.Play();
        }
        
        float currentDelay = initialDelay;
        
        // Activate each light in sequence with exponentially decreasing delay
        for (int i = 0; i < lightObjects.Count; i++)
        {
            if (lightObjects[i] != null)
            {
                lightObjects[i].SetActive(true);
                
                // Calculate next delay with exponential decrease
                currentDelay *= exponentialFactor;
                
                // Ensure we don't go below minimum delay
                currentDelay = Mathf.Max(currentDelay, minimumDelay);
                
                yield return new WaitForSeconds(currentDelay);
            }
        }
        
        // Enable emission on all emission objects
        EnableEmissionOnObjects(true);
        
        lightsActive = true;
    }

    /// <summary>
    /// Waits for the specified delay then turns off all lights
    /// </summary>
    private IEnumerator DeactivateLightsWithDelay()
    {
        yield return new WaitForSeconds(deactivationDelay);
        SetAllLightsState(false);
        lightsActive = false;
    }

    /// <summary>
    /// Sets all light objects to the specified active state
    /// </summary>
    private void SetAllLightsState(bool state)
    {
        foreach (GameObject lightObj in lightObjects)
        {
            if (lightObj != null)
            {
                lightObj.SetActive(state);
            }
        }
        
        // Set emission state for all emission objects
        EnableEmissionOnObjects(state);
    }

    /// <summary>
    /// Enables or disables emission on all emission objects
    /// </summary>
    private void EnableEmissionOnObjects(bool enable)
    {
        foreach (GameObject obj in emissionObjects)
        {
            if (obj != null)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null && renderer.materials.Length > 1)
                {
                    Material emissionMaterial = renderer.materials[1];
                    
                    if (enable)
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

    /// <summary>
    /// Manually activate the light sequence
    /// </summary>
    public void ActivateSequence()
    {
        if (!lightsActive)
        {
            sequenceCoroutine = StartCoroutine(ActivateLightsSequentially());
        }
    }

    /// <summary>
    /// Manually deactivate all lights
    /// </summary>
    public void DeactivateLights()
    {
        if (lightsActive)
        {
            if (sequenceCoroutine != null)
            {
                StopCoroutine(sequenceCoroutine);
            }
            
            SetAllLightsState(false);
            lightsActive = false;
        }
    }
}