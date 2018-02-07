using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class AIController : NetworkBehaviour {

    private NavMeshAgent agent;

    //[SerializeField]
    public GameObject target;

    [SyncVar]
    public String tagName;

    public int lane;

    public int team;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        this.tag = tagName;
        SetTarget();
    }

    void Update()
    {
        //Debug.Log(GetDistanceToTarget());
    }


    private void SetTarget()
    {
        agent.SetDestination(target.transform.position);
    }
	
    public float GetDistanceToTarget()
    {
        return Vector3.Distance(transform.position, target.transform.position);
    }

}
