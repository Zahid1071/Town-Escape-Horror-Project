using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class NPCController : MonoBehaviour
{
    public NPCData data;
    public NavMeshAgent agent;

    public enum MovementState
    {
        Idle,
        Walking,
    }

    public enum BehaviorState
    {
        Normal,
        Suspicious,
        Search,
        Hostile
    }



    public MovementState currentState = MovementState.Idle;
    public BehaviorState currentBehavior = BehaviorState.Normal;


    private bool waiting = false;

    private WeightedZonePlan currentZonePlan;
    private WeightedRoomPlan currentRoomPlan;
    private WeightedHotspot currentHotspot;

    private string lastZoneID = null;
    public float remainHomeFactor;

    private Coroutine hotspotRoutine;

    [SerializeField] private GameObject player;
    [SerializeField] private float searchPointWaitTime = 1.5f;

    private Coroutine searchRoutine;


    private void Start()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();

        // Only start routine if behavior is Normal
        if (currentBehavior == BehaviorState.Normal)
        {
            PickNextWeightedRoutine();
        }
    }


    private void Update()
    {
        if (data == null || waiting || agent.pathPending)
            return;

        // Stop all routine logic if not in Normal behavior
        if (currentBehavior != BehaviorState.Normal)
            return;

        if (agent.remainingDistance < 0.5f)
        {
            hotspotRoutine = StartCoroutine(PerformWeightedHotspotAction());
        }
    }



    private void PickNextWeightedRoutine()
    {


        // 1. Create a boosted, temporary list
        List<WeightedZonePlan> boostedZones = new List<WeightedZonePlan>();
        foreach (var zone in data.weightedRoutinePlans)
        {
            float boostFactor = 1f;

            if (zone.zoneID == lastZoneID)
            {
                boostFactor = (zone.zoneID == data.homeID)
                    ? remainHomeFactor
                    : remainHomeFactor * 0.8f;
            }

            boostedZones.Add(new WeightedZonePlan
            {
                zoneID = zone.zoneID,
                weight = zone.weight * boostFactor,

                roomPlans = zone.roomPlans
            });
        }


        // 2. Pick zone/room/hotspot
        currentZonePlan = WeightedPicker.Pick(boostedZones, z => z.weight);
        currentRoomPlan = WeightedPicker.Pick(currentZonePlan.roomPlans, r => r.weight);
        currentHotspot = WeightedPicker.Pick(currentRoomPlan.hotspots, h => h.weight);

        // 3. Update state memory
        lastZoneID = currentZonePlan.zoneID;

        // 4. Resolve transforms
        Transform hotspotTransform = NPCAnchorManager.Instance.GetAnchor(currentHotspot.hotspotID);
        Transform roomTransform = NPCAnchorManager.Instance.GetAnchor(currentRoomPlan.roomAnchorID);
        Transform zoneTransform = NPCAnchorManager.Instance.GetAnchor(currentZonePlan.zoneID);

        // 5. Move agent
        if (currentHotspot != null && hotspotTransform != null)
        {
            agent.SetDestination(hotspotTransform.position);
            //Debug.Log($"{data.npcName} is heading to {hotspotTransform.name} ({currentHotspot.weight}) in {roomTransform?.name} ({currentRoomPlan.weight}) of {zoneTransform?.name} ({currentZonePlan.weight})");

        }
    }


    private IEnumerator PerformWeightedHotspotAction()
    {
        waiting = true;

        //Debug.Log($"{data.npcName} is doing {currentHotspot.hotspotID} for {currentHotspot.duration} seconds");

        yield return new WaitForSeconds(currentHotspot.duration);

        waiting = false;
        PickNextWeightedRoutine();
    }

    public void InterruptRoutine()
    {
        waiting = false;

        if (agent.hasPath)
            agent.ResetPath();

        if (hotspotRoutine != null)
        {
            StopCoroutine(hotspotRoutine);
            hotspotRoutine = null;
        }
        //Debug.Log($"{data.npcName} interrupted due to behavior change.");
    }
    public void ReactToSuspicion(string sourceRoomID)
    {
        if (currentBehavior != BehaviorState.Normal)
            return;

        InterruptRoutine();

        if (searchRoutine != null)
            StopCoroutine(searchRoutine);

        searchRoutine = StartCoroutine(SuspicionPhase(sourceRoomID));
    }



    private IEnumerator SearchRoomRoutine(string roomID)
    {
        // Get room anchor (assumes you have NPCAnchorManager working)
        Transform roomTransform = NPCAnchorManager.Instance.GetAnchor(roomID);
        if (roomTransform == null)
        {
            Debug.LogWarning($"Room {roomID} not found for search.");
            yield break;
        }

        // Move to room
        agent.SetDestination(roomTransform.position);
        yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance < 0.3f);

        // Find hiding spots in this room
        var hidingSpots = roomTransform.GetComponentsInChildren<HidingSpot>();
        foreach (var spot in hidingSpots)
        {
            agent.SetDestination(spot.transform.position);
            yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance < 0.3f);
            yield return new WaitForSeconds(searchPointWaitTime);

            float searchRoll = Random.value;
            float searchChance = Mathf.Lerp(data.searchChance, data.maxSearchChance, data.infectionLevel);


            if (searchRoll < searchChance)
            {
                Debug.Log($"{data.npcName} is checking {spot.name} (rolled {searchRoll:F2} < {searchChance:F2})");
                // TODO: Play animation here if needed
                yield return new WaitForSeconds(1f);

                if (spot.IsPlayerHere(player))
                {
                    Debug.Log($"{data.npcName} found the player!");
                    // Trigger detection logic (e.g., alert, game over, flee, etc.)
                    yield break;
                }
            }
            else
            {
                Debug.Log($"{data.npcName} skipped checking {spot.name} (rolled {searchRoll:F2} > {searchChance:F2})");
            }
        }

        // Search done — go back to normal
        Debug.Log($"{data.npcName} finished searching.");
    }
    private IEnumerator SuspicionPhase(string sourceRoomID)
    {
        currentBehavior = BehaviorState.Suspicious;

        Transform roomTransform = NPCAnchorManager.Instance.GetAnchor(sourceRoomID);
        if (roomTransform == null)
        {
            Debug.LogWarning($"SuspicionPhase failed: Room {sourceRoomID} not found.");
            currentBehavior = BehaviorState.Normal;
            yield break;
        }

        // Go to the source of the sound
        agent.SetDestination(roomTransform.position);
        yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance < 0.3f);

        // Wait and "look around"
        yield return new WaitForSeconds(1.5f);

        // Now begin actual searching
        currentBehavior = BehaviorState.Search;
        Debug.Log($"{data.npcName} is now searching {sourceRoomID}...");
        yield return StartCoroutine(SearchRoomRoutine(sourceRoomID));

        // Once done, return to normal
        currentBehavior = BehaviorState.Normal;
        PickNextWeightedRoutine();
    }





}
