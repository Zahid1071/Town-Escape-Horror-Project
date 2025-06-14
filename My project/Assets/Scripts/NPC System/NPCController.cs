using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NPCController : MonoBehaviour
{
    public NPCData data;
    public NavMeshAgent agent;
    public Animator animator;

    public enum BehaviorState { Normal, Suspicious, Search, Hostile }
    public BehaviorState currentBehavior = BehaviorState.Normal;

    private bool waiting = false;
    private bool isInHotspot = false;

    private WeightedZonePlan currentZonePlan;
    private WeightedRoomPlan currentRoomPlan;
    private WeightedHotspot currentHotspot;

    private string lastZoneID = null;
    private string lastHotspotID = null;
    private Coroutine hotspotRoutine;
    public float remainHomeFactor;

    [SerializeField] private GameObject player;
    [SerializeField] private float searchPointWaitTime = 1.5f;
    private Coroutine searchRoutine;

    private void Start()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponentInChildren<Animator>();

        if (currentBehavior == BehaviorState.Normal)
            PickNextWeightedRoutine();
    }

    private void Update()
    {
        animator.SetFloat("Speed", agent.velocity.magnitude);

        if (data == null || waiting || agent.pathPending || !agent.enabled || !agent.isOnNavMesh)
            return;

        if (currentBehavior != BehaviorState.Normal)
            return;

        if (agent.remainingDistance < 0.1f)
        {
            hotspotRoutine = StartCoroutine(PerformWeightedHotspotAction());
        }
    }

    private void PickNextWeightedRoutine()
    {
        List<WeightedZonePlan> boostedZones = new List<WeightedZonePlan>();
        foreach (var zone in data.weightedRoutinePlans)
        {
            float boostFactor = (zone.zoneID == lastZoneID)
                ? ((zone.zoneID == data.homeID) ? remainHomeFactor : remainHomeFactor * 0.8f)
                : 1f;

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
    }

    private IEnumerator PerformWeightedHotspotAction()
    {
        waiting = true;
        isInHotspot = true;
        agent.enabled = false;

        Transform currentHotspotTransform = NPCAnchorManager.Instance.GetAnchor(currentHotspot.hotspotID);

        PlayHotspotAnimation();
        yield return SmoothRotateTo(currentHotspotTransform.rotation, 0.2f);
        yield return new WaitForSeconds(currentHotspot.duration);

        yield return ExitHotspotSequence();

        waiting = false;
        isInHotspot = false;
        agent.enabled = true;
        PickNextWeightedRoutine();
    }

    private IEnumerator ExitHotspotSequence()
    {
        if (!string.IsNullOrEmpty(currentHotspot.exitAnimationTrigger))
            animator.SetTrigger(currentHotspot.exitAnimationTrigger);

        yield return InterpolateForward(currentHotspot.exitInterpolationOffset, currentHotspot.exitInterpolationTime);
    }

    private IEnumerator SmoothRotateTo(Quaternion targetRotation, float duration)
    {
        Quaternion startRotation = transform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        transform.rotation = targetRotation;
    }

    public void StartHotspotInterpolation()
    {
        StartCoroutine(InterpolateToHotspotPosition(currentHotspot.entryInterpolationTime, currentHotspot.entryInterpolationOffset));
    }

    private IEnumerator InterpolateToHotspotPosition(float totalDuration, float offsetAmount)
    {
        Transform currentHotspotTransform = NPCAnchorManager.Instance.GetAnchor(currentHotspot.hotspotID);

        Vector3 offset = -currentHotspotTransform.forward * offsetAmount;
        Vector3 targetPosition = currentHotspotTransform.position + offset;

        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;

        while (elapsedTime < totalDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / totalDuration;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        transform.position = targetPosition;
    }

    private IEnumerator InterpolateForward(float offsetAmount, float totalDuration)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = new Vector3(
            transform.position.x + transform.forward.x * offsetAmount,
            transform.position.y,
            transform.position.z + transform.forward.z * offsetAmount
        );

        while (elapsedTime < totalDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / totalDuration;
            Vector3 interpolated = Vector3.Lerp(startPosition, targetPosition, t);
            interpolated.y = startPosition.y;
            transform.position = interpolated;
            yield return null;
        }

        transform.position = targetPosition;
    }

    private void PlayHotspotAnimation()
    {
        switch (currentHotspot.actionType)
        {
            case HotspotAction.ActionType.Sit: animator.SetTrigger("Sit"); break;
            case HotspotAction.ActionType.Nap: animator.SetTrigger("Nap"); break;
            case HotspotAction.ActionType.WatchTV: animator.SetTrigger("WatchTV"); break;
            default: animator.SetTrigger("IdleAction"); break;
        }
    }

    public void CancelHotspotAction()
    {
        if (!isInHotspot)
            return;

        if (hotspotRoutine != null)
        {
            StopCoroutine(hotspotRoutine);
            hotspotRoutine = null;
        }

        StartCoroutine(ExitHotspotAfterInterrupt());
    }


    private IEnumerator ExitHotspotAfterInterrupt()
    {
        yield return ExitHotspotSequence();

        waiting = false;
        isInHotspot = false;

        // Agent is always safely re-enabled after exit completes
        agent.enabled = true;
    }


    public void ReactToSuspicion(string sourceRoomID)
    {
        if (currentBehavior != BehaviorState.Normal)
            return;

        CancelHotspotAction();

        if (searchRoutine != null)
            StopCoroutine(searchRoutine);

        StartCoroutine(WaitForAgentAndStartSuspicion(sourceRoomID));
    }

    private IEnumerator WaitForAgentAndStartSuspicion(string sourceRoomID)
    {
        // Wait until agent is fully re-enabled after exit
        yield return new WaitUntil(() => agent.enabled);

        searchRoutine = StartCoroutine(SuspicionPhase(sourceRoomID));
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

        agent.SetDestination(roomTransform.position);
        yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance < 0.3f);

        animator.SetTrigger("LookAround");
        yield return new WaitForSeconds(1.5f);

        currentBehavior = BehaviorState.Search;
        yield return StartCoroutine(SearchRoomRoutine(sourceRoomID));

        currentBehavior = BehaviorState.Normal;
        PickNextWeightedRoutine();
    }

    private IEnumerator SearchRoomRoutine(string roomID)
    {
        Transform roomTransform = NPCAnchorManager.Instance.GetAnchor(roomID);
        if (roomTransform == null)
        {
            Debug.LogWarning($"Room {roomID} not found for search.");
            yield break;
        }

        agent.SetDestination(roomTransform.position);
        yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance < 0.3f);

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
                yield return new WaitForSeconds(1f);
                if (spot.IsPlayerHere(player))
                {
                    Debug.Log($"{data.npcName} found the player!");
                    yield break;
                }
            }
        }
    }
}
