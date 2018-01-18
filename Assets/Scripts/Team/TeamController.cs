using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TeamController : NetworkBehaviour {


    const int TEAM1 = 1;
    const int TEAM2 = 2;

    [SerializeField]
    private GameObject tower1;

    [SerializeField]
    private GameObject tower2;

    [SerializeField]
    private GameObject prefabT1;

    [SerializeField]
    private GameObject prefabT2;


    private List<GameObject> playersTeam1;

    private List<GameObject> playersTeam2;
   
    public int coin;

    [SerializeField]
    private GameObject [] spawnLocations;

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
        int id = GameObject.FindGameObjectsWithTag(PlayerController.PLAYER_TAG).Length - 1;
        int teamNum;
        GameObject troopSpawn;
        GameObject target;
        if(id == 0)
        {
            teamNum = TEAM1;
            playersTeam1.Add(newPlayer);
            troopSpawn = GameObject.FindGameObjectWithTag("TroopSpawn1");
            target = tower2;
        }
        else
        {
            teamNum = TEAM2;
            playersTeam2.Add(newPlayer);
            troopSpawn = GameObject.FindGameObjectWithTag("TroopSpawn2");
            target = tower1;
        }
        Transform spawnTarget = spawnLocations[id].transform;
        newPlayer.transform.position = spawnTarget.position;
        newPlayer.GetComponent<PlayerController>().InitialisePlayerInfo(id, teamNum, troopSpawn, target);
    }

    [Server]
    public void SendTroop(GameObject player)
    {
        GameObject prefab;
        prefab = (player.GetComponent<PlayerController>().TeamNum == TEAM1) ? prefabT1 : prefabT2;


        GameObject troop = Instantiate(prefab, player.GetComponent<PlayerController>().TroopSpawn.transform.position, Quaternion.identity) as GameObject;
        troop.GetComponent<AIController>().target = player.GetComponent<PlayerController>().Target;
        NetworkServer.Spawn(troop);
    }



}
