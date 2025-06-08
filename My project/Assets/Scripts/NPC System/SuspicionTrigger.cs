using UnityEngine;

public class SuspicionTrigger : MonoBehaviour
{
    [Tooltip("How often this can be triggered (in seconds).")]
    public float cooldown = 2f;

    private float lastTriggerTime = -Mathf.Infinity;
    private RoomTrigger containingRoom;

    private void Start()
    {
        containingRoom = GetComponentInParent<RoomTrigger>();

        if (containingRoom == null)
        {
            Debug.LogWarning($"SuspicionTrigger on {gameObject.name} couldn't find a parent RoomTrigger.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerSuspicion();
        }
    }

    public void TriggerSuspicion()
    {
        if (Time.time - lastTriggerTime < cooldown)
            return;

        lastTriggerTime = Time.time;

        if (containingRoom != null && containingRoom.parentZone != null)
        {
            containingRoom.parentZone.NotifySuspiciousSoundHeard(containingRoom.roomID);
            Debug.Log($"🔊 Suspicion triggered in room {containingRoom.roomID} by {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"SuspicionTrigger on {gameObject.name} has no valid containing room or zone.");
        }
    }
}
