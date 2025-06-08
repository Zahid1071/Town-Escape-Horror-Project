using System.Collections.Generic;
using UnityEngine;

public class RoutineRegistry : MonoBehaviour
{
    private static Dictionary<string, Transform> routineMap = new();

    public static void Register(string id, Transform point)
    {
        if (!routineMap.ContainsKey(id))
            routineMap[id] = point;
        else
            Debug.LogWarning($"⚠️ Duplicate routine ID: {id} already registered.");
    }

    public static Transform Get(string id)
    {
        if (routineMap.TryGetValue(id, out var t))
            return t;

        Debug.LogError($"❌ No transform found for routine ID: {id}");
        return null;
    }

    public static void Clear()
    {
        routineMap.Clear();
    }

    private void Awake()
    {
        Clear(); // Ensure clean start
    }
}
