using UnityEngine;
using UnityEngine.AI;

public class ZeusFollow : MonoBehaviour
{
    public Transform bea;
    public float followDistance = 2.0f;
    public float speed = 5.0f;
    public float rotationSpeed = 5.0f;
    public float lookaheadDistance = 1.0f;

    private Vector3 velocity = Vector3.zero;
    private Vector3 lastBeaPosition = Vector3.zero;
    private float currentMovementSpeed = 0f;
    private float speedAdjustmentRate = 2.0f;

    private Vector3 targetPosition;
    private bool fetchingBall = false;
    private Animator zeusAnimator;

    public Transform holdPosition;  // Place where Zeus holds the ball

    private NavMeshAgent agent;
    private int fetchCounter = 0;  // New variable to track number of fetches
    private float tiredSpeedModifier = 0.5f;  // Speed reduction when tired

    private enum ZeusState
    {
        Fetching,
        TurningToBea,
        ReturningToBea,
        Idle
    }

    private ZeusState currentState = ZeusState.Idle;

    void Start()
    {
        zeusAnimator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        switch (currentState)
        {
            case ZeusState.Fetching:
                MoveTowardsBall();
                break;
            case ZeusState.TurningToBea:
                TurnTowardsBea();
                break;
            case ZeusState.ReturningToBea:
            case ZeusState.Idle:
                MoveTowardsBea();
                break;
        }
    }


    void TurnTowardsBea()
    {
        agent.isStopped = true; // Stop the agent from moving while turning

        Vector3 directionToBea = (bea.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(directionToBea);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);

        if (Quaternion.Angle(transform.rotation, lookRotation) < 5f) // Close enough to the desired rotation
        {
            agent.isStopped = false; // Allow the agent to move again
            currentState = ZeusState.ReturningToBea;
        }
    }

    private void PickupBall(GameObject ball)
    {
        Debug.Log("Attempting to pick up ball: " + ball.name);
        fetchingBall = false;
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

        currentState = ZeusState.TurningToBea;  // After picking up the ball, Zeus should turn to Bea

        currentMovementSpeed = 0f;
        zeusAnimator.SetFloat("movementSpeed", currentMovementSpeed);
        Debug.Log("Zeus picked up the ball!");

    }

    public void BallHitGround(Vector3 position)
    {
        targetPosition = position;
        fetchingBall = true;
        agent.isStopped = false;  // Ensure the NavMeshAgent is active
        Debug.Log("BallHitGround called. Target position: " + targetPosition);

        currentState = ZeusState.Fetching;
    }

    void MoveTowardsBea()
    {
        float distanceToBea = Vector3.Distance(transform.position, bea.position);
        Vector3 beaMovementDirection = (bea.position - lastBeaPosition).normalized;

        if (distanceToBea > followDistance)
        {
            Vector3 lookaheadPosition = bea.position + beaMovementDirection * lookaheadDistance;
            agent.SetDestination(lookaheadPosition);

            // Adjust speed based on distance
            if (distanceToBea > followDistance * 2)
            {
                agent.speed = speed * 2; // Double the speed to catch up
            }
            else
            {
                agent.speed = speed;
            }

            // Apply the tiredSpeedModifier if Zeus has fetched the ball more than twice
            if (fetchCounter >= 2)
            {
                agent.speed *= tiredSpeedModifier;
                currentMovementSpeed = 2.0f;  // Tired walk speed for blend tree
            }
            else
            {
                currentMovementSpeed = 1.0f;  // Regular walk speed for blend tree
            }

            currentMovementSpeed *= agent.velocity.magnitude / agent.speed;  // This scales the speed value based on Zeus's actual speed
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
        Debug.Log("Distance to ball: " + distance);


        // Use a smaller threshold to determine when Zeus is close enough to pick up the ball
        float pickupThreshold = 0.5f;

        if (distance > pickupThreshold)
        {
            // Adjust Zeus's speed if he is tired (i.e., after fetching twice)
            float currentSpeed = speed;
            if (fetchCounter >= 2)
            {
                currentSpeed *= tiredSpeedModifier;  // Reduce speed when tired
                currentMovementSpeed = 2.0f;  // Tired walk speed for blend tree
            }
            else
            {
                currentMovementSpeed = 1.0f;  // Regular walk speed for blend tree
            }

            // Linear movement towards the ball
            Vector3 directionToBall = (targetPosition - transform.position).normalized;
            float moveDistance = Mathf.Min(currentSpeed * Time.deltaTime, distance - pickupThreshold);  // Move by the smaller of the two values
            transform.position += directionToBall * moveDistance;

            // Rotate Zeus to face the ball
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



}
