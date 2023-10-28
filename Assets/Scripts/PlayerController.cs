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


    private bool isCallingZeus = false;
    private int callCount = 0;
    public UIController uiController; // Reference to UIController to control prompts

    private bool hasExitedRoom = false; // Flag to track if Bea has exited the room

    // Variables to control prompt display delays
    private float wakePromptTimer = 0f;
    private float callPromptTimer = 0f;
    private float runPromptTimer = 0f;
    public float promptDelay = 5f; // Delay for the first time the player can perform an action
    public bool shouldZeusWait = false; // Add this new variable at the top with other member variables.


    private float originalRunSpeed;
    private float originalWalkSpeed;
    private bool hasRunPromptShown = false;


    public int GetCallCount()
    {
        return callCount;
    }


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rightDir = true;
        anim = GetComponent<Animator>();

        // Check the current scene's name
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == "Main")  
        {
            isSitting = true;
        }
        else
        {
            isSitting = false;
        }

        if (SceneManager.GetActiveScene().name == "Level2")
        {
            canMove = false;  // Set canMove to false when the scene starts

        } else
        {
            canMove = true;
        }

        timeSinceSceneLoaded = 0;  // Reset the timer

        originalRunSpeed = runSpeed;
        originalWalkSpeed = walkSpeed;
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

        ZeusFollow zeusFollow = FindObjectOfType<ZeusFollow>();

        if (isSitting)
        {
            wakePromptTimer += Time.deltaTime;
            if (wakePromptTimer > promptDelay)
            {
                uiController.ShowWakePrompt(true);
            }
        }
        else
        {
            wakePromptTimer = 0f;
            uiController.ShowWakePrompt(false);
        }


        if (zeusFollow && zeusFollow.IsZeusExhausted())
        {
            if (callCount == 0)
            {
                // First call logic
                callPromptTimer += Time.deltaTime;
                if (callPromptTimer > promptDelay)
                {
                    uiController.ShowCallZeusPrompt(true);
                }
            }
            else if (callCount == 1)
            {
                // After the first call, increase the delay for the second prompt
                callPromptTimer += Time.deltaTime;
                if (callPromptTimer > 2 * promptDelay)
                {
                    uiController.ShowCallZeusPrompt(true);
                }
            }
        }
        else
        {
            callPromptTimer = 0f;
            uiController.ShowCallZeusPrompt(false);
        }

        if (SceneManager.GetActiveScene().name == "Main" && hasExitedRoom && !hasRunPromptShown)
        {
            uiController.ShowRunPrompt(true);
            hasRunPromptShown = true;
        }



        if (Input.GetKeyDown(KeyCode.C) && zeusFollow && zeusFollow.IsZeusExhausted())

        {
            isCallingZeus = true;
            callCount++;

            // Reset callPromptTimer when Zeus is called
            callPromptTimer = 0f;
            uiController.ShowCallZeusPrompt(false);
            anim.SetTrigger("isCallingZeus");
            anim.SetInteger("callCount", callCount);

            if (callCount == 2)
            {
                shouldZeusWait = true; // Set the flag when Bea calls Zeus for the second time.
            }

        }

        // Check if any vet staff is close to Bea
        bool shouldSlow = false;
        foreach (var vet in FindObjectsOfType<VetStaffAI>())
        {
            if (vet.ShouldSlowBea())
            {
                shouldSlow = true;
                break;
            }
        }

        if (shouldSlow)
        {
            Debug.Log("Slowing down Bea.");
            SlowDown();

        }
        else
        {
            Debug.Log("Resetting Bea's speed.");
            ResetSpeed();
        }

        if (shouldSlow == true)
        {
            uiController.ShowPushPrompt(true);

        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetSpeed();
            uiController.ShowPushPrompt(false);


        }



    }

    private void FixedUpdate()
    {
        //Debug.Log("isInCutscene: " + isInCutscene);
        if (!canMove) 
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

        if (SceneManager.GetActiveScene().name == "Main")
        {
            float running = Input.GetAxisRaw("Fire3"); // left shift
            anim.SetFloat("speed", Mathf.Abs(running));

        


            if (running > 0 && grounded)
            {
                uiController.ShowRunPrompt(false);

                rb.velocity = new Vector3(move * runSpeed, rb.velocity.y, 0);

            }
            else
            {
                rb.velocity = new Vector3(move * walkSpeed, rb.velocity.y, 0);

            }

            Debug.Log("Current Velocity: " + rb.velocity);


        }
        else if (SceneManager.GetActiveScene().name == "Level2")
        {
            rb.velocity = new Vector3(move * walkSpeed, rb.velocity.y, 0);

        }


        if (move > 0 && !rightDir) Flip();
        else if (move < 0 && rightDir) Flip();

        if (isCallingZeus)
        {
            anim.ResetTrigger("isCallingZeus");
            isCallingZeus = false;
        }



    }
    // Add this method to be called when Bea exits the room
    public void OnExitRoom()
    {
        hasExitedRoom = true;
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

    public void EndOfCallingAnimation()
    {
        FindObjectOfType<ZeusFollow>().StartFollowingAfterCall();
    }

    private void SlowDown()
    {
        runSpeed = originalRunSpeed * 0.2f; // Reduce speed by 50%. Adjust as needed.
        walkSpeed = originalWalkSpeed * 0.2f; // Reduce speed by 50%. Adjust as needed.
    }

    private void ResetSpeed()
    {
        runSpeed = originalRunSpeed;
        walkSpeed = originalWalkSpeed;
    }



}