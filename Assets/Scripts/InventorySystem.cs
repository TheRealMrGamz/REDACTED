using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    [Header("Inventory Setup")]
    [SerializeField] private int maxSlots = 3;
    [SerializeField] private GameObject[] slotUIElements;
    
    [Header("References")]
    [SerializeField] private InventoryController keyInventory;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDelay = 5f; // Time in seconds before fading starts
    [SerializeField] private float fadeAmount = 0.4f; // How much to fade (0 = invisible, 1 = fully visible)
    [SerializeField] private float fadeDuration = 0.5f; // How long the fade transition takes

    private List<InventoryItem> inventoryItems = new List<InventoryItem>();
    private int selectedSlotIndex = 0;
    private List<GameObject> currentlyDisplayedObjects = new List<GameObject>();
    private Vector3[] originalSlotScales;
    private Image[] slotImages;
    private Color[] originalSlotColors;
    
    // Fade variables
    private float lastInteractionTime;
    private bool isFaded = false;

    // Core inventory data structure
    [System.Serializable]
    public class InventoryItem
    {
        public string itemID;
        public string itemName;
        public Sprite icon;
        public List<GameObject> displayObjects = new List<GameObject>(); // Multiple display objects
        public bool isKey = false;

        public InventoryItem(string id, string name, Sprite icon, List<GameObject> displayObjs, bool isKey = false)
        {
            this.itemID = id;
            this.itemName = name;
            this.icon = icon;
            this.displayObjects = displayObjs;
            this.isKey = isKey;
        }

        // For backward compatibility
        public InventoryItem(string id, string name, Sprite icon, GameObject displayObj, bool isKey = false)
        {
            this.itemID = id;
            this.itemName = name;
            this.icon = icon;
            if (displayObj != null)
            {
                this.displayObjects.Add(displayObj);
            }
            this.isKey = isKey;
        }
    }

    private void Start()
    {
        InitializeInventory();
        InitializeFadeSystem();
        UpdateInventoryUI();
    }

    private void InitializeInventory()
    {
        // Fill inventory with empty slots if needed
        while (inventoryItems.Count < maxSlots)
        {
            inventoryItems.Add(null);
        }
    
        // Store original scales and hide empty slots
        originalSlotScales = new Vector3[maxSlots];
        slotImages = new Image[maxSlots];
        originalSlotColors = new Color[maxSlots];
        
        for (int i = 0; i < maxSlots; i++)
        {
            if (slotUIElements[i] != null)
            {
                originalSlotScales[i] = slotUIElements[i].transform.localScale;
            
                // Cache image references and colors
                slotImages[i] = slotUIElements[i].GetComponentInChildren<Image>();
                if (slotImages[i] != null)
                {
                    originalSlotColors[i] = slotImages[i].color;
                    
                    // Hide empty slots at start
                    slotImages[i].enabled = false;
                }
            }
            else
            {
                originalSlotScales[i] = Vector3.one;
            }
        }
    }

    private void InitializeFadeSystem()
    {
        // Initialize interaction time
        lastInteractionTime = Time.time;
    }

    private void Update()
    {
        // Tab to cycle through inventory slots
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            CycleSelectedSlot();
            RegisterInteraction();
        }
        
        // Check if we should fade the inventory
        CheckFadeStatus();
    }

    private void RegisterInteraction()
    {
        // Update the last interaction time
        lastInteractionTime = Time.time;
        
        // If inventory is faded, unfade it
        if (isFaded)
        {
            StartFadeTransition(false);
        }
    }

    private void CheckFadeStatus()
    {
        float timeSinceLastInteraction = Time.time - lastInteractionTime;
        
        // If enough time has passed and inventory isn't already faded, fade it
        if (timeSinceLastInteraction > fadeDelay && !isFaded)
        {
            StartFadeTransition(true);
        }
    }

    private void StartFadeTransition(bool fadeOut)
    {
        // Start fade coroutine
        StopAllCoroutines();
        StartCoroutine(FadeInventorySlots(fadeOut));
    }

    private System.Collections.IEnumerator FadeInventorySlots(bool fadeOut)
    {
        float elapsedTime = 0f;
        
        // Calculate target alpha for each slot
        Color[] targetColors = new Color[maxSlots];
        Color[] startColors = new Color[maxSlots];
        
        for (int i = 0; i < maxSlots; i++)
        {
            if (slotImages[i] != null && slotImages[i].enabled)
            {
                startColors[i] = slotImages[i].color;
                
                if (fadeOut)
                {
                    // Calculate target faded color (reduce alpha)
                    targetColors[i] = new Color(
                        originalSlotColors[i].r,
                        originalSlotColors[i].g,
                        originalSlotColors[i].b,
                        fadeAmount
                    );
                }
                else
                {
                    // Target color is original color
                    targetColors[i] = originalSlotColors[i];
                }
            }
        }
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / fadeDuration);
            
            // Update each slot's color
            for (int i = 0; i < maxSlots; i++)
            {
                if (slotImages[i] != null && slotImages[i].enabled)
                {
                    slotImages[i].color = Color.Lerp(startColors[i], targetColors[i], normalizedTime);
                }
            }
            
            yield return null;
        }
        
        // Ensure we reach the target colors
        for (int i = 0; i < maxSlots; i++)
        {
            if (slotImages[i] != null && slotImages[i].enabled)
            {
                slotImages[i].color = targetColors[i];
            }
        }
        
        isFaded = fadeOut;
    }

    public void CycleSelectedSlot()
    {
        // Deselect current slot
        if (slotUIElements[selectedSlotIndex] != null)
        {
            slotUIElements[selectedSlotIndex].transform.localScale = originalSlotScales[selectedSlotIndex];
        }

        // Update selected slot index
        selectedSlotIndex = (selectedSlotIndex + 1) % maxSlots;

        // Select new slot
        if (slotUIElements[selectedSlotIndex] != null)
        {
            slotUIElements[selectedSlotIndex].transform.localScale = originalSlotScales[selectedSlotIndex] * 1.1f;
        }

        // Update displayed objects
        UpdateDisplayedObjects();
    }

