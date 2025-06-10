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

    public Transform GetAnchor(string id)
    {
        anchorMap.TryGetValue(id, out var result);
        return result;
    }
}
