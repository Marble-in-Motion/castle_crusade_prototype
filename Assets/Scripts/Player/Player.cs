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

    [SerializeField]
    private Behaviour[] componentsToDisable;

	[SerializeField]
	private GameObject crossbow;

	private float maxTowerHealth = 100f;
	
	private int gameOverValue = GameController.gameInProgress;

	private Bolt bolt;

	private LineRenderer laserLine;

	[SerializeField]
	private LayerMask mask;

	public Animator anim;

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

		bolt = new Bolt ();
        anim = GetComponent<Animator>();

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
                DestroyTroops();
            }
            else if (Input.GetButtonDown("Fire1"))
			{
				Shoot();
			}

            canvasController.SetCurrencyText("Coin: " + teamController.GetCoin().ToString());

			float calc_Health = teamController.GetTowerHealth() / maxTowerHealth;
            canvasController.SetHealthBar(calc_Health);

			gameOverValue = teamController.GetIsGameOver();
			if (gameOverValue == GameController.gameLost) {
                anim.ResetTrigger("Restart");
                anim.SetTrigger ("GameOver");
			}
            else if (gameOverValue == GameController.gameWon)
            {
                anim.ResetTrigger("Restart");
                anim.SetTrigger("GameWin");
            }
            else if (gameOverValue == GameController.gameRestart)
            {
                anim.ResetTrigger("GameWin");
                anim.ResetTrigger("GameOver");
                anim.SetTrigger("Restart");
            }
        }
    }


    public void DestroyTroops()
    {
        
        int attackId = (teamController.GetId() == 1) ? 2 : 1;
        int lane = (teamController.GetId() == 1) ? (id/2) + 1 : (id - 1)/2 + 1;
        String destroyTag = String.Format("NPCT{0}L{1}", attackId, lane);
        Debug.Log(destroyTag);
        GameObject[] troops = GameObject.FindGameObjectsWithTag(destroyTag);
        for (int i = 0; i < troops.Length; i++) { Destroy(troops[i]); }
    }

	[Client]
	void Shoot()
	{
		laserLine = crossbow.GetComponent<LineRenderer>();
		laserLine.SetPosition(0, crossbow.transform.position);
		StartCoroutine(crossbow.GetComponent<CrossbowController>().HandleShoot());
		RaycastHit hit;
		if (Physics.Raycast(crossbow.transform.position, crossbow.transform.forward, out hit, bolt.range, mask)) {
			CmdPlayerShot(hit.collider.name, bolt.damage);
			laserLine.SetPosition(1, hit.point);
		} else {
			laserLine.SetPosition(1, crossbow.transform.position + crossbow.transform.forward * bolt.range);
		}
		crossbow.GetComponent<CrossbowController>().HandleArrow(laserLine.GetPosition (1));
	}

	[Command]
	public void CmdPlayerShot(string id, float damage)
	{
		GameObject target = GameObject.Find(id);
		if (target.GetComponent<NPCHealth> ()) {
			target.GetComponent<NPCHealth>().DeductHealth(damage);
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
