using UnityEngine;
using System.Collections.Generic;

public class NPCAnchorManager : MonoBehaviour
{
    public static NPCAnchorManager Instance { get; private set; }

    private Dictionary<string, Transform> anchorMap = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Include inactive objects (but not prefabs)
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (var t in allTransforms)
        {
            if (t.hideFlags == HideFlags.None && t.gameObject.scene.IsValid())
            {
                if (!anchorMap.ContainsKey(t.name))
                {
                    anchorMap[t.name] = t;
                }
                else
                {
                    //Debug.LogWarning($"Duplicate anchor name found: {t.name}");
                }
            }
        }
    }
    public Transform FindClosestOwnedAnchor(Vector3 position, float maxDistance, string ownerZoneID)
    {
        Transform closest = null;
        float closestDistance = maxDistance;

        foreach (var kvp in anchorMap)
        {
            Transform t = kvp.Value;
            RoomAnchor anchor = t.GetComponent<RoomAnchor>();
            if (anchor == null)
                continue;

            // Only consider anchors owned by the given zone
            if (!anchor.zoneID.Equals(ownerZoneID))
                continue;

            float dist = Vector3.Distance(position, t.position);
            if (dist < closestDistance)
            {
                closest = t;
                closestDistance = dist;
            }
        }

        return closest;
    }


    public Transform GetAnchor(string id)
    {
        anchorMap.TryGetValue(id, out var result);
        return result;
    }
}
