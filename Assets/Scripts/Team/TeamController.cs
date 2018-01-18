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
    public void SetPlayerInfo(GameObject newPlayer) {
        int id = GameObject.FindGameObjectsWithTag("Player").Length - 1;
        int teamNum;
        GameObject troopSpawn;
        GameObject target;
        if(id == 0)
        {
            teamNum = 1;
            playersTeam1.Add(newPlayer);
            troopSpawn = GameObject.FindGameObjectWithTag("TroopSpawn1");
            target = tower2;
        }
        else
        {
            teamNum = 2;
            playersTeam2.Add(newPlayer);
            troopSpawn = GameObject.FindGameObjectWithTag("TroopSpawn2");
            target = tower1;
        }
        newPlayer.GetComponent<PlayerController>().spawnTarget = spawnLocations[id];
        Transform location = spawnLocations[id].transform;
        newPlayer.transform.position = location.position;
        newPlayer.GetComponent<PlayerController>().initialisePlayerInfo(id, teamNum, troopSpawn, target);
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
