using UnityEngine;

public class RoutineIDAttribute : PropertyAttribute
{
    public enum IDType { Zone, Room, Hotspot }
    public IDType Type;

    public RoutineIDAttribute(IDType type)
    {
        this.Type = type;
    }
}
