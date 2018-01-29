using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Player : NetworkSetup
{

    public const string PLAYER_TAG = "Player";
	public const string BAR_TAG = "Bar";
	public const string COINS_TAG = "Coins";
	private const string REMOTE_LAYER_NAME = "RemotePlayer";

    private int id;

    private PlayerMotor motor;

    private TeamController teamController;

    private SpawnController spawnController;

    [SerializeField]
    private float lookSensitivity = 3f;

    [SerializeField]
    private Behaviour[] componentsToDisable;

    private Text CurrencyText;

	private float maxTowerHealth = 100f;
	private Image healthBar;

    public int GetId()
    {
        return id;
    }

    void Start()
    {
        id = FindObjectsOfType<Player>().Length - 1;
        RegisterModel(Player.PLAYER_TAG, GetId());

        spawnController = GameObject.FindGameObjectWithTag(SpawnController.SPAWN_CONTROLLER_TAG).GetComponent<SpawnController>();
        GameController gameController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>();
        teamController = gameController.GetTeamController(id);

        if (isLocalPlayer)
        {
            // Player Initialisation
            gameController.InitialisePlayer(this);
            motor = GetComponent<PlayerMotor>();

            // Camera Settings
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // Canvas Settings
            Canvas canvas = this.GetComponentInChildren<Canvas>();
            canvas.planeDistance = 1;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = this.GetComponentInChildren<Camera>();
            
			//CurrencyText = this.GetComponentInChildren<Text>();
            //CurrencyText.text = "Coin: " + teamController.GetCoin().ToString();

			Text[] texts = this.GetComponentsInChildren<Text>();
			for(int i = 0; i < texts.Length; i++){
				if (texts [i].tag == COINS_TAG) {
					CurrencyText = texts [i];
				}
			}

			Image[] images = this.GetComponentsInChildren<Image>();
			for(int i = 0; i < images.Length; i++){
				if (images [i].tag == BAR_TAG) {
					healthBar = images [i];
				}
			}
				
        }

        if (!isLocalPlayer)
        {
            DisableNonLocalCompontents();
            AssignLayer(REMOTE_LAYER_NAME);
        }

    }

    void Update()
    {
        if (isLocalPlayer)
        {
            UpdateMovement();

            // spawn npc command
            if (Input.GetKeyDown(KeyCode.Y))
            {
                CmdRequestOffensiveTroopSpawn(0, 0);
            }
            else if (Input.GetKeyDown(KeyCode.J))
            {
                CmdRequestOffensiveTroopSpawn(0, 1);
            }
			else if (Input.GetKeyDown(KeyCode.N))
			{
				CmdRequestOffensiveTroopSpawn(0, 2);
			}
			else if (Input.GetKeyDown(KeyCode.B))
			{
				CmdRequestOffensiveTroopSpawn(0, 3);
			}
			else if (Input.GetKeyDown(KeyCode.G))
			{
				CmdRequestOffensiveTroopSpawn(0, 4);
			}

            this.GetComponentInChildren<Text>().text = "Coin: " + teamController.GetCoin().ToString();

			float calc_Health = teamController.towerHealth / maxTowerHealth;
			SetHealthBar (calc_Health);

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
        int teamId = teamController.GetId();
        CmdSpendGold(5);
        spawnController.SpawnOffensive(troopId, spawnId, teamId);
    }

    [Command]
    public void CmdSpendGold(int amount)
    {
        teamController.SpendGold(amount);
    }

    [Command]
    public void CmdAddGold(int amount)
    {
        teamController.AddGold(amount);
    }

	public void SetHealthBar(float calcHealth){
		healthBar.transform.localScale = new Vector3(Mathf.Clamp(calcHealth,0f ,1f), healthBar.transform.localScale.y, healthBar.transform.localScale.z);
	}
}