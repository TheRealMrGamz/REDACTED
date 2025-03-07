using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

namespace Interact.Interactables.Transport
{
    public class CinematicCarTransport : InteractableBase
    {
        [Header("Scene Transition")]
        [SerializeField] private string targetSceneName;
        [SerializeField] private float fadeDuration = 2f; // Increased fade duration
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private bool requireAllObjectivesComplete = true;

        [Header("Car Components")]
        [SerializeField] private GameObject carObject;
        [SerializeField] private Animator carAnimator;
        private MeshRenderer carMeshRenderer;

        [Header("Cameras")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Camera drivingCinematicCamera;
        [SerializeField] private Camera revealCinematicCamera;

        [Header("Objects to Swap")]
        [SerializeField] private GameObject[] objectsToDisable;
        [SerializeField] private GameObject[] objectsToEnable;
        [SerializeField] private AudioClip[] objectRevealSounds;

        [Header("Audio")]
        [SerializeField] private AudioClip interactClip;
        [SerializeField] private AudioClip carDriveClip;
        [SerializeField] private AudioClip postDriveAmbientClip; // New audio clip for after driving
        [SerializeField] private AudioClip revealTransitionClip;
        [SerializeField] private AudioSource audioSource;

        [Header("Transition Timings")]
        [SerializeField] private float revealPauseDuration = 3f; // Increased pause duration
        [SerializeField] private float interObjectRevealDelay = 0.75f; // Increased delay
        [SerializeField] private float betweenStageDelay = 0.75f; // Increased delay

        private bool isInteracting = false;

        private void Start()
        {
            // Ensure initial setup
            if (carAnimator != null)
            {
                carAnimator.enabled = false;
            }

            if (drivingCinematicCamera != null)
            {
                drivingCinematicCamera.gameObject.SetActive(false);
            }

            if (revealCinematicCamera != null)
            {
                revealCinematicCamera.gameObject.SetActive(false);
            }

            if (carObject != null)
            {
                carMeshRenderer = carObject.GetComponent<MeshRenderer>();
            }

            // Hide objects to enable initially
            foreach (GameObject obj in objectsToEnable)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }

        public override void OnInteract(PSXFirstPersonController player)
        {
            if (isInteracting) return;

            if (requireAllObjectivesComplete && !AreAllRequiredObjectivesCompleted())
            {
                NotificationManager.Instance.ShowObjectivesNotification();
                return;
            }
            
            isInteracting = true;

            // Play interaction sound
            if (interactClip != null && audioSource != null)
            {
                audioSource.clip = interactClip;
                audioSource.Play();
            }

            StartCoroutine(CinematicTransition());
        }

        private bool AreAllRequiredObjectivesCompleted()
        {
            if (PhotoObjectivesManager.Instance == null)
            {
                Debug.LogError("PhotoObjectivesManager not found in the scene!");
                return false;
            }

            var levels = PhotoObjectivesManager.Instance.GetLevels();
            var currentLevel = levels[PhotoObjectivesManager.Instance.GetCurrentLevelIndex()];

            return currentLevel.objectives
                .Where(obj => obj.isPictureRequired)
                .All(obj => obj.isCompleted);
        }

        private IEnumerator CinematicTransition()
        {
            // Initial fade to black
            yield return StartCoroutine(FadeScreen(1f, false));
            
            // Fade from black - Preparing for car
            yield return StartCoroutine(FadeScreen(0f, false));
            yield return new WaitForSeconds(betweenStageDelay);

            // Disable main camera, activate driving cinematic camera
            mainCamera.gameObject.SetActive(false);
            drivingCinematicCamera.gameObject.SetActive(true);

            // Activate car
            carObject.SetActive(true);

            // Enable animator and play drive animation
            carAnimator.enabled = true;
            carAnimator.Play("Drive");

            // Play car drive sound
            if (carDriveClip != null && audioSource != null)
            {
                audioSource.clip = carDriveClip;
                audioSource.Play();
            }

            // Wait for drive animation to complete
            yield return new WaitForSeconds(carDriveClip.length);

            // Play post-drive ambient sound
            if (postDriveAmbientClip != null && audioSource != null)
            {
                audioSource.clip = postDriveAmbientClip;
                audioSource.Play();
            }

            // Fade to black - Preparing for object swap
            yield return StartCoroutine(FadeScreen(1f, false));
            yield return new WaitForSeconds(betweenStageDelay);

            // Disable car's MeshRenderer
            if (carMeshRenderer != null)
            {
                carMeshRenderer.enabled = false;
            }

            // Disable driving camera, activate reveal camera
            drivingCinematicCamera.gameObject.SetActive(false);
            revealCinematicCamera.gameObject.SetActive(true);

            // Play reveal transition sound
            if (revealTransitionClip != null && audioSource != null)
            {
                audioSource.clip = revealTransitionClip;
                audioSource.Play();
            }

            // Fade from black - Revealing objects
            yield return StartCoroutine(FadeScreen(0f, false));
            yield return new WaitForSeconds(betweenStageDelay);

            // Disable and enable objects simultaneously
            if (objectsToDisable != null && objectsToEnable != null)
            {
                // Disable objects
                for (int i = 0; i < objectsToDisable.Length; i++)
                {
                    if (objectsToDisable[i] != null)
                    {
                        objectsToDisable[i].SetActive(false);
                    }
                }

                // Enable objects with sounds and delays
                for (int i = 0; i < objectsToEnable.Length; i++)
                {
                    if (objectsToEnable[i] != null)
                    {
                        objectsToEnable[i].SetActive(true);

                        // Play corresponding reveal sound if available
                        if (objectRevealSounds != null && i < objectRevealSounds.Length && objectRevealSounds[i] != null && audioSource != null)
                        {
                            audioSource.clip = objectRevealSounds[i];
                            audioSource.Play();
                        }

                        // Wait between revealing objects
                        yield return new WaitForSeconds(interObjectRevealDelay);
                    }
                }
            }

            // Pause to let player observe revealed objects
            yield return new WaitForSeconds(revealPauseDuration);

            // Final sequence of fades
            yield return StartCoroutine(FadeScreen(0.5f, false)); // Partial fade
            yield return new WaitForSeconds(betweenStageDelay);
            yield return StartCoroutine(FadeScreen(1f, false)); // Full fade to black

            // Disable reveal camera
            revealCinematicCamera.gameObject.SetActive(false);

            isInteracting = false;
            SceneManager.LoadScene(targetSceneName);

        }

        private IEnumerator FadeScreen(float targetAlpha, bool playAudio)
        {
            // Audio playing can be added here if needed
            if (playAudio && audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();
            }

            float elapsedTime = 0f;
            float startAlpha = fadeCanvasGroup.alpha;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / fadeDuration;
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);
                yield return null;
            }

            fadeCanvasGroup.alpha = targetAlpha;
        }
    }
}