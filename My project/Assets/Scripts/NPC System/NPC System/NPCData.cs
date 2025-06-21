using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NPCData", menuName = "NPC/NPC Data")]
public class NPCData : ScriptableObject
{
    public string npcName;
    public GameObject npcPrefab;

    [RoutineID(RoutineIDAttribute.IDType.Zone)]
    public string homeID;


    public bool isInfected = false;
    public float infectionLevel = 0f;

    [Range(0f, 1f)] public float searchChance = 0.3f;      // base chance
    [Range(0f, 1f)] public float maxSearchChance = 0.8f;

    public List<InventoryItem> itemsTheyHave;
    public List<InventoryItem> itemsTheyWant;

    public List<WeightedZonePlan> weightedRoutinePlans;


    [TextArea] public string[] dialogueLines;
}
