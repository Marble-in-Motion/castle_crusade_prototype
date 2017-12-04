using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class Currency : NetworkBehaviour {

    [SyncVar]
    public int currency = 100;

    // Use this for initialization
    public void spend(int amount)
    {
        if(currency > 0)
        {
            currency -= amount;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
