using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class NPCHealth : NetworkBehaviour {

    private const float ARROW_START_DELAY = 0.2f;
    private const string DEATH_TRIGGER = "Die";
    private const float ANIM_WAIT = 5.0f;

	[SyncVar(hook = "OnChangeHealth")]
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

	public void DeductHealth(float damage) {
		currentHealth = currentHealth - damage;
    }

    private void OnChangeHealth(float currentHealth)
    {
        this.currentHealth = currentHealth;

        if (!IsAlive())
        {
            StartCoroutine(DestroyTroop());
        }
    }

    private IEnumerator DestroyTroop()
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

        yield return new WaitForSeconds(3);
        Destroy(gameObject);
    }


}
