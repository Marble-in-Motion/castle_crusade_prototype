using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private float lookSensitivity = 3f;

    public const string PLAYER_TAG = "Player";

    private PlayerMotor motor;

    public TeamController team;

	[SerializeField]
	public GameObject prefabT1;

    [SerializeField]
    public GameObject prefabT2;

    [SerializeField]
    public GameObject[] spawnLocations;

    private GameObject target;
	private GameObject troopSpawn;
    public int playerId;

    void Start()
    {
        motor = GetComponent<PlayerMotor>();
        playerId = getPlayerId();
        int teamNum = getTeam();
        GameObject spawnTarget = spawnLocations[playerId];
		this.transform.position = spawnTarget.transform.position;
        if (teamNum == 1)
        {
            team = GameObject.FindGameObjectWithTag("Tower1").GetComponent<TeamController>();
			target = GameObject.FindGameObjectWithTag("Tower2");
			troopSpawn = GameObject.FindGameObjectWithTag ("TroopSpawn1");
            
        }
        else
        {
            team = GameObject.FindGameObjectWithTag("Tower2").GetComponent<TeamController>();
			target = GameObject.FindGameObjectWithTag("Tower1");
			troopSpawn = GameObject.FindGameObjectWithTag ("TroopSpawn2");

        }
        
        team.players.Add(this.gameObject);
    }



    void Update()
    {
        UpdateMovement();
        if (Input.GetKeyDown(KeyCode.A))
        {
            int currency = team.buy(10);
            this.GetComponentInChildren<Text>().text = "Coin: " + currency.ToString();
        }
		else if (Input.GetKeyDown(KeyCode.S) && isLocalPlayer) {
			CmdRequestTroopSpawn ();
		}

    }

    private int getPlayerId()
    {
        int players = GameObject.FindGameObjectsWithTag("Player").Length - 1;
        return players;
    }

    private int getTeam()
    {
        
        if(playerId == 0){
            return 1;
        }
        else
        {
            return 2;
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
	public void CmdRequestTroopSpawn() {
        GameObject prefab;
        if (team.team == 1)
        {
            prefab = prefabT1;
        }
        else
        {
            prefab = prefabT2;
        }
		GameObject troop = Instantiate(prefab, troopSpawn.transform.position, Quaternion.identity) as GameObject; //SpawnWithClientAuthority WORKS JUST LIKE NetworkServer.Spawn ...THE
		troop.GetComponent<AIController> ().target = target;
        NetworkServer.SpawnWithClientAuthority(troop, this.gameObject); //THIS WILL SPAWN THE troop THAT WAS CREATED ABOVE AND GIVE AUTHORITY TO THIS PLAYER. THIS PLAYER (GAMEOBJECT) MUST
	
	}

}


