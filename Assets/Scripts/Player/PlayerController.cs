using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float lookSensitivity = 3f;

    public const string PLAYER_TAG = "Player";

    private PlayerMotor motor;

    private TeamController team;

    void Start()
    {
        motor = GetComponent<PlayerMotor>();
        int teamNum = getTeam();
        if(teamNum == 1)
        {
            team = GameObject.FindGameObjectWithTag("Tower1").GetComponent<TeamController>();
        }
        else
        {
            team = GameObject.FindGameObjectWithTag("Tower2").GetComponent<TeamController>();
        }
        team.players.Add(this.gameObject);
    }



    void Update()
    {
        UpdateMovement();
        if (Input.GetKeyDown(KeyCode.A))
        {
            int currency = team.buy(10);
            this.GetComponentInChildren<Text>().text = "Coin: " + currency.ToString();
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
