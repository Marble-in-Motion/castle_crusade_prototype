using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class Currency : NetworkBehaviour {

    [SyncVar]
    public int currencyTeam1 = 100;

    [SyncVar]
    public int currencyTeam2 = 100;
    // Use this for initialization
    public void spend(int amount)
    {
        if (!isServer)
            return;

        int team = this.GetComponent<PlayerController>().team;
        if(team == 1)
        {
            currencyTeam1 -= amount;
        }
        else
        {
            currencyTeam2 -= amount;
        }
        

    }
}
