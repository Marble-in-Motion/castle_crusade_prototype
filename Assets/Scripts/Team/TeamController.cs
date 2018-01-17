using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TeamController : MonoBehaviour {

    [SerializeField]
    public GameObject towerSpawn;

    [SerializeField]
    public GameObject towerTarget;

    public List<GameObject> players;

    public int teamNumber;

    public int coin;


	// Use this for initialization
	void Start () {
        players = new List<GameObject>();
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

    public void addTeamMember(GameObject player)
    {
        players.Add(player);
    }

}
