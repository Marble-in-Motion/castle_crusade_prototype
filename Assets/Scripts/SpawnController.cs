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

	private Vector3 ApplyOffset(GameObject lane) {
		float xOffset = Random.Range (-maxOffset, maxOffset);
		Vector3 offset = new Vector3 (xOffset, 0, 0);
		float yRot = lane.transform.rotation.y * Mathf.Rad2Deg;
		offset = Quaternion.Euler(0,yRot,0) * offset;
		Vector3 newPos = lane.transform.position + offset;
		return newPos;
	}
		
	public void SpawnOffensive(int troopId, int spawnId, int teamId)
	{
		GameObject lane = GetSpawnFromId(spawnId, teamId);

		GameObject troop = Instantiate(troopPrefabs[troopId], ApplyOffset(lane), Quaternion.identity) as GameObject;
		troop.GetComponent<AIController>().SetTagName(string.Format("NPCT{0}L{1}", teamId, spawnId + 1));
		int opponentTeamIndex = (teamId == 1) ? 2 : 1;
		Debug.Log (opponentTeamIndex);
		troop.GetComponent<AIController>().SetTargetIndex(opponentTeamIndex - 1);
        NetworkServer.Spawn(troop);
	}
}
