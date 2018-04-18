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
	private const float SCREENSHOT_DELAY = 2.0f;

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
    private InputVCR vcr;

    [SerializeField]
    private Behaviour[] componentsToDisable;

    [SerializeField]
    private GameObject crossbow;

    private GameObject AudioGameObject;

    private float nextActionTime = 0.0f;

    private bool playerAIEnabled = false;
    private bool teamAIEnabled = false;
    private bool screenShotEnabled = false;
	private float nextScreenshotTime;
	private int currentDangerValue = 0;


    void Awake()
    {


        // Canvas Settings
        Canvas canvas = GetComponentInChildren<Canvas>();
        canvas.planeDistance = 1;
        canvasController = canvas.GetComponent<CanvasController>();

        // Crossbow Motor
        crossbowMotor = crossbow.GetComponent<CrossbowMotor>();
        crossbowController = crossbow.GetComponent<CrossbowController>();

        // VCR Recording
        vcr = GetComponent<InputVCR>();
    }

    void Start()
    {
        cooldownAnimReset = true;
		nextScreenshotTime = Time.time + SCREENSHOT_DELAY;

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

            // Audio manager
            audioManager = GetComponentInChildren<AudioManager>();
            audioManager.BuildDict();

        }
        else
        {
            DisableNonLocalCompontents();
            AssignLayer(REMOTE_LAYER_NAME);
        }

        if (isServer)
        {
            //audioManager.PlaySound("ambience");
            ClientPlaySound("ambience");
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

    private void ExecuteControls()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            playerAIEnabled = true;
            CmdTeamAIActivate(true);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CmdToggleScreenShot();
        }
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
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            audioManager.PlaySound("sword");
            CmdRequestOffensiveTroopSpawn(0, laneId);
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            audioManager.PlaySound("sword");
            CmdRequestOffensiveTroopSpawn(1, laneId);
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            audioManager.PlaySound("volley");
            CmdVolley();
        }
        else if (vcr.GetKeyDown("left"))
        {
            crossbowMotor.MoveLeft();
        }
        else if (vcr.GetKeyDown("right"))
        {
            crossbowMotor.MoveRight();
        }
        else if (vcr.GetKeyDown("space"))
        {
            Shoot();
        }
        else if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            Debug.Log("start new recording");
            vcr.NewRecording();
        }
        else if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            String path = "exports/";
            // String fullPath = String.Format("{0}{1}_player{2}.json", path, DateTime.Now.Ticks, id);
            String fullPath = String.Format("{0}player{1}.json", path, id);
            File.WriteAllText(fullPath, vcr.GetRecording().ToString());
            Debug.Log("File written");
        }
        else if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            string path = String.Format("exports/player{0}.json", id);
            Debug.Log("attempt to playback: " + path);
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                Debug.Log(json);
                Recording recording = Recording.ParseRecording(json);
                vcr.Play(recording, 1);
            }
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
                        audioManager.PlaySound("klaxon");
                    }
                }
                if (screenShotEnabled)
                {
                    if (Time.time > nextScreenshotTime)
                    {
                        TakeScreenshot();
                        nextScreenshotTime = Time.time + SCREENSHOT_DELAY;
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
            CmdSetEnableScreenShot();
            CmdSendTroopAnim();
        }
    }

    [Command]
    public void CmdSendTroopAnim()
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);

        if (myTeamController.PlaySendTroopAnim == true)
        {
            Debug.Log("send troops");
            RpcSetSendTroopAlert();
            myTeamController.ResetSendTroopAlert();
        }
    }

    [ClientRpc]
    public void RpcSetSendTroopAlert()
    {
        canvasController.SetSendTroopAlert();
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
    private void CmdToggleScreenShot()
    {
        GameController gameController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>();
        gameController.ToggleScreenShot();
    }

    [Command]
    private void CmdSetEnableScreenShot()
    {
        GameController gameController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>();
        bool state = gameController.ScreenshotEnabled;
        RpcSetEnableScreenShot(state);
    }


    [ClientRpc]
    private void RpcSetEnableScreenShot(bool state)
    {
        screenShotEnabled = state;
    }

    [Command]
    private void CmdSetAIPlayerEnabled()
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);
        int aIActiveId = myTeamController.AIActivePlayer;

        RpcSetAIPlayerEnabled(aIActiveId);
    }


    [ClientRpc]
    private void RpcSetAIPlayerEnabled(int aIActiveId)
    {
        if (aIActiveId == id && teamAIEnabled == true) {
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

    [ClientRpc]
    private void RpcSetCurrencyText(int coin)
    {
        canvasController.SetCurrencyText(coin.ToString());
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
        crossbowController.HandleShoot();
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
                audioManager.PlaySound("volley");
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

    [ClientCallback]
    private void ClientPlaySound(string name)
    {
        audioManager.PlaySound(name);
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
            if (troopId == 0)
            {
                ClientPlaySound("sword");
            }
            else if (troopId == 2)
            {
                ClientPlaySound("horn");
            }
            spawnController.SpawnOffensiveTroop(troopId, laneId, myTeamId, opponentsTeamId);
            myTeamController.ResetSendTroopAlert();
            //RpcResetSendTroopAlert();
        }
        else
        {
            ClientPlaySound("coins");
        }
    }

    [ClientRpc]
    public void RpcResetSendTroopAlert()
    {
        canvasController.ResetSendTroopAlert();
    }

    [Command]
    public void CmdAddGold(int amount)
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);
        myTeamController.AddGold(amount);
    }
		
    private void TakeScreenshot()
    {
		Debug.Log ("taking screenshot");
		CmdGetDanger ();
        string directory = Path.GetFullPath(".");
		string path = Path.Combine(directory + String.Format("/Screenshots/{0}", currentDangerValue), String.Format("Screenshot_{0}_{1}.png",id, UnityEngine.Random.Range(0,20000)));
        ScreenCapture.CaptureScreenshot(path);
    }

	[Command]
	private void CmdGetDanger() {
		TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);
		int danger = myTeamController.GetLaneDangerIndex (laneId);
		RpcSetDanger (danger);
	}

	[ClientRpc]
	private void RpcSetDanger(int value) {
		currentDangerValue = value;
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