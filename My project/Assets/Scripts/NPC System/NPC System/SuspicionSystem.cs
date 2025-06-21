using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;

public class SuspicionSystem : MonoBehaviour
{
    public float suspicionLevel;
    public float suspicionThreshold = 100f;
    public float decayRate = 5f;

    public event Action OnSuspicionThresholdReached;

    private void Update()
    {
        if (suspicionLevel > 0)
        {
            suspicionLevel -= decayRate * Time.deltaTime;
            suspicionLevel = Mathf.Clamp(suspicionLevel, 0f, suspicionThreshold);
        }
    }

    public void AddSuspicion(float amount)
    {
        suspicionLevel += amount;

        if (suspicionLevel >= suspicionThreshold)
        {
            suspicionLevel = suspicionThreshold;
            OnSuspicionThresholdReached?.Invoke();
        }
    }
}
