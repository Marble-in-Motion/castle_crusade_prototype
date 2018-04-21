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
    private float nextAIActionTime = 0;
    private float changeDirectionTime = Params.CHANGE_DIRECTION_TIME;
    private float timePerShot = Params.TIME_PER_SHOT;
    private float AINextTroopSendTime = 0;
    private int AINextNumberTroopsToSend = 1;

    //Set for ability for AI to play AI by sending to random lanes
    private bool randomLaneSend = true;

    private Player player;

    // Use this for initialization
    void Start () {
        player = this.gameObject.GetComponent<Player>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.B))
        {
            player.DeactivateAI();
            player.CmdTeamAIActivate(false);
            nextCommand = AICommands.FIND;
        }
        if (player.GetAIEnabled())
        {
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
        else
        {
            nextCommand = AICommands.FIND;
        }
    }

    [ClientRpc]
    private void RpcSetNextTroopSend(int coin)
    {
        System.Random rnd = new System.Random();
        int interval = rnd.Next(Params.TIME_BETWEEN_TROOP_SEND[0], Params.TIME_BETWEEN_TROOP_SEND[1]);
        AINextTroopSendTime = Time.time + interval;

        int upperBound = (int)(coin / Params.COINS_DIVISOR_FOR_TROOPS_UPPER_BOUND);
        AINextNumberTroopsToSend = rnd.Next(0, upperBound);
    }

    [Command]
    private void CmdAISendTroops()
    {
        TeamController myTeamController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>().GetMyTeamController(player.GetId());
        StartCoroutine(AISendTroopsWithDelay());

        int coin = myTeamController.Coin;
        RpcSetNextTroopSend(coin);
        
    }


    private IEnumerator AISendTroopsWithDelay()
    {
        System.Random rnd = new System.Random();

        int lane = rnd.Next(0, 5);
        for (int i = 0; i < AINextNumberTroopsToSend; i++)
        {
            if (randomLaneSend)
            {
                player.CmdRequestOffensiveTroopSpawn(0, lane);
            }
            else
            {
                player.CmdRequestOffensiveTroopSpawn(0, player.GetLaneId());
            }
            yield return new WaitForSeconds(Params.TROOP_SEND_DELAY_PER_TROOP);
        }
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
        if (AITargetEnemy != null && AITargetEnemy.GetComponent<NPCHealth>().IsAlive())
        {
            player.Shoot();
            nextAIActionTime = Time.time + timePerShot;
        }
        else
        {
            nextCommand = AICommands.FIND;
        }
    }

}

