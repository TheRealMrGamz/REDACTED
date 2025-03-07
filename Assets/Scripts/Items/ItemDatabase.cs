using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [System.Serializable]
    public class ItemData
    {
        public string itemID;
        public string itemName;
        public Sprite icon;
        public GameObject displayPrefab;
        public bool isKey;
    }

    public List<ItemData> items = new List<ItemData>();

    public ItemData GetItemById(string id)
    {
        return items.Find(item => item.itemID == id);
    }
}