using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NPCHealth : NetworkBehaviour {

	[SerializeField]
	private float initialHealth;

	[SyncVar]
	private float currentHealth;

	void Start() {
		currentHealth = initialHealth;
	}
		

	public void DeductHealth(float damage) {
		currentHealth -= damage;

		if (!IsAlive ()) {
			Destroy (gameObject);
		}
	}

	public bool IsAlive() {
		return this.currentHealth > 0;
	}

}
