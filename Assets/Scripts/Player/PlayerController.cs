using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float lookSensitivity = 3f;

    public const string PLAYER_TAG = "Player";

    private PlayerMotor motor;

    public int team;

    void Start()
    {
        motor = GetComponent<PlayerMotor>();
        team = getTeam();
    }



    void Update()
    {
        UpdateMovement();
        if (Input.GetKeyDown(KeyCode.A))
        {
            this.GetComponent<Currency>().spend(10);
        }
     }

    private int getTeam()
    {
        int players = GameObject.FindGameObjectsWithTag("Player").Length;
        if(players == 1){
            return 1;
        }
        else
        {
            return 2;
        }
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
