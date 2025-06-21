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

    private Animator animator;

    private float animSpeed;



    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        SetCurrentMovementState();
        UpdateAnimation();

    }


    private void SetCurrentMovementState()
    {
        float velocity = agent.velocity.magnitude;

        if (agent.pathPending || agent.remainingDistance > 0.1f)
        {
            if (velocity > 2.5f + 0.1f) // small buffer to prevent flickering
                currentMovementState = MovementState.Running;
            else
                currentMovementState = MovementState.Walking;
        }
        else
        {
            currentMovementState = MovementState.Idle;
        }
    }



    private void UpdateAnimation()
    {

        if (animator == null || agent == null) return;
        
        float targetSpeed = agent.velocity.magnitude;
        animSpeed = Mathf.Lerp(animSpeed, targetSpeed, Time.deltaTime * 8f);
        animator.SetFloat("Speed", animSpeed);
       
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

        interactionRoutine = null;
    }


    private IEnumerator PerformInteraction(WeightedHotspot hotspot)
    {

        Transform anchor = NPCAnchorManager.Instance.GetAnchor(hotspot.hotspotID);
        if (anchor != null)
        {
            float t = 0f;
            float duration = 0.4f;
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;

            while (t < duration)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, anchor.position, t / duration);
                transform.rotation = Quaternion.Slerp(startRot, anchor.rotation, t / duration);
                yield return null;
            }

            transform.position = anchor.position;
            transform.rotation = anchor.rotation;
        }

        PlayHotspotAnimation(hotspot);
        yield return new WaitForSeconds(hotspot.duration);
        StartNextRoutine();
    }


    private void PlayHotspotAnimation(WeightedHotspot hotspot)
    {
        if (animator == null) return;

        switch (hotspot.actionType)
        {
            case HotspotAction.ActionType.Sit:
                animator.SetTrigger("Sit");
                break;
            case HotspotAction.ActionType.WatchTV:
                animator.SetTrigger("WatchTV");
                break;
            case HotspotAction.ActionType.Nap:
                animator.SetTrigger("Nap");
                break;
            default:
                Debug.LogWarning($"No animation mapped for: {hotspot.actionType}");
                break;
        }

        Debug.Log($"Playing interaction: {hotspot.actionType}");
    }


    public bool IsBusyWithRoutine()
    {
        return interactionRoutine != null;
    }

}
