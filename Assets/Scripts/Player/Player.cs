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

    private TeamController teamController;

    private SpawnController spawnController;

    [SerializeField]
    private Behaviour[] componentsToDisable;

    private Text CurrencyText;

	private float maxTowerHealth = 100f;
	private Image healthBar;

	private int gameOverValue = 0;

	Animator anim;

    public int GetId()
    {
        return id;
    }

    void Start()
    {
		gameOverValue = 0;
        id = FindObjectsOfType<Player>().Length - 1;
        RegisterModel(Player.PLAYER_TAG, GetId());

        spawnController = GameObject.FindGameObjectWithTag(SpawnController.SPAWN_CONTROLLER_TAG).GetComponent<SpawnController>();
        GameController gameController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>();
        teamController = gameController.GetTeamController(id);

        if (isLocalPlayer)
        {
            // Player Initialisation
            gameController.InitialisePlayer(this);
			anim = GetComponent<Animator> ();

            // Camera Settings
            //Cursor.visible = false;
            //Cursor.lockState = CursorLockMode.Locked;

            // Canvas Settings
            Canvas canvas = this.GetComponentInChildren<Canvas>();
            canvas.planeDistance = 1;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = this.GetComponentInChildren<Camera>();
            
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

            CurrencyText.text = "Coin: " + teamController.GetCoin().ToString();

			float calc_Health = teamController.GetTowerHealth() / maxTowerHealth;
			SetHealthBar (calc_Health);

			gameOverValue = teamController.GetIsGameOver ();
			if (gameOverValue == 1) {
                Debug.Log("lose");
                anim.SetTrigger ("GameOver");
			}
            else if (gameOverValue == 2)
            {
                Debug.Log("win");
                anim.SetTrigger("GameWin");
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


    [Command]
    private void CmdRequestOffensiveTroopSpawn(int troopId, int spawnId)
    {
        int teamId = teamController.GetId();
        bool successfulPurchase = teamController.SpendGold(5);
        if (successfulPurchase) {
            spawnController.SpawnOffensive(troopId, spawnId, teamId);
        }
        
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