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

	private GameObject GetSpawnFromId(int spawnId, int teamId)
	{
		return (teamId == TeamController.TEAM1) ? spawnLocations[spawnId + 5] : spawnLocations[spawnId];
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

	private float calculateAngle(int path)
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

	public Vector3 calculateSpawn(int path, int spawnId, int teamId)
	{
		GameObject lane = GetSpawnFromId(spawnId, teamId);
		GameObject target = GetTargetTower(teamId);

		float angle = calculateAngle(path);
		return ApplyOffset (lane, target, angle);
	}


	public void SpawnOffensive(int troopId, int spawnId, int teamId)
	{
		int path = Random.Range(0, numberOfPaths);
		Vector3 spawn = calculateSpawn (path, spawnId, teamId);
		GameObject lane = GetSpawnFromId(spawnId, teamId);

		GameObject troop = Instantiate(troopPrefabs[troopId], spawn, lane.transform.rotation) as GameObject;
		troop.GetComponent<AIController>().SetTagName(string.Format("NPCT{0}L{1}", teamId, spawnId + 1));
		troop.GetComponent<AIController> ().SetTroopType (troopId);
		troop.GetComponent<AIController>().SetPath(path);

		int opponentTeamIndex = (teamId == 1) ? 2 : 1;

		troop.GetComponent<AIController>().SetTargetIndex(opponentTeamIndex - 1);
        NetworkServer.Spawn(troop);
	}
}
