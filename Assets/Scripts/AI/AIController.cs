using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class AIController : NetworkBehaviour {

    private NavMeshAgent agent;

	[SerializeField]
	private Transform[] targets;

	[SyncVar]
	private int targetIndex;

    [SyncVar]
    private String tagName;

	private float spawnToTargetDistance;

    void Start() {
        GetComponent<Animator>().SetTrigger("Run");
        tag = tagName;
		agent = GetComponent<NavMeshAgent>();
		Transform target = targets[targetIndex].transform;
		agent.SetDestination(target.position);
		spawnToTargetDistance = Vector3.Distance(transform.position, target.position);   
	}

    public String GetTagName()
    {
        return tagName;
    }

    public void SetTagName(String tagName)
    {
        this.tagName = tagName;
    }

	public void SetTargetIndex(int targetIndex){
		this.targetIndex = targetIndex;
	}

    public float GetDistanceRatioToTarget()
	{
		float currentDistanceToTarget = Vector3.Distance (transform.position, targets[targetIndex].position);
		float temp = 1 - (currentDistanceToTarget / spawnToTargetDistance);
		return temp;
    }


}
