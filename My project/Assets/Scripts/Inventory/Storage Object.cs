using UnityEngine;
using System.Collections.Generic;

public class StorageObject : MonoBehaviour, IInteractable
{
    private List<InventoryItem> storedItems = new List<InventoryItem>();

    public void Interact()
    {
        var inventory = InventoryManager.Instance.GetInventory();
        if (inventory.Count == 0)
        {
            Debug.Log("You have no items to store.");
            return;
        }

        foreach (var item in inventory)
        {
            storedItems.Add(item);
        }

        Debug.Log($"Stored items in {gameObject.name}: " + string.Join(", ", storedItems.ConvertAll(i => i.itemName)));

        InventoryManager.Instance.ClearInventory();
        Object.FindFirstObjectByType<InventoryUI>()?.RefreshUI();

    }

    public List<InventoryItem> GetStoredItems() => storedItems;
}
