using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    public NPCZone parentZone;
    public string roomID;

    void Awake()
    {
        roomID = gameObject.name;       
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && parentZone != null)
        {
            //parentZone.NotifyPlayerEntered();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && parentZone != null)
        {
           // parentZone.NotifyPlayerExited();
        }
    }
}
