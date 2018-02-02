using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CrossbowMotor : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    private Vector3 mousePos;

    void Start()
    {
    }

    // Run every physics iteration
    void FixedUpdate()
    {
        PerformRotation();
    }

    // Gets a rotational vector
    public void Rotate(Vector3 mousePosition)
    {
        mousePos = mousePosition;
        mousePos.z = 10;
    }

    //Perform rotation
    void PerformRotation()
    {
        Vector3 worldPos = cam.ScreenToWorldPoint(mousePos);
        transform.LookAt(worldPos);
    }
}
