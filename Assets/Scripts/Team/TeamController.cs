using Assets.Scripts.Team;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TeamController : ITeamController {

    private int id;

    private List<int> playerIds;

    private int coin;

    private int towerHealth;

    public TeamController(int id)
    {
        this.id = id;
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
