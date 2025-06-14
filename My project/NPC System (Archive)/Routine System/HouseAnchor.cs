using UnityEngine;

public class HouseAnchor : MonoBehaviour
{
    public string houseID;                       
    public string routineID;                     
    public Transform entryPoint;
    
    [HideInInspector] public bool isOccupied = false;

    private void Awake()
    {
        if (!string.IsNullOrWhiteSpace(routineID))
        {
            RoutineRegistry.Register(routineID, transform);
        }
    }     
}
