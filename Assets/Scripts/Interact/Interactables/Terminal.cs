using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class Terminal : InteractableBase
{
    [SerializeField] private GameObject ui;
    
    [System.Serializable]
    public class UserCredentials
    {
        public string username;
        public string password;
        public UserAccessLevel accessLevel;
    }
    
    public enum UserAccessLevel
    {
        Guest,
        Standard,
        Admin
    }
    
    [Header("User Authentication")]
    [SerializeField] private UserCredentials[] validUsers;
    private bool isAuthenticated = false;
    private string currentUsername = "";
    private UserAccessLevel currentAccessLevel = UserAccessLevel.Guest;
    private bool isEnteringUsername = true;
    private bool isEnteringPassword = false;
    private string passwordInput = "";
    
    [System.Serializable]
    public class SelfDestructSettings
    {
        public GameObject explosionPrefab;
        public float maxRocketHeight = 50f;
        public float finalExplosionScale = 3f;
        public float terminalLiftDelay = 0.5f;
        public float terminalLiftSpeed = 1f;
        public float terminalAcceleration = 2f;
        public float terminalRotationSpeed = 45f;
        public float cameraFollowSpeed = 8f;
        public AudioClip countdownBeep;
        public AudioClip rocketSound;
        public AudioClip explosionSound;
        [Range(0f, 1f)]
        public float soundVolume = 0.8f;
    }

    [System.Serializable]
    public class TerminalSettings
    {
        public GameObject textPlane;
        public GameObject terminalScreen;
        public Color screenActiveColor = new Color(0.2f, 0.8f, 0.4f);
        public float screenIntensity = 1.5f;
    }

    [System.Serializable]
    public class ScreenSettings
    {
        public Vector2 screenResolution = new Vector2(1024, 768);
        public Color textColor = new Color(0.2f, 0.8f, 0.4f);
        public int fontSize = 24;
        public float cursorBlinkRate = 0.5f;
        public bool enableCRTEffect = true;
        public float flickerSpeed = 5f;
        public float flickerIntensity = 0.02f;
    }

    [System.Serializable]
    public class CameraSettings
    {
        public float transitionSpeed = 5f;
        public float viewDistance = 1f;
        public float viewHeight = 1.6f;
    }

    [System.Serializable]
    public class TextSettings
    {
        public TMP_FontAsset terminalFont;
    }
    [System.Serializable]
    public class TypewriterSettings
    {
        public float characterDelay = 0.005f;     
        public float fastCharacterDelay = 0.001f;    
        public float commandDelay = 0.5f;
        public float bootScreenDelay = 1f;             
        public float shutdownDelay = 1f;
        public AudioClip[] typeSounds;
        public AudioClip[] userTypeSounds;
        [Range(0f, 1f)]
        public float typeSoundVolume = 0.5f;
        public AudioClip bootSound;
        public AudioClip staticLoopSound;
        public AudioClip shutdownSound;
    }

    private AudioSource staticAudioSource;
    private bool isExitingTemporarily = false;

    [Header("Typewriter Settings")]
    [SerializeField] private TypewriterSettings typewriterSettings;

    private const string BOOT_SCREEN = @"HyperTerminal v1.0
Copyright (C) 1983 Hyperion Enterprises
=====================================

BIOS Version 1.0
Memory Test...........OK
Memory Size...........640K

Initializing Hardware:
CPU...........Hyperion 8086/4.77MHz
FPU...........None
Display.......CGA
Drive C:......20MB
Drive A:......360K

Terminal Version 1.0
Loading command interpreter...
Starting HyperOS...
";
    private const string SHUTDOWN_MESSAGE = @"
System is shutting down...
Stopping services...
Saving system state...
Powering off...

It is now safe to turn off your computer.";
    
    [System.Serializable]
    public class TextPositionSettings
    {
        public Vector3 canvasPosition = new Vector3(0.0091f, 0f, 0.00211f);
        public Vector3 canvasRotation = new Vector3(-180f, -85.48499f, -90.00101f);
        public Vector3 canvasScale = new Vector3(0.025f, 0.025f, 0.025f);
        
        public Vector2 textSize = new Vector2(100f, 100f);
        public Vector3 textScale = Vector3.one * 0.025f;
        public Vector4 padding = new Vector4(20f, 20f, 20f, 20f);
        
        public Vector2 anchorMin = new Vector2(0.5f, 0.5f);
        public Vector2 anchorMax = new Vector2(0.5f, 0.5f);
        public Vector2 pivot = new Vector2(0.5f, 0.5f);
    }

    [Header("Text Positioning")]
    [SerializeField] private TextPositionSettings textPosition;

    [Header("Settings")]
    [SerializeField] private TerminalSettings terminalSettings;
    [SerializeField] private ScreenSettings screenSettings;
    [SerializeField] private CameraSettings cameraSettings;
    [SerializeField] private TextSettings textSettings;
    
    private Dictionary<string, string> secretCommands = new Dictionary<string, string>
    {
        { "rainbow", "Enables rainbow text mode" }
    };
    
    private bool rainbowMode = false;
    private Coroutine rainbowCoroutine;
    
    private Camera mainCamera;
    private bool isInteracting;
    private PSXFirstPersonController currentPlayer;
    private Material screenMaterial;
    private Color screenDefaultColor;
    private MeshRenderer screenRenderer;
    
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private Transform cameraTransform;
    private bool isTransitioning;
    private float transitionTime;

    private TextMeshProUGUI outputText;
    private TextMeshProUGUI inputText;
    private string currentText = "";
    private string currentInput = "";
    private bool showCursor;
    private float cursorTimer;
    private Material textPlaneMaterial;
    private Canvas textCanvas;
    private RenderTexture textTexture;
    private bool isSystemBooted = false;
    private bool isShutDown = true;
    private bool isTyping = false;
    private AudioSource audioSource;
    private string pendingText = "";
    private Coroutine typewriterCoroutine;
    private const string LOGIN_PROMPT = "Please login to continue.\nUsername:";
    
    [Header("Self Destruct")]
    [SerializeField] private SelfDestructSettings selfDestructSettings;
    [SerializeField] private GameObject objectToDisable;
    [SerializeField] private GameObject objectToEnable;

    private bool isDestroyed = false;
    private GameObject activeRocket;
    private bool awaitingConfirmation = false;
    private Vector3 terminalViewPosition;
    private Quaternion terminalViewRotation;
    private GameObject terminalContainer;
    private bool isBooting = false;
    [SerializeField] private GameObject greenLight;

    private void Awake()
    {
        InitializeScreenMaterial();
        SetupTerminalDisplay();
    
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 5f;
    
        // Create a separate audio source for the static loop
        staticAudioSource = gameObject.AddComponent<AudioSource>();
        staticAudioSource.playOnAwake = false;
        staticAudioSource.spatialBlend = 1f;
        staticAudioSource.minDistance = 1f;
        staticAudioSource.maxDistance = 5f;
        staticAudioSource.loop = true;
    
        if (validUsers == null || validUsers.Length == 0)
        {
            validUsers = new UserCredentials[]
            {
                new UserCredentials 
                { 
                    username = "admin", 
                    password = "admin123", 
                    accessLevel = UserAccessLevel.Admin 
                },
                new UserCredentials 
                { 
                    username = "user", 
                    password = "password", 
                    accessLevel = UserAccessLevel.Standard 
                },
                new UserCredentials 
                { 
                    username = "guest", 
                    password = "guest", 
                    accessLevel = UserAccessLevel.Guest 
                }
            };
        }
    }
    
    private void InitializeScreenMaterial()
    {
        screenRenderer = terminalSettings.terminalScreen.GetComponent<MeshRenderer>();
        if (screenRenderer != null)
        {
            screenMaterial = new Material(Shader.Find("Standard"));
            screenMaterial.EnableKeyword("_EMISSION");
            screenRenderer.material = screenMaterial;
            screenDefaultColor = Color.black;
            SetScreenActive(false);
        }
    }

    private void UpdateCRTEffect()
    {
        if (screenMaterial != null && screenSettings.enableCRTEffect)
        {
            screenMaterial.SetFloat("_FlickerIntensity", 
                screenSettings.flickerIntensity * (1 + Mathf.Sin(Time.time * screenSettings.flickerSpeed) * 0.5f)
            );
        }
    }

    private void SetupTerminalDisplay()
    {
        textTexture = new RenderTexture(
            (int)screenSettings.screenResolution.x,
            (int)screenSettings.screenResolution.y,
            0,
            RenderTextureFormat.ARGB32);
        textTexture.antiAliasing = 2;
        textTexture.Create();

        GameObject canvasObj = new GameObject("TerminalCanvas");
        canvasObj.transform.SetParent(transform);
        textCanvas = canvasObj.AddComponent<Canvas>();
        textCanvas.renderMode = RenderMode.WorldSpace;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = screenSettings.screenResolution;

        GameObject outputObj = new GameObject("OutputText");
        outputObj.transform.SetParent(textCanvas.transform, false);
        outputText = outputObj.AddComponent<TextMeshProUGUI>();
        ConfigureOutputText(outputText);

        GameObject inputObj = new GameObject("InputText");
        inputObj.transform.SetParent(textCanvas.transform, false);
        inputText = inputObj.AddComponent<TextMeshProUGUI>();
        ConfigureInputText(inputText);

        ConfigureTextTransforms();
        ConfigureCanvasTransform(textCanvas.transform);
        SetupTextPlaneMaterial();
        UpdateTerminalDisplay();
    }
    
    private void ConfigureOutputText(TextMeshProUGUI text)
    {
        text.font = textSettings.terminalFont;
        text.fontSize = screenSettings.fontSize;
        text.color = screenSettings.textColor;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
        
        text.margin = new Vector4(
            -285.7727f,  // Left
            20f,         // Top
            -293.2672f,  // Right
            -350.0462f   // Bottom
        );

        text.overflowMode = TextOverflowModes.Overflow;
        text.enableWordWrapping = true;
    }

    private void ConfigureInputText(TextMeshProUGUI text)
    {
        text.font = textSettings.terminalFont;
        text.fontSize = screenSettings.fontSize;
        text.color = screenSettings.textColor;
        text.alignment = TextAlignmentOptions.BottomLeft;
        text.enableWordWrapping = false;
        text.raycastTarget = false;
        text.margin = new Vector4(
            -279.2238f,  // Left
            59.67012f,   // Top
            -284.2396f,  // Right
            -7.580163f   // Bottom
        );

        text.overflowMode = TextOverflowModes.Overflow;
    }
    
    private void ConfigureTextTransforms()
    {
        RectTransform inputTransform = inputText.GetComponent<RectTransform>();
        inputTransform.anchorMin = new Vector2(0, 0);
        inputTransform.anchorMax = new Vector2(1, 0);
        inputTransform.pivot = new Vector2(0.5f, 0);
        
        inputTransform.offsetMin = new Vector2(-0.006000519f, 49.592f);
        inputTransform.offsetMax = new Vector2(0.006000519f, 99.096f);
        inputTransform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

        RectTransform outputTransform = outputText.GetComponent<RectTransform>();
        outputTransform.anchorMin = new Vector2(0, 0);
        outputTransform.anchorMax = new Vector2(1, 0);
        outputTransform.pivot = new Vector2(0.5f, 0.5f);
        
        outputTransform.offsetMin = new Vector2(0f, 50.0494f);
        outputTransform.offsetMax = new Vector2(0f, 100f/2);
        outputTransform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
    }

    private void ConfigureCanvasTransform(Transform canvasTransform)
    {
        canvasTransform.localPosition = textPosition.canvasPosition;
        canvasTransform.localRotation = Quaternion.Euler(textPosition.canvasRotation);
        canvasTransform.localScale = textPosition.canvasScale;
    }

    private void SetupTextPlaneMaterial()
    {
        if (terminalSettings.textPlane != null)
        {
            Renderer textPlaneRenderer = terminalSettings.textPlane.GetComponent<Renderer>();
            if (textPlaneRenderer != null)
            {
                textPlaneMaterial = new Material(Shader.Find("Unlit/Texture"));
                textPlaneMaterial.mainTexture = textTexture;
                textPlaneRenderer.material = textPlaneMaterial;
            }
        }
    }
    
    private void UpdateTerminalDisplay()
    {
        if (outputText == null || inputText == null) return;

        outputText.text = currentText;

        string inputDisplay = currentInput;
        if (isInteracting && showCursor && !isTyping)
        {
            inputDisplay += "_";
        }
        
        if (isEnteringPassword)
        {
            string maskedPassword = new string('*', currentInput.Length);
            inputText.text = maskedPassword;
        }
        else
        {
            inputText.text = inputDisplay;
        }
    }
    
    private void Start()
    {
        mainCamera = Camera.main;
        cameraTransform = mainCamera.transform;
        
        terminalContainer = new GameObject("TerminalContainer");
        terminalContainer.transform.position = transform.position;
        transform.SetParent(terminalContainer.transform);
        
        UpdateTerminalDisplay();
    }

    private void Update()
    {
        if (isTransitioning)
        {
            UpdateCameraTransition();
        }

        if (isInteracting && !isTransitioning)
        {
            UpdateCursorBlink();
            HandleTerminalInput();
        }
        UpdateCRTEffect();
    }

    private void UpdateCursorBlink()
    {
        cursorTimer += Time.deltaTime;
        if (cursorTimer >= screenSettings.cursorBlinkRate)
        {
            cursorTimer = 0f;
            showCursor = !showCursor;
            UpdateTerminalDisplay();
        }
    }

    public override void OnInteract(PSXFirstPersonController player)
    {
        if (isInteracting) return;

        base.OnInteract(player);
        isInteracting = true;
        currentPlayer = player;
        if (isDestroyed)
        {
            return;
        }
        if (ui != null)
        {
            ui.SetActive(false);
        }

        StartTerminalSession();
    }

    private void StartTerminalSession()
    {
        originalCameraPosition = cameraTransform.position;
        originalCameraRotation = cameraTransform.rotation;

        if (currentPlayer != null)
        {
            currentPlayer.enabled = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            greenLight.SetActive(true);

            SetScreenActive(true);
            currentInput = "";
            showCursor = true;
            cursorTimer = 0f;

            if (isShutDown)
            {
                // Terminal was shut down, need to boot up
                StartCoroutine(ShowBootSequence());
            }
            else if (!isSystemBooted)
            {
                // Terminal wasn't booted yet
                StartCoroutine(ShowBootSequence());
            }
            else
            {
                // System was already booted, just show current state
                if (!isAuthenticated)
                {
                    // Not logged in
                    currentText = "";
                    InitiateLogin();
                }
                else
                {
                    // User was already logged in
                    currentText = "";
                    StartCoroutine(TypeText($"Welcome back, {currentUsername}.\n"));
                }
            }

            isTransitioning = true;
            transitionTime = 0f;
            UpdateTerminalDisplay();
        }
    }


    private IEnumerator ShowBootSequence()
    {
        isBooting = true;
        isTyping = true;
        currentText = "";

        if (typewriterSettings.bootSound != null)
        {
            audioSource.PlayOneShot(typewriterSettings.bootSound, typewriterSettings.typeSoundVolume);
        }
    
        // Start the static loop sound
        if (typewriterSettings.staticLoopSound != null)
        {
            staticAudioSource.clip = typewriterSettings.staticLoopSound;
            staticAudioSource.volume = typewriterSettings.typeSoundVolume * 0.5f;
            staticAudioSource.Play();
        }

        string[] bootLines = BOOT_SCREEN.Split('\n');
        foreach (string line in bootLines)
        {
            float delay = line.Contains("...") ? 
                typewriterSettings.fastCharacterDelay * 0.5f : 
                typewriterSettings.characterDelay * 0.5f;
        
            yield return StartCoroutine(TypeText(line, delay));
            yield return new WaitForSeconds(0.05f);
            currentText += "\n";
        }

        yield return new WaitForSeconds(typewriterSettings.bootScreenDelay * 0.5f);

        currentText = "";
        yield return new WaitForSeconds(0.1f);
    
        isSystemBooted = true;
        isShutDown = false;
        isTyping = false;
        isBooting = false;
    
        // If we're not already authenticated, start login
        if (!isAuthenticated)
        {
            InitiateLogin();
        }
        else
        {
            // User was previously logged in, show welcome back message
            StartCoroutine(TypeText($"Welcome back, {currentUsername}.\n"));
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(TypeText("Type 'help' for available commands.\n"));
        }
    }
    
    private void InitiateLogin()
    {
        isAuthenticated = false;
        isEnteringUsername = true;
        isEnteringPassword = false;
        currentUsername = "";
        currentText = "";
        StartCoroutine(TypeText(LOGIN_PROMPT));
    }

    
    private IEnumerator ShutdownSequence()
    {
        isTyping = true;
        currentText = "";
    
        // Stop static loop sound
        staticAudioSource.Stop();
    
        // Play shutdown sound
        if (typewriterSettings.shutdownSound != null)
        {
            audioSource.PlayOneShot(typewriterSettings.shutdownSound, typewriterSettings.typeSoundVolume);
        }
    
        yield return StartCoroutine(TypeText(SHUTDOWN_MESSAGE));
        yield return new WaitForSeconds(typewriterSettings.shutdownDelay);
    
        currentText = "";
        UpdateTerminalDisplay();
        isSystemBooted = false;
        isShutDown = true;
        isAuthenticated = false; // User logged out on shutdown
        greenLight.SetActive(false);

        ExitTerminal();
        isTyping = false;
    }

    
    private IEnumerator TypeText(string text, float? customDelay = null)
    {
        isTyping = true;
        string currentOutput = currentText;
    
        foreach (char c in text)
        {
            currentOutput += c;
            currentText = currentOutput;
            UpdateTerminalDisplay();
        
            if (c != ' ' && c != '\n' && typewriterSettings.typeSounds != null && typewriterSettings.typeSounds.Length > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, typewriterSettings.typeSounds.Length);
                audioSource.PlayOneShot(typewriterSettings.typeSounds[randomIndex], typewriterSettings.typeSoundVolume);
            }
        
            yield return new WaitForSeconds(customDelay ?? typewriterSettings.characterDelay);
        }
    
        isTyping = false;
    }
    
    private void ExitTerminal()
    {
        if (isBooting) return;

        if (!isInteracting) return;

        isInteracting = false;
        isExitingTemporarily = true;
        isTransitioning = true;
        transitionTime = 0f;
        SetScreenActive(false);

        if (ui != null)
        {
            ui.SetActive(true);
        }

        StartCoroutine(ReturnToPlayerControl());
    }


    private void UpdateCameraTransition()
    {
        transitionTime += Time.deltaTime * cameraSettings.transitionSpeed;
        float t = Mathf.Clamp01(transitionTime);
        
        if (isInteracting)
        {
            Vector3 terminalCenter = terminalSettings.terminalScreen.transform.position;
            Vector3 terminalForward = terminalSettings.terminalScreen.transform.forward;
            
            Vector3 targetPosition = terminalCenter - 
                (terminalForward * cameraSettings.viewDistance) + 
                (Vector3.up * cameraSettings.viewHeight);
            
            Quaternion targetRotation = Quaternion.LookRotation(
                terminalCenter - targetPosition + terminalForward * 0.1f);
            
            cameraTransform.position = Vector3.Lerp(originalCameraPosition, targetPosition, t);
            cameraTransform.rotation = Quaternion.Lerp(originalCameraRotation, targetRotation, t);
        }
        else
        {
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, originalCameraPosition, t);
            cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, originalCameraRotation, t);
        }
        
        if (t >= 1f)
        {
            isTransitioning = false;
        }
    }

    private string ProcessCommand(string command)
    {
        command = command.Trim().ToLower();
        
        // Check for secret commands first for admin users
        if (secretCommands.ContainsKey(command) && currentAccessLevel == UserAccessLevel.Admin)
        {
            return ExecuteSecretCommand(command);
        }
        
        switch (command)
        {
            case "help":
                return GetHelpForAccessLevel();
            case "version":
                return "HyperTerminal v1.0\nBuild 19831115";
            case "shutdown":
                return InitiateShutdown();
            case "logout":
                return LogoutUser();
            case "selfdestruct":
                if (currentAccessLevel == UserAccessLevel.Admin)
                {
                    return InitiateSelfDestruct();
                }
                return "Access denied: Insufficient privileges.";
            case "confirm":
                if (currentAccessLevel == UserAccessLevel.Admin)
                {
                    return HandleConfirmation();
                }
                return "Access denied: Insufficient privileges.";
            default:
                return $"Command not found: {command}";
        }
    }
    
    private string GetHelpForAccessLevel()
    {
        string helpText = "Available commands:\n";
        
        // Common commands for all access levels
        helpText += "help           - Show this message\n";
        helpText += "version        - Show system version\n";
        helpText += "logout         - Log out of the system\n";
        
        // Additional commands based on access level
        if (currentAccessLevel == UserAccessLevel.Admin)
        {
            helpText += "shutdown       - Shutdown the system\n";
            helpText += "selfdestruct   - Initiate self-destruct sequence\n";
            helpText += "rainbow        - Toggle rainbow text mode\n";
        }
        else if (currentAccessLevel == UserAccessLevel.Standard)
        {
            helpText += "shutdown       - Shutdown the system\n";
        }
        
        return helpText;
    }
    
    private string LogoutUser()
    {
        isAuthenticated = false;
        currentUsername = "";
        currentAccessLevel = UserAccessLevel.Guest;
        
        StartCoroutine(LogoutSequence());
        return "";
    }
    
    private IEnumerator LogoutSequence()
    {
        yield return StartCoroutine(TypeText("Logging out..."));
        yield return new WaitForSeconds(1f);
        
        currentText = "";
        yield return new WaitForSeconds(0.5f);
        
        InitiateLogin();
    }
    
    private string ExecuteSecretCommand(string command)
    {
        switch (command)
        {
            case "rainbow":
                return ToggleRainbowMode();
            default:
                return "Unknown command";
        }
    }

    private string ToggleRainbowMode()
    {
        rainbowMode = !rainbowMode;
        if (rainbowMode)
        {
            if (rainbowCoroutine != null) StopCoroutine(rainbowCoroutine);
            rainbowCoroutine = StartCoroutine(RainbowEffect());
            return "Rainbow mode activated!";
        }
        else
        {
            if (rainbowCoroutine != null) StopCoroutine(rainbowCoroutine);
            outputText.color = screenSettings.textColor;
            return "Rainbow mode deactivated.";
        }
    }

    private IEnumerator RainbowEffect()
    {
        while (rainbowMode)
        {
            float hue = (Time.time * 0.5f) % 1f;
            outputText.color = Color.HSVToRGB(hue, 1f, 1f);
            yield return new WaitForSeconds(0.05f);
        }
    }
    
    private string InitiateSelfDestruct()
    {
        if (isDestroyed)
        {
            return "ERROR: Terminal no longer exists.";
        }

        if (awaitingConfirmation)
        {
            return "Awaiting confirmation. Type 'confirm' to proceed.";
        }

        awaitingConfirmation = true;
        return "WARNING: This will permanently destroy this terminal.\nType 'confirm' to proceed with self-destruct sequence.";
    }

    private string HandleConfirmation()
    {
        if (!awaitingConfirmation)
        {
            return "Nothing to confirm.";
        }

        awaitingConfirmation = false;
        StartCoroutine(ExecuteSelfDestruct());
        return "";
    }

    private IEnumerator ExecuteSelfDestruct()
    {
        isTyping = true;

        terminalViewPosition = cameraTransform.position;
        terminalViewRotation = cameraTransform.rotation;

        if (objectToDisable != null)
        {
            objectToDisable.SetActive(false);
        }
        if (objectToEnable != null)
        {
            objectToEnable.SetActive(true);
        }

        for (int i = 3; i > 0; i--)
        {
            if (selfDestructSettings.countdownBeep != null)
            {
                audioSource.PlayOneShot(selfDestructSettings.countdownBeep, selfDestructSettings.soundVolume);
            }
            yield return StartCoroutine(TypeText($"\nSelf-destruct in {i}..."));
            yield return new WaitForSeconds(1f);
        }

        Vector3 spawnPosition = terminalSettings.terminalScreen.transform.position;

        GameObject initialExplosion = Instantiate(selfDestructSettings.explosionPrefab, spawnPosition, Quaternion.identity);
        Destroy(initialExplosion, 2f);

        if (selfDestructSettings.rocketSound != null)
        {
            audioSource.PlayOneShot(selfDestructSettings.rocketSound, selfDestructSettings.soundVolume);
        }
        yield return new WaitForSeconds(selfDestructSettings.terminalLiftDelay);
        StartCoroutine(AscendTerminal());
        yield return new WaitForSeconds(3f);
        Vector3 finalExplosionPos = terminalContainer.transform.position;
        GameObject finalExplosion = Instantiate(selfDestructSettings.explosionPrefab, finalExplosionPos, Quaternion.identity);
        finalExplosion.transform.localScale *= selfDestructSettings.finalExplosionScale;

        if (selfDestructSettings.explosionSound != null)
        {
            audioSource.PlayOneShot(selfDestructSettings.explosionSound, selfDestructSettings.soundVolume);
        }
        
        isDestroyed = true;
        Application.Quit();
        Destroy(terminalContainer);
        
    #if UNITY_EDITOR
        Debug.Break();
    #endif

        isTyping = false;
        Destroy(finalExplosion, 2f);
    }
  
    private IEnumerator AscendTerminal()
    {
        float currentSpeed = selfDestructSettings.terminalLiftSpeed;
        float elapsedTime = 0f;
        Vector3 startRotation = terminalContainer.transform.rotation.eulerAngles;

        while (terminalContainer != null && 
               terminalContainer.transform.position.y < selfDestructSettings.maxRocketHeight)
        {
            elapsedTime += Time.deltaTime;
            currentSpeed += selfDestructSettings.terminalAcceleration * Time.deltaTime;
            terminalContainer.transform.position += Vector3.up * (currentSpeed * Time.deltaTime);
            Vector3 newRotation = startRotation + new Vector3(
                selfDestructSettings.terminalRotationSpeed * elapsedTime,
                selfDestructSettings.terminalRotationSpeed * elapsedTime * 0.7f,
                selfDestructSettings.terminalRotationSpeed * elapsedTime * 0.5f
            );
            terminalContainer.transform.rotation = Quaternion.Euler(newRotation);
            Vector3 directionToTerminal = (terminalContainer.transform.position - cameraTransform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTerminal);
            cameraTransform.rotation = Quaternion.Slerp(
                cameraTransform.rotation,
                targetRotation,
                Time.deltaTime * selfDestructSettings.cameraFollowSpeed
            );

            yield return null;
        }
    }

    private string InitiateShutdown()
    {
        if (!isShutDown)
        {
            StartCoroutine(ShutdownSequence());
            return "";
        }
        return "System is already shut down.";
    }
    
    private void HandleTerminalInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitTerminal();
            return;
        }

        if (isBooting || isTyping) return;

        foreach (char c in Input.inputString)
        {
            ProcessInputCharacter(c);
        }
    }

    private void ProcessInputCharacter(char c)
    {
        if (c == '\b') // Backspace
        {
            if (currentInput.Length > 0)
            {
                currentInput = currentInput[..^1];
                UpdateTerminalDisplay();
            
                // Play user typing sound for backspace
                PlayRandomUserTypeSound();
            }
        }
        else if (c == '\r' || c == '\n') // Enter
        {
            // Play user typing sound for enter
            PlayRandomUserTypeSound();
            ProcessEnterKey();
        }
        else if (!char.IsControl(c))
        {
            currentInput += c;
            UpdateTerminalDisplay();
        
            // Play user typing sound
            PlayRandomUserTypeSound();
        }
    }
    private void PlayRandomUserTypeSound()
    {
        if (typewriterSettings.userTypeSounds != null && typewriterSettings.userTypeSounds.Length > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, typewriterSettings.userTypeSounds.Length);
            audioSource.PlayOneShot(typewriterSettings.userTypeSounds[randomIndex], typewriterSettings.typeSoundVolume);
        }
    }


    private void ProcessEnterKey()
    {
        if (isTyping || isShutDown || isBooting) return;

        string input = currentInput;
        currentInput = "";
    
        if (!isAuthenticated)
        {
            if (isEnteringUsername)
            {
                currentUsername = input;
                isEnteringUsername = false;
                isEnteringPassword = true;
                currentText = "";
                StartCoroutine(TypeText($"Password:"));
            }
            else if (isEnteringPassword)
            {
                passwordInput = input;
                isEnteringPassword = false;
            
                // Attempt to authenticate
                bool authSuccess = false;
                foreach (UserCredentials user in validUsers)
                {
                    if (user.username.Equals(currentUsername, System.StringComparison.OrdinalIgnoreCase) && 
                        user.password.Equals(passwordInput))
                    {
                        authSuccess = true;
                        currentAccessLevel = user.accessLevel;
                        break;
                    }
                }
            
                if (authSuccess)
                {
                    isAuthenticated = true;
                    StartCoroutine(LoginSuccess());
                }
                else
                {
                    StartCoroutine(LoginFailed());
                }
            }
        }
        else
        {
            // Process command if authenticated
            currentText = "";
            UpdateTerminalDisplay();
            StartCoroutine(ProcessCommandWithTypewriter(input));
        }
    }

    private IEnumerator LoginSuccess()
    {
        currentText = "";
        yield return StartCoroutine(TypeText($"Login successful. Welcome, {currentUsername}.\n"));
        yield return new WaitForSeconds(0.5f);
    
        string accessLevelText = "";
        switch (currentAccessLevel)
        {
            case UserAccessLevel.Admin:
                accessLevelText = "Administrator";
                break;
            case UserAccessLevel.Standard:
                accessLevelText = "Standard User";
                break;
            case UserAccessLevel.Guest:
                accessLevelText = "Guest";
                break;
        }
    
        yield return StartCoroutine(TypeText($"Access level: {accessLevelText}\n"));
        yield return new WaitForSeconds(0.25f);
        yield return StartCoroutine(TypeText("Type 'help' for available commands.\n"));
    }
