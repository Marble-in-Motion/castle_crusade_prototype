﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TeamController : NetworkBehaviour {

    public const int TEAM1 = 1;
    public const int TEAM2 = 2;

    [SerializeField]
    public int id;

    public List<int> playerIds;

    [SyncVar]
    private int coin;

    private int towerHealth;

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

    public int SpendGold(int amount)
    {
        if (coin - amount > 0)
        {
            coin -= amount;
        }
        return coin;
    }

    public void AddPlayer(int playerId)
    {
        playerIds.Add(playerId);
    }

    public bool HasPlayer(int playerId)
    {
        return playerIds.Contains(playerId);
    }


}
