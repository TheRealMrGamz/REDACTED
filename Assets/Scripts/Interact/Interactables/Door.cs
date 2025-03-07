using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : InteractableBase
{
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] public AudioSource audioSource;

    private bool isOpen;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    private void Start()
    {
        closedRotation = transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(0f, 0f, openAngle);
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
    }

    public override void OnInteract(PSXFirstPersonController player)
    {
        isOpen = !isOpen;
        interactionPrompt = isOpen ? "Close" : "Open";

        audioSource.PlayOneShot(isOpen ? openSound : closeSound);

        base.OnInteract(player);
    }
}