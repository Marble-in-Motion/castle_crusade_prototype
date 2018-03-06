using UnityEngine;
using UnityEngine.Networking;

public class Manager : NetworkManager
{
	public override void OnServerConnect(NetworkConnection conn)
	{
		Debug.Log("Server connected");
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		Debug.Log ("Client connected");
		this.GetComponentInParent<NetworkManagerHUD> ().showGUI = false;
	}

	public override void OnServerReady(NetworkConnection conn)
	{
		Debug.Log ("Ready");
		this.GetComponentInParent<NetworkManagerHUD> ().showGUI = false;
	}
}