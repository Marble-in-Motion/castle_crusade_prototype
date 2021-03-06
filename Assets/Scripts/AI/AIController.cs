﻿using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

/**
 * Controller for NPC units i.e. SpartanKing
 **/
public class AIController : NetworkSetup
{

    [SerializeField]
    private string AIName;

    private NavMeshAgent agent;

    [SerializeField]
    public GameObject[] targets;

    private float spawnToTargetDistance;
    public GameObject target;

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

    private int opposingTeamId;
    public int OpposingTeamId
    {
        get
        {
            return opposingTeamId;
        }
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        RegisterModel(AIName);
        if (GetComponent<Animation>() != null)
        {
            if (GetComponent<Animation>().Play("run") != false)
            {
                GetComponent<Animation>().Play("run");
            }
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
    public void RpcSetOpposingTeamId(int teamId)
    {
        this.opposingTeamId = teamId;
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
        GetComponent<NPCHealth>().SetHealth(Params.NPC_HEALTH[TroopType]);
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
        spawnToTargetDistance = Vector3.Distance(transform.position, target.transform.position);
    }

}
