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

[System.Serializable]
public class WeightedHotspot
{
    [RoutineID(RoutineIDAttribute.IDType.Hotspot)]
    public string hotspotID;

    [Range(0f, 1f)] public float weight = 1f;
    public float duration = 5f;
    public HotspotAction.ActionType actionType = HotspotAction.ActionType.Idle;

    [Header("Interpolation Settings")]
    public float entryInterpolationOffset = 0.4f;
    public float entryInterpolationTime = 0.5f;
    public float exitInterpolationOffset = 0.4f;
    public float exitInterpolationTime = 0.5f;

    [Header("Exit Animation")]
    public string exitAnimationTrigger = "IdleAction";
}


