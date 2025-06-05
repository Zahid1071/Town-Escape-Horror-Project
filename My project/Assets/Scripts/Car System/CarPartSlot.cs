using UnityEngine;

public class CarPartSlot : MonoBehaviour, IInteractable
{
    public InventoryItem requiredItem;
    private bool isInstalled = false;

    public void Interact()
    {
        if (isInstalled)
        {
            Debug.Log($"{requiredItem.itemName} already installed.");
            return;
        }

        var inventory = InventoryManager.Instance.GetInventory();
        var item = inventory.Find(i => i == requiredItem);

        if (item != null)
        {
            isInstalled = true;
            InventoryManager.Instance.RemoveItem(item);
            Debug.Log($"✅ Installed {requiredItem.itemName}.");
            Object.FindFirstObjectByType<InventoryUI>()?.RefreshUI();
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log($"❌ You need a {requiredItem.itemName} to install here.");
        }
    }

    public bool IsInstalled() => isInstalled;
    public string GetRequiredItemName() => requiredItem.itemName;
}
