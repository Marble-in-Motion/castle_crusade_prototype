using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Player : NetworkSetup
{

    public const string PLAYER_TAG = "Player";

    private int id;

    private int teamId;

    private PlayerMotor motor;

    [SerializeField]
    private float lookSensitivity = 3f;

    [SerializeField]
    Behaviour[] componentsToDisable;

    [SerializeField]
    string remoteLayerName = "RemotePlayer";

    public Text CurrencyText;


    public int GetId()
    {
        return id;
    }

    public int GetTeamId()
    {
        return teamId;
    }
    
    public void SetTeamId(int id)
    {
        teamId = id;
    }


    void Start()
    {
        motor = GetComponent<PlayerMotor>();
        id = FindObjectsOfType<Player>().Length - 1;

        GameController gameController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>();
        gameController.InitialisePlayer(this);


        //Canvas canvas = this.GetComponentInChildren<Canvas>();
        //canvas.planeDistance = 1;
        //canvas.renderMode = RenderMode.ScreenSpaceCamera;
        //canvas.worldCamera = this.GetComponentInChildren<Camera>();
        //CurrencyText = this.GetComponentInChildren<Text>();
        //CurrencyText.text = "Coin: " + 100.ToString();

        if (isLocalPlayer)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            DisableNonLocalCompontents();
            AssignLayer(remoteLayerName);
        }

        RegisterModel(Player.PLAYER_TAG);
    }

    void Update()
    {
        UpdateMovement();

        // spawn npc command
  //      if (Input.GetKeyDown(KeyCode.A))
  //      {
  //          int currency = teamController.buy(10);
  //          this.GetComponentInChildren<Text>().text = "Coin: " + currency.ToString();
  //      }
		//else if (Input.GetKeyDown(KeyCode.S) && isLocalPlayer) {
		//	CmdRequestTroopSpawn();
		//}

    }



    private void DisableNonLocalCompontents()
    {
        foreach (Behaviour behaviour in componentsToDisable)
        {
            behaviour.enabled = false;
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

	//[Command]
	//public void CmdRequestTroopSpawn() {
 //       //create troop controller
 //       teamController.SendTroop(this.gameObject);
	//}

    

}


