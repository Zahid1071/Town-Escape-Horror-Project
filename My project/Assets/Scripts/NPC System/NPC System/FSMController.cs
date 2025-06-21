using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;
using Unity.VisualScripting;

public class FSMController
{
    private NPCController npc;

    public float walkingSpeed = 2.5f;
    public float runningSpeed = 4.5f;


    public FSMController(NPCController controller)
    {
        npc = controller;
    }

    public void SetState(NPCController.AIState newState)
    {
        npc.currentState = newState;
        Debug.Log($"Switched to state: {newState}");

        if (newState == NPCController.AIState.Search)
        {
            npc.StartCoroutine(SearchRoutine());
        }
        else if (newState == NPCController.AIState.Natural)
        {
            npc.interaction.StartNextRoutine(); // 🔥 force resume routine
        }
    }

    public void Update()
    {
        switch (npc.currentState)
        {
            case NPCController.AIState.Natural:

                npc.GetComponent<NavMeshAgent>().speed = walkingSpeed;
                if (!npc.interaction.IsBusyWithRoutine())
                    npc.interaction.StartNextRoutine();
                break;


            case NPCController.AIState.Suspicious:
                npc.GetComponent<NavMeshAgent>().speed = runningSpeed;
                MoveToSuspiciousTarget();
                break;
            case NPCController.AIState.Search:
                npc.GetComponent<NavMeshAgent>().speed = walkingSpeed+((runningSpeed-walkingSpeed)/3);
                break;
            case NPCController.AIState.Chase:
                npc.GetComponent<NavMeshAgent>().speed = runningSpeed;
                ChasePlayer();
                break;
            case NPCController.AIState.Capture:
                CapturePlayer();
                break;
        }
    }


    private void ChasePlayer()
    {
        if (!npc.perception.CanSeePlayer)
        {
            // Player lost — become suspicious and move to last known position
            npc.perception.searchTarget = null; // clear old anchor
            npc.fsm.SetState(NPCController.AIState.Suspicious);
            return;
        }

        Vector3 playerPos = npc.perception.player.position;
        npc.GetComponent<NavMeshAgent>().SetDestination(playerPos);

        if (Vector3.Distance(npc.transform.position, playerPos) < 1.5f)
        {
            //npc.fsm.SetState(NPCController.AIState.Capture);
        }
    }



    private void CapturePlayer()
    {
        Debug.Log("Player Caught!");
        // TODO: Add capture animation or game over here.
    }


    private void MoveToSuspiciousTarget()
    {
        Vector3 targetPosition = npc.perception.LastKnownPlayerPosition;
        npc.GetComponent<NavMeshAgent>().SetDestination(targetPosition);

        if (!npc.GetComponent<NavMeshAgent>().pathPending &&
            npc.GetComponent<NavMeshAgent>().remainingDistance < 0.3f)
        {
            string homeZone = npc.interaction.data.homeID;

            Transform anchor = NPCAnchorManager.Instance.FindClosestOwnedAnchor(targetPosition,10f,homeZone);

            if (anchor != null)
            {
                npc.perception.searchTarget = anchor;
                npc.fsm.SetState(NPCController.AIState.Search);
            }
            else
            {
                Debug.Log("No valid anchor near last seen player position. Returning to Natural.");
                npc.fsm.SetState(NPCController.AIState.Natural);
            }
        }
    }

    private IEnumerator SearchRoutine()
    {

        yield return new WaitForSeconds(2);
        Transform roomAnchor = npc.perception.searchTarget;

        if (roomAnchor == null)
        {
            npc.fsm.SetState(NPCController.AIState.Natural);
            yield break;
        }

        yield return new WaitUntil(() =>
            !npc.GetComponent<NavMeshAgent>().pathPending &&
            npc.GetComponent<NavMeshAgent>().remainingDistance < 0.3f);


        // Get all hiding spots
        var hidingSpots = roomAnchor.GetComponentsInChildren<HidingSpot>();

        foreach (var spot in hidingSpots)
        {
            // If player is seen mid-search — switch to chase
            if (npc.perception.CanSeePlayer)
            {
                npc.fsm.SetState(NPCController.AIState.Chase);
                yield break;
            }

            npc.GetComponent<NavMeshAgent>().SetDestination(spot.transform.position);
            yield return new WaitUntil(() =>
                !npc.GetComponent<NavMeshAgent>().pathPending &&
                npc.GetComponent<NavMeshAgent>().remainingDistance < 0.3f);



            if (spot.isOccupied == true)
            {
                Debug.Log($"Player Cought in: {spot.name}");
            }
            else
            {
                Debug.Log($"Nothing found in: {spot.name}");
            }
            
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("Player not found. Returning to routine.");
        npc.fsm.SetState(NPCController.AIState.Natural);
    }





}


