using UnityEngine;

public class BallInteraction : MonoBehaviour
{
    public Transform holdPosition;
    public float pickUpRadius = 2f;
    public float throwForce = 10f;
    public KeyCode pickUpKey = KeyCode.E;
    public KeyCode throwKey = KeyCode.Q;

    private GameObject ball;
    private Rigidbody ballRb;

    // Static variable to track if ball has been thrown
    public static bool ballHasBeenThrown = false;

    void Update()
    {
        if (ball == null)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, pickUpRadius);
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Ball") && Input.GetKeyDown(pickUpKey))
                {
                    ball = collider.gameObject;
                    ballRb = ball.GetComponent<Rigidbody>();
                    ballRb.isKinematic = true;

                    // Unparent the ball before setting position
                    ball.transform.SetParent(null);
                    ball.transform.position = holdPosition.position;

                    // Reparent the ball after setting position
                    ball.transform.SetParent(holdPosition);
                    break;
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(throwKey))
            {
                ballRb.isKinematic = false;

                // Unparent the ball before throwing
                ball.transform.SetParent(null);

                Vector3 throwDirection = (transform.forward + Vector3.up).normalized;
                ballRb.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);
                ballHasBeenThrown = true;
                ball = null;
            }
        }
    }

}