private IEnumerator LoginFailed()
{
    yield return StartCoroutine(TypeText("Login failed. Invalid username or password.\n"));
    yield return new WaitForSeconds(1f);
    
    currentText = "";
    isEnteringUsername = true;
    isEnteringPassword = false;
    currentUsername = "";
    passwordInput = "";
    
    yield return StartCoroutine(TypeText(LOGIN_PROMPT));
}
    private IEnumerator ProcessCommandWithTypewriter(string command)
    {
        yield return new WaitForSeconds(typewriterSettings.commandDelay);

        string response = ProcessCommand(command);
        
        if (!string.IsNullOrEmpty(response))
        {
            yield return StartCoroutine(TypeText($"{command}\n{response}\n"));
        }
    }


    private void SetScreenActive(bool active)
    {
        if (screenMaterial != null)
        {
            Color emissionColor = active ? 
                terminalSettings.screenActiveColor * terminalSettings.screenIntensity : 
                screenDefaultColor;
            screenMaterial.SetColor("_EmissionColor", emissionColor);
            screenMaterial.SetFloat("_Glossiness", 0);
            screenMaterial.SetFloat("_Metallic", 0);
        }
    }

    private IEnumerator ReturnToPlayerControl()
    {
        while (isTransitioning)
        {
            yield return null;
        }
        
        currentPlayer.enabled = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDestroy()
    {
        if (textTexture != null)
        {
            textTexture.Release();
        }
    
        if (screenMaterial != null)
        {
            Destroy(screenMaterial);
        }

        if (textPlaneMaterial != null)
        {
            Destroy(textPlaneMaterial);
        }

        if (audioSource != null)
        {
            Destroy(audioSource);
        }
    
        if (staticAudioSource != null)
        {
            Destroy(staticAudioSource);
        }
    }
    private void OnDrawGizmos()
    {
        if (terminalSettings.terminalScreen != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(
                terminalSettings.terminalScreen.transform.position,
                terminalSettings.terminalScreen.transform.localScale
            );
        }
    }
}