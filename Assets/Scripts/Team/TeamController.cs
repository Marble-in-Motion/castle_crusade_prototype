using System.Collections;
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
	public float towerHealth;

	private float nextActionTime = 0.0f;
    private float secondsToCoinIncrease = 1.0f;

	[SyncVar]
	private int gameOverValue;

    void Start()
    {
		gameOverValue = 0;
    }

    public int GetId()
    {
        return id;
    }
    
    public float GetTowerHealth()
    {
        return towerHealth;
    }

    public int GetCoin()
    {
        return coin;
    }

	public int GetIsGameOver() {
		return gameOverValue;
	}

	public void SetGameOver(int gameOverValue) {
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
            nextActionTime += secondsToCoinIncrease;
            AddGold(1);
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
			Debug.Log ("Tower health " + id + " is 0");
			TellGameControllerGameOver ();
		}
	}
		
	private void TellGameControllerGameOver() {
		GameController gameController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>();
		gameController.GameIsOver(id);
	}
}