using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    public NPCController npcController;

    public void StartHotspotInterpolation()
    {
        npcController.StartHotspotInterpolation();
    }
}
