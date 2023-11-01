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

    bool grounded = false;

    Collider[] groundColls;
    float groundRad = 0.2f;
    public LayerMask groundLayer;
    public Transform groundCheck;

    public bool isInCutscene = false;

    bool canMove = false;
    float timeSinceSceneLoaded = 0;

    private bool isCallingZeus = false;
    private int callCount = 0;
    public UIController uiController; 

    private bool hasExitedRoom = false; 

    private float wakePromptTimer = 0f;
    private float callPromptTimer = 0f;
    public float promptDelay = 1f; 
    public bool shouldZeusWait = false; 

    private float originalRunSpeed;
    private float originalWalkSpeed;
    private bool hasRunPromptShown = false;

    private float timeSinceZeusStopped = 0f;
    public bool IsCallPromptActive { get; private set; } = false;


    public int GetCallCount()
    {
        return callCount;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rightDir = true;
        anim = GetComponent<Animator>();

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
            canMove = false;  

        } else
        {
            canMove = true;
        }

        timeSinceSceneLoaded = 0; 

        originalRunSpeed = runSpeed;
        originalWalkSpeed = walkSpeed;
    }

    void Update()
    {
        timeSinceSceneLoaded += Time.deltaTime;

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

        // Check if Zeus is exhausted
        if (zeusFollow && zeusFollow.IsZeusExhausted())
        {
            timeSinceZeusStopped += Time.deltaTime;

            if (timeSinceZeusStopped > 2f && BallInteraction.ballThrowCount >= 4)
            {
                if (callCount == 0)
                {
                    callPromptTimer += Time.deltaTime;
                    if (callPromptTimer > promptDelay)
                    {
                        uiController.ShowCallZeusPrompt(true);
                        IsCallPromptActive = true;

                    }
                }
                else if (callCount == 1)
                {
                    callPromptTimer += Time.deltaTime;
                    if (callPromptTimer > promptDelay)
                    {
                        uiController.ShowCallZeusPrompt(true);
                        IsCallPromptActive = true;

                    }
                }
            }
        }
        else
        {
            timeSinceZeusStopped = 0f;
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

            callPromptTimer = 0f;
            uiController.ShowCallZeusPrompt(false);
            anim.SetTrigger("isCallingZeus");
            anim.SetInteger("callCount", callCount);

            if (callCount == 2)
            {
                shouldZeusWait = true; 
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
            SlowDown();

        }
        else
        {
            ResetSpeed();
        }

        if (shouldSlow == true)
            uiController.ShowPushPrompt(true);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetSpeed();
            uiController.ShowPushPrompt(false);

        }

    }

    private void FixedUpdate()
    {
        if (!canMove || isInCutscene)
            return;

        if (isSitting)
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                anim.SetTrigger("sitting");
                isSitting = false;
            }
            return;  
        }

        if (isSitting == false)
        {
            anim.SetTrigger("goIdle");  
        }


        // Check for grounding
        groundColls = Physics.OverlapSphere(groundCheck.position, groundRad, groundLayer);
        grounded = groundColls.Length > 0;

        float move = Input.GetAxis("Horizontal"); // a, d key
        float speed = 0f; 

        if (SceneManager.GetActiveScene().name == "Main")
        {
            float running = Input.GetAxisRaw("Fire3"); 
            if (running > 0 && grounded)
            {
                speed = 2f; 
                uiController.ShowRunPrompt(false);
            }
            else
            {
                speed = 1f; 
            }
        }
        else if (SceneManager.GetActiveScene().name == "Level2")
        {
            speed = 1f; 
        }

        anim.SetFloat("speed", Mathf.Abs(move) * speed);

        rb.velocity = new Vector3(move * (speed == 2f ? runSpeed : walkSpeed), rb.velocity.y, 0);

        if (move > 0 && !rightDir) Flip();
        else if (move < 0 && rightDir) Flip();

        if (isCallingZeus)
        {
            anim.ResetTrigger("isCallingZeus");
            isCallingZeus = false;
        }

        ZeusPickup zeusPickup = FindObjectOfType<ZeusPickup>();
        if (zeusPickup != null)
        {
            bool isCarrying = zeusPickup.IsCarryingZeus();

            anim.SetFloat("IsCarryingZeus", zeusPickup.IsCarryingZeus() ? 1f : 0f);

            if (isCarrying && Mathf.Approximately(move, 0f))
            {
                anim.SetTrigger("goIdleHolding");
            }

        }
    }

    public void OnExitRoom()
    {
        hasExitedRoom = true;
    }

    void Flip()
    {
        rightDir = !rightDir;

        Vector3 currentRotation = transform.eulerAngles;
        currentRotation.y += 180; 
        transform.eulerAngles = currentRotation;
    }

    public void EndOfCallingAnimation()
    {
        FindObjectOfType<ZeusFollow>().StartFollowingAfterCall();
    }

    private void SlowDown()
    {
        runSpeed = originalRunSpeed * 0.2f; 
        walkSpeed = originalWalkSpeed * 0.2f;
    }

    private void ResetSpeed()
    {
        runSpeed = originalRunSpeed;
        walkSpeed = originalWalkSpeed;
    }



}