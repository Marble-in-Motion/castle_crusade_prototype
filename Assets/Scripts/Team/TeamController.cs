﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TeamController : NetworkBehaviour
{

    public const int TEAM1 = 1;
    public const int TEAM2 = 2;

    [SerializeField]
    private int id;

    private List<int> playerIds;

    [SyncVar]
    private int coin;

    private int towerHealth;

    private float nextActionTime = 0.0f;
    public float period = 1.0f;

    void Start()
    {
        playerIds = new List<int>();
        coin = 100;
        towerHealth = 100;
    }

    public int GetId()
    {
        return id;
    }

    public int GetCoin()
    {
        return coin;
    }

    [ClientCallback]
    public void SpendGold(int amount)
    {
        if (coin - amount >= 0)
        {
            coin -= amount;
        }
    }

    public void CmdAddPlayer(int playerId)
    {
        playerIds.Add(playerId);
    }

    public bool HasPlayer(int playerId)
    {
        return playerIds.Contains(playerId);
    }

    [ClientCallback]
    public void AddGold(int amount)
    {
        coin += amount;
    }

    void Update()
    {
        if (Time.time > nextActionTime)
        {
            nextActionTime += period;
            AddGold(1);
        }
    }


}