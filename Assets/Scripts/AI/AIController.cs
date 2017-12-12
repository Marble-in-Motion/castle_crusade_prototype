using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class AIController : NetworkBehaviour {

    private NavMeshAgent agent;

    //[SerializeField]
    public GameObject target;



    void Start () {
        agent = GetComponent<NavMeshAgent>();
        SetTarget();
    }


    private void SetTarget()
    {
        agent.SetDestination(target.transform.position);
     
    }
	
}
