using UnityEngine.Networking;
using UnityEngine;
using System;
using Assets.Scripts.Player;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class AIPlayer : NetworkSetup
{

    enum AICommands { FIND, AIM, KILL }
    private AICommands nextCommand = AICommands.FIND;
    private GameObject AITargetEnemy;
    private float AIActionTime = 0.0f;
    private float AIMoveDelay = 0.5f;
    private float nextAIActionTime = 0;
    private float changeDirectionTime = 0.4f;
    private float timePerShot = 0.2f;
    private float AINextTroopSendTime = 0;
    private int AINextNumberTroopsToSend = 1;

    private Player player;

    // Use this for initialization
    void Start () {
        player = this.gameObject.GetComponent<Player>();
	}
	
	// Update is called once per frame
	void Update () {
        if (player.GetAIEnabled())
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                player.DeactivateAI();
            }
            if (Time.time > AINextTroopSendTime)
            {
                CmdAISendTroops();
            }
            if (Time.time > nextAIActionTime)
            {
                if (nextCommand == AICommands.FIND)
                {
                    AITargetEnemy = FindTarget();
                    if (AITargetEnemy != null)
                    {
                        nextCommand = AICommands.AIM;
                    }
                }
                else if (nextCommand == AICommands.AIM)
                {
                    MoveTowardsTarget();

                }
                else if (nextCommand == AICommands.KILL)
                {
                    KillTarget();
                }
            }
        }
    }


    [Command]
    private void CmdAISendTroops()
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(player.GetId());

        for (int i = 0; i < AINextNumberTroopsToSend; i++)
        {
            player.CmdRequestOffensiveTroopSpawn(0, player.GetLaneId());
        }
        System.Random rnd = new System.Random();
        int interval = rnd.Next(1, 11);
        AINextTroopSendTime = Time.time + interval;

        int coin = myTeamController.Coin;
        int upperBound = coin / 40;
        AINextNumberTroopsToSend = rnd.Next(1, upperBound);
    }

    private GameObject FindTarget()
    {
        List<GameObject> troopsInLane = player.FindEnemyTroopsInLane();
        GameObject target = player.GetCrossbowMotor().AIFindTarget(troopsInLane);
        return target;
    }

    private void MoveTowardsTarget()
    {
        int currentPath = player.GetCrossbowMotor().ActivePath;
        int targetPath = AITargetEnemy.GetComponent<AIController>().Path;
        if (currentPath > targetPath)
        {
            player.GetCrossbowMotor().MoveLeft();
            nextAIActionTime = Time.time + changeDirectionTime;
        }
        else if (currentPath < targetPath)
        {
            player.GetCrossbowMotor().MoveRight();
            nextAIActionTime = Time.time + changeDirectionTime;
        }

        currentPath = player.GetCrossbowMotor().ActivePath;
        if (currentPath == targetPath)
        {
            nextCommand = AICommands.KILL;
        }
    }

    private void KillTarget()
    {
        player.Shoot();
        if (!AITargetEnemy.GetComponent<NPCHealth>().IsAlive())
        {
            nextCommand = AICommands.FIND;
        }
        else
        {
            nextAIActionTime = Time.time + timePerShot;
        }
    }

}

