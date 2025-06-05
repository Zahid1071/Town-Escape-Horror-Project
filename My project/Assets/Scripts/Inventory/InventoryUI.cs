using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private TMP_Text[] slotTexts;

    private void Start()
    {
        inventoryPanel.SetActive(false);
        RefreshUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            bool isActive = inventoryPanel.activeSelf;
            inventoryPanel.SetActive(!isActive);

            if (!isActive)
                RefreshUI();
        }
    }

    public void RefreshUI()
    {
        var items = InventoryManager.Instance.GetInventory();

        for (int i = 0; i < slotTexts.Length; i++)
        {
            if (i < items.Count)
                slotTexts[i].text = items[i].itemName;
            else
                slotTexts[i].text = "[Empty]";
        }
    }
}
