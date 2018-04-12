using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SpawnController : NetworkBehaviour
{
	public const string SPAWN_CONTROLLER_TAG = "SpawnController";
	public const int maxOffset = 25;
	public const int numberOfPaths = 3;

	//troops
	[SerializeField]
	private GameObject[] troopPrefabs;

	//spawn location - lanes
	[SerializeField]
	private GameObject[] spawnLocations;

	//targets
	[SerializeField]
	private GameObject[] towers;

    private GameObject GetMyLocalSpawn(int laneId, int myTeamId)
    {
        return (myTeamId == TeamController.TEAM1)
            ? spawnLocations[laneId]
            : spawnLocations[laneId + 5];
    }

    private GameObject GetOpponentsSpawn(int laneId, int myTeamId)
    {
        return (myTeamId == TeamController.TEAM1)
            ? spawnLocations[laneId + 5]
            : spawnLocations[laneId];
    }

    private GameObject GetMyTower(int myTeamId)
    {
        return (myTeamId == TeamController.TEAM1) ? towers[0] : towers[1];
    }

    private GameObject GetOpponentsTower(int myTeamId)
    {
        return (myTeamId == TeamController.TEAM1) ? towers[1] : towers[0];
    }

	private Vector3 ApplyOffset(GameObject lane, GameObject tower, float theta)
	{
        GameObject newSpawn = new GameObject();
        newSpawn.transform.position = lane.transform.position;
        newSpawn.transform.RotateAround(tower.transform.position, Vector3.up, theta);

        Vector3 v = newSpawn.transform.position;
        Destroy(newSpawn);
        return v;
  	}

	private float CalculateAngle(int path)
	{
		float theta = 0;
		int pathWidth = 10;
		switch (path) {
			case 0:
					theta = Random.Range(-maxOffset, -maxOffset + pathWidth);
					break;
			case 1:
					theta = Random.Range(-maxOffset + 2*pathWidth , -maxOffset + 3*pathWidth);
					break;
			case 2:
					theta = Random.Range(-maxOffset + 4*pathWidth, -maxOffset + 5*pathWidth);
					break;
		}
		return theta;
	}

    // Calculate default look location for crossbow
	public Vector3 CalculateDefaultCrossbowTarget(int path, int laneId, int myTeamId)
	{
		GameObject lane = GetMyLocalSpawn(laneId, myTeamId);
		GameObject myTower = GetMyTower(myTeamId);

		float theta = 0;
		int pathWidth = 10;
		switch (path) {
		case 0:
			theta = -maxOffset + (-maxOffset + pathWidth)/2;
			break;
		case 1:
			theta = -maxOffset + 2*pathWidth + (-maxOffset + 3*pathWidth)/2;
			break;
		case 2:
			theta = -maxOffset + 4*pathWidth +  (-maxOffset + 5*pathWidth)/2;
			break;
		}
		return ApplyOffset(lane, myTower, theta);
	}

	public Vector3 CalculateSpawn(GameObject lane, int path, int myTeamId)
	{
		GameObject targetTower = GetOpponentsTower(myTeamId);

		float angle = CalculateAngle(path);
		return ApplyOffset(lane, targetTower, angle);
	}

    public void SpawnOffensiveTroop(int troopId, int laneId, int myTeamId, int opponentsTeamId)
	{
		int path = Random.Range(0, numberOfPaths);
        GameObject lane = GetOpponentsSpawn(laneId, myTeamId);
        Vector3 spawn = CalculateSpawn(lane, path, myTeamId);

		GameObject troop = Instantiate(troopPrefabs[troopId], spawn, lane.transform.rotation) as GameObject;
        NetworkServer.Spawn(troop);

        AIController ai = troop.GetComponent<AIController>();
        ai.RpcSetTeamId(myTeamId);
        ai.RpcSetLaneId(laneId);
        ai.RpcSetTroopType(troopId);
        ai.RpcSetPath(path);
        ai.RpcSetTarget(opponentsTeamId);
    }


}
