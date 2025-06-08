using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomPropertyDrawer(typeof(RoutineIDAttribute))]
public class RoutineIDDrawer : PropertyDrawer
{
    private Dictionary<string, Transform> idCache;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attr = (RoutineIDAttribute)attribute;

        if (idCache == null)
            CacheIDs(attr.Type);

        List<string> ids = idCache.Keys.ToList();

        if (ids.Count == 0)
        {
            EditorGUI.HelpBox(position, "No valid IDs found in scene.", MessageType.Warning);
            return;
        }

        int currentIndex = Mathf.Max(0, ids.IndexOf(property.stringValue));
        int newIndex = EditorGUI.Popup(position, property.displayName, currentIndex, ids.ToArray());

        property.stringValue = ids[newIndex];
    }

    private void CacheIDs(RoutineIDAttribute.IDType type)
    {
        idCache = new();
        
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform t in allTransforms)
        {
            if (!t.gameObject.scene.IsValid())
                continue;

            string name = t.name.ToLower();

            switch (type)
            {
                case RoutineIDAttribute.IDType.Zone:
                    if (name.Contains("house") || name.Contains("zone"))
                        idCache[t.name] = t;
                    break;

                case RoutineIDAttribute.IDType.Room:
                    if (name.Contains("room") || name.Contains("trigger"))
                        idCache[t.name] = t;
                    break;

                case RoutineIDAttribute.IDType.Hotspot:
                    if (name.Contains("spot") || name.Contains("point")|| name.Contains("action"))
                        idCache[t.name] = t;
                    break;
            }
        }
    }
}
