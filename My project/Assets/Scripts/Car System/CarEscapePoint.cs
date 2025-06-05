using UnityEngine;

public class CarEscapePoint : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        CarPartSlot[] parts = transform.root.GetComponentsInChildren<CarPartSlot>(true);

        if (parts == null || parts.Length == 0)
        {
            Debug.LogWarning("No parts found for escape check.");
            return;
        }

        foreach (var part in parts)
        {
            if (!part.IsInstalled())
            {
                Debug.Log("❌ Some parts are still missing. You can't escape yet.");
                return;
            }
        }

        Debug.Log("✅ All parts installed. You escaped!");
        // TODO: fade out, load next scene, trigger win event
    }
}
