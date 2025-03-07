using UnityEngine;
using System.Collections;

public class LightningController : MonoBehaviour
{
    public Light skyLight;  // Assign a Directional Light in the Inspector
    public float minTime = 5f;  // Minimum time between storms
    public float maxTime = 15f; // Maximum time between storms
    public int flashCount = 3;  // Number of flashes per storm
    public float flashDelay = 0.1f; // Delay between flashes
    public float flashFadeSpeed = 10f; // How fast the flash fades
    public float minIntensity = 2f; // Minimum flash brightness
    public float maxIntensity = 5f; // Maximum flash brightness

    private float originalIntensity;

    void Start()
    {
        if (skyLight != null)
        {
            originalIntensity = skyLight.intensity;
        }
        StartCoroutine(LightningRoutine());
    }

    IEnumerator LightningRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minTime, maxTime));

            // Perform multiple flashes
            for (int i = 0; i < flashCount; i++)
            {
                StartCoroutine(FlashLightning());
                yield return new WaitForSeconds(flashDelay);
            }
        }
    }

    IEnumerator FlashLightning()
    {
        if (skyLight == null) yield break;

        float targetIntensity = Random.Range(minIntensity, maxIntensity);

        // Fast fade-in
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * flashFadeSpeed;
            skyLight.intensity = Mathf.Lerp(originalIntensity, targetIntensity, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.05f); // Brief hold at peak intensity

        // Fast fade-out
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * flashFadeSpeed;
            skyLight.intensity = Mathf.Lerp(targetIntensity, originalIntensity, t);
            yield return null;
        }
    }
}