using UnityEngine;

public class HidingSpot : MonoBehaviour
{
    public Transform hidePosition; // Where the player gets moved to
    public bool isOccupied = false;

    private int normalLayer;
    private int hiddenLayer = 7;


    void Start()
    {
        normalLayer = GameObject.Find("Player").layer;
    }


    public void HidePlayer(GameObject player)
    {
        if (isOccupied) return;

        player.transform.position = hidePosition.position;
        player.transform.rotation = hidePosition.rotation;

        isOccupied = true;
        player.GetComponent<PlayerHider>()?.SetHidden(true);
        player.layer = hiddenLayer;
    }

    public void RevealPlayer(GameObject player)
    {
        isOccupied = false;
        player.GetComponent<PlayerHider>()?.SetHidden(false);

        player.transform.position = hidePosition.position;
        player.layer = normalLayer;
    }

    public bool IsPlayerHere(GameObject player)
    {
        return isOccupied && player.GetComponent<PlayerHider>().isHidden &&
            player.transform.position == hidePosition.position;
    }

}
