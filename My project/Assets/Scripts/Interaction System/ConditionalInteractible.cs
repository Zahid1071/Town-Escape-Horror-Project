using UnityEngine;

public class ConditionalInteractible : MonoBehaviour, IInteractable
{
    [SerializeField] private InventoryItem requiredItem;
    [SerializeField] private MonoBehaviour targetScript; // The actual logic (e.g., door open script)

    public void Interact()
    {
        var inventory = InventoryManager.Instance.GetInventory();
        if (inventory.Contains(requiredItem))
        {
            Debug.Log($"✅ Used {requiredItem.itemName}");
            (targetScript as IInteractable)?.Interact();
        }
        else
        {
            Debug.Log($"❌ You need {requiredItem.itemName} to do this.");
        }
    }
}
