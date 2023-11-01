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

    public float firstTimePromptDelay = 1f; 
    public float subsequentPromptDelay = 3f; 

    private bool hasInteractedWithBall = false;
    private bool isFirstInteraction = true;
    private bool isFirstInteractionThrow = true;

    public static int ballThrowCount = 0;
    private Light ballLight; 


    private ZeusFollow zeusFollow;

    void Start()
    {
        zeusFollow = FindObjectOfType<ZeusFollow>();
    }


    void Update()
    {
        if (zeusFollow && zeusFollow.HasZeusStopped())
        {
            this.enabled = false;  
            return;
        }


        if (ball == null)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, pickUpRadius);
            bool ballInRange = false;
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Ball"))
                {
                    ballInRange = true;
                    ballLight = collider.GetComponentInChildren<Light>();

                    if (ballLight)
                    {
                        ballLight.enabled = true;  
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
                ballLight.enabled = false;  
            }

            if (Input.GetKeyDown(throwKey))
            {
                ballRb.isKinematic = false;
                ball.transform.SetParent(null);
                Vector3 throwDirection = (transform.forward + Vector3.up).normalized;
                ballRb.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);
                ballHasBeenThrown = true;
                ballThrowCount++;  
                isFirstInteractionThrow = false;

                ball = null;
                uiController.ShowThrowBallPrompt(false);
            }
        }
    }
}
