using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class NPCHealth : NetworkBehaviour {

    private const float ARROW_START_DELAY = 0.2f;

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
        Debug.Log(this.GetComponentInParent<NavMeshAgent>().speed);
		float flightTime = distance / (boltSpeed - this.GetComponentInParent<NavMeshAgent>().speed) - ARROW_START_DELAY;
		return flightTime;
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
            RpcTriggerDeath();
			StartCoroutine(DeathDelay(boltSpeed, crossBowPosition));
		}
	}

    [ClientRpc]
    private void RpcTriggerDeath()
    {
        GetComponent<Animator>().SetTrigger("RoundKick");
    }

    public float GetHealth() {
		return currentHealth;
	}

	public bool IsAlive() {
		return this.currentHealth > 0;
	}

}
