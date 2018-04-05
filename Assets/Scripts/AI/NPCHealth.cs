using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class NPCHealth : NetworkBehaviour {

    private const float ARROW_START_DELAY = 0.2f;
    private const string DEATH_TRIGGER = "Die";
    private const float ANIM_WAIT = 5.0f;

	
	private float currentHealth;
    public float CurrentHealth
    {
        get
        {
            return currentHealth;
        }
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }


    void Start() {
		currentHealth = Params.NPC_HEALTH[GetComponentInParent<AIController>().TroopType];
	}

	private float CalculateFlightTime(int boltSpeed, Vector3 crossBowPosition)
	{
		Vector3 troopPostion = this.transform.position;
		float distance = Vector3.Distance(troopPostion, crossBowPosition);
        float flightTime;
        try
        {
            flightTime = distance / (boltSpeed - this.GetComponentInParent<NavMeshAgent>().speed) - ARROW_START_DELAY;
        }
        catch
        {
            flightTime = 0;
        }
		return flightTime;
	}

	private IEnumerator DeathDelay(int boltSpeed, Vector3 crossBowPosition)
	{
		float wait = CalculateFlightTime(boltSpeed, crossBowPosition) ;
        wait = wait + ANIM_WAIT;
		yield return new WaitForSeconds(wait);
		Destroy(gameObject);

        // This may need to be NetworkServer.Destory
	}

    [Command]
	public void CmdDeductHealth(float damage, int boltSpeed, Vector3 crossBowPosition) {
		currentHealth -= damage;

		if (!IsAlive()) {
            RpcKill();
            StartCoroutine(DeathDelay(boltSpeed, crossBowPosition));
		}
	}

    [ClientRpc]
    private void RpcKill()
    {
        if (GetComponent<Animation>() != null)
        {
            GetComponent<Animation>().Play("die");
        }
        else
        {
            GetComponent<Animator>().SetTrigger(DEATH_TRIGGER);
        }
        Destroy(GetComponent<BoxCollider>());
        Destroy(GetComponent<NavMeshAgent>());
    }
    
}
