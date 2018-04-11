using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class AIController : NetworkSetup {

    [SerializeField]
    private string AIName;

    private NavMeshAgent agent;

	[SerializeField]
	private GameObject[] targets;

    private float spawnToTargetDistance;
    private GameObject target;

    private int troopType;
    public int TroopType
    {
        get
        {
            return troopType;
        }
    }

    private int path;
    public int Path
    {
        get
        {
            return path;
        }
    }

    private int laneId;
    public int LaneId
    {
        get
        {
            return laneId;
        }
    }

    private int teamId;
    public int TeamId
    {
        get
        {
            return teamId;
        }
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start() {
        RegisterModel(AIName);
        if (GetComponent<Animation>() != null)
        {
            GetComponent<Animation>().Play("run");
        }
        else
        {
            GetComponent<Animator>().SetTrigger("Run");
        }
	}

    public float GetDistanceRatioToTarget()
	{
		float currentDistanceToTarget = Vector3.Distance(transform.position, target.transform.position);
		float temp = 1 - (currentDistanceToTarget / spawnToTargetDistance);
		return temp;
    }

    [ClientRpc]
    public void RpcSetTeamId(int teamId)
    {
        this.teamId = teamId;
    }

    [ClientRpc]
    public void RpcSetLaneId(int laneId)
    {
        this.laneId = laneId;
    }

    [ClientRpc]
    public void RpcSetTroopType(int troopType)
    {
        this.troopType = troopType;
        agent.speed = Params.NPC_SPEED[troopType];
    }

    [ClientRpc]
    public void RpcSetPath(int path)
    {
        if (path >= 0 && path <= 2)
        {
            this.path = path;
        }
    }

    [ClientRpc]
    public void RpcSetTarget(int targetTeamId)
    {
        target = targets[targetTeamId - 1];
        agent.SetDestination(target.transform.position);
        spawnToTargetDistance = Vector3.Distance(transform.position, target.transform.position);
    }

}
