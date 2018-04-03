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
	private GameObject [] troopPrefabs;

	//spawn location - lanes
	[SerializeField]
	private GameObject[] spawnLocations;

	//targets
	[SerializeField]
	private GameObject[] towers;

	private GameObject GetSpawnFromId(int laneId, int teamId)
	{
		return (teamId == TeamController.TEAM1)
            ? spawnLocations[laneId + 5]
            : spawnLocations[laneId];
	}

	private GameObject GetTargetTower(int teamId)
	{
		return (teamId == TeamController.TEAM1) ? towers[1] : towers[0];
	}

	private Vector3 ApplyOffset(GameObject lane, GameObject target, float theta)
	{

        GameObject newSpawn = new GameObject();
        newSpawn.transform.position = lane.transform.position;
        newSpawn.transform.RotateAround(target.transform.position, Vector3.up, theta);

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
			default:
					break;
		}
		return theta;
	}

	public Vector3 calculateDefaultSpawn(int path, int laneId, int teamId)
	{
		GameObject lane = GetSpawnFromId(laneId, teamId);
		GameObject target = GetTargetTower(teamId);

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
		default:
			break;
		}
		return ApplyOffset (lane, target, theta);
	}

	public Vector3 CalculateSpawn(int path, int laneId, int teamId)
	{
		GameObject lane = GetSpawnFromId(laneId, teamId);
		GameObject target = GetTargetTower(teamId);

		float angle = CalculateAngle(path);
		return ApplyOffset (lane, target, angle);
	}


	public void SpawnOffensive(int troopId, int laneId, int teamId)
	{
		int path = Random.Range(0, numberOfPaths);
		Vector3 spawn = CalculateSpawn (path, laneId, teamId);
		GameObject lane = GetSpawnFromId(laneId, teamId);

		GameObject troop = Instantiate(troopPrefabs[troopId], spawn, lane.transform.rotation) as GameObject;
		troop.GetComponent<AIController>().SetTagName(string.Format("NPCT{0}L{1}", teamId, laneId));
		troop.GetComponent<AIController> ().SetTroopType (troopId);
		troop.GetComponent<AIController>().SetPath(path);

		int opponentTeamIndex = (teamId == 1) ? 2 : 1;

		troop.GetComponent<AIController>().SetTargetIndex(opponentTeamIndex - 1);
        NetworkServer.Spawn(troop);
	}
}
