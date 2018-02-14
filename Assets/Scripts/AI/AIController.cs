using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class AIController : NetworkBehaviour {

    private NavMeshAgent agent;

	public Transform[] targets;

	[SyncVar]
	private Transform target;

    private float spawnToTargetDistance;

    [SyncVar]
    private String tagName;

    void Start() {
        this.tag = tagName;
		agent = GetComponent<NavMeshAgent>();

    }

    public void SetTarget(GameObject targetTower)
    {
		for (int i = 0; i < targets.Length; i++) {
			if (targetTower.transform == targets [i]) {
				target = targets [i].transform;
				agent.SetDestination(target.position);
				spawnToTargetDistance = Vector3.Distance(transform.position, target.position);
			}
		}
	}

    public String GetTagName()
    {
        return tagName;
    }

    public void SetTagName(String tagName)
    {
        this.tagName = tagName;
    }

    public float GetDistanceRatioToTarget()
	{	if (target != null) {
			Debug.Log ("target is not null");
			float currentDistanceToTarget = Vector3.Distance (transform.position, target.position);
			float temp = 1 - (currentDistanceToTarget / spawnToTargetDistance);
			return temp;
		} else {
			Debug.Log ("target is null");
		}
		return 0;
    }

}
