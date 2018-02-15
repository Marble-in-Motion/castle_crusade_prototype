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

    private int id;

    private TeamController teamController;

    private SpawnController spawnController;

    private CanvasController canvasController;

    private AIController aiController;

    [SerializeField]
    private Behaviour[] componentsToDisable;

	[SerializeField]
	private GameObject crossbow;

	private float maxTowerHealth = 100f;

	private Bolt bolt;

	private LineRenderer laserLine;

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
        RegisterModel(Player.PLAYER_TAG, GetId());

        spawnController = GameObject.FindGameObjectWithTag(SpawnController.SPAWN_CONTROLLER_TAG).GetComponent<SpawnController>();
        GameController gameController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>();
        teamController = gameController.GetTeamController(id);

        bolt = new Bolt();

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
                CmdRequestOffensiveTroopSpawn(0, GetLaneId(GetId(), teamController.GetId()) - 1);
            }
            else if (Input.GetKeyDown(KeyCode.Backspace))
            {
                CmdRequestOffensiveTroopSpawn(1, GetLaneId(GetId(), teamController.GetId()) - 1);
            }
            else if (Input.GetKeyDown(KeyCode.Slash))
            {
                CmdRequestOffensiveTroopSpawn(2, GetLaneId(GetId(), teamController.GetId()) - 1);
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                CmdDestroyTroops(GetId(), teamController.GetId());
            }
            else if (Input.GetButtonDown("Fire1"))
			{
				Shoot();
			} else if (Input.GetKeyDown(KeyCode.Delete))
            {
                String path = "exports/";
                String fullPath = String.Format("{0}{1}_player{2}.json", path, DateTime.Now.Ticks, GetId());
                System.IO.File.WriteAllText(fullPath, vcr.GetRecording().ToString());
                Debug.Log("File written");
            }

            canvasController.SetCurrencyText("Coin: " + teamController.GetCoin().ToString());
			float calc_Health = teamController.GetTowerHealth() / maxTowerHealth;
            canvasController.SetHealthBar(calc_Health);
            canvasController.SetGameOverValue(teamController.GetIsGameOver());

            GameObject[] troops = GetTroopsInLane(teamController.GetId(), GetLaneId(GetId(), teamController.GetId()));
            for (int i = 0; i < troops.Length; i++) {
                AIController ai = troops[i].GetComponent<AIController>();
                canvasController.SetSpartanDistance(troops[i].name, ai.GetDistanceRatioToTarget());
            }
        }
    }

    private int GetTargetId(int teamId)
    {
        return (teamController.GetId() == TeamController.TEAM1) ? TeamController.TEAM2 : TeamController.TEAM1;
    }

    private int GetLaneId(int playerId, int teamId)
    {
        return (teamId == TeamController.TEAM1) ? (playerId / 2) + 1 : (playerId - 1) / 2 + 1;
    }

    private GameObject[] GetTroopsInLane(int targetTeamId, int lane)
    {
        String troopTag = String.Format("NPCT{0}L{1}", targetTeamId, lane);
        return GameObject.FindGameObjectsWithTag(troopTag);
    }

	[ClientRpc]
	void RpcShootVolley(GameObject[] troops) {
		StartCoroutine(crossbow.GetComponent<CrossbowController> ().HandleVolley (troops));
	}

	[Command]
	private void CmdDestroyTroops(int id, int teamId)
	{
		bool successfulPurchase = teamController.SpendGold(100);
		if (successfulPurchase)
		{
			int targetTeamId = GetTargetId(teamId);
			int lane = GetLaneId(id, teamId);
			GameObject[] troops = GetTroopsInLane(targetTeamId, lane);
			RpcShootVolley (troops);
		}
	}

	[Client]
	void Shoot()
	{
		laserLine = crossbow.GetComponent<LineRenderer>();
		laserLine.SetPosition(0, crossbow.transform.position);
		StartCoroutine(crossbow.GetComponent<CrossbowController>().HandleShoot());
		RaycastHit hit;
		if (Physics.Raycast(crossbow.transform.position, crossbow.transform.forward, out hit, bolt.range)) {
			CmdPlayerShot(hit.collider.name, bolt.damage, this.transform.position);
			laserLine.SetPosition(1, hit.point);
		} else {
			laserLine.SetPosition(1, crossbow.transform.position + crossbow.transform.forward * bolt.range);
		}
		crossbow.GetComponent<CrossbowController>().HandleArrow(laserLine.GetPosition (1));
	}

	[Command]
	public void CmdPlayerShot(string id, float damage, Vector3 crossbowPosition)
	{
		GameObject target = GameObject.Find(id);
		if (target.GetComponent<NPCHealth> ()) {
			target.GetComponent<NPCHealth>().DeductHealth(damage, crossbow.GetComponent<CrossbowController>().GetArrowSpeed(),crossbowPosition);
			if (!target.GetComponent<NPCHealth>().IsAlive())
			{
				this.GetComponentInParent<Player>().CmdAddGold(5);
			}
		}
		if (target.transform != null /*&& target.collider.tag == "NPC"*/) {
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
        int cost = 0;
        switch (troopId)
        {
            case 0: cost = 10;break;
            case 1: cost = 40; break;
            case 2: cost = 40; break;


        }
        int teamId = teamController.GetId();
        bool successfulPurchase = teamController.SpendGold(cost);
        if (successfulPurchase) {
            spawnController.SpawnOffensive(troopId, spawnId, teamId);
        }
        
    }

    [Command]
    private void CmdAddGold(int amount)
    {
        teamController.AddGold(amount);
    }

}
