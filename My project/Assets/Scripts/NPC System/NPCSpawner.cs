using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public NPCController[] npcControllers;

    void Start()
    {
        var allHouses = FindObjectsByType<HouseAnchor>(FindObjectsSortMode.None);

        int assigned = 0;

        foreach (var npc in npcControllers)
        {
            if (npc.data == null || string.IsNullOrEmpty(npc.data.homeID))
            {
                Debug.LogWarning($"❌ NPC {npc.name} has no NPCData or homeID set.");
                continue;
            }

            foreach (var house in allHouses)
            {
                if (house.houseID == npc.data.homeID && !house.isOccupied)
                {
                    npc.data.homeID = house.houseID;
                    house.isOccupied = true;
                    npc.data.homeID = house.entryPoint.name;
                    assigned++;
                    break;
                }
            }
        }

        Debug.Log($"✅ Assigned {assigned} NPCs to their homes.");
    }
}
