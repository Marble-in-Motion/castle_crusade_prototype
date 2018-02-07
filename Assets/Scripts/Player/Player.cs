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

    void Start()
    {
        id = FindObjectsOfType<Player>().Length - 1;
        RegisterModel(Player.PLAYER_TAG, GetId());

        spawnController = GameObject.FindGameObjectWithTag(SpawnController.SPAWN_CONTROLLER_TAG).GetComponent<SpawnController>();
        GameController gameController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>();
        teamController = gameController.GetTeamController(id);

		bolt = new Bolt ();

        if (isLocalPlayer)
        {
            // Player Initialisation
            gameController.InitialisePlayer(this);

            // Camera Settings
            Cursor.visible = false;

            // Canvas Settings
            Canvas canvas = GetComponentInChildren<Canvas>();
            canvas.planeDistance = 1;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = GetComponentInChildren<Camera>();
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
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                CmdDestroyTroops(GetId(), teamController.GetId());
            }
            else if (Input.GetButtonDown("Fire1"))
			{
				Shoot();
			}

            canvasController.SetCurrencyText("Coin: " + teamController.GetCoin().ToString());
			float calc_Health = teamController.GetTowerHealth() / maxTowerHealth;
            canvasController.SetHealthBar(calc_Health);
            canvasController.SetGameOverValue(teamController.GetIsGameOver());

            GameObject[] troops = GetTroopsInLane(teamController.GetId(), GetLaneId(GetId(), teamController.GetId()));
            Debug.Log(troops.Length);
            for (int i = 0; i < troops.Length; i++) {
                AIController ai = troops[i].GetComponent<AIController>();
                Debug.Log(troops[i].name);
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

    [Command]
    private void CmdDestroyTroops(int id, int teamId)
    {
        bool successfulPurchase = teamController.SpendGold(50);
        if (successfulPurchase)
        {
            int targetTeamId = GetTargetId(teamId);
            int lane = GetLaneId(id, teamId);
            GameObject[] troops = GetTroopsInLane(targetTeamId, lane);
            for (int i = 0; i < troops.Length; i++) { Destroy(troops[i]); }
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
        int teamId = teamController.GetId();
        bool successfulPurchase = teamController.SpendGold(10);
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
