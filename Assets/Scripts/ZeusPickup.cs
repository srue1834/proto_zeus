using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
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
    public float firstTimePickupDelay = 1f;  // Delay for the first time
    public float subsequentPickupDelay = 1f;  // Delay for subsequent times
    private bool hasPickedUpZeus = false;
    private bool isFirstPickup = true;

    public ParticleSystem portalDissolveParticles;
    Animator anim;

    public Volume coldToneVolume;



    void Start()
    {
        anim = player.GetComponent<Animator>();
        zeus_anim = GetComponent<Animator>();
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == "Main")
        {
            NavMeshAgent zeusAgent = GetComponent<NavMeshAgent>();
            if (zeusAgent != null)
                zeusAgent.enabled = false;
        }

        // Initially, we assume the scene starts with the normal tone, so disable the cold tone volume
        if (coldToneVolume != null)
        {
            coldToneVolume.enabled = false;
        }
    }


    private void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        PlayerController playerController = player.GetComponent<PlayerController>();
        ZeusFollow zeusFollow = FindObjectOfType<ZeusFollow>(); // Get the ZeusFollow component

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
                    portalDissolveParticles.Stop();
                    anim.SetFloat("IsCarryingZeus", 0f); // For false
                }
                else
                {
                    // Pick up Zeus
                    transform.position = carryPosition.position;
                    transform.SetParent(carryPosition);
                    transform.localRotation = Quaternion.identity;
                    portal.SetActive(true);
                    portalDissolveParticles.Play();
                    anim.SetFloat("IsCarryingZeus", 1f); // For true
                }
                uiController.ShowCarryZeusPrompt(false);
            }
        }
        else if (SceneManager.GetActiveScene().name == "Level2")
        {
            // Check if Zeus is currently in the StoppedAtBea state and Bea has called Zeus twice
            if (distance <= interactionRadius && !IsCarryingZeus() && !hasPickedUpZeus && playerController.GetCallCount() >= 2 && zeusFollow.CurrentState == ZeusFollow.ZeusState.StoppedAtBea)
            {
                pickupZeusTimer += Time.deltaTime;
                uiController.ShowCarryZeusPrompt(true);
                
            }
            else
            {
                pickupZeusTimer = 0f;
                uiController.ShowCarryZeusPrompt(false);
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

                        // Disable the cold tone PPV when Zeus is dropped
                        if (coldToneVolume != null)
                        {
                            coldToneVolume.enabled = false;
                        }
                    }
                    else
                    {
                        
                        // Pick up Zeus
                        transform.position = carryPosition.position; // Explicitly set Zeus's position before setting parent
                        transform.SetParent(carryPosition);
                        transform.localRotation = Quaternion.identity;

                        // Disable NavMeshAgent
                        NavMeshAgent zeusAgent = GetComponent<NavMeshAgent>();
                        if (zeusAgent != null)
                            zeusAgent.enabled = false;

                        portal.SetActive(true);

                        // Enable the cold tone PPV when Zeus is picked up
                        if (coldToneVolume != null)
                        {
                            coldToneVolume.enabled = true;
                        }
                    }
                }
            }
        }
    }

    public bool IsCarryingZeus()
    {
        return transform.parent == carryPosition;
    }

}