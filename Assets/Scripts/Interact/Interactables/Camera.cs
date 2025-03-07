﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.IO;
using TMPro;

public class EnhancedCamera : InteractableBase
{
    [Header("Camera Settings")]
    [SerializeField] private Transform viewfinderPosition;
    [SerializeField] private GameObject viewfinderDisplay;
    [SerializeField] private AudioClip cameraSound;
    [SerializeField] private AudioClip equipSound;
    [SerializeField] private AudioClip focusSound;
    [SerializeField] private AudioClip filmAdvanceSound;
    
    [Header("Camera Model References")]
    [SerializeField] private GameObject cameraViewmodel;
    [SerializeField] private GameObject cameraModel;
    [SerializeField] private bool moveEntireViewmodel = false;
    
    [Header("Camera Features")]
    [SerializeField] private float[] apertureStops = { 1.4f, 2f, 2.8f, 4f, 5.6f, 8f, 11f, 16f };
    [SerializeField] private float[] shutterSpeeds = { 1000f, 500f, 250f, 125f, 60f, 30f, 15f };
    [SerializeField] private float[] focalLengths = { 35f, 50f, 85f };
    [SerializeField] private float focusSpeed = 5f;
    [SerializeField] private float maxFocusDistance = 10f;
    [SerializeField] private int maxFilmCount = 24;
    
    [Header("Lens Settings")]
    [SerializeField] private string[] customLensScriptNames; // Changed to array
    [SerializeField] private Vector3 lensPosition = Vector3.zero;
    [SerializeField] private bool isLensFlipped = false;
    
    [SerializeField] private Vector2 viewfinderResolution = new Vector2(512, 512);
    [SerializeField] private float viewfinderScale = 0.1f;
    
    [Header("Flash Effect")]
    [SerializeField] private Light flashLight;
    [SerializeField] private float flashIntensity = 2f;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private float flashFalloff = 5f;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private GameObject flashEffect;
    
    [Header("Zoom Settings")]
    [SerializeField] private float zoomFOV = 40f;
    
    [Header("Zoom Position Settings")]
    [SerializeField] private Vector3 zoomedPosition = new Vector3(0f, -0.2f, 0.3f);
    [SerializeField] private Vector3 defaultPosition = new Vector3(0.4f, -0.3f, 0.5f);
    [SerializeField] private float zoomPositionSpeed = 10f;
    
    [Header("Zoom Settings")]
    [SerializeField] private float defaultFOV = 60f;
    [SerializeField] private float zoomedFOV = 30f;
    [SerializeField] private float zoomSpeed = 15f;
    [SerializeField] private float maxZoomFOV = 85f;
    
    [Header("Collision Settings")]
    [SerializeField] private float collisionCheckRadius = 0.1f;
    [SerializeField] private float minCollisionDistance = 0.2f;
    [SerializeField] private float collisionPullbackFactor = 0.2f;
    
    [Header("Procedural Camera Movement")]
    [SerializeField] private float movementAmplitude = 0.02f;
    [SerializeField] private float movementSpeed = 14f;
    
    private Vector3 defaultCameraModelPos;
    private float movementTimer;
    private float currentMovementAmount;
    private float targetMovementAmount;
    
    private Camera cameraLens;
    private bool isEquipped;
    private Camera playerCamera;
    private AudioSource audioSource;
    private bool isZoomed;
    private RenderTexture viewfinderTexture;
    private RenderTexture photoTexture;
    private Material viewfinderMaterial;
    private Canvas viewfinderCanvas;
    private PSXFirstPersonController activePlayer;
    
    private int currentApertureIndex;
    private int currentShutterIndex;
    private int currentFocalLengthIndex;
    private float currentFocusDistance = 2f;
    private int remainingFilm;
    private Vector3 defaultModelLocalPos;
    private Vector3 defaultViewmodelPos;
  
