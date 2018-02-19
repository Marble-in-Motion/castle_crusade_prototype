using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class TeamController : NetworkBehaviour
{

    public const int TEAM1 = 1;
    public const int TEAM2 = 2;

    public const float STARTING_HEALTH = 100.0f;
    public const int STARTING_COIN = 200;

    [SerializeField]
    private int id;

    [SyncVar]
    public int coin;

	[SyncVar]
	public float towerHealth;

	private float initialTowerHealth;
	private float nextActionTime = 0.0f;
    private float secondsToCoinIncrease = 1.0f;

	[SyncVar]
	private GameController.GameState gameOverValue;

    void Start()
    {
		gameOverValue = 0;
		initialTowerHealth = towerHealth;
    }

    public int GetId()
    {
        return id;
    }
    
    public float GetTowerHealth()
    {
        return towerHealth;
    }

	public float GetInitialTowerHealth() {
		return initialTowerHealth;
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
            nextActionTime += secondsToCoinIncrease;
            AddGold(5);
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
        towerHealth = STARTING_HEALTH;
        coin = STARTING_COIN;
    }
}