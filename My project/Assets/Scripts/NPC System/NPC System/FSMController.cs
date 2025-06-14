using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;

public class FSMController
{
    private NPCController npc;

    private float searchTimer = 0f;

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
    }




    public void Update()
    {
        switch (npc.currentState)
        {
            case NPCController.AIState.Natural:
                if (!npc.interaction.IsInteracting())
                    npc.interaction.StartNextRoutine();
                break;


            case NPCController.AIState.Suspicious:
                MoveToSuspiciousTarget();
                break;
            case NPCController.AIState.Search:
                SearchRoom();
                break;
            case NPCController.AIState.Chase:
                ChasePlayer();
                break;
            case NPCController.AIState.Capture:
                CapturePlayer();
                break;
        }
    }


    private void ChasePlayer()
    {
        if (npc.perception.player == null)
            return;

        npc.GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(npc.perception.player.position);

        float distance = Vector3.Distance(npc.transform.position, npc.perception.player.position);
        if (distance < 1.5f) // capture range
        {
            npc.fsm.SetState(NPCController.AIState.Capture);
        }
    }


    private void CapturePlayer()
    {
        Debug.Log("Player Caught!");
        // TODO: Add capture animation or game over here.
    }


    private void MoveToSuspiciousTarget()
    {
        if (npc.perception.searchTarget == null)
        {
            npc.fsm.SetState(NPCController.AIState.Natural);
            return;
        }

        npc.GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(npc.perception.searchTarget.position);

        if (!npc.GetComponent<UnityEngine.AI.NavMeshAgent>().pathPending && npc.GetComponent<UnityEngine.AI.NavMeshAgent>().remainingDistance < 0.3f)
        {
            npc.fsm.SetState(NPCController.AIState.Search);
        }
    }

    private void SearchRoom()
    {
        // TEMP simple search: stand still & wait

        if (searchTimer <= 0f)
        {
            Debug.Log("Finished searching.");
            npc.fsm.SetState(NPCController.AIState.Natural);
        }
        else
        {
            searchTimer -= Time.deltaTime;
        }
    }

    private IEnumerator SearchRoutine()
    {
        Transform roomAnchor = npc.perception.searchTarget;

        if (roomAnchor == null)
        {
            npc.fsm.SetState(NPCController.AIState.Natural);
            yield break;
        }

        // Move to room first
        npc.GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(roomAnchor.position);
        yield return new WaitUntil(() => !npc.GetComponent<UnityEngine.AI.NavMeshAgent>().pathPending && npc.GetComponent<UnityEngine.AI.NavMeshAgent>().remainingDistance < 0.3f);

        Debug.Log("Arrived at search room");

        // Search hiding spots (optional)
        var hidingSpots = roomAnchor.GetComponentsInChildren<HidingSpot>();

        foreach (var spot in hidingSpots)
        {
            npc.GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(spot.transform.position);
            yield return new WaitUntil(() => !npc.GetComponent<UnityEngine.AI.NavMeshAgent>().pathPending && npc.GetComponent<UnityEngine.AI.NavMeshAgent>().remainingDistance < 0.3f);

            Debug.Log($"Checking hiding spot: {spot.name}");
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("Finished searching");
        npc.fsm.SetState(NPCController.AIState.Natural);
    }




}


