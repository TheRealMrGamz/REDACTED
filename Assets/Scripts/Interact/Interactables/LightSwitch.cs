using UnityEngine;
using System.Collections;

public class SimpleLightSwitch : InteractableBase
{
    [SerializeField] private GameObject[] onStateObjects;  // Objects to show when switched on
    [SerializeField] private GameObject[] offStateObjects; // Objects to show when switched off
    [SerializeField] private GameObject[] emissionObjects; // Objects that only change emission
    [SerializeField] private bool isOn = true;  // Starts on by default
    [SerializeField] private string onPrompt = "Press E to turn OFF";
    [SerializeField] private string offPrompt = "Press E to turn ON";
    
    [Header("Flicker Settings")]
    [SerializeField] private float flickerDuration = 3f;    // How long the entire flicker effect lasts
    [SerializeField] private float minFlickerTime = 0.05f;  // Minimum time for a single flicker
    [SerializeField] private float maxFlickerTime = 0.2f;   // Maximum time for a single flicker
    
    [Header("Sound Effects")]
    [SerializeField] private AudioClip interactionSound;    // Sound played when manually toggling the switch
    [SerializeField] private AudioClip flickerSound;        // Sound played during flickering
    [SerializeField] private AudioSource audioSource;       // Reference to the AudioSource component
    
    private bool isFlickering = false;
    private Coroutine flickerCoroutine;

    private void Start()
    {
        SetObjectStates();
        interactionPrompt = isOn ? onPrompt : offPrompt;
        
        // Create AudioSource if not assigned
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f; // Make sound fully 3D
        }
    }

    public override void OnInteract(PSXFirstPersonController player)
    {
        isOn = !isOn;
        SetObjectStates();
        interactionPrompt = isOn ? onPrompt : offPrompt;
        
        // Play interaction sound if not flickering
        if (!isFlickering && interactionSound != null)
        {
            audioSource.clip = interactionSound;
            audioSource.Play();
        }
        
        base.OnInteract(player);
    }

    /// <summary>
    /// Causes the light to flicker for a specified duration and then turns it on
    /// </summary>
    /// <param name="duration">Optional: Override the default flicker duration</param>
    public void StartFlicker(float duration = -1f)
    {
        if (!isFlickering)
        {
            // Use the parameter if provided, otherwise use the serialized value
            float actualDuration = duration > 0 ? duration : flickerDuration;
            flickerCoroutine = StartCoroutine(FlickerRoutine(actualDuration));
            
            // Play flicker sound once when starting the flicker sequence
            if (flickerSound != null)
            {
                audioSource.clip = flickerSound;
                audioSource.Play();
            }
        }
    }

    /// <summary>
    /// Immediately stops the flickering effect and turns the lights on
    /// </summary>
    public void StopFlicker()
    {
        if (isFlickering && flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            isFlickering = false;
            
            // Stop any currently playing sounds
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            // Turn the lights on
            isOn = true;
            SetObjectStates();
            interactionPrompt = onPrompt;
        }
    }
    
    public void IsOn()
    {
        isOn = true;
        SetObjectStates();
    }

    private IEnumerator FlickerRoutine(float duration)
    {
        isFlickering = true;
        float endTime = Time.time + duration;
        
        while (Time.time < endTime)
        {
            // Toggle the light state
            isOn = !isOn;
            SetObjectStates();
            
            // Wait for a random time before the next flicker
            float waitTime = Random.Range(minFlickerTime, maxFlickerTime);
            yield return new WaitForSeconds(waitTime);
        }
        
        // Always ensure we end with lights on
        isOn = true;
        SetObjectStates();
        interactionPrompt = onPrompt;
        isFlickering = false;
    }

    private void SetObjectStates()
    {
        // Handle regular on/off objects
        if (onStateObjects != null)
        {
            foreach (GameObject obj in onStateObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(isOn);
                }
            }
        }

        if (offStateObjects != null)
        {
            foreach (GameObject obj in offStateObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(!isOn);
                }
            }
        }

        // Handle emission-only objects
        if (emissionObjects != null)
        {
            foreach (GameObject obj in emissionObjects)
            {
                if (obj != null)
                {
                    Renderer renderer = obj.GetComponent<Renderer>();
                    if (renderer != null && renderer.materials.Length > 1)
                    {
                        Material emissionMaterial = renderer.materials[1];
                        
                        if (isOn)
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
    }
}