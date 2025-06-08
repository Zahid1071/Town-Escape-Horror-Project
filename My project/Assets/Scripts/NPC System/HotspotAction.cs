using UnityEngine;

public class HotspotAction : MonoBehaviour
{
    public enum ActionType
    {
        Idle,
        Sit,
        Nap,
        WatchTV
    }

    public ActionType actionType = ActionType.Idle;
    public float duration = 5f;

    public NPCZone zone; // Optional reference

}
