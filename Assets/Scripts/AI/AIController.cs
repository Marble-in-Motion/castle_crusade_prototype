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

	[SyncVar]
	private int troopType;

    [SyncVar]
    public int path;

    private float spawnToTargetDistance;

    void Start() {
        if (GetComponent<Animation>() != null)
        {
            GetComponent<Animation>().Play("run");
        }
        else
        {
            GetComponent<Animator>().SetTrigger("Run");
        }
        tag = tagName;
		agent = GetComponent<NavMeshAgent>();
		agent.speed = Params.NPC_SPEED [troopType];
		Transform target = targets[targetIndex].transform;
		agent.SetDestination(target.position);
		spawnToTargetDistance = Vector3.Distance(transform.position, target.position);
	}

    public void SetPath(int path)
    {
        if (path >= 0 && path <= 2)
        {
            this.path = path;
        }
    }

    public int GetPath()
    {
        return path;
    }

    public String GetTagName()
    {
        return tagName;
    }

    public void SetTagName(String tagName)
    {
        this.tagName = tagName;
    }

	public int GetTroopType()
	{
		return troopType;
	}

	public void SetTroopType(int type)
	{
		troopType = type;
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
