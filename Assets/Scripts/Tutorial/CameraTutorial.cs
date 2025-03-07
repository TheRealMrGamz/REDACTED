using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTutorial : MonoBehaviour
{
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private GameObject triggerObject;
    [SerializeField] private TutorialMessage[] cameraInstructions = new TutorialMessage[]
    {
        new TutorialMessage { message = "Left Click to Take Photo", inputHint = "Left Mouse", displayDuration = 3f },
        new TutorialMessage { message = "Right Click to Zoom", inputHint = "Right Mouse", displayDuration = 3f },
        new TutorialMessage { message = "Press E to Put Down Camera", inputHint = "E Key", displayDuration = 3f }
    };
    [SerializeField] private float delayBetweenMessages = 0.5f;

    private void Start()
    {
        if (triggerObject != null)
        {
            StartCoroutine(WatchTriggerObject());
        }
    }

    private IEnumerator WatchTriggerObject()
    {
        while (true)
        {
            yield return new WaitUntil(() => !triggerObject.activeSelf);
            ShowCameraTutorial();
            yield return new WaitUntil(() => triggerObject.activeSelf);
        }
    }

    public void ShowCameraTutorial()
    {
        StartCoroutine(DisplayCameraInstructions());
    }
    
    private IEnumerator DisplayCameraInstructions()
    {
        foreach (TutorialMessage instruction in cameraInstructions)
        {
            // Show the current message
            tutorialManager.ShowTutorialMessage(instruction);
            
            // Wait for the message duration plus fade time plus delay
            float totalWaitTime = instruction.displayDuration + tutorialManager.GetComponent<TutorialManager>().fadeDuration * 2 + delayBetweenMessages;
            yield return new WaitForSeconds(totalWaitTime);
        }
    }
}