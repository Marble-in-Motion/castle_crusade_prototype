using UnityEngine;
using UnityEngine.Networking;

public class Spawner : NetworkBehaviour {

    [SerializeField]
    public GameObject prefab;

    [SerializeField]
    public GameObject spawnLocation;

    [SerializeField]
    public int spawnNumber;


    public override void OnStartServer()
    {
        for (int i=0; i < spawnNumber; i++)
        {
            GameObject obj = GameObject.Instantiate(prefab, spawnLocation.transform.position, Quaternion.identity) as GameObject;
            NetworkServer.Spawn(obj);
        }
    }

	[Command]
	public void CmdSpawn(int number, GameObject enemyPrefab)
	{
		for (int i=0; i < number; i++)
		{
			GameObject obj = GameObject.Instantiate(enemyPrefab, spawnLocation.transform.position, Quaternion.identity) as GameObject;
			NetworkServer.Spawn(obj);
		}
	}
}
