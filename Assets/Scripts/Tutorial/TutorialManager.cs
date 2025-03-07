using UnityEngine;
using TMPro;
using System.Collections;
using System;

[Serializable]
public class TutorialMessage
{
    [TextArea(1, 3)]
    public string message;
    public string inputHint;
    public float displayDuration = 3f;
}

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup tutorialCanvasGroup;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private float defaultDisplayDuration = 3f;
    [SerializeField] public float fadeDuration = 0.5f;
    
    private Coroutine activeDisplayCoroutine;
    private bool isDisplaying = false;

    private void Start()
    {
        // Ensure the canvas is hidden at start
        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.alpha = 0;
        }
    }

    // Public function to display a tutorial message
    public void ShowTutorialMessage(string message, float duration = -1)
    {
        // If already displaying, stop the current coroutine
        if (isDisplaying && activeDisplayCoroutine != null)
        {
            StopCoroutine(activeDisplayCoroutine);
        }
        
        // Set the display duration (use default if no custom duration provided)
        float displayTime = duration > 0 ? duration : defaultDisplayDuration;
        
        // Start the display coroutine
        activeDisplayCoroutine = StartCoroutine(DisplayMessage(message, displayTime));
    }
    
    // Public function to display a tutorial message with input hint
    public void ShowTutorialMessage(TutorialMessage tutorialMessage)
    {
        string fullMessage = !string.IsNullOrEmpty(tutorialMessage.inputHint) 
            ? $"{tutorialMessage.message} ({tutorialMessage.inputHint})" 
            : tutorialMessage.message;
            
        ShowTutorialMessage(fullMessage, tutorialMessage.displayDuration);
    }

    private IEnumerator DisplayMessage(string message, float duration)
    {
        isDisplaying = true;
        
        // Set the message text
        if (tutorialText != null)
        {
            tutorialText.text = message;
        }
        
        // Fade in
        yield return StartCoroutine(FadeCanvas(true));
        
        // Wait for the specified duration
        yield return new WaitForSeconds(duration);
        
        // Fade out
        yield return StartCoroutine(FadeCanvas(false));
        
        isDisplaying = false;
    }

    private IEnumerator FadeCanvas(bool fadeIn)
    {
        float startAlpha = fadeIn ? 0 : 1;
        float endAlpha = fadeIn ? 1 : 0;
        float elapsedTime = 0;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / fadeDuration;
            
            tutorialCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, normalizedTime);
            
            yield return null;
        }
        
        tutorialCanvasGroup.alpha = endAlpha;
    }
}