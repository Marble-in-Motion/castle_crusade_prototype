using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TeamController : NetworkBehaviour {

    [SerializeField]
    public GameObject tower1;

    [SerializeField]
    public GameObject tower2;

    [SerializeField]
    public GameObject prefabT1;

    [SerializeField]
    public GameObject prefabT2;


    public List<GameObject> playersTeam1;

    public List<GameObject> playersTeam2;
   
    public int coin;

    static int playerCount;

    [SerializeField]
    public GameObject [] spawnLocations;

    // Use this for initialization
    void Start () {
        playersTeam1 = new List<GameObject>();
        playersTeam1 = new List<GameObject>();
        coin = 100;
	}

    public int getCoin()
    {
        return coin;
    }

    public int buy(int amount)
    {
        if(coin - amount > 0)
        {
            coin -= amount;
        }
        return coin;
        
    }

    [Server]
    public void SetPlayerId(GameObject newPlayer)
    {
        int players = GameObject.FindGameObjectsWithTag("Player").Length - 1;
        newPlayer.GetComponent<PlayerController>().playerId = players;
    }

    [Server]
    public void SetPlayerTeam(GameObject newPlayer) {
        int id = GameObject.FindGameObjectsWithTag("Player").Length - 1;
        newPlayer.GetComponent<PlayerController>().playerId = id;
        if(id == 0)
        {
            newPlayer.GetComponent<PlayerController>().teamNum = 1;
            playersTeam1.Add(newPlayer);
            newPlayer.GetComponent<PlayerController>().troopSpawn = GameObject.FindGameObjectWithTag("TroopSpawn1");
            newPlayer.GetComponent<PlayerController>().target = tower2;
        }
        else
        {
            newPlayer.GetComponent<PlayerController>().teamNum = 2;
            playersTeam2.Add(newPlayer);
            newPlayer.GetComponent<PlayerController>().troopSpawn = GameObject.FindGameObjectWithTag("TroopSpawn2");
            newPlayer.GetComponent<PlayerController>().target = tower1;
        }
        newPlayer.GetComponent<PlayerController>().spawnTarget = spawnLocations[id];
        Transform location = spawnLocations[id].transform;
        newPlayer.transform.position = location.position;

    }

    [Server]
    public void SendTroop(GameObject player)
    {
        GameObject prefab;
        if (player.GetComponent<PlayerController>().teamNum == 1)
        {
            prefab = prefabT1;
        }
        else
        {
            prefab = prefabT2;
        }
        GameObject troop = Instantiate(prefab, player.GetComponent<PlayerController>().troopSpawn.transform.position, Quaternion.identity) as GameObject; //SpawnWithClientAuthority WORKS JUST LIKE NetworkServer.Spawn ...THE
        troop.GetComponent<AIController>().target = player.GetComponent<PlayerController>().target;
        NetworkServer.Spawn(troop); //THIS WILL SPAWN THE troop THAT WAS CREATED ABOVE AND GIVE AUTHORITY TO THIS PLAYER. THIS PLAYER (GAMEOBJECT) MUST
    }

}
