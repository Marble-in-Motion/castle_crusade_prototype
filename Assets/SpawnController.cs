using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SpawnController : NetworkBehaviour
{
    public const string SPAWN_CONTROLLER_TAG = "SpawnController";

    [SerializeField]
    GameObject[] troopPrefabs;

    [SerializeField]
    GameObject[] spawnLocations;

    [SerializeField]
    GameObject[] towers;

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
        return (teamId == 1) ? towers[1] : towers[0];
    }

    //spawn troops
    public void SpawnOffensive(int troopId, int laneId, int teamId)
    {
        GameObject troopPrefab = GetTroopFromId(troopId);
        GameObject lane = GetSpawnFromId(laneId);
        GameObject targetTower = GetTargetTower(teamId);

        GameObject obj = Instantiate(troopPrefab, lane.transform.position, Quaternion.identity) as GameObject;
        //set target

        NetworkServer.Spawn(obj);
    }
}
