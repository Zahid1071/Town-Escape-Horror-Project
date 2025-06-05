using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private const int MaxSlots = 4;
    private List<InventoryItem> inventory = new List<InventoryItem>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    
    public void RemoveItem(InventoryItem item)
    {
    inventory.Remove(item);
    }

    public bool AddItem(InventoryItem item)
    {
        if (inventory.Count >= MaxSlots)
        {
            Debug.Log("Inventory full.");
            return false;
        }

        inventory.Add(item);
        Debug.Log("Added: " + item.itemName);
        return true;
    }

    public void ClearInventory()
    {
        inventory.Clear();
    }

    public List<InventoryItem> GetInventory() => inventory;

    public int SlotCount() => MaxSlots;
}
