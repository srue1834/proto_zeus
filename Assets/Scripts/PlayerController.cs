using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float runSpeed;
    public float walkSpeed;

    Rigidbody rb;
    bool rightDir;

    // jump
    bool grounded = false;

    Collider[] groundColls;
    float groundRad = 0.2f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float jumpForce;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rightDir = true;
        
    }

    private void FixedUpdate()
    {
        if (grounded && Input.GetAxis("Jump") > 0)
        {
            grounded = false;
            rb.AddForce(new Vector3(0, jumpForce, 0)); // apply force in y direction
        }

        // draw sphere 
        groundColls = Physics.OverlapSphere(groundCheck.position, groundRad, groundLayer);
        if (groundColls.Length > 0) grounded = true;
        else grounded = false;


        float move = Input.GetAxis("Horizontal"); // a, d key

        float running = Input.GetAxisRaw("Fire3"); // left shift

        if (running > 0 && grounded)
        {
            rb.velocity = new Vector3(move * runSpeed, rb.velocity.y, 0);

        } else
        {
            rb.velocity = new Vector3(move * walkSpeed, rb.velocity.y, 0);
        } 

        if (move > 0 && !rightDir) Flip();
        else if (move < 0 && rightDir) Flip();

    }

    // flip z value
    void Flip()
    {
        rightDir = !rightDir;
        Vector3 scale = transform.localScale;
        scale.z *= -1;

        transform.localScale = scale; // inverted scale

    }
}
