using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;
using UnityEditor;

public class PerceptionController : MonoBehaviour
{
    public float maxViewDistance = 10f;
    public float maxViewAngle = 90f;
    public LayerMask obstacleMask;
    public Transform player;
    private NPCController npc;


    [HideInInspector]
    public Transform searchTarget;

    private float lostSightTimer = 0f;
    public float gracePeriod = 2f;
    public float keepChasingDistance = 3f; // if player is close, stay in Chase

    public event Action OnPlayerSeen;
    public event Action OnPlayerHeard;

    public bool CanSeePlayer { get; private set; } = false;
    public Vector3 LastKnownPlayerPosition { get; private set; }
    void Start()
    {
        npc = GetComponent<NPCController>();

    }
    private void Update()
    {
        VisionCheck();
        HandleVisionLoss();
    }

    private void VisionCheck()
    {
        if (player == null) return;

        Vector3 npcEyePos = transform.position + Vector3.up * 1.7f;
        Vector3 playerHeadPos = player.position + Vector3.up * 1.6f;
        Vector3 directionToPlayer = (playerHeadPos - npcEyePos).normalized;

        float distance = Vector3.Distance(npcEyePos, playerHeadPos);
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (distance <= maxViewDistance && angle <= maxViewAngle)
        {
            if (Physics.Raycast(npcEyePos, directionToPlayer, out RaycastHit hit, maxViewDistance, ~0))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    LastKnownPlayerPosition = playerHeadPos;
                    if (!CanSeePlayer)
                    {
                        CanSeePlayer = true;
                        OnPlayerSeen?.Invoke();
                    }
                    return;
                }
            }
        }

        CanSeePlayer = false;
    }


    public void HearPlayerFootstep(Transform soundSource)
    {
        Transform closestAnchor = NPCAnchorManager.Instance.FindClosestOwnedAnchor(
            soundSource.position,
            100f,
            npc.interaction.data.homeID
        );

        if (closestAnchor == null)
        {
            Debug.LogWarning("No nearby anchor found — ignoring sound.");
            return;
        }

        if (npc.currentState != NPCController.AIState.Chase)
        {
            npc.perception.LastKnownPlayerPosition = soundSource.position; // ✅ fix: set last known position
            searchTarget = closestAnchor;
            OnPlayerHeard?.Invoke();
        }
    }



    void HandleVisionLoss()
    {
        if (CanSeePlayer)
        {
            lostSightTimer = 0f;
        }
        else if (npc.currentState == NPCController.AIState.Chase)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer < keepChasingDistance)
            {
                // Still close enough to keep chasing
                lostSightTimer = 0f;
            }
            else
            {
                lostSightTimer += Time.deltaTime;

                if (lostSightTimer >= gracePeriod)
                {
                    npc.fsm.SetState(NPCController.AIState.Suspicious);
                }
            }
        }

    }

}