using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TeamController : MonoBehaviour {

    public List<GameObject> players;
    public int team;

    public int coin;


	// Use this for initialization
	void Start () {
        players = new List<GameObject>();
        coin = 100;
	}

    public int buy(int amount)
    {
        if(coin - amount > 0)
        {
            coin -= amount;
        }
        return coin;
        
    }

}
