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

    private List<int> playerIds;

    [SyncVar]
    private int coin;

	[SyncVar]
	public float towerHealth;

	private float nextActionTime = 0.0f;
    private float secondsToCoinIncrease = 1.0f;

	[SyncVar]
	private int isGameOver;

    void Start()
    {
        playerIds = new List<int>();
        coin = 100;
        towerHealth = 100f;
		isGameOver = 0;
    }

    public int GetId()
    {
        return id;
    }

    public int GetCoin()
    {
        return coin;
    }

	public int GetIsGameOver() {
		return isGameOver;
	}

	public void SetGameOver() {
		isGameOver = 1;
	}

    [ClientCallback]
    public bool SpendGold(int amount)
    {
        if (coin - amount >= 0)
        {
            coin -= amount;
            return true;
        }
        return false;
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
            nextActionTime += secondsToCoinIncrease;
            AddGold(1);
        }

    }

	[ClientCallback]
	public void DeductTowerHealth(int damage)  {
		towerHealth = towerHealth - damage;
		if (towerHealth <= 0) {
			Debug.Log ("Tower health " + id + " is 0");
			TellGameControllerGameOver ();
		}
	}
		
	private void TellGameControllerGameOver() {
		GameController gameController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>();
		gameController.GameIsOver();
	}
}