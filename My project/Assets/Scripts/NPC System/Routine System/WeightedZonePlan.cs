using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeightedZonePlan
{
    [RoutineID(RoutineIDAttribute.IDType.Zone)]
    public string zoneID;
    [Range(0f, 1f)] public float weight = 1f;
    public List<WeightedRoomPlan> roomPlans;
}

[Serializable]
public class WeightedRoomPlan
{
    [RoutineID(RoutineIDAttribute.IDType.Room)]
    public string roomAnchorID;
    [Range(0f, 1f)] public float weight = 1f;
    public List<WeightedHotspot> hotspots;
}

[Serializable]
public class WeightedHotspot
{
    [RoutineID(RoutineIDAttribute.IDType.Hotspot)]
    public string hotspotID;
    [Range(0f, 1f)] public float weight = 1f;
    public float duration = 5f;
}
