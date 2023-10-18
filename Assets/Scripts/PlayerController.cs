using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerController : MonoBehaviour
{
    public float runSpeed;
    public float walkSpeed;

    Rigidbody rb;
    bool rightDir;
    bool isSitting;
    Animator anim;

    // jump
    bool grounded = false;

    Collider[] groundColls;
    float groundRad = 0.2f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float jumpForce;

    public bool isInCutscene = false;

    bool canMove = false;
    float timeSinceSceneLoaded = 0;



    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rightDir = true;
        anim = GetComponent<Animator>();

        // Check the current scene's name
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == "Main")  // Replace "Level1" with the exact name of your first level
        {
            isSitting = true;
        }
        else
        {
            isSitting = false;
        }

        canMove = false;  // Set canMove to false when the scene starts
        timeSinceSceneLoaded = 0;  // Reset the timer

    }

    void Update()
    {
        // Update the timer
        timeSinceSceneLoaded += Time.deltaTime;

        // If more than 13.90 seconds have passed since the scene was loaded, allow the player to move
        if (timeSinceSceneLoaded > 13.90f)
        {
            canMove = true;
        }
    }

    private void FixedUpdate()
    {
        //Debug.Log("isInCutscene: " + isInCutscene);
        if (!canMove)  // If the player can't move, skip the rest of the FixedUpdate logic
            return;

        if (isInCutscene)
        {
            return;
        }

        if (isSitting)
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                anim.SetTrigger("sitting");  // Replace "StandUpAnimation" with the name of your animation if different.
                isSitting = false;
            }
            return;  // Skip the rest of the FixedUpdate logic if Bea is sitting.
        }


        if (grounded && Input.GetAxis("Jump") > 0)
        {
            grounded = false;
            rb.AddForce(new Vector3(0, jumpForce, 0)); // apply force in y direction
        }

        // draw sphere 
        groundColls = Physics.OverlapSphere(groundCheck.position, groundRad, groundLayer);
        if (groundColls.Length > 0) grounded = true;
        else grounded = false;


        float move = Input.GetAxis("Horizontal"); // a, d key
        anim.SetFloat("walk_speed", Mathf.Abs(move));

        if (isSitting == false)
        {
            anim.SetTrigger("goIdle");  // Transition to the "idle" state when the player starts moving
        }

        float running = Input.GetAxisRaw("Fire3"); // left shift
        anim.SetFloat("speed", Mathf.Abs(running));


        if (running > 0 && grounded)
        {
            rb.velocity = new Vector3(move * runSpeed, rb.velocity.y, 0);

        }
        else
        {
            rb.velocity = new Vector3(move * walkSpeed, rb.velocity.y, 0);
        }



        if (move > 0 && !rightDir) Flip();
        else if (move < 0 && rightDir) Flip();

    }

    // flip z value
    void Flip()
    {
        rightDir = !rightDir;

        // Rotate around the Y-axis
        Vector3 currentRotation = transform.eulerAngles;
        currentRotation.y += 180;  // Add 180 degrees to the current Y rotation
        transform.eulerAngles = currentRotation;
    }

}