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

    private float exhaustedMoveTime = 0f;
    private float maxExhaustedMoveTime = 1f;  // Zeus will move for 1.5 seconds before stopping
    private Vector3 lastWalkingDirection = Vector3.forward;  // default to forward

    private bool hasTurnedAfterExhausted = false;  // Add this at the class level


    private enum ZeusState
    {
        Fetching,
        TurningToBea,
        ReturningToBea,
        Idle,
        Exhausted
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
            case ZeusState.Exhausted:
                MoveInExhaustedState();
                break;
        }
    }

    void MoveInExhaustedState()
    {
        if (exhaustedMoveTime >= maxExhaustedMoveTime)
        {
            // Zeus has moved long enough, stop him
            currentMovementSpeed = 0f;
            if (agent.enabled) // Check if the agent is enabled before stopping it
            {
                agent.isStopped = true;
            }
            zeusAnimator.SetFloat("movementSpeed", currentMovementSpeed);
            return; // Exit the function
        }

        // Otherwise, keep moving towards the ball (at half speed)
        float currentSpeed = speed * 0.5f;
        Vector3 directionToBall = (targetPosition - transform.position).normalized;
        transform.position += directionToBall * currentSpeed * Time.deltaTime;

        // Rotate Zeus to face the ball
        Quaternion currentLookRotation = Quaternion.LookRotation(directionToBall);
        transform.rotation = Quaternion.Slerp(transform.rotation, currentLookRotation, rotationSpeed * Time.deltaTime);

        // Update the exhaustedMoveTime
        exhaustedMoveTime += Time.deltaTime;
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

    }

    public void BallHitGround(Vector3 position)
    {
        Debug.Log("Fetch counter: " + fetchCounter);
        if (fetchCounter >= 5)
        {
            currentState = ZeusState.Exhausted;
            agent.ResetPath(); // Clear the agent's destination
            return;
        }

        targetPosition = position;
        fetchingBall = true;
        agent.isStopped = false;  // Ensure the NavMeshAgent is active

        currentState = ZeusState.Fetching;
    }

    void MoveTowardsBea()
    {
        if (currentState == ZeusState.Exhausted)
            return;

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