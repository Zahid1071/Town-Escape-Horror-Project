using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;


public class NPCController : MonoBehaviour
{
    public enum AIState { Natural, Suspicious, Search, Chase, Capture, Dead }
    public AIState currentState;

    [HideInInspector]
    public FSMController fsm;

    [HideInInspector]
    public InteractionController interaction;

    [HideInInspector]
    public PerceptionController perception;
    private SuspicionSystem suspicion;
    
    private void Awake()
    {
        fsm = new FSMController(this);
        interaction = GetComponent<InteractionController>();
        perception = GetComponent<PerceptionController>();
        suspicion = GetComponent<SuspicionSystem>();

        perception.OnPlayerSeen += HandlePlayerSeen;
        perception.OnPlayerHeard += HandlePlayerHeard;
        suspicion.OnSuspicionThresholdReached += HandleSuspicionReached;
    }

    private void Start()
    {
        fsm.SetState(AIState.Natural);
    }

    private void Update()
    {
        fsm.Update();
    }

    private void HandlePlayerSeen()
    {
        interaction.CancelInteraction();
        fsm.SetState(AIState.Chase);
    }

    private void HandlePlayerHeard()
    {
        if (currentState == AIState.Chase || currentState == AIState.Capture)
            return;

        interaction.CancelInteraction(); // Cancel if already interacting
        suspicion.AddSuspicion(20f);     // System will fire threshold event if needed
    }



    private void HandleSuspicionReached()
    {
        fsm.SetState(AIState.Suspicious);
    }
}


