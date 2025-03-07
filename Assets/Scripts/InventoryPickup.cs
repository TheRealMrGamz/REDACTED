using UnityEngine;
using System.Collections.Generic;

public class InventoryPickup : MonoBehaviour
{
    [Header("Item Information")]
    [SerializeField] private string itemID;
    [SerializeField] private string itemName;
    [SerializeField] private Sprite itemIcon;
    [SerializeField] private bool isKey = false;
    [SerializeField] private int preferredSlot = -1;  // -1 means any available slot
    
    [Header("Display Objects")]
    [SerializeField] private List<GameObject> itemDisplayObjects = new List<GameObject>(); // Multiple display objects
    
    [Header("Key Settings (if isKey = true)")]
    [SerializeField] private string keyID;
    
    [Header("Visual Settings")]
    [SerializeField] private GameObject pickupVisual;
    [SerializeField] private bool hideVisualOnPickup = true;
    
    [Header("Sound Settings")]
    [SerializeField] private AudioClip pickupSound;

    // Reference to the inventory system
    private InventorySystem inventorySystem;
    // Reference to the key inventory controller
    private InventoryController keyInventory;

    private void Start()
    {
        // Find the inventory system in the scene
        inventorySystem = FindObjectOfType<InventorySystem>();
        
        if (inventorySystem == null)
        {
            Debug.LogError("InventoryPickup: No InventorySystem found in the scene!");
        }
        
        // If this is a key item, also find the key inventory
        if (isKey)
        {
            keyInventory = FindObjectOfType<InventoryController>();
            
            if (keyInventory == null)
            {
                Debug.LogError("InventoryPickup: No InventoryController found for keys!");
            }
        }
        
        // Make sure all display objects are disabled initially
        foreach (GameObject displayObj in itemDisplayObjects)
        {
            if (displayObj != null)
            {
                displayObj.SetActive(false);
            }
        }
    }

    // This method will be called by the TriggerEventHandler's UnityEvent
    public void AddToInventory()
    {
        if (inventorySystem == null)
        {
            Debug.LogError("Cannot add to inventory: InventorySystem not found!");
            return;
        }

        // Handle the pickup based on item type
        if (isKey)
        {
            AddKeyToInventory();
        }
        else
        {
            AddRegularItemToInventory();
        }

        // Play pickup sound if assigned
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        // Hide the visual representation if specified
        if (hideVisualOnPickup && pickupVisual != null)
        {
            pickupVisual.SetActive(false);
        }
    }

    private void AddRegularItemToInventory()
    {
        // Add the item to the inventory system with multiple display objects
        inventorySystem.AddItem(itemID, itemName, itemIcon, itemDisplayObjects, preferredSlot);
        
        Debug.Log($"Added {itemName} to inventory");
    }

    private void AddKeyToInventory()
    {
        if (keyInventory == null)
        {
            Debug.LogError("Cannot add key: KeyInventory not found!");
            return;
        }

        // Create the key object
        Key newKey = new Key();
        newKey.keyID = keyID;

        // Add the key to the inventory system with multiple display objects
        inventorySystem.AddKeyItem(newKey, itemIcon, itemDisplayObjects, preferredSlot);
        
        Debug.Log($"Added key {keyID} to inventory");
    }
}