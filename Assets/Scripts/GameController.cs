using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameController : NetworkBehaviour
{

    public const string GAME_CONTROLLER_TAG = "GameController";
    public const string NPC_TAG = "NPC";

    public enum GameState { GAME_IN_PROGRESS, GAME_END, SAND_BOX }

    private GameState currentGameState;

    [SerializeField]
    private List<GameObject> spawnPoints;

    private TeamController teamController1;

    private TeamController teamController2;
    
    private Camera sceneCamera;

    private float coinIncreaseTime;

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

    public void SetTeamAIEnabled(bool state)
    {
        screenshotEnabled = state;
    }

    void Start()
    {
        currentGameState = GameState.GAME_IN_PROGRESS;
        coinIncreaseTime = Time.time + Params.COIN_INCREASE_INTERVAL;
        sceneCamera = Camera.main;
        sceneCamera.gameObject.SetActive(true);

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

    public void ToggleScreenShot()
    {
        
        screenshotEnabled = !screenshotEnabled;
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
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleSandBox();
        }
    }

}