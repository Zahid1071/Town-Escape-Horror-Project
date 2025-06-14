using UnityEngine;

public class FootstepNoiseEmitter : MonoBehaviour
{
    public float footstepInterval = 0.5f;
    private float timer = 0f;

    void Update()
    {
        if (IsWalking())
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                EmitFootstep();
                timer = footstepInterval;
            }
        }
        else
        {
            timer = 0f;
        }
    }

    private void EmitFootstep()
    {
        foreach (var npc in Object.FindObjectsByType<PerceptionController>(FindObjectsSortMode.None))
        {
            npc.HearPlayerFootstep(transform);
        }
    }

    private bool IsWalking()
    {
        return Input.GetKey(KeyCode.W);
    }
}
