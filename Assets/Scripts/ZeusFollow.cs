using UnityEngine;

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

    public float fetchSpeedMultiplier = 1.0f;
    public float incrementDistance = 1.0f; // The amount to increase the distance Zeus starts from.
    public bool hasFetchedOnce = false;

    public Transform ballHoldPosition;  // The position on Zeus where the ball will be held (e.g., his mouth).
    private GameObject fetchedBall;
    private Rigidbody fetchedBallRb;
    public float pickUpRadius = 1.0f;

    public bool IsFetchingBall
    {
        get { return fetchingBall; }
    }



    void Start()
    {
        zeusAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        if (fetchingBall)
        {
            MoveTowardsBall();
            return; // Make sure that if Zeus is fetching the ball, he doesn't execute the MoveTowardsBea logic
        }
        else
        {
            MoveTowardsBea();
        }
    }


    public void BallHitGround(Vector3 position)
    {
        if (!hasFetchedOnce)
        {
            targetPosition = position;
            fetchingBall = true;
            hasFetchedOnce = true; // Set this flag once the ball hits the ground for the first time.
        }
        else
        {
            incrementDistance += 1.0f; // Increase the distance by 1 unit each time. Adjust this value if needed.
            targetPosition = position - (position - transform.position).normalized * incrementDistance;
            fetchingBall = true;
        }
    }

    void MoveTowardsBea()
    {
        float distanceToBea = Vector3.Distance(transform.position, bea.position);
        Vector3 beaMovementDirection = (bea.position - lastBeaPosition).normalized;

        if (distanceToBea > followDistance)
        {
            Vector3 lookaheadPosition = bea.position + beaMovementDirection * lookaheadDistance;
            Vector3 directionToLookaheadPosition = (lookaheadPosition - transform.position).normalized;

            transform.position = Vector3.SmoothDamp(transform.position, transform.position + directionToLookaheadPosition, ref velocity, 0.1f, speed);

            directionToLookaheadPosition.y = 0;
            Quaternion desiredRotation = Quaternion.LookRotation(directionToLookaheadPosition);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);

            currentMovementSpeed = Mathf.Lerp(currentMovementSpeed, 1.0f, speedAdjustmentRate * Time.deltaTime);
        }
        else
        {
            currentMovementSpeed = Mathf.Lerp(currentMovementSpeed, 0f, speedAdjustmentRate * Time.deltaTime);
        }

        zeusAnimator.SetFloat("movementSpeed", currentMovementSpeed);
        lastBeaPosition = bea.position;
    }

    void MoveTowardsBall()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;

        // Check if the ball is within the pickUpRadius of Zeus
        Collider[] colliders = Physics.OverlapSphere(transform.position, pickUpRadius);
        bool isCloseToBall = false;
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Ball"))
            {
                isCloseToBall = true;
                break;
            }
        }

        if (!isCloseToBall)
        {
            Vector3 directionToBall = (targetPosition - transform.position).normalized;
            directionToBall.y = 0; // Ensure Zeus doesn't tilt upwards or downwards
            Quaternion desiredRotationToBall = Quaternion.LookRotation(directionToBall);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotationToBall, rotationSpeed * Time.deltaTime);

            transform.position += direction * speed * Time.deltaTime;
            currentMovementSpeed = 1.0f;

        }
        else
        {
            fetchingBall = false;
            currentMovementSpeed = 0f;

            if (!hasFetchedOnce)
            {
                hasFetchedOnce = true;

                // Grab the ball
                fetchedBall = GameObject.FindGameObjectWithTag("Ball");  // Assuming the ball has the tag "Ball".
                if (fetchedBall)
                {
                    fetchedBallRb = fetchedBall.GetComponent<Rigidbody>();
                    fetchedBallRb.isKinematic = true;
                    fetchedBall.transform.SetParent(ballHoldPosition);
                    fetchedBall.transform.position = ballHoldPosition.position;
                }
            }
        }

        zeusAnimator.SetFloat("movementSpeed", currentMovementSpeed);
    }

   
}
