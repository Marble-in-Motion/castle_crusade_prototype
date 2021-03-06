﻿using UnityEngine;
using UnityEngine.Networking;

/**
 * Handler for tower collisions
 * Deduct health if troop collides with target
 **/
public class CollisionHandler : NetworkBehaviour
{

    [SerializeField]
    private int teamId;

    void OnTriggerEnter(Collider other)
    {
        int troopId = other.gameObject.GetComponent<AIController>().TroopType;
        int towerDamage = Params.NPC_DAMAGE[troopId]; // need to get troopId

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