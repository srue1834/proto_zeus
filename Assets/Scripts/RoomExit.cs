using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomExit : MonoBehaviour
{
    public Transform player;
    private bool hasEntered = false;  // Flag to track if Bea has entered the room

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == player)
        {
            if (hasEntered)
            {
                // Bea is exiting the room
                VetStaffAI.OnBeaExitedRoom();
                player.GetComponent<PlayerController>().OnExitRoom(); // Call the OnExitRoom method
            }
            else
            {
                // Bea is entering the room for the first time
                hasEntered = true;
            }
        }
    }
}

