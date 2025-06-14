using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;
public class PerceptionController : MonoBehaviour
{
    public float viewDistance = 10f;
    public float viewAngle = 90f;
    public LayerMask obstacleMask;
    public Transform player;

    [HideInInspector]
    public Transform searchTarget;

    public event Action OnPlayerSeen;
    public event Action OnPlayerHeard;

    private void Update()
    {
        VisionCheck();
    }

    private void VisionCheck()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (distToPlayer <= viewDistance)
        {
            float angle = Vector3.Angle(transform.forward, dirToPlayer);
            if (angle < viewAngle / 2f)
            {
                if (!Physics.Raycast(transform.position, dirToPlayer, distToPlayer, obstacleMask))
                {
                    OnPlayerSeen?.Invoke();
                }
            }
        }
    }

    public void HearPlayerFootstep(Transform soundSource)
    {
        searchTarget = soundSource;
        OnPlayerHeard?.Invoke();
    }

}