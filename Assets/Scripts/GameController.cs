using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System;
using System.IO;

public class GameController : NetworkBehaviour
{

    public const string GAME_CONTROLLER_TAG = "GameController";
    public const string SPAWN_CONTROLLER_TAG = "SpawnController";
    public const string NPC_TAG = "NPC";

    public enum GameState { GAME_IN_PROGRESS, GAME_END, SAND_BOX }

    private GameState currentGameState;

    [SerializeField]
    private List<GameObject> spawnPoints;

    private TeamController teamController1;

    private TeamController teamController2;
    
    private Camera sceneCamera;

    private SpawnController spawnController;

    private float coinIncreaseTime;

    private float nextTroopSendSandBox = 0;

	  //Recording data
	  public int randomSeed;
	  private int testSeed = -995728914;
    public bool GetCurrentGameOver()
    {
        return currentGameState == GameState.GAME_END;
    }

    private bool screenshotEnabled = false;
    public bool ScreenshotEnabled
    {
        get
        {
            return screenshotEnabled;
        }
    }

    void Start()
    {
        currentGameState = GameState.GAME_IN_PROGRESS;
        coinIncreaseTime = Time.time + Params.COIN_INCREASE_INTERVAL;
        sceneCamera = Camera.main;
        sceneCamera.gameObject.SetActive(true);
        spawnController = GameObject.FindGameObjectWithTag(SPAWN_CONTROLLER_TAG).GetComponent<SpawnController>();
        teamController1 = GameObject.FindGameObjectWithTag(TeamController.TEAM_CONTROLLER_1_TAG).GetComponent<TeamController>();
        teamController2 = GameObject.FindGameObjectWithTag(TeamController.TEAM_CONTROLLER_2_TAG).GetComponent<TeamController>();
    }

    private void ToggleSandBox()
    {
        if(currentGameState == GameState.SAND_BOX)
        {
            RestartGame();
            currentGameState = GameState.GAME_IN_PROGRESS;
            teamController1.SetTeamResult(TeamController.TeamResult.UNDECIDED);
            teamController2.SetTeamResult(TeamController.TeamResult.UNDECIDED);
        }
        else
        {
            RestartGame();
            currentGameState = GameState.SAND_BOX;
            teamController1.SetTeamResult(TeamController.TeamResult.SAND_BOX);
            teamController2.SetTeamResult(TeamController.TeamResult.SAND_BOX);
        }
    }

	public void StartTests()
	{
		spawnController.SeedRandom (testSeed);
		for (int i = 1; i < 3; i++)
		{
			string playersTag = Player.PLAYER_TAG + " " + i;
			GameObject[] players = GameObject.FindGameObjectsWithTag(playersTag);
			for (int j = 0; j < players.Length; j++)
			{
				players [j].GetComponent<Player>().RpcStartTesting ();
			}
		}
	}

	public void StartRecording()
	{
		for (int i = 1; i < 3; i++)
		{
			string playersTag = Player.PLAYER_TAG + " " + i;
			GameObject[] players = GameObject.FindGameObjectsWithTag(playersTag);
			for (int j = 0; j < players.Length; j++)
			{
				players [j].GetComponent<Player>().RpcStartRecording ();
			}
		}
	}

	public void StopRecording()
	{
		for (int i = 1; i < 3; i++)
		{
			string playersTag = Player.PLAYER_TAG + " " + i;
			GameObject[] players = GameObject.FindGameObjectsWithTag(playersTag);
			for (int j = 0; j < players.Length; j++)
			{
				players [j].GetComponent<Player>().RpcStopRecording ();
			}
		}
	}

	public void SaveGameplay(string gameplay, int id)
	{
		String path = "exports/Recordings/Current/";
		String fullPath = String.Format("{0}{1}_player_{2}.json", path, randomSeed, id);
		File.WriteAllText(fullPath, gameplay);
		Debug.Log("File written");
	}

    private void RandomTroopSend()
    {
        System.Random rnd = new System.Random();
        int lane = rnd.Next(0, 4);
        spawnController.SpawnOffensiveTroop(0, lane, 1, 2);
        spawnController.SpawnOffensiveTroop(0, lane, 2, 1);
    }