// And also update SelectSlot()
    public void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSlots)
            return;

        // Deselect current slot
        if (slotUIElements[selectedSlotIndex] != null)
        {
            slotUIElements[selectedSlotIndex].transform.localScale = originalSlotScales[selectedSlotIndex];
        }

        // Update selected slot index
        selectedSlotIndex = slotIndex;

        // Select new slot
        if (slotUIElements[selectedSlotIndex] != null)
        {
            slotUIElements[selectedSlotIndex].transform.localScale = originalSlotScales[selectedSlotIndex] * 1.2f;
        }

        // Update displayed objects
        UpdateDisplayedObjects();
        
        // Register this as an interaction
        RegisterInteraction();
    }

    private void UpdateDisplayedObjects()
    {
        // Disable all previously displayed objects
        foreach (GameObject obj in currentlyDisplayedObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
        currentlyDisplayedObjects.Clear();

        // Display new objects if slot has an item
        InventoryItem currentItem = inventoryItems[selectedSlotIndex];
        if (currentItem != null && currentItem.displayObjects.Count > 0)
        {
            foreach (GameObject obj in currentItem.displayObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                    currentlyDisplayedObjects.Add(obj);
                }
            }
        }
    }

    // This method lets you add a regular item with multiple display objects
    public void AddItem(string id, string name, Sprite icon, List<GameObject> displayObjs, int slotIndex = -1)
    {
        InventoryItem newItem = new InventoryItem(id, name, icon, displayObjs);
        
        // If slot index is specified and valid, place item there
        if (slotIndex >= 0 && slotIndex < maxSlots && inventoryItems[slotIndex] == null)
        {
            inventoryItems[slotIndex] = newItem;
        }
        // Otherwise, find the first empty slot
        else
        {
            bool added = false;
            for (int i = 0; i < maxSlots; i++)
            {
                if (inventoryItems[i] == null)
                {
                    inventoryItems[i] = newItem;
                    added = true;
                    break;
                }
            }
            
            // If no empty slots were found, we can't add the item
            if (!added)
            {
                Debug.LogWarning("Cannot add item: Inventory is full");
                return;
            }
        }
        
        UpdateInventoryUI();
        RegisterInteraction();
    }

    // For backward compatibility - single display object
    public void AddItem(string id, string name, Sprite icon, GameObject displayObj, int slotIndex = -1)
    {
        List<GameObject> displayObjs = new List<GameObject>();
        if (displayObj != null)
        {
            displayObjs.Add(displayObj);
        }
        AddItem(id, name, icon, displayObjs, slotIndex);
    }

    // This method lets you add a key item with multiple display objects
    public void AddKeyItem(Key key, Sprite icon, List<GameObject> displayObjs, int slotIndex = -1)
    {
        // Add to key inventory first
        keyInventory.AddKey(key);
        
        // Then add to visual inventory
        InventoryItem keyItem = new InventoryItem(key.keyID, "Key: " + key.keyID, icon, displayObjs, true);
        
        // If slot index is specified and valid, place item there
        if (slotIndex >= 0 && slotIndex < maxSlots && inventoryItems[slotIndex] == null)
        {
            inventoryItems[slotIndex] = keyItem;
        }
        // Otherwise, find the first empty slot
        else
        {
            bool added = false;
            for (int i = 0; i < maxSlots; i++)
            {
                if (inventoryItems[i] == null)
                {
                    inventoryItems[i] = keyItem;
                    added = true;
                    break;
                }
            }
            
            // If no empty slots were found, we can't add the item
            if (!added)
            {
                Debug.LogWarning("Cannot add key item: Inventory is full");
                return;
            }
        }
        
        UpdateInventoryUI();
        RegisterInteraction();
    }

    // For backward compatibility - single display object
    public void AddKeyItem(Key key, Sprite icon, GameObject displayObj, int slotIndex = -1)
    {
        List<GameObject> displayObjs = new List<GameObject>();
        if (displayObj != null)
        {
            displayObjs.Add(displayObj);
        }
        AddKeyItem(key, icon, displayObjs, slotIndex);
    }

    public void RemoveItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSlots)
            return;

        InventoryItem itemToRemove = inventoryItems[slotIndex];
        if (itemToRemove != null)
        {
            // If it's a key, also remove from key inventory
            if (itemToRemove.isKey)
            {
                Key keyToRemove = keyInventory.FindKey(itemToRemove.itemID);
                if (keyToRemove != null)
                {
                    keyInventory.RemoveKey(keyToRemove);
                }
            }
            
            // If these were the displayed objects, hide them
            if (slotIndex == selectedSlotIndex)
            {
                foreach (GameObject obj in currentlyDisplayedObjects)
                {
                    if (obj != null)
                    {
                        obj.SetActive(false);
                    }
                }
                currentlyDisplayedObjects.Clear();
            }
            
            // Clear the slot
            inventoryItems[slotIndex] = null;
            
            UpdateInventoryUI();
            RegisterInteraction();
        }
    }

    public void RemoveItemByID(string itemID)
    {
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i] != null && inventoryItems[i].itemID == itemID)
            {
                RemoveItem(i);
                break;
            }
        }
    }

    public InventoryItem GetSelectedItem()
    {
        RegisterInteraction();
        return inventoryItems[selectedSlotIndex];
    }

    public InventoryItem GetItemInSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < maxSlots)
        {
            RegisterInteraction();
            return inventoryItems[slotIndex];
        }
        return null;
    }

    private void UpdateInventoryUI()
    {
        // Update UI for each slot
        for (int i = 0; i < maxSlots; i++)
        {
            if (slotImages[i] != null)
            {
                // If slot has an item, show its icon
                if (inventoryItems[i] != null)
                {
                    slotImages[i].enabled = true;
                    slotImages[i].sprite = inventoryItems[i].icon;
                    
                    // Reset color to original or faded based on current state
                    if (isFaded)
                    {
                        slotImages[i].color = new Color(
                            originalSlotColors[i].r,
                            originalSlotColors[i].g,
                            originalSlotColors[i].b,
                            fadeAmount
                        );
                    }
                    else
                    {
                        slotImages[i].color = originalSlotColors[i];
                    }
                }
                // Otherwise, hide the slot completely
                else
                {
                    slotImages[i].enabled = false;
                }
            }
        }
    
        // Update displayed objects for currently selected slot
        UpdateDisplayedObjects();
    }

    // Save inventory data to PlayerPrefs (simple persistence)
    public void SaveInventory()
    {
        for (int i = 0; i < maxSlots; i++)
        {
            if (inventoryItems[i] != null)
            {
                PlayerPrefs.SetString("InventorySlot_" + i, inventoryItems[i].itemID);
            }
            else
            {
                PlayerPrefs.SetString("InventorySlot_" + i, "empty");
            }
        }
        
        PlayerPrefs.Save();
        RegisterInteraction();
    }

    // Load inventory data from PlayerPrefs
    public void LoadInventory()
    {
        // This is just a basic implementation - you'd need to implement a more robust system
        // to actually recreate the correct items with their properties
        for (int i = 0; i < maxSlots; i++)
        {
            string itemID = PlayerPrefs.GetString("InventorySlot_" + i, "empty");
            if (itemID != "empty")
            {
                // Handle item recreation based on ID
                // This would need an item database or similar in a real implementation
                Debug.Log("Would load item with ID: " + itemID);
            }
        }
        RegisterInteraction();
    }
}