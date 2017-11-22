using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float lookSensitivity = 3f;

    public const string PLAYER_TAG = "Player";

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
        float yRot = Input.GetAxisRaw("Mouse X");
        Vector3 x = new Vector3(0f, yRot, 0f) * lookSensitivity;
        

        float xRot = Input.GetAxisRaw("Mouse Y");
        Vector3 y = new Vector3(xRot, 0f, 0f) * lookSensitivity;

        motor.Rotate(x, y);
    }
}
