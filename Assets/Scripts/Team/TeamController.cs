using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;


public class TeamController : NetworkBehaviour
{
    public const string TEAM_CONTROLLER_1_TAG = "TeamController1";
    public const string TEAM_CONTROLLER_2_TAG = "TeamController2";

    public const int TEAM1 = 1;
    public const int TEAM2 = 2;

    public enum TeamResult { UNDECIDED, LOST, WON, SAND_BOX }

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

    private static bool NEURAL_NET_ACTIVE = true;

    private bool training = false;

    bool threadRunning;
    Thread thread;

    private float nextScreenShot = 0;

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

    public void ToggleScreenShotEnabled()
    {
        training = !training;
    }

    private Socket sender;
    
    void Start()
    {
        nextSendTroopAlert = Time.time + Params.SEND_TROOP_ALERT_DELAY;
        result = TeamResult.UNDECIDED;
		towerHealth = Params.STARTING_TOWER_HEALTH;
        endOfCoolDown = Time.time;
        currentTime = Time.time;
        lastActivePlayerId = -1;

        if (NEURAL_NET_ACTIVE && id == TEAM1)
        {
            StartClient();
        }
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
        if(workDone)
        {
            print("end of thread: " + Time.time);
            workDone = false;
        }
        if(training && Time.time > nextScreenShot && result == TeamResult.UNDECIDED)
        {
            TakeTeamTrainScreenShot();
            nextScreenShot = Time.time + Params.TRAIN_SCREENSHOT_DELAY;
        }
    }

    private void TakeTeamScreenShotRealTime()
    {
        GameObject[] players = FindPlayersInTeam();
        this.GetComponent<HiResScreenShot>().TakeScreenShotsRealTime(players, id);
    }

    private void TakeTeamTrainScreenShot()
    {
        GameObject[] players = FindPlayersInTeam();
    
        int[] dangers = new int[players.Length];
        for (int lane = 0; lane < players.Length; lane++)
        {
            dangers[lane] = GetLaneDangerIndex(lane);
        }
        this.GetComponent<HiResScreenShot>().TakeScreenShotsTrain(players, dangers);
        
    }

    private GameObject[] FindPlayersInTeam()
    {
        string playersTag = Player.PLAYER_TAG + " " + id;

        GameObject[] players = GameObject.FindGameObjectsWithTag(playersTag);

        return players;
    }

    private void SendTroopAlert()
    {

        if (Time.time > nextSendTroopAlert && sendTroopAlerting == false)
        {
            playSendTroopAnim = true;
            sendTroopAlerting = true;

            GameObject[] players = FindPlayersInTeam();

            int length = players.Length;

            if(length > 0)
            {
                int playerIndex = UnityEngine.Random.Range(0, length);

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

    public void ResetSandboxModeAlert()
    {

        GameObject[] players = FindPlayersInTeam();

        foreach(GameObject player in players)
        {
            Player p = player.GetComponent<Player>();
            p.RpcResetSandboxAlert();
        }

    }

    public void SandboxAlert()
    {

        GameObject[] players = FindPlayersInTeam();

        int length = players.Length;
        foreach(GameObject player in players)
        {
            Player p = player.GetComponent<Player>();
            p.RpcSetSandboxAlert();
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

    bool workDone = false;

    private void CheckChangeAI()
    {
        if (NEURAL_NET_ACTIVE)
        {
            if (Time.time > timeToScreenCheck)
            {
                //print("time to sc: " + timeToScreenCheck);

                TakeTeamScreenShotRealTime();

                print("start of thread: " + Time.time);


                Thread thread = new Thread(NeuralAIThread);
                thread.Start();

                timeToScreenCheck = Time.time + maxTimeAtScreen;
            }
        }
        else
        {
            if (Time.time > timeToScreenCheck)
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
    }

    private void UpdateAIActive()
    {
        var watch = new Stopwatch();
        watch.Start();

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

        watch.Stop();
        print("end of thread");
        print("elapsed: " + watch.Elapsed);
    }


    private void StartClient()
    {
        // Connect to a remote device.  
        try
        {
            // Establish the remote endpoint for the socket.  
            // This example uses port 11000 on the local computer.  
            IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = ipHostInfo.AddressList[1];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 9999);

            // Create a TCP/IP  socket.  
            sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            // Connect the socket to the remote endpoint. Catch any errors.  
            try
            {
                sender.Connect(remoteEP);
                print(string.Format("Socket connected to {0}", sender.RemoteEndPoint.ToString()));

            }
            catch (ArgumentNullException ane)
            {
                print(string.Format("ArgumentNullException : {0}", ane.ToString()));
            }
            catch (SocketException se)
            {
                print(string.Format("SocketException : {0}", se.ToString()));
            }
            catch (Exception e)
            {
                print(string.Format("Unexpected exception : {0}", e.ToString()));
            }

        }
        catch (Exception e)
        {
            print(e.ToString());
        }
    }



    private void NeuralAIThread()
    {
        // Data buffer for incoming data.  
        byte[] bytes = new byte[1024];

        workDone = false;

        // Encode the data string into a byte array.
        byte[] msg = Encoding.ASCII.GetBytes("run");

        // Send the data through the socket.
        int bytesSent = sender.Send(msg);

        // Receive the response from the remote device.  
        int bytesRec = sender.Receive(bytes);
        string output = Encoding.ASCII.GetString(bytes, 0, bytesRec);

        print(output);

        float[] dangers = GetDangerScores(output);
        UpdateAIActiveNeural(dangers);
        workDone = true;

    }

    public float[] GetDangerScores(string output)
    {
        string[] scoresString = output.Split(',');

        float[] scores = new float[5];

        for (int i = 0; i < 5; i++)
        {
            scores[i] = float.Parse(scoresString[i]);
        }

        return scores;
    }

    private void UpdateAIActiveNeural(float[] dangers)
    {
        int aiLane = 0;
        int aiLane2 = 0;
        float maxDanger = 0;
        float maxDanger2 = 0;
        for (int lane = 0; lane < 5; lane++)
        {
            float index = dangers[lane];
            if (index > maxDanger)
            {
                maxDanger = index;
                aiLane = lane;
            }
            else
            {
                if (index > maxDanger2)
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
        if (result != TeamResult.SAND_BOX)
        {
            towerHealth = towerHealth - damage;
        }
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