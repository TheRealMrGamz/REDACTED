using UnityEngine;
using System.Collections.Generic;

public class PSXFirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float runMultiplier = 1.6f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -20f;

    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 85f;

    [Header("Bob Settings")]
    [SerializeField] private float bobSpeed = 14f;
    [SerializeField] private float bobAmount = 0.05f;
    [SerializeField] private float bobTransitionSpeed = 6f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private float baseFootstepInterval = 0.5f;
    [SerializeField] private float sprintFootstepMultiplier = 0.8f;
    
    [Header("Surface Footstep Sounds")]
    [SerializeField] private FootstepSoundGroup[] surfaceFootsteps;
    [SerializeField] private AudioClip[] defaultFootstepSounds;
    [SerializeField] private float footstepRaycastDistance = 1.2f;
    [SerializeField] private LayerMask footstepRaycastLayers = -1; // Default to all layers

    [System.Serializable]
    public class FootstepSoundGroup
    {
        public string surfaceTag;
        public AudioClip[] sounds;
    }

    private CharacterController controller;
    private Camera playerCamera;
    private float cameraPitch;
    private float verticalVelocity;
    private float bobTimer;
    private Vector3 defaultCameraPos;
    private float currentBobAmount;
    private float targetBobAmount;
    private Vector2 currentInput;
    private Vector2 inputSmoothVelocity;
    [SerializeField] private float inputSmoothTime = 0.1f;

    private float lastFootstepTime;
    private Dictionary<string, AudioClip[]> footstepSoundDict = new Dictionary<string, AudioClip[]>();

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        defaultCameraPos = playerCamera.transform.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentBobAmount = 0f;
        targetBobAmount = 0f;

        // Initialize the dictionary for quick tag-to-sounds lookup
        foreach (FootstepSoundGroup group in surfaceFootsteps)
        {
            footstepSoundDict[group.surfaceTag] = group.sounds;
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleLook();
        HandleBob();
    }

    private void HandleMovement()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        currentInput = Vector2.SmoothDamp(
            currentInput,
            input,
            ref inputSmoothVelocity,
            inputSmoothTime
        );

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        float forwardSpeedMultiplier = 1f;
        float backwardSpeedMultiplier = 0.6f;
        float sidewaysSpeedMultiplier = 0.8f;

        Vector3 move = transform.right * x * sidewaysSpeedMultiplier +
                       transform.forward * z * (z > 0 ? forwardSpeedMultiplier : backwardSpeedMultiplier);

        move = Vector3.ClampMagnitude(move, 1f);

        float currentSpeed = moveSpeed;
        if (isRunning) currentSpeed *= runMultiplier;
        move *= currentSpeed;

        if (controller.isGrounded)
        {
            verticalVelocity = -1f;
            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = jumpForce;
            }
        }

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        cameraPitch = Mathf.Clamp(cameraPitch - mouseY, -maxLookAngle, maxLookAngle);

        playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleBob()
    {
        if (!controller.isGrounded)
        {
            targetBobAmount = 0f;
        }
        else
        {
            bool isMoving = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
            bool isRunning = Input.GetKey(KeyCode.LeftShift);

            targetBobAmount = isMoving ? (isRunning ? bobAmount * 1.2f : bobAmount) : 0f;

            if (isMoving)
            {
                bobTimer += Time.deltaTime * bobSpeed;

                float currentFootstepInterval = isRunning
                    ? baseFootstepInterval * sprintFootstepMultiplier
                    : baseFootstepInterval;

                // Play footstep sound at bob peaks
                if (bobTimer > Mathf.PI && Time.time - lastFootstepTime >= currentFootstepInterval)
                {
                    PlayFootstepSound();
                    lastFootstepTime = Time.time;
                    bobTimer %= Mathf.PI * 2;
                }
            }
        }

        currentBobAmount = Mathf.Lerp(currentBobAmount, targetBobAmount, Time.deltaTime * bobTransitionSpeed);

        float bobOffset = Mathf.Sin(bobTimer) * currentBobAmount;
        Vector3 targetPosition = defaultCameraPos + new Vector3(0f, bobOffset, 0f);

        playerCamera.transform.localPosition = Vector3.Lerp(
            playerCamera.transform.localPosition,
            targetPosition,
            Time.deltaTime * bobTransitionSpeed
        );
    }

    private void PlayFootstepSound()
    {
        if (footstepAudioSource == null) return;

        // Determine what surface the player is standing on
        AudioClip[] surfaceSounds = GetSurfaceFootstepSounds();
        
        if (surfaceSounds.Length > 0)
        {
            AudioClip footstepClip = surfaceSounds[Random.Range(0, surfaceSounds.Length)];

            // Randomize pitch slightly
            float originalPitch = footstepAudioSource.pitch;
            footstepAudioSource.pitch = Random.Range(0.9f, 1.1f);

            footstepAudioSource.PlayOneShot(footstepClip);

            // Reset pitch to the original value
            footstepAudioSource.pitch = originalPitch;
        }
    }

    private AudioClip[] GetSurfaceFootstepSounds()
    {
        // Cast a ray downward to detect the surface
        RaycastHit hit;
        Vector3 rayStart = transform.position + new Vector3(0, 0.1f, 0); // Slightly above player's position
        
        if (Physics.Raycast(rayStart, Vector3.down, out hit, footstepRaycastDistance, footstepRaycastLayers))
        {
            // Check if the hit object has a tag that matches our sound groups
            string surfaceTag = hit.collider.tag;
            
            if (footstepSoundDict.ContainsKey(surfaceTag))
            {
                return footstepSoundDict[surfaceTag];
            }
            
            // Debug info to help during development
            Debug.Log($"Surface detected: {surfaceTag}, but no matching sound group found.");
        }
        
        // If no surface is detected or no matching tag, use default footstep sounds
        return defaultFootstepSounds;
    }
}