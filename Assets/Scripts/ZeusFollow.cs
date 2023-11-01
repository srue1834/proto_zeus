using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class ZeusFollow : MonoBehaviour
{
    public Transform bea;
    public float followDistance = 2.0f;
    public float speed = 5.0f;
    public float rotationSpeed = 5.0f;
    public float lookaheadDistance = 1.0f;

    private Vector3 lastBeaPosition = Vector3.zero;
    private float currentMovementSpeed = 0f;
    private float speedAdjustmentRate = 2.0f;

    private Vector3 targetPosition;
    private Animator zeusAnimator;

    public Transform holdPosition;  

    private NavMeshAgent agent;
    private int fetchCounter = 0;  
    private float tiredSpeedModifier = 0.5f; 

    private float exhaustedMoveTime = 0f;
    private float maxExhaustedMoveTime = 1f;  

    [Header("Sound")]
    public AudioSource zeusAudioSource; 
    public AudioClip pantingClip;       
    public AudioClip snoringClip;      
    
    public enum ZeusState
    {
        Fetching,
        TurningToBea,
        ReturningToBea,
        SlowlyApproachingBea,
        Idle,
        Exhausted,
        StoppedAtBea,
        WaitingForBeaCall
    }

    public bool IsZeusExhausted()
    {
        return fetchCounter >= 3;
    }

    public int GetFetchCount()
    {
        return fetchCounter;
    }

    public ZeusState CurrentState
    {
        get { return currentState; }
    }

    public bool HasZeusStopped()
    {
        return (currentState == ZeusState.StoppedAtBea || currentState == ZeusState.Exhausted);
    }

    private ZeusState currentState = ZeusState.Idle;

    void Start()
    {

        zeusAnimator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (SceneManager.GetActiveScene().name == "Main")
        {
            if (agent != null)
            {
                agent.enabled = false;
            }
        }
        else
        {
            if (zeusAudioSource == null)
            {
                zeusAudioSource = GetComponent<AudioSource>();
            }
        }
        
    }

    void Update()
    {
        PlayerController playerController = bea.GetComponent<PlayerController>();

        if (playerController.shouldZeusWait && (currentState == ZeusState.Idle || currentState == ZeusState.Exhausted))
        {
            currentState = ZeusState.WaitingForBeaCall; 
            playerController.shouldZeusWait = false; 
        }

        switch (currentState)
        {
            case ZeusState.Fetching:
                MoveTowardsBall();
                if (fetchCounter == 2 && !zeusAudioSource.isPlaying)
                {
                    PlaySound(pantingClip);
                }
                break;

            case ZeusState.TurningToBea:
                TurnTowardsBea();
                break;

            case ZeusState.ReturningToBea:
                MoveTowardsBea();
                break;

            case ZeusState.SlowlyApproachingBea:
                SlowlyMoveTowardsBea();
                if (zeusAudioSource.isPlaying)
                {
                    PlaySound(pantingClip);
                }
                break;

            case ZeusState.Idle:
                MoveTowardsBea();
                break;

            case ZeusState.Exhausted:
                MoveInExhaustedState();
                if (zeusAudioSource.isPlaying)
                {
                    PlaySound(snoringClip);
                }
                break;

            case ZeusState.StoppedAtBea:
                break;
        }
    }
    private void PlaySound(AudioClip clip)
    {
        if (zeusAudioSource.isPlaying)
        {
            zeusAudioSource.Stop();
        }
        zeusAudioSource.clip = clip;
        zeusAudioSource.Play();
    }

    public void StartFollowingAfterCall()
    {
        if (currentState == ZeusState.WaitingForBeaCall)
        {
            StartCoroutine(WaitBeforeApproaching());
        }
    }

    void SlowlyMoveTowardsBea()
    {
        if (agent.enabled)
        {
            agent.isStopped = true;

            if (!zeusAudioSource.isPlaying)
            {
                PlaySound(snoringClip);
            }
        }

        float slowSpeed = speed * 0.1f;  
        Vector3 directionToBea = (bea.position - transform.position).normalized;
        transform.position += directionToBea * slowSpeed * Time.deltaTime;

        Quaternion currentLookRotation = Quaternion.LookRotation(directionToBea);
        transform.rotation = Quaternion.Slerp(transform.rotation, currentLookRotation, rotationSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, bea.position) < 1.0f) 
        {
            currentState = ZeusState.StoppedAtBea;
        }
    }

    public bool IsExhaustedAfterFifthFetch()
    {
        return fetchCounter >= 3 && currentState == ZeusState.Exhausted;
    }


    void MoveInExhaustedState()
    {
        if (exhaustedMoveTime >= maxExhaustedMoveTime)
        {
            currentMovementSpeed = 0f;
            if (agent.enabled) 
            {
                agent.isStopped = true;
            }
            zeusAnimator.SetFloat("movementSpeed", currentMovementSpeed);

            return; 
        }

        float currentSpeed = speed * 0.5f;
        Vector3 directionToBall = (targetPosition - transform.position).normalized;
        transform.position += directionToBall * currentSpeed * Time.deltaTime;

        Quaternion currentLookRotation = Quaternion.LookRotation(directionToBall);
        transform.rotation = Quaternion.Slerp(transform.rotation, currentLookRotation, rotationSpeed * Time.deltaTime);

        exhaustedMoveTime += Time.deltaTime;

    }

    void TurnTowardsBea()
    {
        agent.isStopped = true;

        Vector3 directionToBea = (bea.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(directionToBea);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);

        if (Quaternion.Angle(transform.rotation, lookRotation) < 5f)
        {
            agent.isStopped = false; 
            currentState = ZeusState.ReturningToBea;
        }
    }

    private void PickupBall(GameObject ball)
    {
        ball.transform.position = holdPosition.position;
        ball.transform.SetParent(holdPosition);
        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        if (ballRb == null)
        {
            Debug.LogError("The ball does not have a Rigidbody component!");
            return;
        }
        ballRb.isKinematic = true;
        BallInteraction.ballHasBeenThrown = false;
        fetchCounter++;

        currentState = ZeusState.TurningToBea;

        currentMovementSpeed = 0f;
        zeusAnimator.SetFloat("movementSpeed", currentMovementSpeed);

        BallScript ballScript = ball.GetComponent<BallScript>();
        if (ballScript != null)
        {
            ballScript.SetColliderRadius(0.35f);
        }
    }

    public void BallHitGround(Vector3 position)
    {
        if (fetchCounter >= 3)
        {
            currentState = ZeusState.Exhausted;
            zeusAnimator.SetBool("isFalling", true);
            agent.ResetPath(); 
            return;
        }

        targetPosition = position;
        agent.isStopped = false;  

        currentState = ZeusState.Fetching;
    }

    void MoveTowardsBea()
    {
        if (currentState == ZeusState.Exhausted)
        {
            zeusAnimator.SetBool("isTiredIdle", true);
            return;
        }

        float distanceToBea = Vector3.Distance(transform.position, bea.position);
        Vector3 beaMovementDirection = (bea.position - lastBeaPosition).normalized;

        if (distanceToBea > followDistance)
        {
            Vector3 lookaheadPosition = bea.position + beaMovementDirection * lookaheadDistance;
            agent.SetDestination(lookaheadPosition);

            if (distanceToBea > followDistance * 2)
            {
                agent.speed = speed * 2;
            }
            else
            {
                agent.speed = speed;
            }

            if (fetchCounter >= 2)
            {
                agent.speed *= tiredSpeedModifier;
                currentMovementSpeed = 2.0f;  
            }
            else
            {
                currentMovementSpeed = 1.0f; 
            }


            currentMovementSpeed *= agent.velocity.magnitude / agent.speed;  
        }
        else
        {
            agent.speed = 0;
            currentMovementSpeed = Mathf.Lerp(currentMovementSpeed, 0f, speedAdjustmentRate * Time.deltaTime);
        }

        zeusAnimator.SetFloat("movementSpeed", currentMovementSpeed);
        lastBeaPosition = bea.position;
    }

    void MoveTowardsBall()
    {
        float distance = Vector3.Distance(transform.position, targetPosition);
        float pickupThreshold = 0.4f;

        if (distance > pickupThreshold)
        {
            GameObject ballObject = GameObject.FindGameObjectWithTag("Ball");
            BallScript ballScript = ballObject.GetComponent<BallScript>();
            if (ballScript != null)
            {
                ballScript.SetColliderRadius(0.1f);
            }

            float currentSpeed = speed;
            if (fetchCounter >= 2)
            {
                currentSpeed *= tiredSpeedModifier;  
                currentMovementSpeed = 2.0f;  
            }
            else
            {
                currentMovementSpeed = 1.0f;  
            }

            Vector3 directionToBall = (targetPosition - transform.position).normalized;
            float moveDistance = Mathf.Min(currentSpeed * Time.deltaTime, distance - pickupThreshold);  
            transform.position += directionToBall * moveDistance;

            Quaternion lookRotation = Quaternion.LookRotation(directionToBall);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            PickupBall(GameObject.FindGameObjectWithTag("Ball"));
            currentMovementSpeed = 0f;
        }

        zeusAnimator.SetFloat("movementSpeed", currentMovementSpeed);
    }

    IEnumerator WaitBeforeApproaching()
    {
        yield return new WaitForSeconds(4f); 
        currentState = ZeusState.SlowlyApproachingBea;
    }

}