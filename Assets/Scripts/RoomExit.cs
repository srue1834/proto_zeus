using UnityEngine;

public class RoomExit : MonoBehaviour
{
    public Transform player;
    private bool hasEntered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == player)
        {
            if (hasEntered)
            {
                VetStaffAI.OnBeaExitedRoom();
                player.GetComponent<PlayerController>().OnExitRoom(); 
            }
            else
            {
                hasEntered = true;
            }
        }
    }
}

