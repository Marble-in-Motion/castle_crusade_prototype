using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Player : NetworkSetup
{

    public const string PLAYER_TAG = "Player";
    private const string REMOTE_LAYER_NAME = "RemotePlayer";

    public int id;

    private PlayerMotor motor;

    private TeamController teamController;

    private SpawnController spawnController;

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
        id = FindObjectsOfType<Player>().Length - 1;
        RegisterModel(Player.PLAYER_TAG, GetId());
        
        spawnController = GameObject.FindGameObjectWithTag(SpawnController.SPAWN_CONTROLLER_TAG).GetComponent<SpawnController>();
        GameController gameController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>();
        //gameController.InitialisePlayer(this);
        


        if (isLocalPlayer)
        {
            // Player Initialisation
            //GameController gameController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>();
            gameController.InitialisePlayer(this);
            teamController = gameController.GetTeamController(GetId());
            motor = GetComponent<PlayerMotor>();
            
            // Camera Settings
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // Canvas Settings
            Canvas canvas = this.GetComponentInChildren<Canvas>();
            canvas.planeDistance = 1;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = this.GetComponentInChildren<Camera>();
            CurrencyText = this.GetComponentInChildren<Text>();
            CurrencyText.text = "Coin: " + teamController.GetCoin().ToString();
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
                CmdSpendGold(10);
                Debug.Log("End");
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                Debug.Log("Q_1: " + id);
                CmdRequestOffensiveTroopSpawn(0, 0);
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                Debug.Log("W_2: " + id);
                CmdRequestOffensiveTroopSpawn(0, 1);
            }

            this.GetComponentInChildren<Text>().text = "Coin: " + teamController.GetCoin().ToString();
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

    [Command]
    private void CmdRequestOffensiveTroopSpawn(int troopId, int spawnId)
    {
        int teamId = GetTeamId();
        Debug.Log("Team: " + teamId.ToString());
        spawnController.SpawnOffensive(troopId, spawnId, teamId);
    }

    [Command]
    public void CmdSpendGold(int amount)
    {
        Debug.Log("Attempt Spend");
        Debug.Log(id);
        int teamid = id % 2;
        TeamController temp;
        if (teamid == 0)
        {
            temp = GameObject.FindGameObjectWithTag("TeamController1").GetComponent<TeamController>();
        }
        else
        {
            temp = GameObject.FindGameObjectWithTag("TeamController2").GetComponent<TeamController>();
        }
         
        temp.CmdSpendGold(amount);
    }



}


