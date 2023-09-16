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


    void Start()
    {
        zeusAnimator = GetComponent<Animator>();
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

    public void BallHitGround(Vector3 position)
    {
        targetPosition = position;
        fetchingBall = true;
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
        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance > 1.0f)
        {
            transform.position += direction * speed * Time.deltaTime;
            currentMovementSpeed = 1.0f;
        }
        else
        {
            fetchingBall = false;
            currentMovementSpeed = 0f;
        }

        zeusAnimator.SetFloat("movementSpeed", currentMovementSpeed);
    }
}
