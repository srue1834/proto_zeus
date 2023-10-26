using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZeusPickup : MonoBehaviour
{
    public Transform player;
    public Transform carryPosition;  // The position on Bea where Zeus should be placed when carried
    public float interactionRadius = 5.0f;  // The distance within which Bea can interact with Zeus
    Animator zeus_anim;
    public GameObject portal;
    public Camera parallaxCamera; // Drag and drop the parallax camera here from the inspector


    void Start()
    {
        zeus_anim = GetComponent<Animator>();
    }

    private void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        // Draw a debug line between Zeus and Bea
        Debug.DrawLine(transform.position, player.position, Color.red);

        // Continuous logging of Zeus's position and parent for debugging

        PlayerController playerController = player.GetComponent<PlayerController>();


        if (distance <= interactionRadius && Input.GetKeyDown(KeyCode.P) && playerController)
        {
            // If Zeus has been called twice
            if (playerController.GetCallCount() >= 2)
            {
                if (IsCarryingZeus())
                {
                    // Drop Zeus
                    transform.SetParent(null);

                    // Enable NavMeshAgent
                    NavMeshAgent zeusAgent = GetComponent<NavMeshAgent>();
                    if (zeusAgent != null)
                        zeusAgent.enabled = true;

                    portal.SetActive(false);
                }
                else
                {
                    // Disable NavMeshAgent
                    NavMeshAgent zeusAgent = GetComponent<NavMeshAgent>();
                    if (zeusAgent != null)
                        zeusAgent.enabled = false;

                    // Pick up Zeus
                    transform.position = carryPosition.position; // Explicitly set Zeus's position before setting parent
                    transform.SetParent(carryPosition);
                    transform.localRotation = Quaternion.identity;

                    // Notify all vet staff that Zeus has been picked up
                    VetStaffAI.OnZeusPickedUp();
                    portal.SetActive(true);

                    ZeusFollow zeusFollow = GetComponent<ZeusFollow>();
                    if (zeusFollow && zeusFollow.IsExhaustedAfterFifthFetch())
                    {
                        ParallaxController parallaxController = FindObjectOfType<ParallaxController>();
                        if (zeusFollow && zeusFollow.IsExhaustedAfterFifthFetch())
                        {
                            ChangeBackgroundToSad();
                        }

                    }
                }
            }
        }

    }

    private void ChangeBackgroundToSad()
    {
        foreach (ParallaxImage image in parallaxCamera.GetComponentsInChildren<ParallaxImage>(true))
        {
            image.ChangeToSadColor();
        }
    }



    public bool IsCarryingZeus()
    {
        return transform.parent == carryPosition;
    }

}