﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SpawnController : NetworkBehaviour
{
    public const string SPAWN_CONTROLLER_TAG = "SpawnController";

    //troops
    [SerializeField]
    private GameObject[] troopPrefabs;

    //spawn location - lanes
    [SerializeField]
    private GameObject[] spawnLocations;

    //targets
    [SerializeField]
    private GameObject[] towers;

    private GameObject GetTroopFromId(int troopId)
    {
        return troopPrefabs[troopId];
    }

    private GameObject GetSpawnFromId(int spawnId)
    {
        return spawnLocations[spawnId];
    }

    private GameObject GetTargetTower(int teamId)
    {
        return (teamId == TeamController.TEAM1) ? towers[1] : towers[0];
    }


    public void SpawnOffensive(int troopId, int spawnId, int teamId)
    {
        GameObject troopPrefab = GetTroopFromId(troopId);
        GameObject lane = GetSpawnFromId(spawnId);
        GameObject targetTower = GetTargetTower(teamId);

        GameObject troop = Instantiate(troopPrefab, lane.transform.position, Quaternion.identity) as GameObject;
        troop.GetComponent<AIController>().target = targetTower;
        NetworkServer.Spawn(troop);
    }
}