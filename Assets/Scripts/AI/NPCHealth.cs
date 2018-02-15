using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NPCHealth : NetworkBehaviour {

	//    public const float ARROW_START_DELAY = 0.2f;

	[SerializeField]
	private float initialHealth;

	[SyncVar]
	private float currentHealth;

	void Start() {
		currentHealth = initialHealth;
	}

	private float CalculateFlightTime(int boltSpeed, Vector3 crossBowPosition)
	{
		Vector3 troopPostion = this.transform.position;
		float distance = Vector3.Distance(troopPostion, crossBowPosition);
		float flightTime = distance / boltSpeed;
		return flightTime; //- ARROW_START_DELAY;
	}

	IEnumerator DeathDelay(int boltSpeed, Vector3 crossBowPosition)
	{
		float wait = CalculateFlightTime(boltSpeed, crossBowPosition) ;
		yield return new WaitForSeconds(wait);
		Destroy(gameObject);
	}

	public void DeductHealth(float damage, int boltSpeed, Vector3 crossBowPosition) {
		currentHealth -= damage;

		if (!IsAlive ()) {
			StartCoroutine(DeathDelay(boltSpeed, crossBowPosition));
		}
	}

	public float GetHealth() {
		return currentHealth;
	}

	public bool IsAlive() {
		return this.currentHealth > 0;
	}

}
