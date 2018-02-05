using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class CollisionHandler : MonoBehaviour {

	[SerializeField]
	private GameObject teamController;

	private int NPCDamage;

	void OnTriggerEnter(Collider other)
	{
		NPCDamage = 10;
		teamController.GetComponent<TeamController>().CmdDeductTowerHealth(NPCDamage);
		Destroy(other.gameObject);
	}

}