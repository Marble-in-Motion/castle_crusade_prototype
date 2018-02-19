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

    [SyncVar]
    public int coin;

	[SyncVar]
	private float towerHealth;

	private float nextActionTime = 0.0f;

	[SyncVar]
	private GameController.GameState gameOverValue;

    [SyncVar]
    private int coinsPerSecond = Params.STARTING_COIN_INCREASE_AMOUNT; 

    void Start()
    {
		gameOverValue = 0;
		towerHealth = Params.STARTING_TOWER_HEALTH;
    }

    public int GetId()
    {
        return id;
    }

    [Command]
    public void CmdIncreaseCoinPerInterval(int increase)
    {
        coinsPerSecond += increase;
    }

    [Command]
    public void CmdResetCoinPerInterval()
    {
        coinsPerSecond = 5;
    }


    private float GetTowerHealth()
    {
        return towerHealth;
    }

	public float GetTowerHealthRatio(){
		return GetTowerHealth() / Params.STARTING_TOWER_HEALTH;
	}

    public int GetCoin()
    {
        return coin;
    }

	public GameController.GameState GetIsGameOver() {
		return gameOverValue;
	}

	public void SetGameOver(GameController.GameState gameOverValue) {
		this.gameOverValue = gameOverValue;
	}

    [ClientCallback]
    public bool SpendGold(int amount)
    {
        if (coin - amount >= 0)
        {
            if (isServer)
            {
                coin -= amount;
            }
            return true;
        }
        return false;
    }

    [ClientCallback]
    public void AddGold(int amount)
    {
        coin += amount;
    }

    
    private void AddCoinPerSecond()
    {
        if (!isServer) return;

        if (Time.time > nextActionTime)
        {
			nextActionTime += Params.COIN_DELAY;
            AddGold(coinsPerSecond);
        }
    }

    void Update()
    {
        AddCoinPerSecond();

    }

	[Command]
	public void CmdDeductTowerHealth(int damage)  {
		towerHealth = towerHealth - damage;
		if (towerHealth <= 0) {
			TellGameControllerGameOver();
		}
	}
		
	private void TellGameControllerGameOver() {
		GameController gameController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>();
		gameController.GameIsOver(id);
	}

    public void Restart()
    {
        towerHealth = Params.STARTING_TOWER_HEALTH;
        coin = Params.STARTING_COIN;
    }
}