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
    private float maxTimeAtScreen = 2.5f;

    //Danger score params
    private const int troopCountDivisor = 8;
    private const float troopTooCloseRatio = 0.55f;
    private const int troopDistanceMultiplyer = 5;

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
        int aiLane = 0;
        int maxDanger = 0;
        for(int lane = 0; lane < 5; lane++)
        {
            int index = GetLaneDangerIndex(lane);
            if (index > maxDanger)
            {
                maxDanger = index;
                aiLane = lane;
            }
        }
        Debug.Log(maxDanger);
        aIActivePlayer = ConvertLaneToPlayerId(aiLane);
    }

    private int GetLaneDangerIndex(int lane)
    {
        int troopCountDanger = GenerateTroopNumberDangerIndex(lane);
        int troopDistanceDanger = GenerateTroopDistanceDangerIndex(lane);
        int index = troopCountDanger + troopDistanceDanger;
        if (index > 10)
        {
            index = 10;
        }
        
        return index;
    }

    private int GenerateTroopNumberDangerIndex(int lane)
    {
        List<GameObject> troops = GetTroopsInLane(lane);
        int troopCount = troops.Count;
        int danger = troopCount / troopCountDivisor;
        if(danger > 5)
        {
            danger = 5;
        }
        return danger;
    }

    private int GenerateTroopDistanceDangerIndex(int lane)
    {
        List<GameObject> troops = GetTroopsInLane(lane);
        int troopCount = troops.Count;
        float totalDistanceToTower = 0;
        float nearestTroopDistance = 0;
        foreach (GameObject troop in troops)
        {
            float distanceRatioToTarget = troop.GetComponent<AIController>().GetDistanceRatioToTarget();
            totalDistanceToTower += distanceRatioToTarget;
            if(distanceRatioToTarget > nearestTroopDistance)
            {
                nearestTroopDistance = distanceRatioToTarget;
            }

        }
        int index = 0;
        if (troopCount > 0)
        {
            float averageDistance = totalDistanceToTower / troopCount;
            index = (int)(averageDistance * troopDistanceMultiplyer);
            if (index > 5)
            {
                index = 5;
            }
            if (nearestTroopDistance > troopTooCloseRatio)
            {
                index = 10;
            }
        }
        return index;
    }

    private int ConvertLaneToPlayerId(int lane)
    {
        return lane * 2 + (id - 1);
    }

    private int ConvertPlayerIdToLane(int PlayerId)
    {
        return (PlayerId - (id - 1))/2;
    }

    private bool CheckIfNoTroopsPresent()
    {
        int lane = ConvertPlayerIdToLane(aIActivePlayer);
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