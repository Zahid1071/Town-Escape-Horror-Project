using UnityEngine;

public class RoomAnchor : MonoBehaviour
{
    [HideInInspector]
    public string roomID;
    [HideInInspector]
    public string zoneID;
    void Awake()
    {
        roomID = name;
        zoneID = transform.parent.name;
    }

}