  private void Awake()
{
    interactionPrompt = "Grab Camera";
    remainingFilm = maxFilmCount;
    
    if (cameraViewmodel == null)
    {
        return;
    }

    if (cameraModel == null)
    {
        Debug.LogWarning("Camera model is not assigned - using viewmodel as model");
        cameraModel = cameraViewmodel;
    }
    
    if (cameraModel != null && cameraModel.GetComponent<Camera>() == null)
    {
        GameObject cameraLensObj = new GameObject("CameraLens");
        cameraLensObj.transform.SetParent(cameraModel.transform);
        
        cameraLensObj.transform.localPosition = lensPosition;
        
        float yRotation = isLensFlipped ? 180f : 0f;
        cameraLensObj.transform.localRotation = Quaternion.Euler(0, yRotation, 0);
        
        cameraLens = cameraLensObj.AddComponent<Camera>();
        cameraLens.enabled = false; 
        
        if (customLensScriptNames != null)
        {
            foreach (string scriptName in customLensScriptNames)
            {
                if (!string.IsNullOrEmpty(scriptName))
                {
                    Type scriptType = Type.GetType(scriptName);
                    if (scriptType != null && scriptType.IsSubclassOf(typeof(MonoBehaviour)))
                    {
                        cameraLensObj.AddComponent(scriptType);
                        Debug.Log($"Successfully attached {scriptName} to camera lens");
                    }
                    else
                    {
                        Debug.LogError($"Could not find script type {scriptName} or it's not a MonoBehaviour");
                    }
                }
            }
        }
        
    }
    else if (cameraModel != null)
    {
        cameraLens = cameraModel.GetComponent<Camera>();
        
        cameraLens.transform.localPosition = lensPosition;
        float yRotation = isLensFlipped ? 180f : 0f;
        cameraLens.transform.localRotation = Quaternion.Euler(0, yRotation, 0);
        
        if (customLensScriptNames != null)
        {
            foreach (string scriptName in customLensScriptNames)
            {
                if (!string.IsNullOrEmpty(scriptName))
                {
                    Type scriptType = Type.GetType(scriptName);
                    if (scriptType != null && scriptType.IsSubclassOf(typeof(MonoBehaviour)))
                    {
                        cameraLens.gameObject.AddComponent(scriptType);
                    }
                }
            }
        }
    }
    if (flashLight == null)
    {
        GameObject flashObj = new GameObject("CameraFlash");
        flashObj.transform.SetParent(cameraModel.transform);
        flashObj.transform.localPosition = Vector3.zero;
        flashLight = flashObj.AddComponent<Light>();
        flashLight.type = LightType.Spot;
        flashLight.spotAngle = 90f;
        flashLight.range = 10f;
        flashLight.intensity = 0f;
        flashLight.color = flashColor;
    }
    if (flashEffect == null)
    {
        flashEffect = GameObject.CreatePrimitive(PrimitiveType.Quad);
        flashEffect.name = "FlashEffect";
        flashEffect.transform.SetParent(cameraModel.transform);
        flashEffect.transform.localPosition = new Vector3(0, 0, 0.2f);
        flashEffect.transform.localRotation = Quaternion.Euler(0, 180, 0);
        flashEffect.transform.localScale = new Vector3(0.2f, 0.2f, 1f);

        Material flashMaterial = new Material(Shader.Find("Unlit/Transparent"));
        flashMaterial.color = new Color(1f, 1f, 1f, 0f);
        flashEffect.GetComponent<MeshRenderer>().material = flashMaterial;
    }
    
    CreateViewfinderDisplay();
}
    
