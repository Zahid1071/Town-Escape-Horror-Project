using UnityEngine;

public class PlayerHider : MonoBehaviour
{
    public bool isHidden = false;
    private HidingSpot currentSpot;
    public GameObject visualRoot; // Drag "Mesh" here
    public MonoBehaviour[] scriptsToDisable; // Drag First Person Controller, Player Input, etc.

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isHidden && currentSpot != null)
            {
                currentSpot.RevealPlayer(gameObject);
                currentSpot = null;
            }
            else if (!isHidden && currentSpot != null)
            {
                currentSpot.HidePlayer(gameObject);
            }
        }
    }



    public void SetHidden(bool value)
    {
        isHidden = value;

        // Hide visuals
        if (visualRoot != null)
            visualRoot.SetActive(!value);

        // Disable movement/input
        foreach (var script in scriptsToDisable)
        {
            if (script != null)
                script.enabled = !value;
        }

        // Optional: lock camera or pause other systems
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isHidden && other.TryGetComponent(out HidingSpot spot))
        {
            currentSpot = spot;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isHidden && other.TryGetComponent(out HidingSpot spot))
        {
            if (spot == currentSpot)
            {
                currentSpot = null;
            }
        }
    }


}
