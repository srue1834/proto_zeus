using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamaraController : MonoBehaviour
{
    public Transform target;
    public float smooth = 5f;
    Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - target.position; // how far away the mc is
    }

    void FixedUpdate()
    {
        Vector3 tarcamPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, tarcamPos, smooth * Time.deltaTime);

    }
}
