using UnityEngine;

public class VetStaffAI : MonoBehaviour
{
    public Transform player; 
    public float detectionRadius = 5.0f; 
    Animator vet_anim;

    public float chaseSpeed = 4.0f;
    private bool isChasing = false;
    public float backOffDistance = 5.0f;

    private Rigidbody rb; 

    void Start()
    {
        vet_anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>(); 
    }

    private void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        // if Bea is within the detection radius
        if (distance <= detectionRadius)
        {
            Vector3 lookDirection = player.position - transform.position;
            lookDirection.y = 0; 

            // rotate the vet staff to face Bea
            Quaternion rotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 5.0f);
        }

        if (isChasing)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * chaseSpeed * Time.deltaTime;

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
        return distance <= 1.0f;
    }

    private void BackOff()
    {
        if (rb != null)
        {
            Vector3 pushDirection = (transform.position - player.position).normalized;
            rb.AddForce(pushDirection * 1000);  
        }
    }

    public bool ShouldSlowBea()
    {
        bool close = IsCloseToBea();
        return close;
    }
}
