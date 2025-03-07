using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectionManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelData
    {
        public string sceneName;      // Scene name to load
        public bool isUnlocked;       // Whether the level is unlocked
        public string levelName;      // Display name for the level
        public Texture levelImage;    // Raw image texture to display for this level
    }

    [Header("Level Data")]
    public LevelData[] levels = new LevelData[3];  // Array to store level data

    [Header("UI References")]
    [SerializeField] private GameObject[] levelPanels;  // References to level panel gameObjects
    [SerializeField] private Text[] levelTexts;         // References to level name text components
    [SerializeField] private RawImage[] levelImages;    // References to level preview raw images
    [SerializeField] private Button[] levelButtons;     // References to level selection buttons
    [SerializeField] private RawImage[] lockImages;     // References to lock icon raw images

    [Header("Transition Settings")]
    [SerializeField] private CanvasGroup fadeCanvasGroup; // Canvas group for fade effect
    [SerializeField] private AudioSource transitionSound; // Audio source for transition sound
    [SerializeField] private float fadeDuration = 1.0f;   // Duration of the fade effect
    [SerializeField] private float fadeDelay = 1.0f;      // Delay before changing scene after fade completes

    private void Start()
    {
        // Make sure fade canvas is invisible at start
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0;
            fadeCanvasGroup.blocksRaycasts = false;
        }
        
        InitializeLevelUI();
    }

    private void InitializeLevelUI()
    {
        for (int i = 0; i < levels.Length && i < levelPanels.Length; i++)
        {
            // Set level name text
            if (levelTexts[i] != null)
                levelTexts[i].text = levels[i].levelName;

            // Set level preview image
            if (levelImages[i] != null && levels[i].levelImage != null)
                levelImages[i].texture = levels[i].levelImage;

            // Set up button interactability and lock visibility based on unlock status
            if (levelButtons[i] != null)
                levelButtons[i].interactable = levels[i].isUnlocked;

            if (lockImages[i] != null)
                lockImages[i].gameObject.SetActive(!levels[i].isUnlocked);

            // Add click listener to button
            int levelIndex = i; // Local variable for closure
            levelButtons[i].onClick.AddListener(() => OnLevelSelected(levelIndex));
        }
    }

    public void OnLevelSelected(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levels.Length && levels[levelIndex].isUnlocked)
        {
            StartCoroutine(LoadLevelWithTransition(levels[levelIndex].sceneName));
        }
    }

    private IEnumerator LoadLevelWithTransition(string sceneName)
    {
        // Disable all buttons during transition
        foreach (Button button in levelButtons)
        {
            button.interactable = false;
        }

        // Play transition sound
        if (transitionSound != null)
        {
            transitionSound.Play();
        }

        // Activate fade canvas and block raycasts
        fadeCanvasGroup.blocksRaycasts = true;
        
        // Fade to black
        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            fadeCanvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        fadeCanvasGroup.alpha = 1; // Ensure we reach full black

        // Wait for the specified delay after fade completes
        yield return new WaitForSeconds(fadeDelay);

        // Load the selected level
        SceneManager.LoadScene(sceneName);
    }

    // Method to unlock a level (call this when player completes a level)
    public void UnlockLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levels.Length)
        {
            levels[levelIndex].isUnlocked = true;
            
            // Update UI
            if (levelButtons[levelIndex] != null)
                levelButtons[levelIndex].interactable = true;
                
            if (lockImages[levelIndex] != null)
                lockImages[levelIndex].gameObject.SetActive(false);
                
            // Save unlock status (you can implement your own save system)
            SaveLevelProgress();
        }
    }

    private void SaveLevelProgress()
    {
        // Example using PlayerPrefs - you may want to use a more robust save system
        for (int i = 0; i < levels.Length; i++)
        {
            PlayerPrefs.SetInt("Level_" + i + "_Unlocked", levels[i].isUnlocked ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    private void LoadLevelProgress()
    {
        // Example using PlayerPrefs - you may want to use a more robust save system
        for (int i = 0; i < levels.Length; i++)
        {
            levels[i].isUnlocked = PlayerPrefs.GetInt("Level_" + i + "_Unlocked", i == 0 ? 1 : 0) == 1;
        }
    }
}