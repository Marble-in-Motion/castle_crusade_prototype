using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CrossbowMotor : MonoBehaviour
{

    private Vector3 rotationX = Vector3.zero;
    private Vector3 rotationY = Vector3.zero;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Run every physics iteration
    void FixedUpdate()
    {
        PerformRotation();
    }

    // Gets a rotational vector
    public void Rotate(Vector3 x, Vector3 y)
    {
        this.rotationX = x;
        this.rotationY = y;
    }

    //Perform rotation
    void PerformRotation()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotationX));
        transform.Rotate(-rotationY);
    }
}
