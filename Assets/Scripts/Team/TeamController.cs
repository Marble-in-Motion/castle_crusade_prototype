using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class TeamController : NetworkBehaviour
{
    public const string TEAM_CONTROLLER_1_TAG = "TeamController1";
    public const string TEAM_CONTROLLER_2_TAG = "TeamController2";

    public const int TEAM1 = 1;
    public const int TEAM2 = 2;

    private int coinsPerSecond = Params.STARTING_COIN_INCREASE_AMOUNT;
    private float towerHealth;
    private float nextActionTime = 0.0f;

    [SerializeField]
    private AudioSource seigeAudio;

    [SerializeField]
    private int id;
    public int Id
    {
        get
        {
            return id;
        }
    }

    private int coin = Params.STARTING_COIN;
    public int Coin
    {
        get
        {
            return coin;
        }
    }
    
	private GameController.GameState gameOverState;
    public GameController.GameState GameOverState
    {
        get
        {
            return gameOverState;
        }
        set
        {
            gameOverState = GameOverState;
        }
    }

    private float endOfCoolDown;
    public float EndOfCoolDown
    {
        get
        {
            return endOfCoolDown;
        }
    }

    private float currentTime;
    public float CurrentTime
    {
        get
        {
            return currentTime;
        }
    }

    void Start()
    {
		gameOverState = GameController.GameState.GAME_RESTART;
		towerHealth = Params.STARTING_TOWER_HEALTH;
        
        endOfCoolDown = Time.time;
        currentTime = Time.time;
    }

    void Update()
    {
        AddCoinPerSecond();
        currentTime = Time.time;
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

    public void UpdateCoolDown()
    {
        endOfCoolDown = Time.time + Params.DESTROY_COOL_DOWN;
    }

    public void IncreaseCoinPerInterval(int increase)
    {
        coinsPerSecond += increase;
    }

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

    public void AddGold(int amount)
    {
        coin += amount;
    }

    public void DeductTowerHealth(int damage)
    {
        towerHealth = towerHealth - damage;
        seigeAudio.PlayOneShot(seigeAudio.clip);

        if (towerHealth <= 0)
        {
            TellGameControllerGameOver();
        }
    }

    public float GetTowerHealthRatio()
    {
        return towerHealth / Params.STARTING_TOWER_HEALTH;
    }
       
		
	private void TellGameControllerGameOver() {
		GameController gameController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>();
		gameController.GameIsOver(id);
	}

    public void Restart()
    {
        towerHealth = Params.STARTING_TOWER_HEALTH;
        coin = Params.STARTING_COIN;
        coinsPerSecond = 5;
    }
}