using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConditionalObjectEnabler : MonoBehaviour
{
    [System.Serializable]
    public class SpawnCondition
    {
        public GameObject objectToEnable;
        public Collider triggerZone;
        public float facingAngleTolerance = 45f;
        public float visibilityCheckInterval = 0.1f;
        public float visibleDuration = 1.5f;
        public float fadeDuration = 1f;

        [HideInInspector] public bool hasBeenSeen = false;
    }

    [Header("Spawn Conditions")]
    public List<SpawnCondition> spawnConditions = new List<SpawnCondition>();

    private Camera mainCamera;
    private GameObject player;

    void Start()
    {
        mainCamera = Camera.main;
        player = GameObject.FindGameObjectWithTag("Player");

        foreach (var condition in spawnConditions)
        {
            if (condition.objectToEnable != null)
            {
                PrepareObjectForFading(condition.objectToEnable);
                condition.objectToEnable.SetActive(false);
            }
        }

        if (player == null)
            Debug.LogError("No player found! Ensure player is tagged.");
    }

    void PrepareObjectForFading(GameObject obj)
    {
        // Convert all materials to fade mode
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                
                // Switch to Standard Shader with Transparent rendering mode
                material.shader = Shader.Find("Standard");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
            }
            renderer.materials = materials;
        }
    }

    void Update()
    {
        CheckAndEnableObjects();
    }

    void CheckAndEnableObjects()
    {
        foreach (SpawnCondition condition in spawnConditions)
        {
            if (condition.hasBeenSeen)
                continue;

            if (condition.triggerZone != null && 
                !condition.triggerZone.bounds.Contains(player.transform.position))
                continue;

            if (!IsPlayerFacingAway(condition.objectToEnable.transform.position, condition.facingAngleTolerance))
                continue;

            condition.objectToEnable.SetActive(true);
            StartCoroutine(CheckObjectVisibility(condition));
        }
    }

    bool IsPlayerFacingAway(Vector3 objectPosition, float angleTolerance)
    {
        if (player == null) return false;

        Vector3 directionToObject = (objectPosition - player.transform.position).normalized;
        float angle = Vector3.Angle(player.transform.forward, directionToObject);

        return angle > angleTolerance;
    }

    IEnumerator CheckObjectVisibility(SpawnCondition condition)
    {
        bool isVisible = false;
        float visibilityTimer = 0f;

        while (!condition.hasBeenSeen)
        {
            yield return new WaitForSeconds(condition.visibilityCheckInterval);

            bool currentlyVisible = IsObjectVisible(condition.objectToEnable);

            if (currentlyVisible)
            {
                visibilityTimer += condition.visibilityCheckInterval;
                isVisible = true;

                if (visibilityTimer >= condition.visibleDuration)
                {
                    condition.hasBeenSeen = true;
                    StartCoroutine(FadeOutObject(condition.objectToEnable, condition.fadeDuration));
                    break;
                }
            }
            else if (isVisible)
            {
                visibilityTimer = 0f;
                isVisible = false;
            }
        }
    }

    bool IsObjectVisible(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogWarning("No renderer found on object. Using alternative visibility check.");
            return IsInCameraView(obj.transform.position);
        }

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }

    bool IsInCameraView(Vector3 worldPosition)
    {
        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(worldPosition);
        return (viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
                viewportPoint.z > 0);
    }

    IEnumerator FadeOutObject(GameObject obj, float duration)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            
            foreach (Renderer renderer in renderers)
            {
                Material[] materials = renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    Color color = materials[i].color;
                    materials[i].color = new Color(color.r, color.g, color.b, alpha);
                }
                renderer.materials = materials;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        obj.SetActive(false);
    }
}