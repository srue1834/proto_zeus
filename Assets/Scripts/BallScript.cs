using UnityEngine;

public class BallScript : MonoBehaviour
{
    private Rigidbody rb;
    public ZeusFollow zeusFollow;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (!zeusFollow.IsFetchingBall && collision.gameObject.CompareTag("Ground") && BallInteraction.ballHasBeenThrown)  // Check if Zeus isn't fetching the ball.
        {
            rb.velocity = Vector3.zero;
            zeusFollow.BallHitGround(transform.position);
        }
    }
}
