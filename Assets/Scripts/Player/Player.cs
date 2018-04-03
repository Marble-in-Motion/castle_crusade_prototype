using UnityEngine.Networking;
using UnityEngine;
using System;
using Assets.Scripts.Player;

public class Player : NetworkSetup
{

    enum AICommands { FIND, AIM, KILL }

    public const string PLAYER_TAG = "Player";
    private const string REMOTE_LAYER_NAME = "RemotePlayer";
    private const float CLOSE_DISTANCE = 0.5f;
    private const int NUM_TROOPS_FOR_WARNING = 3;
    private const float KLAXON_FIRE_TIME = 5.0f;

    private int id;

    private int laneId;
    public int LaneId
    {
        get
        {
            Debug.Log(laneId);
            return laneId;
        }
    }

    private int myTeamId;
    public int MyTeamId
    {
        get
        {
            return myTeamId;
        }
    }

    private int opponentsTeamId;
    public int OpponentsTeamId
    {
        get
        {
            return opponentsTeamId;
        }
    }


    private bool cooldownAnimReset;

    private SpawnController spawnController;
    private CanvasController canvasController;
    private CrossbowMotor crossbowMotor;

    private AIController aiController;

    [SerializeField]
    private Behaviour[] componentsToDisable;

    [SerializeField]
    private GameObject crossbow;

    [SerializeField]
    private GameObject AudioGameObject;

    private AudioManager audioManager;

    private LineRenderer laserLine;

    private float nextActionTime = 0.0f;

    private float AIActionTime = 0.0f;
    private float AIMoveDelay = 0.5f;

    private Boolean AIEnabled = false;

    private float nextAIActionTime = 0;
    private float changeDirectionTime = 0.4f;
    private float timePerShot = 0.2f;
    private GameObject AITargetEnemy;
    private float AINextTroopSendTime = 0;
    private int AINextNumberTroopsToSend = 1;

    private AICommands nextCommand = AICommands.FIND;

    public int GetId()
    {
        return id;
    }

    private InputVCR vcr;

    void Awake()
    {
        vcr = GetComponent<InputVCR>();
        vcr.NewRecording();
    }

    void Start()
    {
        id = FindObjectsOfType<Player>().Length - 1;
        RegisterModel(PLAYER_TAG, GetId());
        spawnController = GameObject.FindGameObjectWithTag(SpawnController.SPAWN_CONTROLLER_TAG).GetComponent<SpawnController>();

        cooldownAnimReset = true;

        //get audio manager
        audioManager = AudioGameObject.GetComponent<AudioManager>();
        audioManager.BuildDict();
        audioManager.PlaySound("ambience");

        if (isLocalPlayer)
        {
            // Player Initialisation
            CmdInitialisePlayer();

            // Camera Settings
            Cursor.visible = false;

            // Canvas Settings
            Canvas canvas = GetComponentInChildren<Canvas>();
            canvas.planeDistance = 1;
            canvasController = canvas.GetComponent<CanvasController>();

            // crossbow
            crossbowMotor = crossbow.GetComponent<CrossbowMotor>();
            
        }

        if (!isLocalPlayer)
        {
            DisableNonLocalCompontents();
            AssignLayer(REMOTE_LAYER_NAME);
        }
    }

    [Command]
    void CmdInitialisePlayer()
    {
        GameController gameController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>();
        gameController.DeactiveScreenCamera();
        Transform transform = gameController.GetPlayerTransform(id);

        RpcSetPlayerTransform(transform.position, transform.rotation);
        RpcSetMyTeamId(gameController.GetMyTeamControllerId(id));
        RpcSetOpponentsTeamId(gameController.GetOpponentsTeamControllerId(id));
        RpcSetLaneId();
    }

    [ClientRpc]
    void RpcSetPlayerTransform(Vector3 position, Quaternion rotation)
    {
        transform.rotation = rotation;
        transform.position = position;
    }


    [ClientRpc]
    void RpcSetMyTeamId(int teamId)
    {
        myTeamId = teamId;
    }

    [ClientRpc]
    void RpcSetOpponentsTeamId(int teamId)
    {
        opponentsTeamId = teamId;
    }

    [ClientRpc]
    void RpcSetLaneId()
    {
        laneId = (myTeamId == TeamController.TEAM1)
            ? (id/ 2) + 1
            : (id - 1) / 2 + 1;
    }


    //[Command]
    //void CmdSetRenderTexture()
    //{
    //    TeamController opponentsTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetOpponentsTeamController(id);
    //    RenderTexture texture = opponentsTeamController.GetRenderTexture();
    //    RpcSetRenderTexture(texture);
    //}

    //[ClientRpc]
    //void RpcSetRenderTexture(RenderTexture texture)
    //{
    //    canvasController.SetRenderTexture(texture);
    //}

    
    void ExecuteControls()
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
        else if (Input.GetKeyDown(KeyCode.Slash))
        {
            CmdRequestOffensiveTroopSpawn(1, laneId);
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            CmdRequestOffensiveTroopSpawn(2, laneId);
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            CmdDestroyTroops();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            crossbowMotor.MoveLeft();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            crossbowMotor.MoveRight();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
        else if (Input.GetKeyDown(KeyCode.Delete))
        {
            String path = "exports/";
            String fullPath = String.Format("{0}{1}_player{2}.json", path, DateTime.Now.Ticks, GetId());
            System.IO.File.WriteAllText(fullPath, vcr.GetRecording().ToString());
            Debug.Log("File written");
        }

    }

