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
    private InputVCR vcr;

    [SerializeField]
    private Behaviour[] componentsToDisable;

    [SerializeField]
    private GameObject crossbow;

    [SerializeField]
    private GameObject AudioGameObject;
      
    private float nextActionTime = 0.0f;

    // AI PARAMS
    private Boolean AIEnabled = false;
    enum AICommands { FIND, AIM, KILL }
    private AICommands nextCommand = AICommands.FIND;
    private GameObject AITargetEnemy;
    private float AIActionTime = 0.0f;
    private float AIMoveDelay = 0.5f;
    private float nextAIActionTime = 0;
    private float changeDirectionTime = 0.4f;
    private float timePerShot = 0.2f;
    private float AINextTroopSendTime = 0;
    private int AINextNumberTroopsToSend = 1;

    void Awake()
    {
        // Audio manager
        audioManager = AudioGameObject.GetComponent<AudioManager>();
        audioManager.BuildDict();
        audioManager.PlaySound("ambience");

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
            
        } else
        {
            DisableNonLocalCompontents();
            AssignLayer(REMOTE_LAYER_NAME);
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
            CmdRequestOffensiveTroopSpawn(0, laneId);
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            CmdRequestOffensiveTroopSpawn(1, laneId);
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            CmdDestroyTroops();
        }
        else if (vcr.GetKeyDown("left"))
        {
            crossbowMotor.MoveLeft();
            CmdTakeScreenshot();
        }
        else if (vcr.GetKeyDown("right"))
        {
            crossbowMotor.MoveRight();
            CmdTakeScreenshot();
        }
        else if (vcr.GetKeyDown("space"))
        {
            Shoot();
            CmdTakeScreenshot();
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
        Debug.Log(string.Format("{0}: id: {1}, teamId: {2}, opponentsTeamId: {3}, laneId: {4}", NetworkName, id, myTeamId, opponentsTeamId, laneId));

        if (isLocalPlayer)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                AIEnabled = !AIEnabled;
            }

            if (!AIEnabled)
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
            }
            else
            {
                if (Time.time > AINextTroopSendTime)
                {
                    CmdAISendTroops();                   
                }
                if (Time.time > nextAIActionTime)
                {
                    if (nextCommand == AICommands.FIND)
                    {
                        AITargetEnemy = FindTarget();
                        if (AITargetEnemy != null)
                        {
                            nextCommand = AICommands.AIM;
                        }
                    }
                    else if (nextCommand == AICommands.AIM)
                    {
                        MoveTowardsTarget();

                    }
                    else if (nextCommand == AICommands.KILL)
                    {
                        KillTarget();
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

    [ClientRpc]
    private void RpcShootVolley(GameObject[] troops)
    {
        StartCoroutine(crossbowController.HandleVolley(troops));
    }

    [Command]
    private void CmdDestroyTroops()
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

    private void Shoot()
    {
        LineRenderer laserLine = crossbow.GetComponent<LineRenderer>();
        laserLine.SetPosition(0, crossbow.transform.position);
        StartCoroutine(crossbowController.HandleShoot());
        RaycastHit hit;
        if (Physics.Raycast(crossbow.transform.position, crossbow.transform.forward, out hit, Params.Bolt.RANGE))
        {
            CmdPlayerShot(hit.collider.name, Params.Bolt.DAMAGE, this.transform.position);
            laserLine.SetPosition(1, hit.point);
        }
        else
        {
            laserLine.SetPosition(1, crossbow.transform.position + crossbow.transform.forward * Params.Bolt.RANGE);
        }
        crossbowController.HandleArrow(laserLine.GetPosition(1));
    }

    [Command]
    private void CmdPlayerShot(string targetId, float damage, Vector3 crossbowPosition)
    {
        GameObject target = GameObject.Find(targetId);
        if (target == null)
        {
            return;
        }
        if (target.GetComponent<NPCHealth>())
        {
            target.GetComponent<NPCHealth>().CmdDeductHealth(damage, crossbowController.GetArrowSpeed(), crossbowPosition);
            if (!target.GetComponent<NPCHealth>().IsAlive())
            {
                CmdAddGold(Params.NPC_REWARD[target.GetComponentInParent<AIController>().TroopType]);
            }
        }
        if (target.transform != null /*&& target.collider.tag == "NPC"*/)
        {
            target.transform.position = (target.transform.position /*- (normal of the hit)*/);
        }
    }

    [Command]
    private void CmdRequestOffensiveTroopSpawn(int troopId, int laneId)
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);
        SpawnController spawnController = GameObject.FindGameObjectWithTag(SpawnController.SPAWN_CONTROLLER_TAG).GetComponent<SpawnController>();
        int cost = Params.NPC_COST[troopId];

        bool successfulPurchase = myTeamController.SpendGold(cost);
        if (successfulPurchase)
        {
            if (troopId == 0)
            {
                audioManager.PlaySound("sword");
            }
            else if (troopId == 2)
            {
                audioManager.PlaySound("horn");
            }
            spawnController.SpawnOffensiveTroop(troopId, laneId, myTeamId, opponentsTeamId);
        }
        else
        {
            audioManager.PlaySound("coins");
        }
    }

    [Command]
    private void CmdAddGold(int amount)
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);
        myTeamController.AddGold(amount);
    }

    [Command]
    private void CmdTakeScreenshot()
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);    
        
        if (myTeamController.LastActivePlayerId != id)
        {
            RpcTakeScreenshot(myTeamId, myTeamController.ScreenshotCount);
            myTeamController.IncrementScreenshotCount();
            myTeamController.SetLastActivePlayerId(id);
        }
    }

    [ClientRpc]
    private void RpcTakeScreenshot(int teamId, int screenshotCount)
    {
        if (myTeamId == teamId)
        {
            Debug.Log("id: " + id);
            string directory = Path.GetFullPath(".");
            string path = Path.Combine(directory, String.Format("Screenshot_{0}_{1}_{2}.png", myTeamId, id, screenshotCount));
            ScreenCapture.CaptureScreenshot(path);
        }
    }

    private void DisableNonLocalCompontents()
    {
        foreach (Behaviour behaviour in componentsToDisable)
        {
            behaviour.enabled = false;
        }
    }



    // AI IMPLEMENTATION ######################################################

    [Command]
    private void CmdAISendTroops()
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);

        for (int i = 0; i < AINextNumberTroopsToSend; i++)
        {
            CmdRequestOffensiveTroopSpawn(0, laneId);
        }
        System.Random rnd = new System.Random();
        int interval = rnd.Next(1, 11);
        AINextTroopSendTime = Time.time + interval;

        int coin = myTeamController.Coin;
        int upperBound = coin / 40;
        AINextNumberTroopsToSend = rnd.Next(1, upperBound);
    }

    private GameObject FindTarget()
    {
        List<GameObject> troopsInLane = FindEnemyTroopsInLane();
        GameObject target = crossbowMotor.AIFindTarget(troopsInLane);
        return target;
    }

    private void MoveTowardsTarget()
    {
        int currentPath = crossbowMotor.ActivePath;
        int targetPath = AITargetEnemy.GetComponent<AIController>().Path;
        if (currentPath > targetPath)
        {
            crossbowMotor.MoveLeft();
            nextAIActionTime = Time.time + changeDirectionTime;
        }
        else if (currentPath < targetPath)
        {
            crossbowMotor.MoveRight();
            nextAIActionTime = Time.time + changeDirectionTime;
        }

        currentPath = crossbow.GetComponent<CrossbowMotor>().ActivePath;
        if (currentPath == targetPath)
        {
            nextCommand = AICommands.KILL;
        }
    }

    private void KillTarget()
    {
        Shoot();
        if (!AITargetEnemy.GetComponent<NPCHealth>().IsAlive())
        {
            nextCommand = AICommands.FIND;
        }
        else
        {
            nextAIActionTime = Time.time + timePerShot;
        }
    }

}