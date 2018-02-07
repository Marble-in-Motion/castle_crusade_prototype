using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class AIController : NetworkBehaviour {

    private NavMeshAgent agent;

    public GameObject target;

    private float spawnToTargetDistance;

    [SyncVar]
    private String tagName;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        this.tag = tagName;
        SetTarget();
        spawnToTargetDistance = Vector3.Distance(transform.position, target.transform.position);
    }

    private void SetTarget()
    {
        agent.SetDestination(target.transform.position);
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
    {
        float currentDistanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        return currentDistanceToTarget / spawnToTargetDistance;
    }

}
