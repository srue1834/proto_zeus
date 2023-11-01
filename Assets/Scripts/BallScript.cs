using UnityEngine;

[RequireComponent(typeof(SphereCollider))]

public class BallScript : MonoBehaviour
{
    private Rigidbody rb;
    public ZeusFollow zeusFollow;
    private SphereCollider ballCollider;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ballCollider = GetComponent<SphereCollider>();
    }

    public void SetColliderRadius(float radius)
    {
        ballCollider.radius = radius;
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

