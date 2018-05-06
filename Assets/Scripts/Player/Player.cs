using UnityEngine.Networking;
using UnityEngine;
using System;
using Assets.Scripts.Player;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Player : NetworkSetup
{
    public const string PLAYER_TAG = "Player";
    private const string REMOTE_LAYER_NAME = "RemotePlayer";
    private const float CLOSE_DISTANCE = 0.5f;
    private const int NUM_TROOPS_FOR_WARNING = 3;
    private const float KLAXON_FIRE_TIME = 5.0f;

    [SyncVar]
    private int id;

    [SyncVar]
    private int laneId;

    [SyncVar]
    private int myTeamId;

    [SyncVar]
    private int opponentsTeamId;

    private bool cooldownAnimReset; // remove this

    private CanvasController canvasController;
    private CrossbowMotor crossbowMotor;
    private CrossbowController crossbowController;
    private AIController aiController;
    private AudioManager audioManager;
    private PlaybackTester playbackTester;

    [SerializeField]
    private Behaviour[] componentsToDisable;

    [SerializeField]
    private GameObject crossbow;

    [SerializeField]
    private GameObject AudioGameObject;

    private float nextActionTime = 0.0f;

    private float nextShotTime = 0.0f;

    private bool playerAIEnabled = false;
    private bool teamAIEnabled = false;
    private bool teamNeuralNet = false;

    private bool sendTroopAlerting = false;

	private string gameplay;
	public string Gameplay
	{
		get
		{
			return gameplay;
		}
	}

    void Awake()
	{
        // Audio manager
        audioManager = AudioGameObject.GetComponent<AudioManager>();
        audioManager.BuildDicts();

        // Canvas Settings
        Canvas canvas = GetComponentInChildren<Canvas>();
        canvas.planeDistance = 1;
        canvasController = canvas.GetComponent<CanvasController>();

        // Crossbow Motor
        crossbowMotor = crossbow.GetComponent<CrossbowMotor>();
        crossbowController = crossbow.GetComponent<CrossbowController>();

        // VCR Recording
        playbackTester = GetComponent<PlaybackTester>();
		playbackTester.StartRecording ();
    }

    public bool GetTeamNeuralNet()
    {
        return teamNeuralNet;
    }

    void Start()
    {
        cooldownAnimReset = true;

        NetworkManagerHUD hud = FindObjectOfType<NetworkManagerHUD>();
        if (hud != null)
        {
            hud.showGUI = false;
        }

        if (isLocalPlayer)
        {
            // Player Initialisation
            CmdInitialisePlayer();

            // Camera Settings
            Cursor.visible = false;

            // Player join sound
            //RpcClientPlaySound("gong");

        }
        else
        {
            DisableNonLocalCompontents();
            AssignLayer(REMOTE_LAYER_NAME);
        }

        if (isServer)
        {
            audioManager.PlaySingleSound(Params.MAIN_MUSIC);
        }

        //RpcResetMainMusic();


    }

    [ClientRpc]
    public void RpcResetMainMusic()
    {
        if (isServer)
        {
            audioManager.PlaySingleSound(Params.MAIN_MUSIC);
        }

    }

    [Command]
    private void CmdInitialisePlayer()
    {
        id = FindObjectsOfType<Player>().Length - 1;
        RegisterModel(PLAYER_TAG, id);

        GameController gameController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>();
        gameController.DeactiveScreenCamera();

        Transform transform = gameController.GetPlayerTransform(id);
        RpcSetPlayerTransform(transform.position, transform.rotation);
        myTeamId = gameController.GetMyTeamControllerId(id);

        tag = PLAYER_TAG + " " + myTeamId;

        RpcSetRenderTexture(myTeamId);

        opponentsTeamId = gameController.GetOpponentsTeamControllerId(id);

        laneId = (myTeamId == TeamController.TEAM1)
            ? (id / 2)
            : (id - 1) / 2;

        SpawnController spawnController = GameObject.FindGameObjectWithTag(SpawnController.SPAWN_CONTROLLER_TAG).GetComponent<SpawnController>();
        for (int i = 0; i <= 2; i++)
        {
            RpcSetCrossbowTargets(spawnController.CalculateDefaultCrossbowTarget(i, laneId, myTeamId));
        }

        RpcHighlightSector(myTeamId, laneId);

        

    }
    [ClientRpc]
    public void RpcResetAimPlayer()
    {
        if (isLocalPlayer)
        {
            crossbowMotor.ResetAim();
            
        }
        
    }

    [ClientRpc]
    private void RpcHighlightSector(int myTeamId, int laneId)
    {
        if (isLocalPlayer)
        {
            canvasController.HighlightSector(myTeamId, laneId);
        }
    }

    [ClientRpc]
    protected void RpcSetPlayerTransform(Vector3 position, Quaternion rotation)
    {
        transform.rotation = rotation;
        transform.position = position;
    }

    [ClientRpc]
    private void RpcSetRenderTexture(int teamId)
    {
        canvasController.SetRenderTexture(teamId);
    }

    [ClientRpc]
    private void RpcSetCrossbowTargets(Vector3 target)
    {
        crossbowMotor.SetDefaultTarget(target);
    }

	[Command]
	private void CmdStartRecording()
	{
		GameController gameController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>();
		gameController.StartRecording ();
	}
		
	[ClientRpc]
	public void RpcStartRecording()
	{
		playbackTester.StartRecording();
	}

	[Command]
	private void CmdStopRecording()
	{
		GameController gameController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>();
		gameController.StopRecording ();
	}
		
	[ClientRpc]
	public void RpcStopRecording()
	{
		gameplay = playbackTester.StopRecording ();
		CmdSaveGameplay ();
	}

	[Command]
	private void CmdSaveGameplay()
	{
		GameController gameController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>();
		gameController.SaveGameplay (gameplay, id);
	}

	[Command]
	private void CmdStartTesting()
	{
		GameController gameController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>();
		gameController.StartTests();
	}

	[ClientRpc]
	public void RpcStartTesting()
	{
		if (isLocalPlayer)
		{
			playbackTester.RunTests (id, myTeamId);
		}
	}

    private void ExecuteControls()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            playerAIEnabled = true;
            CmdTeamAIActivate(true);
        }
        if (Input.GetKeyDown (KeyCode.Y)) {
          CmdRequestOffensiveTroopSpawn (0, 0);
        } else if (Input.GetKeyDown (KeyCode.J)) {
          CmdRequestOffensiveTroopSpawn (0, 1);
        } else if (Input.GetKeyDown (KeyCode.N)) {
          CmdRequestOffensiveTroopSpawn (0, 2);
        } else if (Input.GetKeyDown (KeyCode.B)) {
          CmdRequestOffensiveTroopSpawn (0, 3);
        } else if (Input.GetKeyDown (KeyCode.G)) {
          CmdRequestOffensiveTroopSpawn (0, 4);
        } else if (playbackTester.GetKeyDown (Params.SK_KEY) || playbackTester.GetKeyDown (Params.SK_KEY_ALT)) {
          CmdRequestOffensiveTroopSpawn (0, laneId);
        } else if (playbackTester.GetKeyDown (Params.BR_KEY) || playbackTester.GetKeyDown (Params.BR_KEY_ALT)) {
          CmdRequestOffensiveTroopSpawn (1, laneId);
        } else if (playbackTester.GetKeyDown (Params.VOLLEY_KEY) || playbackTester.GetKeyDown (Params.VOLLEY_KEY_ALT)) {
          CmdVolley ();
        } else if (playbackTester.GetKeyDown (Params.LEFT_KEY)) {
          crossbowMotor.MoveLeft ();
        } else if (playbackTester.GetKeyDown (Params.RIGHT_KEY)) {
          crossbowMotor.MoveRight ();
        } else if (playbackTester.GetKeyDown (Params.SHOOT_KEY)) {
          Shoot ();
        } else if (Input.GetKeyDown (Params.START_RECORDING_KEY)) {
          CmdStartRecording ();
        } else if (Input.GetKeyDown (Params.STOP_RECORDING_KEY)) {
          CmdStopRecording ();
        } else if (Input.GetKeyDown (Params.PLAYBACK_KEY)) {
          string path = String.Format ("exports/player{0}.json", id);
          Debug.Log ("attempt to playback: " + path);
          using (StreamReader r = new StreamReader (path)) {
            string json = r.ReadToEnd ();
            Debug.Log (json);
            Recording recording = Recording.ParseRecording (json);
            playbackTester.StartPlayback (recording);
          }
        } else if (Input.GetKeyDown (Params.TEST_KEY))
        {
          CmdStartTesting ();
        }
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            if (!teamAIEnabled)
            {
                ExecuteControls();

                GameObject[] enemyTroops = FindEnemyTroopsInLane().ToArray();
                int numCloseTroops = 0;

                for (int i = 0; i < enemyTroops.Length; i++)
                {
                    AIController enemyTroopsAi = enemyTroops[i].GetComponent<AIController>();
                    float distanceRatioToTarget = enemyTroopsAi.GetDistanceRatioToTarget();
                    if (distanceRatioToTarget > CLOSE_DISTANCE)
                    {
                        numCloseTroops++;
                    }
                    if ((numCloseTroops >= NUM_TROOPS_FOR_WARNING) && (Time.time > nextActionTime))
                    {
                        nextActionTime = Time.time + KLAXON_FIRE_TIME;
                        RpcClientPlaySingleSound(Params.KLAXON);
                    }
                }
            }
        }

    }

    private void LateUpdate()
    {
        if (isLocalPlayer)
        {
            CmdSetCurrencyText();
            CmdSetHealthBar();
            CmdSetTeamResult();
            CmdSetVolleyCooldown();
            CmdSetTeamAI();
            CmdSetAIPlayerEnabled();
            CmdSendTroopAnim();
            CmdGetNeuralNet();
            CmdSetLeaderboardText();
        }
    }

    [Command]
    public void CmdGetNeuralNet()
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);
        RpcSetTeamNeuralNet(myTeamController.IsNeuralNetActive());
    }

    [ClientRpc]
    private void RpcSetTeamNeuralNet(bool state)
    {
        teamNeuralNet = state;
        if (teamNeuralNet)
        {
            this.GetComponent<AIPlayer>().SetNeuralParams();
        }
    }

    [Command]
    public void CmdSendTroopAnim()
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);

        if (myTeamController.PlaySendTroopAnim == true && sendTroopAlerting == false)
        {
			if (myTeamController.Result == 0) { 
				RpcSetSendTroopAlert ();
				sendTroopAlerting = true;
			}
            //myTeamController.ResetSendTroopAlert(myTeamId);
        }

    }

    [ClientRpc]
    public void RpcSetSandboxAlert()
    {
        Debug.Log("Sandbox");
        canvasController.SetSandboxAlert();
    }

    [ClientRpc]
    public void RpcResetSandboxAlert()
    {
        Debug.Log("Reset Sandbox");
        canvasController.ResetSandboxAlert();
    }

    [ClientRpc]
    public void RpcSetSendTroopAlert()
    {
        Debug.Log("send troops");
        canvasController.SetSendTroopAlert();
    }

    [ClientRpc]
    public void RpcResetSendTroopAlert()
    {
        sendTroopAlerting = false;
        canvasController.ResetSendTroopAlert();      
    }

    [ClientRpc]
    public void RpcSetSpendGoldAnim()
    {
        if(isLocalPlayer)
        {
            canvasController.SetSpendGold();
        }
    }

    [ClientRpc]
    public void RpcResetVolleyAnim()
    {
        if (isLocalPlayer)
        {
            canvasController.ResetVolleyAnim();
        }
    }

    [ClientRpc]
    public void RpcResetAITimerAnim()
    {
        if (isLocalPlayer)
        {
            canvasController.ResetAITimerAlert();
        }
    }


    [Command]
    public void CmdTeamAIActivate(bool active)
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);
        myTeamController.SetTeamAIEnabled(active);
    }

    [Command]
    private void CmdSetTeamAI()
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);
        bool state = myTeamController.TeamAIEnabled;
        
        RpcSetTeamAIEnabled(state);
    }


    [ClientRpc]
    private void RpcSetTeamAIEnabled(bool state)
    {
        teamAIEnabled = state;
    }
    
    [Command]
    private void CmdSetAIPlayerEnabled()
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);
        int aIActiveId = myTeamController.AIActivePlayer;
        int aIActiveId2 = myTeamController.AIActivePlayer2;
        RpcSetAIPlayerEnabled(aIActiveId, aIActiveId2);
    }


    [ClientRpc]
    private void RpcSetAIPlayerEnabled(int aIActiveId, int aIActiveId2)
    {
        if ((aIActiveId == id || aIActiveId2 == id) && teamAIEnabled == true) {
            playerAIEnabled = true;
        }
        else
        {
            playerAIEnabled = false;
        }
    }

    [Command]
    private void CmdSetCurrencyText()
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);
        int coin = myTeamController.Coin;
        RpcSetCurrencyText(coin);
    }

    [Command]
    public void CmdSetLeaderboardText()
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);
        TeamController enemyTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetOpponentsTeamController(id);
        TeamController.TeamResult teamResult = myTeamController.Result;
        float leaderboardTimer = myTeamController.AiTime;
        if(teamResult == TeamController.TeamResult.UNDECIDED)
        {
            RpcSetLeaderboardText(leaderboardTimer, enemyTeamController.TeamAIEnabled);
        }
    }

    [ClientRpc]
    private void RpcSetCurrencyText(int coin)
    {
        canvasController.SetCurrencyText(coin.ToString());
    }

    [ClientRpc]
    private void RpcSetLeaderboardText(float time, bool isOpponentAiEnabled)
    {
        String displayString = String.Format("{0} : {1:00}", (int)time / 60, (int)time % 60);
        canvasController.SetLeaderboardText(displayString, isOpponentAiEnabled, teamAIEnabled);
    }

    [Command]
    private void CmdSetHealthBar()
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);
        TeamController opponentsTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetOpponentsTeamController(id);

        float myHealthRatio = myTeamController.GetTowerHealthRatio();
        float opponentsHealthRatio = opponentsTeamController.GetTowerHealthRatio();
        RpcSetHealthBar(myHealthRatio, opponentsHealthRatio);
    }

    [ClientRpc]
    private void RpcSetHealthBar(float myHealth, float opponentsHealth)
    {
        if (myTeamId == TeamController.TEAM1)
        {
            canvasController.SetBlueHealthBar(myHealth);
            canvasController.SetRedHealthBar(opponentsHealth);
        }
        else
        {
            canvasController.SetRedHealthBar(myHealth);
            canvasController.SetBlueHealthBar(opponentsHealth);
        }
    }

    [Command]
    private void CmdSetTeamResult()
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);
        TeamController.TeamResult teamResult = myTeamController.Result;
        RpcSetTeamResult(teamResult);
    }

    [ClientRpc]
    private void RpcSetTeamResult(TeamController.TeamResult teamResult)
    {
        canvasController.SetGameOverValue(teamResult);
    }

    [Command]
    private void CmdSetVolleyCooldown()
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);
        float endOfCoolDown = myTeamController.EndOfCoolDown;
        float currentTime = myTeamController.CurrentTime;
        RpcSetVolleyCooldown(endOfCoolDown, currentTime);
    }

    [ClientRpc]
    private void RpcSetVolleyCooldown(float endOfCoolDown, float currentTime)
    {
        if (endOfCoolDown > currentTime && cooldownAnimReset)
        {
            canvasController.SetArrowCooldown();
            cooldownAnimReset = false;
        }
        else if (endOfCoolDown <= currentTime)
        {
            cooldownAnimReset = true;
        }
    }

    public List<GameObject> FindEnemyTroopsInLane()
    {
        return GetTroopsInLane(opponentsTeamId, laneId);
    }

    private List<GameObject> GetTroopsInLane(int troopTeamId, int laneId)
    {
        List<GameObject> troopsInLane = new List<GameObject>();
        GameObject[] allTroops = GameObject.FindGameObjectsWithTag(GameController.NPC_TAG);
        for (int i = 0; i < allTroops.Length; i++)
        {
            AIController ai = allTroops[i].GetComponent<AIController>();
            if (ai.TeamId == troopTeamId && ai.LaneId == laneId)
            {
                troopsInLane.Add(allTroops[i]);
            }
        }
        return troopsInLane;
    }


    public void Shoot()
    {
        if (Time.time > nextShotTime)
        {
            crossbowController.HandleShoot();
            nextShotTime = Time.time + Params.MIN_TIME_BETWEEN_SHOTS;
        }
    }

    [Command]
    public void CmdApplyDamage(GameObject troop)
    {
        troop.GetComponent<NPCHealth>().DeductHealth(Params.Bolt.DAMAGE);
    }


    [Command]
    private void CmdVolley()
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);

        if (myTeamController.CurrentTime > myTeamController.EndOfCoolDown)
        {
            bool successfulPurchase = myTeamController.SpendGold(Params.DESTROY_COST);
            if (successfulPurchase)
            {
                RpcClientPlaySingleSound(Params.VOLLEY);
                myTeamController.UpdateCoolDown();
                GameObject[] troops = GetTroopsInLane(opponentsTeamId, laneId).ToArray();
                RpcShootVolley(troops);
            }
        }
    }

    [ClientRpc]
    private void RpcShootVolley(GameObject[] troops)
    {
        StartCoroutine(crossbowController.HandleVolley(troops));
    }

    [ClientRpc]
    public void RpcClientPlaySingleSound(string name)
    {
        if(isLocalPlayer)
        {
            audioManager.PlaySingleSound(name);
        }
    }

    [ClientRpc]
    public void RpcClientPlayArraySound(string name, int index)
    {
        if (isLocalPlayer)
        {
            bool success = audioManager.PlayArraySound(name, index);
            if (!success) { print("audio play array failed"); }
        }
    }


    [Command]
    public void CmdRequestOffensiveTroopSpawn(int troopId, int laneId)
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);
        SpawnController spawnController = GameObject.FindGameObjectWithTag(SpawnController.SPAWN_CONTROLLER_TAG).GetComponent<SpawnController>();
        int cost = Params.NPC_COST[troopId];

        bool successfulPurchase = myTeamController.SpendGold(cost);
        if (successfulPurchase)
        {
            if (troopId == Params.KING_TROOP_ID)
            {
				if (teamAIEnabled == false) 
				{
					if (myTeamController.Result == 0) {
						RpcSetSpendGoldAnim ();
					}
				}
                RpcClientPlaySingleSound(Params.SWORD);
            }
            else if (troopId == Params.RAM_TROOP_ID)
            {
                RpcClientPlaySingleSound(Params.HORN);
            }
			StartCoroutine(spawnController.SpawnOffensiveTroop(troopId, laneId, myTeamId, opponentsTeamId));
//			spawnController.SpawnOffensiveTroop(troopId, laneId, myTeamId, opponentsTeamId)

            //shouldn't need this line, test first tho
            //RpcResetSendTroopAlert();

            myTeamController.ResetSendTroopAlert(myTeamId);
            
        }
        else
        {
            RpcClientPlaySingleSound(Params.COINS);
        }
    }

    [Command]
    public void CmdAddGold(int amount)
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);
        myTeamController.AddGold(amount);
    }
	
    private void DisableNonLocalCompontents()
    {
        foreach (Behaviour behaviour in componentsToDisable)
        {
            behaviour.enabled = false;
        }
    }

    public int GetLaneId()
    {
        return laneId;
    }

    public int GetId()
    {
        return id;
    }

    public CrossbowMotor GetCrossbowMotor()
    {
        return crossbowMotor;
    }

    public bool GetAIEnabled()
    {
        return playerAIEnabled;
    }

    public void DeactivateAI()
    {
        playerAIEnabled = false;
    }

}