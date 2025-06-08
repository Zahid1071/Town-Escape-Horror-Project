using UnityEngine;

public class NPCZone : MonoBehaviour
{
    public string houseID;
    public NPCController ownerNPC;

    private int playerInsideCount = 0;

    private void Awake()
    {
        houseID = transform.name;
    }

    public void NotifyPlayerEntered()
    {
        playerInsideCount++;
        Debug.Log($"Player entered {houseID} (now inside: {playerInsideCount})");

        if (ownerNPC != null)
        {
            ownerNPC.currentBehavior = NPCController.BehaviorState.Suspicious;
            ownerNPC.InterruptRoutine();
            Debug.Log($"🚨 {ownerNPC.data.npcName} is now suspicious (player entered {houseID})");
        }
    }

    public void NotifyPlayerExited()
    {
        playerInsideCount = Mathf.Max(0, playerInsideCount - 1);
        Debug.Log($"Player exited {houseID} (still inside: {playerInsideCount})");

        if (ownerNPC != null && playerInsideCount == 0)
        {
            ownerNPC.currentBehavior = NPCController.BehaviorState.Normal;
            Debug.Log($"{ownerNPC.data.npcName} is no longer suspicious.");
        }
    }

    public bool IsPlayerInside() => playerInsideCount > 0;

    // 🔊 Called by SuspicionTrigger (e.g. creaky door, knockable prop)
    public void NotifySuspiciousSoundHeard(string roomID)
    {
        if (ownerNPC == null)
            return;

        ownerNPC.ReactToSuspicion(roomID);
    }
}