 private void CreateViewfinderDisplay()
    {
        viewfinderTexture = new RenderTexture((int)viewfinderResolution.x, (int)viewfinderResolution.y, 24);
        photoTexture = new RenderTexture(Screen.width, Screen.height, 24);
        
        if (viewfinderDisplay == null)
        {
            viewfinderDisplay = GameObject.CreatePrimitive(PrimitiveType.Quad);
            viewfinderDisplay.name = "ViewfinderDisplay";
            viewfinderDisplay.transform.SetParent(transform);
            
            if (viewfinderPosition != null)
            {
                viewfinderDisplay.transform.position = viewfinderPosition.position;
                viewfinderDisplay.transform.rotation = viewfinderPosition.rotation;
            }
            else
            {
                viewfinderDisplay.transform.localPosition = new Vector3(0, 0, 0.1f);
                viewfinderDisplay.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            
            viewfinderDisplay.transform.localScale = new Vector3(viewfinderScale, viewfinderScale * (viewfinderResolution.y / viewfinderResolution.x), 1);
        }
        
        viewfinderMaterial = new Material(Shader.Find("Unlit/Texture"));
        viewfinderMaterial.mainTexture = viewfinderTexture;
        viewfinderDisplay.GetComponent<Renderer>().material = viewfinderMaterial;
        viewfinderDisplay.SetActive(false);
        
        GameObject canvasObj = new GameObject("ViewfinderCanvas");
        viewfinderCanvas = canvasObj.AddComponent<Canvas>();
        viewfinderCanvas.renderMode = RenderMode.WorldSpace;
        canvasObj.transform.SetParent(viewfinderDisplay.transform);
        canvasObj.transform.localPosition = new Vector3(0, 0, -0.01f);
        canvasObj.transform.localRotation = Quaternion.identity;
        canvasObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        
    }
 
    private void UpdateViewfinder()
    {
        if (!isZoomed || cameraLens == null) return;
    
        Vector3 worldLensPosition = cameraModel.transform.TransformPoint(lensPosition);
        cameraLens.transform.position = worldLensPosition;
    
        if (isLensFlipped)
        {
            cameraLens.transform.rotation = cameraModel.transform.rotation * Quaternion.Euler(0, 180f, 0);
        }
        else
        {
            cameraLens.transform.rotation = cameraModel.transform.rotation;
        }
    
        float baseFOV = 70f;
        float currentFocalLength = focalLengths[currentFocalLengthIndex];
        cameraLens.fieldOfView = baseFOV * (50f / currentFocalLength);
    
        Ray ray = cameraLens.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
    
        if (Physics.Raycast(ray, out hit))
        {
            PhotoManifest[] evokingObjects = FindObjectsOfType<PhotoManifest>();
            foreach (var evokingObject in evokingObjects)
            {
                evokingObject.OnCameraView(cameraLens);
            }
        }
    
        cameraLens.targetTexture = viewfinderTexture;
        cameraLens.Render();
        cameraLens.targetTexture = null;
    }

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        photoTexture = new RenderTexture(Screen.width, Screen.height, 24);
    }
    private bool CheckCameraCollision(Vector3 targetPosition)
    {
        // Get the player camera's position
        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = targetPosition - origin;
        float distance = direction.magnitude;

        int layerMask = ~LayerMask.GetMask("Player", "PhysicalUI", "Camera");

        RaycastHit[] hits = Physics.SphereCastAll(
            origin, 
            0.1f, 
            direction.normalized, 
            distance, 
            layerMask
        );

        foreach (RaycastHit hit in hits)
        {
            if (hit.distance < 0.2f) continue;

            if (hit.collider.transform.IsChildOf(activePlayer.transform) || 
                hit.collider.transform.IsChildOf(cameraModel.transform))
            {
                continue;
            }

            return true;
        }

        return false;
    }
    
    
    private void HandleCameraInput()
{
    if (Input.GetButtonDown("Fire2"))
    {
        isZoomed = true;
        viewfinderDisplay.SetActive(true);
    }
    else if (Input.GetButtonUp("Fire2"))
    {
        isZoomed = false;
        viewfinderDisplay.SetActive(false);

        PhotoManifest[] manifestObjects = FindObjectsOfType<PhotoManifest>();
        foreach (var manifestObject in manifestObjects)
        {
            manifestObject.OnCameraExit();
        }
    }

    if (isZoomed)
    {
        UpdateViewfinder();
    
        if (Input.GetButtonDown("Fire1"))
        {
            StartCoroutine(TakePhoto());
        }
    
        HandleCameraSettings();
    }

    if (moveEntireViewmodel)
    {
        Vector3 targetPosition = isZoomed ? zoomedPosition : defaultPosition;
    
        if (CheckCameraCollision(targetPosition))
        {
            viewfinderDisplay.SetActive(false);

            targetPosition = Vector3.Lerp(defaultPosition, targetPosition, 0.2f);
        }
        else if (isZoomed)
        {
            viewfinderDisplay.SetActive(true);
        }

        cameraViewmodel.transform.localPosition = Vector3.Lerp(
            cameraViewmodel.transform.localPosition, 
            targetPosition, 
            Time.deltaTime * zoomPositionSpeed
        );
    }
    else
    {
        Vector3 targetLocalPosition = isZoomed ? 
            defaultModelLocalPos + (zoomedPosition - defaultPosition) : 
            defaultModelLocalPos;
    
        Vector3 worldTargetPosition = cameraModel.transform.parent.TransformPoint(targetLocalPosition);
    
        if (CheckCameraCollision(worldTargetPosition))
        {
            viewfinderDisplay.SetActive(false);

            targetLocalPosition = Vector3.Lerp(defaultModelLocalPos, targetLocalPosition, 0.2f);
        }
        else if (isZoomed)
        {
            viewfinderDisplay.SetActive(true);
        }
    
        cameraModel.transform.localPosition = Vector3.Lerp(
            cameraModel.transform.localPosition, 
            targetLocalPosition, 
            Time.deltaTime * zoomPositionSpeed
        );
    }
}
private void HandleCameraSettings()
{
    if (!isZoomed) return;

    if (Input.GetKeyDown(KeyCode.Q))
    {
        currentApertureIndex = (currentApertureIndex + 1) % apertureStops.Length;
        if (focusSound != null)
            audioSource.PlayOneShot(focusSound, 0.5f);

        Debug.Log($"Aperture changed to f/{apertureStops[currentApertureIndex]}");
    }

    if (Input.GetKeyDown(KeyCode.R))
    {
        currentShutterIndex = (currentShutterIndex + 1) % shutterSpeeds.Length;
        if (focusSound != null)
            audioSource.PlayOneShot(focusSound, 0.5f);

        Debug.Log($"Shutter speed changed to 1/{shutterSpeeds[currentShutterIndex]}");
    }
    

    float focusInput = Input.GetAxis("Mouse ScrollWheel");
    if (Mathf.Abs(focusInput) > 0.01f)
    {
        currentFocusDistance = Mathf.Clamp(
            currentFocusDistance + focusInput * focusSpeed,
            0.5f,
            maxFocusDistance
        );

        Debug.Log($"Focus distance: {currentFocusDistance:F2}m");
    }
}

