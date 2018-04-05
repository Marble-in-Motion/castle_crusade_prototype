using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class AIController : MonoBehaviour {

    private NavMeshAgent agent;

	[SerializeField]
	private GameObject[] targets;

	private int targetTeamId;
	private int troopType;
    public int TroopType
    {
        get
        {
            return troopType;
        }
        set
        {
            troopType = TroopType;
            agent.speed = Params.NPC_SPEED[troopType];
        }
    }

    private int path;
    public int Path
    {
        get
        {
            return path;
        }
        set
        {
            if (Path >= 0 && Path <= 2)
            {
                path = Path;
            }
        }
    }
    private float spawnToTargetDistance;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start() {
        if (GetComponent<Animation>() != null)
        {
            GetComponent<Animation>().Play("run");
        }
        else
        {
            GetComponent<Animator>().SetTrigger("Run");
        }
	}

	public void SetTarget(int targetTeamId)
    {
        Transform target = targets[targetTeamId - 1].transform;
        agent.SetDestination(target.position);
        spawnToTargetDistance = Vector3.Distance(transform.position, target.position);
    }

    public float GetDistanceRatioToTarget()
	{
		float currentDistanceToTarget = Vector3.Distance (transform.position, targets[targetTeamId - 1].transform.position);
		float temp = 1 - (currentDistanceToTarget / spawnToTargetDistance);
		return temp;
    }


}
