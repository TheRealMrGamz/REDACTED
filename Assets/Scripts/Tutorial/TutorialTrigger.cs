using UnityEngine;
using System.Collections;

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private TutorialMessage[] tutorialMessages;
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private bool triggerOnStart = false;
    [SerializeField] private float delayBetweenMessages = 0.5f;

    private bool hasTriggered = false;

    private void Start()
    {
        if (triggerOnStart)
        {
            ShowTutorialMessages();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && hasTriggered)
            return;

        if (other.CompareTag("Player"))
        {
            ShowTutorialMessages();
        }
    }

    // Public function to manually trigger tutorial messages
    public void ShowTutorialMessages()
    {
        if (triggerOnce && hasTriggered)
            return;
            
        StartCoroutine(DisplayMessagesSequentially());
        hasTriggered = true;
    }
    
    private IEnumerator DisplayMessagesSequentially()
    {
        foreach (TutorialMessage message in tutorialMessages)
        {
            // Show the current message
            tutorialManager.ShowTutorialMessage(message);
            
            // Wait for the message duration plus fade time plus delay
            float totalWaitTime = message.displayDuration + tutorialManager.GetComponent<TutorialManager>().fadeDuration * 2 + delayBetweenMessages;
            yield return new WaitForSeconds(totalWaitTime);
        }
    }
}