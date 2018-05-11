using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;


/**
 * Health controller for NPC units
 **/
public class NPCHealth : NetworkBehaviour
{

    private const float ARROW_START_DELAY = 0.2f;
    private const string DEATH_TRIGGER = "Die";
    private const float ANIM_WAIT = 5.0f;

    [SerializeField]
    private GameObject fire;
    private bool fireFlag;

    [SyncVar(hook = "OnChangeHealth")]
    private float currentHealth;
    public float CurrentHealth
    {
        get
        {
            return currentHealth;
        }
    }

    public void SetHealth(float health)
    {
        this.currentHealth = health;
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    void Start()
    {
        fireFlag = true;
    }

    public void DeductHealth(float damage)
    {
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
        if (GetComponent<AIController>().TroopType == 0)
        {
            GetComponent<Animation>().Play("die");
            Destroy(GetComponent<NavMeshAgent>());
        }
        else if (GetComponent<AIController>().TroopType == 1 && fireFlag)
        {
            Destroy(GetComponent<NavMeshAgent>());
            GetComponent<Animator>().StopPlayback();
            Instantiate(fire, transform.position, Quaternion.identity);
            fireFlag = false;
        }

        yield return new WaitForSeconds(3);

        Destroy(GetComponent<BoxCollider>());
        Destroy(gameObject);

    }

}
