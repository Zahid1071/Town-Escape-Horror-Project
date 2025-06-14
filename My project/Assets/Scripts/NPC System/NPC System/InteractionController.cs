using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InteractionController : MonoBehaviour
{
    public NPCData data;
    public NavMeshAgent agent;

    private Coroutine interactionRoutine;

    private WeightedZonePlan currentZonePlan;
    private WeightedRoomPlan currentRoomPlan;
    private WeightedHotspot currentHotspot;
    private string lastZoneID = null;
    private string lastHotspotID = null;

    public float remainHomeFactor = 1f;


    public enum MovementState { Idle, Walking, Running}

    public MovementState currentMovementState;


    void Update()
    {
        if (agent.pathPending || agent.remainingDistance > 0.1f)
            currentMovementState = MovementState.Walking;
        else
            currentMovementState = MovementState.Idle;
    }

    public void StartNextRoutine()
    {
        PickNextWeightedRoutine();
    }

    private void PickNextWeightedRoutine()
    {
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

        currentZonePlan = WeightedPicker.Pick(boostedZones, z => z.weight);
        currentRoomPlan = WeightedPicker.Pick(currentZonePlan.roomPlans, r => r.weight);

        List<WeightedHotspot> possibleHotspots = currentRoomPlan.hotspots
            .Where(h => h.hotspotID != lastHotspotID).ToList();

        if (possibleHotspots.Count == 0)
            possibleHotspots = currentRoomPlan.hotspots;

        currentHotspot = WeightedPicker.Pick(possibleHotspots, h => h.weight);
        lastZoneID = currentZonePlan.zoneID;
        lastHotspotID = currentHotspot.hotspotID;

        Transform hotspotTransform = NPCAnchorManager.Instance.GetAnchor(currentHotspot.hotspotID);
        if (hotspotTransform != null)
            agent.SetDestination(hotspotTransform.position);

        // Start checking arrival
        if (interactionRoutine != null)
            StopCoroutine(interactionRoutine);

        interactionRoutine = StartCoroutine(WaitForArrival(hotspotTransform));
    }

    private IEnumerator WaitForArrival(Transform hotspotTransform)
    {
        while (agent.pathPending || agent.remainingDistance > 0.2f)
            yield return null;

        StartInteraction(currentHotspot);
    }

    public void StartInteraction(WeightedHotspot hotspot)
    {
        if (interactionRoutine != null)
            StopCoroutine(interactionRoutine);

        interactionRoutine = StartCoroutine(PerformInteraction(hotspot));
    }

    public void CancelInteraction()
    {
        if (interactionRoutine != null)
            StopCoroutine(interactionRoutine);
    }

    private IEnumerator PerformInteraction(WeightedHotspot hotspot)
    {
        // Entry interpolation here (reimplement your old smooth sit entry)

        // Play animation based on hotspot type
        PlayHotspotAnimation(hotspot);

        yield return new WaitForSeconds(hotspot.duration);

        // Exit interpolation here (reimplement your old smooth exit)

        // Automatically start next routine after interaction
        StartNextRoutine();
    }

    private void PlayHotspotAnimation(WeightedHotspot hotspot)
    {
        // TEMP: Hook up your real animation triggers later
        Debug.Log($"Playing interaction: {hotspot.actionType}");
    }

    public bool IsInteracting()
    {
        return interactionRoutine != null;
    }

}
