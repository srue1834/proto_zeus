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

    public static bool ballHasBeenThrown = false;
    public UIController uiController;

    private float ballPromptTimer = 0f;
    public float firstTimePromptDelay = 1f;  // Adjusted delay for the first time
    public float subsequentPromptDelay = 3f;  // Adjusted delay for subsequent times

    private bool hasInteractedWithBall = false;
    private bool isFirstInteraction = true;
    private bool isFirstInteractionThrow = true;

    public static int ballThrowCount = 0;
    private Light ballLight;  // Reference to the ball's light



    void Update()
    {
        // Reference to PlayerController
        PlayerController playerController = GetComponent<PlayerController>();

        // If the call prompt is active, do nothing
        if (playerController && playerController.IsCallPromptActive)
            return;

        if (ball == null)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, pickUpRadius);
            bool ballInRange = false;
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Ball"))
                {
                    ballInRange = true;

                    // Get the ball's light component
                    ballLight = collider.GetComponentInChildren<Light>();
                    if (ballLight)
                    {
                        ballLight.enabled = true;  // Turn on the light when the ball is on the ground
                    }



                    uiController.ShowPickUpBallPrompt(true);
                    

                    if (Input.GetKeyDown(pickUpKey))
                    {
                        ball = collider.gameObject;
                        ballRb = ball.GetComponent<Rigidbody>();
                        ballRb.isKinematic = true;
                        ball.transform.SetParent(null);
                        ball.transform.position = holdPosition.position;
                        ball.transform.SetParent(holdPosition);
                        hasInteractedWithBall = true;
                        isFirstInteraction = false;
                        uiController.ShowPickUpBallPrompt(false);
                        break;
                    }
                }
            } 
            if (!ballInRange)
            {
                uiController.ShowPickUpBallPrompt(false);
            }
        }
        else
        {
            uiController.ShowThrowBallPrompt(true);
            if (ballLight)
            {
                ballLight.enabled = false;  // Turn off the light when the ball is picked up
            }

            if (Input.GetKeyDown(throwKey))
            {
                ballRb.isKinematic = false;
                ball.transform.SetParent(null);
                Vector3 throwDirection = (transform.forward + Vector3.up).normalized;
                ballRb.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);
                ballHasBeenThrown = true;
                ballThrowCount++;  // Increment throw count
                isFirstInteractionThrow = false;

                ball = null;
                uiController.ShowThrowBallPrompt(false);
            }
        }
    }
}
