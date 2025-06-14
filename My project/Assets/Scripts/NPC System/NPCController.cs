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
    private float speed;


    [SerializeField] private GameObject player;
    [SerializeField] private float searchPointWaitTime = 1.5f;

    private Coroutine searchRoutine;

    private Vector3 previousPosition;

    private string lastHotspotID = null;


    private void Start()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponentInChildren<Animator>();

        previousPosition = transform.position;

        if (currentBehavior == BehaviorState.Normal)
        {
            PickNextWeightedRoutine();
        }
    }



    private void Update()
    {

        speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);

        if (data == null || waiting || agent.pathPending||!agent.enabled || !agent.isOnNavMesh)
            return;

        // Stop all routine logic if not in Normal behavior
        if (currentBehavior != BehaviorState.Normal)
            return;

        if (agent.remainingDistance < 0.1f)
        {
            hotspotRoutine = StartCoroutine(PerformWeightedHotspotAction());
        }

        
        
    }

    private float CalculateSpeed()
    {
        float distanceMoved = Vector3.Distance(transform.position, previousPosition);
        previousPosition = transform.position;

        // Ignore tiny micro-movements
        if (distanceMoved < 0.001f)
            return 0f;

        float speed = distanceMoved / Time.deltaTime;

        // Safety clamp to prevent unrealistic spikes
        if (speed > 5f) // whatever your max walk speed is
            speed = 5f;

        return speed;
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

        // Filter hotspots to avoid repeating last used hotspot
        List<WeightedHotspot> possibleHotspots = currentRoomPlan.hotspots
            .Where(h => h.hotspotID != lastHotspotID)
            .ToList();

        // Safety check: if filtering would remove everything, allow all again
        if (possibleHotspots.Count == 0)
        {
            possibleHotspots = currentRoomPlan.hotspots;
        }

        currentHotspot = WeightedPicker.Pick(possibleHotspots, h => h.weight);

        // 3. Update state memory
        lastZoneID = currentZonePlan.zoneID;
        lastHotspotID = currentHotspot.hotspotID;

        // 4. Resolve transforms
        Transform hotspotTransform = NPCAnchorManager.Instance.GetAnchor(currentHotspot.hotspotID);
        Transform roomTransform = NPCAnchorManager.Instance.GetAnchor(currentRoomPlan.roomAnchorID);
        Transform zoneTransform = NPCAnchorManager.Instance.GetAnchor(currentZonePlan.zoneID);

        // 5. Move agent
        if (currentHotspot != null && hotspotTransform != null)
        {
            agent.SetDestination(hotspotTransform.position);
            // Debug.Log($"{data.npcName} is heading to {hotspotTransform.name} ({currentHotspot.weight}) in {roomTransform?.name} ({currentRoomPlan.weight}) of {zoneTransform?.name} ({currentZonePlan.weight})");
        }
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

    private IEnumerator PerformWeightedHotspotAction()
    {
        waiting = true;
        agent.enabled = false;

        Transform currentHotspotTransform = NPCAnchorManager.Instance.GetAnchor(currentHotspot.hotspotID);

        // Play sit animation immediately (interpolation triggered via AnimationEvent for sit down)
        PlayHotspotAnimation();

        // Smooth rotation
        yield return SmoothRotateTo(currentHotspotTransform.rotation, 0.2f);

        // Wait for hotspot duration
        yield return new WaitForSeconds(currentHotspot.duration);

        // Trigger exit animation
        if (!string.IsNullOrEmpty(currentHotspot.exitAnimationTrigger))
            animator.SetTrigger(currentHotspot.exitAnimationTrigger);

        // Interpolate exit movement
        yield return InterpolateForward(currentHotspot.exitInterpolationOffset, currentHotspot.exitInterpolationTime);

        waiting = false;
        agent.enabled = true;
        PickNextWeightedRoutine();
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





    public void StartHotspotInterpolation()
    {
        StartCoroutine(InterpolateToHotspotPosition(
            currentHotspot.entryInterpolationTime, 
            currentHotspot.entryInterpolationOffset));
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
            case HotspotAction.ActionType.Sit:
                animator.SetTrigger("Sit");
                break;
            case HotspotAction.ActionType.Nap:
                animator.SetTrigger("Nap");
                break;
            case HotspotAction.ActionType.WatchTV:
                animator.SetTrigger("WatchTV");
                break;
            default:
                animator.SetTrigger("IdleAction");
                break;
        }
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


        animator.SetTrigger("LookAround");
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
