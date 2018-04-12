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

    public enum TeamResult { UNDECIDED, LOST, WON }

    private int coinsPerSecond = Params.STARTING_COIN_INCREASE_AMOUNT;
    private float towerHealth;
    private float nextActionTime = 0.0f;

    private float timeToScreenCheck = 0;
    private float maxTimeAtScreen = 5;

    private bool teamAIEnabled = false;
    public bool TeamAIEnabled
    {
        get
        {
            return teamAIEnabled;
        }
    }

    public void SetTeamAIEnabled(bool state)
    {
        teamAIEnabled = state;
    }

    private int aIActivePlayer = 0;
    public int AIActivePlayer
    {
        get
        {
            return aIActivePlayer;
        }
    }

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
    
	private TeamResult result;
    public TeamResult Result
    {
        get
        {
            return result;
        }
    }

    // Enum must use setter method 
    public void SetTeamResult(TeamResult teamResult)
    {
        result = teamResult;
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

    private int screenshotCount;
    public int ScreenshotCount
    {
        get
        {
            return screenshotCount;
        }
    }

    public void IncrementScreenshotCount()
    {
        screenshotCount++;
    }

    private int lastActivePlayerId;
    public int LastActivePlayerId
    {
        get
        {
            return lastActivePlayerId;
        }
    }

    public void SetLastActivePlayerId(int playerId)
    {
        lastActivePlayerId = playerId;
    }

    void Start()
    {
        result = TeamResult.UNDECIDED;
		towerHealth = Params.STARTING_TOWER_HEALTH;
        endOfCoolDown = Time.time;
        currentTime = Time.time;
        lastActivePlayerId = -1;
    }

    void Update()
    {
        AddCoinPerSecond();
        currentTime = Time.time;
        if (teamAIEnabled)
        {
            CheckChangeAI();
        }
        
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

    private void CheckChangeAI()
    {
        if(Time.time > timeToScreenCheck)
        {
            UpdateAIActive();
            timeToScreenCheck = Time.time + maxTimeAtScreen;
        }
        else if (CheckIfNoTroopsPresent())
        {
            UpdateAIActive();
            timeToScreenCheck = Time.time + maxTimeAtScreen;
        }
    }

    private void UpdateAIActive()
    {
        int aIPlayer = 0;
        int maxTroops = int.MinValue;
        for(int lane = 0; lane < 5; lane++)
        {
            List<GameObject> troops = GetTroopsInLane(lane);
            int troopCount = troops.Count;
            if(troopCount > maxTroops)
            {
                maxTroops = troopCount;
                aIPlayer = lane;
            }
        }

        aIActivePlayer = aIPlayer + (id-1)*5;
        Debug.Log(aIActivePlayer);
    }

    private bool CheckIfNoTroopsPresent()
    {
        int lane = aIActivePlayer - (id - 1) * 5;
        int troops = GetTroopsInLane(lane).Count;
        return (troops == 0);
    }

    private List<GameObject> GetTroopsInLane(int laneId)
    {
        List<GameObject> troopsInLane = new List<GameObject>();
        GameObject[] allTroops = GameObject.FindGameObjectsWithTag(GameController.NPC_TAG);
        for (int i = 0; i < allTroops.Length; i++)
        {
            AIController ai = allTroops[i].GetComponent<AIController>();
            if (ai.OpposingTeamId == id && ai.LaneId == laneId)
            {
                troopsInLane.Add(allTroops[i]);
            }
        }
        return troopsInLane;
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
            GameController gameController = GameObject.FindGameObjectWithTag(GameController.GAME_CONTROLLER_TAG).GetComponent<GameController>();
            gameController.GameIsOver(id);
        }
    }

    public float GetTowerHealthRatio()
    {
        return towerHealth / Params.STARTING_TOWER_HEALTH;
    }
       
    public void Restart()
    {
        towerHealth = Params.STARTING_TOWER_HEALTH;
        coin = Params.STARTING_COIN;
        coinsPerSecond = 5;
    }
}