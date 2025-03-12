using UnityEngine;
using System.Collections;

public class HallwayTeleportationSystem : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Camera playerCamera;
    public AudioSource breathingSound;
    public HallwayEndTrigger doorTrigger;

    [Header("Settings")]
    public float teleportHeightY = 20f;
    public float raycastDistance = 15f;
    public LayerMask triggerLayer;

    private bool hasTurnedAway = false;
    private bool isInHallway = false;
    private bool isTeleporting = false;

    void Start()
    {
        if (player == null) player = transform;
        if (playerCamera == null) playerCamera = Camera.main;
        if (triggerLayer.value == 0)
            triggerLayer = LayerMask.GetMask("Default");

        Debug.Log("[HallwayTeleportationSystem] Initialized.");
    }

    void Update()
    {
        if (!isInHallway || isTeleporting) return;

        bool isLookingAtDoor = IsPlayerLookingAtDoor();
        Debug.Log($"[HallwayTeleportationSystem] Player looking at door: {isLookingAtDoor}");

        if (!isLookingAtDoor && !hasTurnedAway)
        {
            hasTurnedAway = true;
            Debug.Log("[HallwayTeleportationSystem] Player turned away from the door.");
            if (breathingSound != null && !breathingSound.isPlaying)
                breathingSound.Play();
        }

        if (isLookingAtDoor && hasTurnedAway)
        {
            Debug.Log("[HallwayTeleportationSystem] Player turned back towards the door. Teleporting...");
            StartCoroutine(TeleportPlayer());
        }
    }

    bool IsPlayerLookingAtDoor()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, raycastDistance, triggerLayer))
        {
            HallwayEndTrigger hitTrigger = hit.transform.GetComponent<HallwayEndTrigger>();
            if (hitTrigger != null && hitTrigger.isDoorTrigger)
            {
                Debug.Log("[HallwayTeleportationSystem] Raycast hit door trigger.");
                return true;
            }
        }

        float angleTowardsDoor = Vector3.Angle(playerCamera.transform.forward, 
            (doorTrigger.transform.position - playerCamera.transform.position).normalized);

        bool withinAngle = angleTowardsDoor < 45f;
        Debug.Log($"[HallwayTeleportationSystem] Angle to door: {angleTowardsDoor}, Within threshold: {withinAngle}");
        return withinAngle;
    }

    IEnumerator TeleportPlayer()
    {
        isTeleporting = true;
        Debug.Log("[HallwayTeleportationSystem] Teleporting player...");

        if (breathingSound != null)
            breathingSound.Stop();

        yield return new WaitForSeconds(0.05f);

        Vector3 originalPosition = player.position;
        Quaternion originalRotation = player.rotation;
        Vector3 newPosition = originalPosition + new Vector3(0, teleportHeightY, 0);
        player.position = newPosition;
        player.rotation = originalRotation;

        Debug.Log($"[HallwayTeleportationSystem] Player teleported from {originalPosition} to {newPosition}");

        hasTurnedAway = false;
        isTeleporting = false;
    }

    public void EnterHallway()
    {
        isInHallway = true;
        hasTurnedAway = false;
        Debug.Log("[HallwayTeleportationSystem] Player entered hallway.");
    }

    public void ExitHallway()
    {
        isInHallway = false;
        hasTurnedAway = false;

        if (breathingSound != null)
            breathingSound.Stop();

        Debug.Log("[HallwayTeleportationSystem] Player exited hallway.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EnterHallway();
            Debug.Log("[HallwayTeleportationSystem] Player triggered hallway entry.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ExitHallway();
            Debug.Log("[HallwayTeleportationSystem] Player left the hallway.");
        }
    }
}
