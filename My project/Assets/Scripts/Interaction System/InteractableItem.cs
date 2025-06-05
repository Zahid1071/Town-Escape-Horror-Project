using UnityEngine;

public class InteractableItem : MonoBehaviour, IInteractable
{
    public InventoryItem itemData;

    public void Interact()
    {
        if (itemData == null)
        {
            Debug.LogWarning("No item assigned.");
            return;
        }

        bool added = InventoryManager.Instance.AddItem(itemData);
        if (added)
        {
            Destroy(gameObject);
        }
    }
}
