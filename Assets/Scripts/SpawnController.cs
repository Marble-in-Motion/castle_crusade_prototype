using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SpawnController : NetworkBehaviour
{
	public const string SPAWN_CONTROLLER_TAG = "SpawnController";
	public const int maxOffset = 30;

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

	private Vector3 ApplyOffset(GameObject lane, GameObject target) {
        float theta = Random.Range(-36, 36);

        GameObject newSpawn = new GameObject();
        newSpawn.transform.position = lane.transform.position;
        newSpawn.transform.RotateAround(target.transform.position, Vector3.up, theta);

        Vector3 v = newSpawn.transform.position;
        Destroy(newSpawn);
        return v;
    }
		
	public void SpawnOffensive(int troopId, int spawnId, int teamId)
	{
		GameObject lane = GetSpawnFromId(spawnId, teamId);
        GameObject target = GetTargetTower(teamId);
  
		GameObject troop = Instantiate(troopPrefabs[troopId], ApplyOffset(lane, target), Quaternion.identity) as GameObject;
		troop.GetComponent<AIController>().SetTagName(string.Format("NPCT{0}L{1}", teamId, spawnId + 1));
		troop.GetComponent<AIController> ().SetTroopType (troopId);
		int opponentTeamIndex = (teamId == 1) ? 2 : 1;

		troop.GetComponent<AIController>().SetTargetIndex(opponentTeamIndex - 1);
        NetworkServer.Spawn(troop);
	}
}
