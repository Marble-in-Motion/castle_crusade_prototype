using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class CollisionHandler : NetworkBehaviour {

	[SerializeField]
	private int teamId;
    
	void OnTriggerEnter(Collider other)
	{
		int towerDamage = Params.NPC_DAMAGE[0]; // need to get troopId

        DeductTowerHealth(towerDamage);
        DestroyTroop(other.gameObject);
	}


    void DeductTowerHealth(int damage)
    {
        TeamController teamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetTeamControllerById(teamId);
        teamController.DeductTowerHealth(damage);
    }

    void DestroyTroop(GameObject troop)
    {
        NetworkServer.Destroy(troop);
    }

}