    public void ToggleScreenShot()
    {
        if(currentGameState == GameState.SAND_BOX)
        {
            RestartGame();
            currentGameState = GameState.GAME_IN_PROGRESS;
            teamController1.SetTeamResult(TeamController.TeamResult.UNDECIDED);
            teamController2.SetTeamResult(TeamController.TeamResult.UNDECIDED);
        }
        else
        {
            RestartGame();
            currentGameState = GameState.SAND_BOX;
            teamController1.SetTeamResult(TeamController.TeamResult.SAND_BOX);
            teamController2.SetTeamResult(TeamController.TeamResult.SAND_BOX);
        }


    }

    private void RandomTroopSend()
    {
        System.Random rnd = new System.Random();
        int lane = rnd.Next(0, 4);
        spawnController.SpawnOffensiveTroop(0, lane, 1, 2);
        spawnController.SpawnOffensiveTroop(0, lane, 2, 1);
    }
        
    public void DeactiveScreenCamera()
    {
        sceneCamera.gameObject.SetActive(false);
    }

    public int GetMyTeamControllerId(int playerId)
    {
        return GetMyTeamController(playerId).Id;
    }

    public int GetOpponentsTeamControllerId(int playerId)
    {
        return GetOpponentsTeamController(playerId).Id;
    }

    public TeamController GetMyTeamController(int playerId)
    {
        return (CalculateTeamId(playerId) == TeamController.TEAM1)
            ? teamController1
            : teamController2;
    }

    public TeamController GetOpponentsTeamController(int playerId)
    {
        return (CalculateTeamId(playerId) == TeamController.TEAM1)
            ? teamController2
            : teamController1;
    }

    public TeamController GetTeamControllerById(int teamId)
    {
        return (teamId == TeamController.TEAM1)
            ? teamController1
            : teamController2;
    }

    public Transform GetPlayerTransform(int playerId)
    {
        return spawnPoints[playerId].transform;
    }

    private int CalculateTeamId(int playerId)
    {
        return (playerId % 2 == 0) ? TeamController.TEAM1 : TeamController.TEAM2;
    }

	public void GameIsOver(int losingTeamId) {
        TeamController.TeamResult team1Result = (losingTeamId == TeamController.TEAM1) ? TeamController.TeamResult.LOST : TeamController.TeamResult.WON;
        TeamController.TeamResult team2Result = (losingTeamId == TeamController.TEAM2) ? TeamController.TeamResult.LOST : TeamController.TeamResult.WON;

        teamController1.SetTeamResult(team1Result);
        teamController2.SetTeamResult(team2Result);

        if (!screenshotEnabled)
        {
            teamController1.SetTeamAIEnabled(false);
            teamController2.SetTeamAIEnabled(false);
        }

        currentGameState = GameState.GAME_END;
    }

    private void DestroyAllTroops()
    {
        GameObject[] allTroops = GameObject.FindGameObjectsWithTag(NPC_TAG);
        for (int i = 0; i < allTroops.Length; i++)
        {
            NetworkServer.Destroy(allTroops[i]);
        }
    }

    private void CheckTime()
    {
        if (Time.time > coinIncreaseTime)
        {
            coinIncreaseTime = Time.time + Params.COIN_INCREASE_INTERVAL;
            teamController1.IncreaseCoinPerInterval(Params.COIN_BOOST);
			teamController2.IncreaseCoinPerInterval(Params.COIN_BOOST);
        }
    }

    private void RestartGame()
    {
        teamController1.SetTeamResult(TeamController.TeamResult.UNDECIDED);
        teamController2.SetTeamResult(TeamController.TeamResult.UNDECIDED);

        teamController1.Restart();
        teamController2.Restart();
        DestroyAllTroops();
        coinIncreaseTime = Time.time + Params.COIN_INCREASE_INTERVAL;
        currentGameState = GameState.GAME_IN_PROGRESS;
    }

    private void Update()
    {
        CheckTime();
        if (currentGameState == GameState.GAME_END)
        {
            DestroyAllTroops();
            if (screenshotEnabled)
            {
                RestartGame();
            }
               
        }
        if(currentGameState == GameState.SAND_BOX)
        {
            if(Time.time > nextTroopSendSandBox)
            {
                nextTroopSendSandBox = Time.time + Params.TROOP_SEND_INTERVAL_SAND_BOX;
                RandomTroopSend();
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleSandBox();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            teamController1.ToggleScreenShotEnabled();
            teamController2.ToggleScreenShotEnabled();
            screenshotEnabled = !screenshotEnabled;
        }
    }

}