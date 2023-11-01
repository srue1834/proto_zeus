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
    public GameObject portal;

    public UIController uiController;

    private float pickupZeusTimer = 0f;

    private bool hasPickedUpZeus = false;
    private bool isFirstPickup = true;

    public ParticleSystem portalDissolveParticles;
    Animator anim;

    public Volume coldToneVolume;

    void Start()
    {
        anim = player.GetComponent<Animator>();
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == "Main")
        {
            NavMeshAgent zeusAgent = GetComponent<NavMeshAgent>();
            if (zeusAgent != null)
                zeusAgent.enabled = false;
        }

        if (coldToneVolume != null)
        {
            coldToneVolume.enabled = false;
        }
    }


    private void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        PlayerController playerController = player.GetComponent<PlayerController>();
        ZeusFollow zeusFollow = FindObjectOfType<ZeusFollow>();

        if (SceneManager.GetActiveScene().name == "Main")
        {
            if (distance <= interactionRadius && !IsCarryingZeus() && !hasPickedUpZeus)
            {
                pickupZeusTimer += Time.deltaTime;
                
                uiController.ShowCarryZeusPrompt(true);
                
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
                    anim.SetFloat("IsCarryingZeus", 0f);
                }
                else
                {
                    // Pick up Zeus
                    transform.position = carryPosition.position;
                    transform.SetParent(carryPosition);
                    transform.localRotation = Quaternion.identity;
                    portal.SetActive(true);
                    portalDissolveParticles.Play();
                    anim.SetFloat("IsCarryingZeus", 1f);
                }
                uiController.ShowCarryZeusPrompt(false);
            }
        }
        else if (SceneManager.GetActiveScene().name == "Level2")
        {
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
                if (playerController.GetCallCount() >= 2)
                {
                    if (IsCarryingZeus())
                    {
                        transform.SetParent(null);

                        NavMeshAgent zeusAgent = GetComponent<NavMeshAgent>();
                        if (zeusAgent != null)
                            zeusAgent.enabled = true;

                        portal.SetActive(false);

                        if (coldToneVolume != null)
                        {
                            coldToneVolume.enabled = false;
                        }
                    }
                    else
                    {
                        transform.position = carryPosition.position;
                        transform.SetParent(carryPosition);
                        transform.localRotation = Quaternion.identity;

                        NavMeshAgent zeusAgent = GetComponent<NavMeshAgent>();
                        if (zeusAgent != null)
                            zeusAgent.enabled = false;

                        portal.SetActive(true);

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