    public override void OnInteract(PSXFirstPersonController player)
    {
        if (!isEquipped)
        {
            EquipCamera(player);
        }
        base.OnInteract(player);
    }
    
    private void Update()
    {
        if (isEquipped)
        {
            HandleCameraInput();
            HandleProceduralCameraMovement();
        }
    }
    private void HandleProceduralCameraMovement()
    {
        if (activePlayer == null) return;

        bool isMoving = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
    
        targetMovementAmount = isMoving 
            ? (Input.GetKey(KeyCode.LeftShift) ? movementAmplitude * 1.2f : movementAmplitude) 
            : 0f;
    
        if (isMoving)
        {
            movementTimer += Time.deltaTime * movementSpeed;
        }
    
        currentMovementAmount = Mathf.Lerp(currentMovementAmount, targetMovementAmount, Time.deltaTime * 6f);
    
        float verticalOffset = Mathf.Sin(movementTimer) * currentMovementAmount;
        float horizontalOffset = Mathf.Cos(movementTimer * 0.5f) * (currentMovementAmount * 0.5f);
    
        Vector3 targetPosition = defaultModelLocalPos + new Vector3(
            horizontalOffset, 
            verticalOffset, 
            0f
        );
    
        if (moveEntireViewmodel)
        {
            cameraViewmodel.transform.localPosition = Vector3.Lerp(
                cameraViewmodel.transform.localPosition,
                defaultViewmodelPos + new Vector3(horizontalOffset, verticalOffset, 0f),
                Time.deltaTime * 6f
            );
        }
        else
        {
            cameraModel.transform.localPosition = Vector3.Lerp(
                cameraModel.transform.localPosition,
                targetPosition,
                Time.deltaTime * 6f
            );
        }
    }
    private void EquipCamera(PSXFirstPersonController player)
    {
        activePlayer = player;
        playerCamera = player.GetComponentInChildren<Camera>();
        defaultFOV = playerCamera.fieldOfView;
        isEquipped = true;

        cameraViewmodel.transform.SetParent(playerCamera.transform);
        cameraViewmodel.transform.localPosition = defaultPosition;
        cameraViewmodel.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
    
        defaultViewmodelPos = cameraViewmodel.transform.localPosition;
        defaultModelLocalPos = cameraModel.transform.localPosition;
    
        activePlayer = player;
    
        playerCamera.fieldOfView = defaultFOV;

        if (equipSound != null)
        {
            audioSource.PlayOneShot(equipSound);
        }
    
        interactionPrompt = "";
    }
    private IEnumerator TakePhoto()
    {
        if (this.remainingFilm <= 0 || this.cameraLens == null)
        {
            yield break;
        }
        this.cameraLens.targetTexture = this.photoTexture;
        this.cameraLens.Render();
        this.cameraLens.targetTexture = null;
        PhotoMetadata metadata = new PhotoMetadata
        {
            playerPosition = base.transform.position,
            playerRotation = base.transform.rotation
        };
        bool flag = false;
        foreach (PhotoManifest photoManifest in FindObjectsOfType<PhotoManifest>())
        {
            Vector3 vector = this.cameraLens.WorldToViewportPoint(photoManifest.transform.position);
            if (vector.z > 0f && vector.x >= 0f && vector.x <= 1f && vector.y >= 0f && vector.y <= 1f && photoManifest.TryManifest())
            {
                flag = true;
            }
        }
        bool flag2 = PhotoObjectivesManager.Instance != null && PhotoObjectivesManager.Instance.ValidatePhoto(this.cameraLens, metadata);
        if (flag2 || flag)
        {
            if (this.cameraSound != null)
            {
                this.audioSource.PlayOneShot(this.cameraSound);
            }
            base.StartCoroutine(this.TriggerFlash());
        }
        if (this.filmAdvanceSound != null)
        {
            this.audioSource.PlayOneShot(this.filmAdvanceSound);
        }
        this.remainingFilm--;
        if (flag2 || flag)
        {
            base.StartCoroutine(this.SavePhoto());
        }
        yield break;
    }

private IEnumerator TriggerFlash()
{
    flashLight.intensity = flashIntensity;
    flashLight.color = flashColor;

    yield return new WaitForSeconds(flashDuration);

    flashLight.intensity = 0f;
}

private IEnumerator SavePhoto()
{
    Texture2D texture2D = new Texture2D(this.photoTexture.width, this.photoTexture.height, TextureFormat.RGB24, false);
    RenderTexture.active = this.photoTexture;
    texture2D.ReadPixels(new Rect(0f, 0f, (float)this.photoTexture.width, (float)this.photoTexture.height), 0, 0);
    texture2D.Apply();
    PhotoMetadata photoMetadata = new PhotoMetadata
    {
        playerPosition = base.transform.position,
        playerRotation = base.transform.rotation
    };
    string text = Path.Combine(Application.persistentDataPath, "GamePhotos");
    Directory.CreateDirectory(text);
    string str = string.Format("Photo_{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
    string text2 = Path.Combine(text, str + ".png");
    string path = Path.Combine(text, str + ".json");
    File.WriteAllBytes(text2, texture2D.EncodeToPNG());
    File.WriteAllText(path, JsonUtility.ToJson(photoMetadata));
    PhotoManager instance = PhotoManager.Instance;
    if (instance != null)
    {
        instance.AddPhoto(texture2D, photoMetadata, text2);
    }
    if (PhotoBoard.Instance != null)
    {
        PhotoBoard.Instance.AddPhoto(texture2D, photoMetadata, text2, null, null);
    }
    yield return null;
    yield break;
}
    
    private void OnDisable()
    {
        if (isEquipped && playerCamera != null)
        {
            playerCamera.fieldOfView = defaultFOV;
        }
    }
    
    private void OnDestroy()
    {
        if (photoTexture != null)
            photoTexture.Release();
    }
}