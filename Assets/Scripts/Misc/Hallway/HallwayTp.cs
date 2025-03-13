using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class HallwayFlickerTeleport : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Light[] hallwayLights;
    public Renderer emissionObject;
    public GameObject[] objectsToDisable;  // Objects to disable after teleport
    public GameObject[] objectsToDestroy;  // Objects to destroy after teleport

    [Header("Teleportation Settings")]
    public Transform teleportDestination;
    public float teleportHeightY = 20f;

    [Header("Flicker Settings")]
    public float flickerDuration = 3.0f;
    public float flickerMinIntensity = 0.1f;
    public float flickerMaxIntensity = 1.2f;
    public float flickerSpeed = 15f;
    public Color emissionColor = Color.white;

    private bool isTeleporting = false;
    private float[] originalLightIntensities;
    private Material emissiveMaterial;
    private Color originalEmissionColor;

    void Start()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;

        // Store original light intensities
        if (hallwayLights != null && hallwayLights.Length > 0)
        {
            originalLightIntensities = new float[hallwayLights.Length];
            for (int i = 0; i < hallwayLights.Length; i++)
            {
                if (hallwayLights[i] != null)
                    originalLightIntensities[i] = hallwayLights[i].intensity;
            }
        }

        // Get emissive material
        if (emissionObject != null && emissionObject.materials.Length >= 2)
        {
            emissiveMaterial = emissionObject.materials[1];
            if (emissiveMaterial.HasProperty("_EmissionColor"))
            {
                originalEmissionColor = emissiveMaterial.GetColor("_EmissionColor");
                emissiveMaterial.EnableKeyword("_EMISSION");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTeleporting)
        {
            StartCoroutine(FlickerAndTeleport());
        }
    }

    IEnumerator FlickerAndTeleport()
    {
        isTeleporting = true;
        float flickerTimer = 0;

        while (flickerTimer < flickerDuration)
        {
            float flicker = Mathf.Lerp(flickerMinIntensity, flickerMaxIntensity,
                                      Mathf.PerlinNoise(Time.time * flickerSpeed, 0));

            if (hallwayLights != null)
            {
                foreach (Light light in hallwayLights)
                {
                    if (light != null)
                    {
                        int index = System.Array.IndexOf(hallwayLights, light);
                        float originalIntensity = originalLightIntensities[index];
                        light.intensity = originalIntensity * flicker;
                    }
                }
            }

            if (emissiveMaterial != null && emissiveMaterial.HasProperty("_EmissionColor"))
            {
                emissiveMaterial.SetColor("_EmissionColor", emissionColor * flicker);
            }

            yield return null;
            flickerTimer += Time.deltaTime;
        }

        // Turn off lights and emission before teleport
        if (hallwayLights != null)
        {
            foreach (Light light in hallwayLights)
            {
                if (light != null) light.intensity = 0;
            }
        }
        if (emissiveMaterial != null && emissiveMaterial.HasProperty("_EmissionColor"))
        {
            emissiveMaterial.SetColor("_EmissionColor", Color.black);
        }

        // **Unload Objects Before Teleporting**
        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                Debug.Log($"Disabled: {obj.name}");
            }
        }

        foreach (GameObject obj in objectsToDestroy)
        {
            if (obj != null)
            {
                Destroy(obj);
                Debug.Log($"Destroyed: {obj.name}");
            }
        }

        // **Unload Unused Assets (helps free memory)**
        yield return Resources.UnloadUnusedAssets();
        Debug.Log("Unloaded unused assets!");

        // **Teleport Player**
        if (player != null)
        {
            Vector3 newPosition = teleportDestination != null ? teleportDestination.position
                                                              : player.position + new Vector3(0, teleportHeightY, 0);
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
                player.position = newPosition;
                controller.enabled = true;
            }
            else
            {
                player.position = newPosition;
            }

            Debug.Log($"Player teleported to {newPosition}");
        }

        yield return new WaitForSeconds(1.0f);

        // Restore lights and emission
        if (hallwayLights != null)
        {
            for (int i = 0; i < hallwayLights.Length; i++)
            {
                if (hallwayLights[i] != null)
                    hallwayLights[i].intensity = originalLightIntensities[i];
            }
        }
        if (emissiveMaterial != null && emissiveMaterial.HasProperty("_EmissionColor"))
        {
            emissiveMaterial.SetColor("_EmissionColor", originalEmissionColor);
        }

        isTeleporting = false;
    }
}
