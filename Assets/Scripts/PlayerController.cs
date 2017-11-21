using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float lookSensitivity = 3f;

    private PlayerMotor motor;

    void Start()
    {
        motor = GetComponent<PlayerMotor>();
    }

    void Update()
    {
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        //Calculate rotation as a 3D vector (turning around)
        float yRot = Input.GetAxisRaw("Mouse X");
        Vector3 rotation = new Vector3(0f, yRot, 0f) * lookSensitivity;

        float xRot = Input.GetAxisRaw("Mouse Y");
        Vector3 cameraRotation = new Vector3(xRot, 0f, 0f) * lookSensitivity;

        motor.Rotate(rotation, cameraRotation);
    }
}
