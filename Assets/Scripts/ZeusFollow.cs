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

    void Start()
    {
        zeusAnimator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (fetchingBall)
        {
            MoveTowardsBall();
        }
        else
        {
            MoveTowardsBea();
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
    }


    public void BallHitGround(Vector3 position)
    {
        //transform.position = position;

        targetPosition = position;
        fetchingBall = true;
        agent.isStopped = false;  // Ensure the NavMeshAgent is active
        Debug.Log("BallHitGround called. Target position: " + targetPosition);

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

            currentMovementSpeed = Mathf.Lerp(currentMovementSpeed, 1.0f, speedAdjustmentRate * Time.deltaTime);
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
        float pickupThreshold = 0.2f;

        if (distance > pickupThreshold)
        {
            // Linear movement towards the ball
            Vector3 directionToBall = (targetPosition - transform.position).normalized;
            float moveDistance = Mathf.Min(speed * Time.deltaTime, distance - pickupThreshold);  // Move by the smaller of the two values
            transform.position += directionToBall * moveDistance;

            // Rotate Zeus to face the ball
            Quaternion lookRotation = Quaternion.LookRotation(directionToBall);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);

            currentMovementSpeed = 1.0f;
        }
        else
        {
            PickupBall(GameObject.FindGameObjectWithTag("Ball"));
            currentMovementSpeed = 0f;
        }

        zeusAnimator.SetFloat("movementSpeed", currentMovementSpeed);
    }



}
