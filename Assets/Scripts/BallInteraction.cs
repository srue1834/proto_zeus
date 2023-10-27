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
    public float firstTimePromptDelay = 2f;  // Delay for the first time
    public float subsequentPromptDelay = 10f;  // Delay for subsequent times
    private bool hasInteractedWithBall = false;
    private bool isFirstInteraction = true;

    void Update()
    {
        if (ball == null)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, pickUpRadius);
            bool ballInRange = false;
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Ball"))
                {
                    ballInRange = true;
                    ballPromptTimer += Time.deltaTime;

                    if ((isFirstInteraction && ballPromptTimer > firstTimePromptDelay) ||
                        (!isFirstInteraction && ballPromptTimer > subsequentPromptDelay))
                    {
                        uiController.ShowPickUpBallPrompt(true);
                    }

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
                        ballPromptTimer = 0f;
                        uiController.ShowPickUpBallPrompt(false);
                        break;
                    }
                }
            }
            if (!ballInRange)
            {
                ballPromptTimer = 0f;
                uiController.ShowPickUpBallPrompt(false);
            }
        }
        else
        {
            ballPromptTimer += Time.deltaTime;

            if (ballPromptTimer > subsequentPromptDelay)
            {
                uiController.ShowThrowBallPrompt(true);
            }

            if (Input.GetKeyDown(throwKey))
            {
                ballRb.isKinematic = false;
                ball.transform.SetParent(null);
                Vector3 throwDirection = (transform.forward + Vector3.up).normalized;
                ballRb.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);
                ballHasBeenThrown = true;
                ball = null;
                ballPromptTimer = 0f;
                uiController.ShowThrowBallPrompt(false);
            }
        }
    }
}
