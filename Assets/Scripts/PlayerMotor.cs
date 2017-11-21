using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{

    [SerializeField]
    private Camera cam;

    private Vector3 rotation = Vector3.zero;
    private Vector3 cameraRotation = Vector3.zero;

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
    public void Rotate(Vector3 rotation, Vector3 cameraRotation)
    {
        this.rotation = rotation;
        this.cameraRotation = cameraRotation;
    }

    //Perform rotation
    void PerformRotation()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
        if (cam != null)
        {
            cam.transform.Rotate(-cameraRotation);
        }
    }
}
