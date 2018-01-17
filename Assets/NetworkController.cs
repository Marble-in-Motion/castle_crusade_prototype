using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkController : NetworkBehaviour {

	public int PlayerGetTeam () {
        int players = GameObject.FindGameObjectsWithTag("Player").Length - 1;
        if (players == 0)
        {
            return 1;
        }
        else
        {
            return 2;
        }
    }

    public int getPlayerId()
    {
      int players = GameObject.FindGameObjectsWithTag("Player").Length - 1;
      return players;
    }

}
