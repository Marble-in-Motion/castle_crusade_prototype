using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using System;
using Assets.Scripts.Team;

public class Player : NetworkSetup
{

    public const string PLAYER_TAG = "Player";
    private const string REMOTE_LAYER_NAME = "RemotePlayer";

    private int id;

    private PlayerMotor motor;

    private ITeamController teamController;

    [SerializeField]
    private float lookSensitivity = 3f;

    [SerializeField]
    Behaviour[] componentsToDisable;

    public Text CurrencyText;


    public int GetId()
    {
        return id;
    }

    public int GetTeamId()
    {
        return (teamController != null) ? teamController.GetId() : -1;
    }
    
    void Start()
    {
        if (isLocalPlayer)
        {
            motor = GetComponent<PlayerMotor>();
            id = FindObjectsOfType<Player>().Length - 1;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            RegisterModel(Player.PLAYER_TAG, id);

            GameController gameController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>();
            gameController.InitialisePlayer(this);
            teamController = gameController.GetTeamController(id);


            Canvas canvas = this.GetComponentInChildren<Canvas>();
            canvas.planeDistance = 1;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = this.GetComponentInChildren<Camera>();
            CurrencyText = this.GetComponentInChildren<Text>();
            CurrencyText.text = "Coin: " + 100.ToString();
        }
        

        if (!isLocalPlayer)
        {
            DisableNonLocalCompontents();
            AssignLayer(REMOTE_LAYER_NAME);
            //Destroy(this);
        }

    }

    void Update()
    {
        if (isLocalPlayer)
        {
            UpdateMovement();

            // spawn npc command
            if (Input.GetKeyDown(KeyCode.A))
            {
                Debug.Log("A: " + id);
                int currency = teamController.SpendGold(10);
                this.GetComponentInChildren<Text>().text = "Coin: " + currency.ToString();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                Debug.Log("S: " + id);
                //CmdRequestTroopSpawn();
            }
        }
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


