using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VetStaffAI : MonoBehaviour
{
    public Transform player; // Reference to Bea's transform
    public float detectionRadius = 5.0f; // Distance within which the vet staff will detect Bea
    Animator vet_anim;

    public float chaseSpeed = 4.0f;
    private bool isChasing = false;
    public float backOffDistance = 5.0f; // Distance vet staff backs off

    private Rigidbody rb; // Reference to the Rigidbody component

    void Start()
    {
        vet_anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>(); // Initialize the Rigidbody reference
    }

    private void Update()
    {
        // Calculate distance between vet staff and Bea
        float distance = Vector3.Distance(transform.position, player.position);

        // If Bea is within the detection radius
        if (distance <= detectionRadius)
        {
            // Calculate the direction to look at
            Vector3 lookDirection = player.position - transform.position;
            lookDirection.y = 0; // Keep rotation only on the Y-axis

            // Rotate the vet staff to face Bea
            Quaternion rotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 5.0f);
        }

        if (isChasing)
        {
            // Follow Bea
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * chaseSpeed * Time.deltaTime;

            // Play running animation (we'll set this up in the next step)
            vet_anim.SetBool("IsRunning", true);
        }

        if (IsCloseToBea() && Input.GetKeyDown(KeyCode.Space))
        {
            BackOff();
        }
    }

    public static void OnZeusPickedUp()
    {
        foreach (var vet in FindObjectsOfType<VetStaffAI>())
        {
            vet.ReactToZeusPickup();
        }
    }

    private void ReactToZeusPickup()
    {
        vet_anim.SetTrigger("Angry");
    }

    public static void OnBeaExitedRoom()
    {
        foreach (var vet in FindObjectsOfType<VetStaffAI>())
        {
            vet.isChasing = true;
        }
    }

    private bool IsCloseToBea()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        return distance <= 1.0f; // Adjust this value as needed
    }

    private void BackOff()
    {
        if (rb != null)
        {
            Vector3 pushDirection = (transform.position - player.position).normalized;
            rb.AddForce(pushDirection * 1000);  // 500 is the push force, adjust as needed
        }
    }

    public bool ShouldSlowBea()
    {
        bool close = IsCloseToBea();
        return close;
    }
}
