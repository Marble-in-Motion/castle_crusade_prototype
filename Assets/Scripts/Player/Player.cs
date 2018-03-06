using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using System;
using Assets.Scripts.Player;

public class Player : NetworkSetup
{

    public const string PLAYER_TAG = "Player";
    private const string REMOTE_LAYER_NAME = "RemotePlayer";
    private const float CLOSE_DISTANCE = 0.5f;
    private const int NUM_TROOPS_FOR_WARNING = 3;
    private const float KLAXON_FIRE_TIME = 5.0f;

    private int id;
    private int teamId;

    private TeamController myTeamController;

    private TeamController opponentsTeamController;

    private SpawnController spawnController;

    private CanvasController canvasController;

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

    private float endOfCoolDown;

    public int GetId()
    {
        return id;
    }

    public int GetTeamId()
    {
        return teamId;
    }

    public int GetOpponentTeamId()
    {
        return opponentsTeamController.GetId();
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
        RegisterModel(Player.PLAYER_TAG, GetId());
        endOfCoolDown = Time.time;
        spawnController = GameObject.FindGameObjectWithTag(SpawnController.SPAWN_CONTROLLER_TAG).GetComponent<SpawnController>();
        GameController gameController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>();
        myTeamController = gameController.GetMyTeamController(id);
        opponentsTeamController = gameController.GetOpponentTeamController(id);
        teamId = myTeamController.GetId();

        if (isLocalPlayer)
        {
            // Player Initialisation
            gameController.InitialisePlayer(this);

            // Camera Settings
            Cursor.visible = false;

            // Canvas Settings
            Canvas canvas = GetComponentInChildren<Canvas>();
            canvas.planeDistance = 1;
            canvasController = canvas.GetComponent<CanvasController>();
            canvasController.SetRenderTexture(opponentsTeamController.GetRenderTexture());

            //get audio manager
            audioManager = AudioGameObject.GetComponent<AudioManager>();
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
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                CmdRequestOffensiveTroopSpawn(0, GetSpawnId() - 1);
            }
			else if (Input.GetKeyDown(KeyCode.Slash))
            {
                CmdRequestOffensiveTroopSpawn(1, GetSpawnId() - 1);
            }
            else if (Input.GetKeyDown(KeyCode.Backspace))
            {
                CmdRequestOffensiveTroopSpawn(2, GetSpawnId() - 1);
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                if (Time.time > myTeamController.getEndOfCoolDown())
                {
                    CmdDestroyTroops(GetId(), GetTeamId());
                    

					canvasController.SetArrowCooldown ();
                }
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                crossbow.GetComponent<CrossbowMotor>().moveLeft();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                crossbow.GetComponent<CrossbowMotor>().moveRight();
            }
            else if (Input.GetKeyDown(KeyCode.S))
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

            canvasController.SetCurrencyText(myTeamController.GetCoin().ToString());
            canvasController.SetHealthBar(myTeamController.GetTowerHealthRatio());
            canvasController.SetOpponentsHealthBar(opponentsTeamController.GetTowerHealthRatio());
            canvasController.SetGameOverValue(myTeamController.GetIsGameOver());

            GameObject[] myTroops = GetTroopsInLane(GetTeamId(), GetSpawnId());
            Dictionary<String, float> troopLocs = new Dictionary<string, float>();
            for (int i = 0; i < myTroops.Length; i++)
            {
                AIController ai = myTroops[i].GetComponent<AIController>();
                troopLocs.Add(myTroops[i].name, ai.GetDistanceRatioToTarget());
            }
            canvasController.SetSpartanDistances(troopLocs);

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
    }

    public int GetSpawnId()
    {
        return GetLaneId(GetId(), GetTeamId());
    }

    public GameObject[] FindEnemyTroopsInLane()
    {
        return GetTroopsInLane(opponentsTeamController.GetId(), GetSpawnId());
    }

    private int GetLaneId(int playerId, int teamId)
    {
        return (teamId == TeamController.TEAM1)
            ? (playerId / 2) + 1
            : (playerId - 1) / 2 + 1;
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
    private void CmdDestroyTroops(int id, int teamId)
    {
        bool successfulPurchase = myTeamController.SpendGold(Params.DESTROY_COST);
        if (successfulPurchase)
        {
            int targetTeamId = opponentsTeamController.GetId();
            int lane = GetSpawnId();
            myTeamController.CmdUpdateCoolDown();
            GameObject[] troops = GetTroopsInLane(targetTeamId, lane);
            RpcShootVolley(troops);
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
    public void CmdPlayerShot(string id, float damage, Vector3 crossbowPosition)
    {
        GameObject target = GameObject.Find(id);
        if (target == null)
        {
            return;
        }
        if (target.GetComponent<NPCHealth>())
        {
            target.GetComponent<NPCHealth>().DeductHealth(damage, crossbow.GetComponent<CrossbowController>().GetArrowSpeed(), crossbowPosition);
            if (!target.GetComponent<NPCHealth>().IsAlive())
            {
                this.GetComponentInParent<Player>().CmdAddGold(Params.NPC_REWARD[target.GetComponentInParent<AIController>().GetTroopType()]);
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
        int cost = Params.NPC_COST[troopId];
        int teamId = GetTeamId();
        bool successfulPurchase = myTeamController.SpendGold(cost);
        if (successfulPurchase)
        {
            spawnController.SpawnOffensive(troopId, spawnId, teamId);
        }

    }

    [Command]
    private void CmdAddGold(int amount)
    {
        myTeamController.AddGold(amount);
    }

}