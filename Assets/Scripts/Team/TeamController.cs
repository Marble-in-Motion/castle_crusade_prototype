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
    private float maxTimeAtScreen = Params.MAX_TIME_AT_SCREEN;

    //Danger score params
    private int troopCountDivisor = Params.TROOP_COUNT_PER_DANGER_INDEX;
    private float troopTooCloseRatio = Params.TROOP_CLOSE_DISTANCE;
    private int troopDistanceMultiplyer = Params.TROOP_RATIO_MULTIPLYER;

    private bool sendTroopAlerting = false;

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

    private int aIActivePlayer2 = 0;
    public int AIActivePlayer2
    {
        get
        {
            return aIActivePlayer2;
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

    private float nextSendTroopAlert;

    private bool playSendTroopAnim = false;
    public bool PlaySendTroopAnim
    {
        get
        {
            return playSendTroopAnim;
        }
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
        nextSendTroopAlert = Time.time + Params.SEND_TROOP_ALERT_DELAY;
        result = TeamResult.UNDECIDED;
		towerHealth = Params.STARTING_TOWER_HEALTH;
        endOfCoolDown = Time.time;
        currentTime = Time.time;
        lastActivePlayerId = -1;
    }

    void Update()
    {
        AddCoinPerSecond();
        SendTroopAlert();

        currentTime = Time.time;
        if (teamAIEnabled)
        {
            CheckChangeAI();
        }
    }

    private void SendTroopAlert()
    {

        if (Time.time > nextSendTroopAlert && sendTroopAlerting == false)
        {
            playSendTroopAnim = true;
            sendTroopAlerting = true;
            
            string playersTag = Player.PLAYER_TAG + " " + id;

            GameObject[] players = GameObject.FindGameObjectsWithTag(playersTag);

            int length = players.Length;

            if(length > 0)
            {
                int playerIndex = Random.Range(0, length);

                Player p = players[playerIndex].GetComponent<Player>();

                p.RpcClientPlayArraySound(Params.MORE_TROOPS, Params.PLAY_RANDOM);
            }
            
        }
    }

    public void ResetSendTroopAlert(int id)
    {
        sendTroopAlerting = false;

        string playersTag = Player.PLAYER_TAG + " " + id;

        GameObject[] players = GameObject.FindGameObjectsWithTag(playersTag);

        playSendTroopAnim = false;

        foreach(GameObject player in players)
        {
            Player p = player.GetComponent<Player>();
            p.RpcResetSendTroopAlert();
        }


        nextSendTroopAlert = Time.time + Params.SEND_TROOP_ALERT_DELAY;

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
        else if (CheckIfNoTroopsPresent(aIActivePlayer) || CheckIfNoTroopsPresent(aIActivePlayer2))
        {
            UpdateAIActive();
            timeToScreenCheck = Time.time + maxTimeAtScreen;
        }
    }

    private void UpdateAIActive()
    {
        int aiLane = 0;
        int aiLane2 = 0;
        int maxDanger = 0;
        int maxDanger2 = 0;
        for (int lane = 0; lane < 5; lane++)
        {
            int index = GetLaneDangerIndex(lane);
            if (index > maxDanger)
            {
                maxDanger = index;
                aiLane = lane;
            }
            else
            {
                if(index > maxDanger2)
                {
                    maxDanger2 = index;
                    aiLane2 = lane;
                }
            }
        }
        aIActivePlayer = ConvertLaneToPlayerId(aiLane);
        aIActivePlayer2 = ConvertLaneToPlayerId(aiLane2);
    }

    public int GetLaneDangerIndex(int lane)
    {
        int troopCountDanger = GenerateTroopNumberDangerIndex(lane);
        int troopDistanceDanger = GenerateTroopDistanceDangerIndex(lane);
        Debug.Log("count: " + troopCountDanger);
        Debug.Log("distance: " + troopDistanceDanger);
        int index = troopCountDanger + troopDistanceDanger;
        if (index > 8)
        {
            index = 8;
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
            index = 1 + (int)(averageDistance * troopDistanceMultiplyer);
            if (index > 5)
            {
                index = 5;
            }
            if (nearestTroopDistance > troopTooCloseRatio)
            {
                index = 8;
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

    private bool CheckIfNoTroopsPresent(int active)
    {
        int lane = ConvertPlayerIdToLane(active);
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
            if (ai.OpposingTeamId == id && ai.LaneId == laneId && allTroops[i].GetComponent<NPCHealth>().IsAlive())
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

    [ClientRpc]
    public void RpcPlaySeigeSound()
    {
            seigeAudio.PlayOneShot(seigeAudio.clip);
    }

    public void DeductTowerHealth(int damage)
    {
        towerHealth = towerHealth - damage;
        RpcPlaySeigeSound();

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