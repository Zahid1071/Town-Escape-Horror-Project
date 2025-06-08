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
        Hostile,
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
            Debug.Log($"{data.npcName} is heading to {hotspotTransform.name} ({currentHotspot.weight}) in {roomTransform?.name} ({currentRoomPlan.weight}) of {zoneTransform?.name} ({currentZonePlan.weight})");

        }
    }


    private IEnumerator PerformWeightedHotspotAction()
    {
        waiting = true;

        Debug.Log($"{data.npcName} is doing {currentHotspot.hotspotID} for {currentHotspot.duration} seconds");

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
        Debug.Log($"{data.npcName} interrupted due to behavior change.");
    }
    public void ReactToSuspicion(string sourceRoomID)
    {
        if (currentBehavior == BehaviorState.Suspicious || currentBehavior == BehaviorState.Hostile)
            return; // Already reacting

        currentBehavior = BehaviorState.Suspicious;
        Debug.Log($"{data.npcName} is now suspicious due to noise in {sourceRoomID}");

        // You'll later trigger actual search behavior here
    }


}
