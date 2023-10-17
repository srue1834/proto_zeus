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
        if (collision.gameObject.CompareTag("Ground") && BallInteraction.ballHasBeenThrown)
        {
            rb.velocity = Vector3.zero;
            zeusFollow.BallHitGround(transform.position);
        }
    }
}