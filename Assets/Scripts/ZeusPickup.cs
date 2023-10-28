using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class ZeusPickup : MonoBehaviour
{
    public Transform player;
    public Transform carryPosition;
    public float interactionRadius = 5.0f;
    Animator zeus_anim;
    public GameObject portal;
    public Camera parallaxCamera;

    public UIController uiController;

    private float pickupZeusTimer = 0f;
    public float firstTimePickupDelay = 5f;  // Delay for the first time
    public float subsequentPickupDelay = 10f;  // Delay for subsequent times
    private bool hasPickedUpZeus = false;
    private bool isFirstPickup = true;

    void Start()
    {
        zeus_anim = GetComponent<Animator>();
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == "Main")
        {
            NavMeshAgent zeusAgent = GetComponent<NavMeshAgent>();
            if (zeusAgent != null)
                zeusAgent.enabled = false;
        }
    }

   
    private void Update()
    {

        float distance = Vector3.Distance(transform.position, player.position);
        PlayerController playerController = player.GetComponent<PlayerController>();

        if (SceneManager.GetActiveScene().name == "Main")
        {
            if (distance <= interactionRadius && !IsCarryingZeus() && !hasPickedUpZeus)
            {
                pickupZeusTimer += Time.deltaTime;
                if (pickupZeusTimer > firstTimePickupDelay)
                {
                    uiController.ShowCarryZeusPrompt(true);
                }
            }
            else
            {
                pickupZeusTimer = 0f;
            }

            if (distance <= interactionRadius && Input.GetKeyDown(KeyCode.P))
            {
                if (IsCarryingZeus())
                {
                    // Drop Zeus
                    transform.SetParent(null);
                    portal.SetActive(false);
                   
                }
                else
                {
                    // Pick up Zeus
                    transform.position = carryPosition.position;
                    transform.SetParent(carryPosition);
                    transform.localRotation = Quaternion.identity;
                    portal.SetActive(true);
                }
                uiController.ShowCarryZeusPrompt(false);

            }

        } else
        {
            if (distance <= interactionRadius && !IsCarryingZeus() && !hasPickedUpZeus && playerController.GetCallCount() >= 2)
            {
                pickupZeusTimer += Time.deltaTime;

                if ((isFirstPickup && pickupZeusTimer > firstTimePickupDelay) ||
                    (!isFirstPickup && pickupZeusTimer > subsequentPickupDelay))
                {
                    uiController.ShowCarryZeusPrompt(true);
                }
            }

            else
            {
                pickupZeusTimer = 0f;
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                hasPickedUpZeus = true;
                isFirstPickup = false;
                pickupZeusTimer = 0f;
                uiController.ShowCarryZeusPrompt(false);
            }

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