    void Update()
    {
        if (isLocalPlayer)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                AIEnabled = !AIEnabled;
            }

            if (!AIEnabled)
            {
                ExecuteControls();
                
                GameObject[] enemyTroops = FindEnemyTroopsInLane();
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
                        AITargetEnemy = findTarget();
                        if(AITargetEnemy != null)
                        {
                            nextCommand = AICommands.AIM;
                        }
                    }
                    else if (nextCommand == AICommands.AIM)
                    {
                        moveTowardsTarget();
                        
                    }
                    else if(nextCommand == AICommands.KILL)
                    {
                        killTarget();
                    }
                }
                
            }
            CmdSetCurrencyText();
            CmdSetHealthBar();
            CmdSetGameState();
            CmdSetVolleyCooldown(); 
        }
    }

    [Command]
    void CmdSetCurrencyText()
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);
        int coin = myTeamController.GetCoin();
        RpcSetCurrencyText(coin);
    }

    [ClientRpc]
    void RpcSetCurrencyText(int coin)
    {
        canvasController.SetCurrencyText(coin.ToString());
    }

    [Command]
    void CmdSetHealthBar()
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);
        TeamController opponentsTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetOpponentsTeamController(id);

        float myHealthRatio = myTeamController.GetTowerHealthRatio();
        float opponentsHealthRatio = opponentsTeamController.GetTowerHealthRatio();
        RpcSetHealthBar(myHealthRatio, opponentsHealthRatio);
    }

    [ClientRpc]
    void RpcSetHealthBar(float myHealth, float opponentsHealth)
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
    void CmdSetGameState()
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);
        GameController.GameState gameState = myTeamController.GetIsGameOver();
        RpcSetGameState(gameState);
    }

    [ClientRpc]
    void RpcSetGameState(GameController.GameState gameState)
    {
        canvasController.SetGameOverValue(gameState);
    }

    [Command]
    void CmdSetVolleyCooldown()
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);
        float endOfCoolDown = myTeamController.GetEndOfCoolDown();
        float currentTime = myTeamController.GetCurrentTime();
        RpcSetVolleyCooldown(endOfCoolDown, currentTime);
    }

    [ClientRpc]
    void RpcSetVolleyCooldown(float endOfCoolDown, float currentTime)
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

    [Command]
    void CmdAISendTroops()
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);

        for (int i = 0; i < AINextNumberTroopsToSend; i++)
        {
            CmdRequestOffensiveTroopSpawn(0, laneId - 1);
        }
        System.Random rnd = new System.Random();
        int interval = rnd.Next(1, 11);
        AINextTroopSendTime = Time.time + interval;

        int coin = myTeamController.GetCoin();
        int upperBound = coin / 40;
        AINextNumberTroopsToSend = rnd.Next(1, upperBound);
    }

    private GameObject findTarget()
    {
        GameObject[] troopsInLane = FindEnemyTroopsInLane(); ;
        GameObject target = crossbowMotor.AIFindTarget(troopsInLane);
        return target;
    }

    private void moveTowardsTarget()
    {
        int currentPath = crossbowMotor.ActivePath;
        int targetPath = AITargetEnemy.GetComponent<AIController>().GetPath();
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

    private void killTarget()
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

    public GameObject[] FindEnemyTroopsInLane()
    {
        return GetTroopsInLane(opponentsTeamId, laneId);
    }

    private GameObject[] GetTroopsInLane(int teamId, int lane)
    {
        String troopTag = String.Format("NPCT{0}L{1}", teamId, lane);
        return GameObject.FindGameObjectsWithTag(troopTag);
    }

    [ClientRpc]
    void RpcShootVolley(GameObject[] troops)
    {
        StartCoroutine(crossbow.GetComponent<CrossbowController>().HandleVolley(troops));
    }

    [Command]
    private void CmdDestroyTroops()
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);

        if (myTeamController.GetCurrentTime() > myTeamController.GetEndOfCoolDown())
        {
            bool successfulPurchase = myTeamController.SpendGold(Params.DESTROY_COST);
            if (successfulPurchase)
            {
                audioManager.PlaySound("volley");
                myTeamController.CmdUpdateCoolDown();
                GameObject[] troops = GetTroopsInLane(opponentsTeamId, laneId);
                RpcShootVolley(troops);
            }
        }
    }

    [Client]
    void Shoot()
    {
        laserLine = crossbow.GetComponent<LineRenderer>();
        laserLine.SetPosition(0, crossbow.transform.position);
        StartCoroutine(crossbow.GetComponent<CrossbowController>().HandleShoot());
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
        crossbow.GetComponent<CrossbowController>().HandleArrow(laserLine.GetPosition(1));
    }

    [Command]
    public void CmdPlayerShot(string targetId, float damage, Vector3 crossbowPosition)
    {
        GameObject target = GameObject.Find(targetId);
        if (target == null)
        {
            return;
        }
        if (target.GetComponent<NPCHealth>())
        {
            target.GetComponent<NPCHealth>().DeductHealth(damage, crossbow.GetComponent<CrossbowController>().GetArrowSpeed(), crossbowPosition);
            if (!target.GetComponent<NPCHealth>().IsAlive())
            {
                CmdAddGold(Params.NPC_REWARD[target.GetComponentInParent<AIController>().GetTroopType()]);
            }
        }
        if (target.transform != null /*&& target.collider.tag == "NPC"*/)
        {
            target.transform.position = (target.transform.position /*- (normal of the hit)*/);
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
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(id);
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
            spawnController.SpawnOffensive(troopId, spawnId, myTeamId);
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

}