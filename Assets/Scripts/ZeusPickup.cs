using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeusPickup : MonoBehaviour
{
    public Transform player;
    public Transform carryPosition;  // The position on Bea where Zeus should be placed when carried
    public float interactionRadius = 5.0f;  // The distance within which Bea can interact with Zeus
    Animator zeus_anim;

    public GameObject portal;


    void Start()
    {
        zeus_anim = GetComponent<Animator>();
    }

    private void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        // Draw a debug line between Zeus and Bea
        Debug.DrawLine(transform.position, player.position, Color.red);
        //Debug.Log("Distance between Zeus and Bea: " + distance);



        if (distance <= interactionRadius && Input.GetKeyDown(KeyCode.P))
        {
            // Attach Zeus to Bea's carry position
            transform.SetParent(carryPosition);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            // Notify all vet staff that Zeus has been picked up
            VetStaffAI.OnZeusPickedUp();
            portal.SetActive(true);

        }
    }

    public bool IsCarryingZeus()
    {
        return transform.parent == carryPosition;
    }

}
