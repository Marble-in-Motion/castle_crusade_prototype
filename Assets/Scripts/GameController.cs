using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameController : NetworkBehaviour
{

    public const string GAME_CONTROLLER_TAG = "GameController";
    public const string ENEMY_TAG = "NPCT{0}L{1}";

    public enum GameState { GAME_RESTART, GAME_IN_PROGRESS, GAME_LOST, GAME_WON }

    [SerializeField]
    private List<GameObject> spawnPoints;

    private TeamController teamController1;

    private TeamController teamController2;
    
    private Camera sceneCamera;

    private bool restart;

    private float coinIncreaseTime;

    void Start()
    {
        coinIncreaseTime = Time.time + Params.COIN_INCREASE_INTERVAL;
        sceneCamera = Camera.main;
        sceneCamera.gameObject.SetActive(true);
        restart = false;

        teamController1 = GameObject.FindGameObjectWithTag(TeamController.TEAM_CONTROLLER_1_TAG).GetComponent<TeamController>();
        teamController2 = GameObject.FindGameObjectWithTag(TeamController.TEAM_CONTROLLER_2_TAG).GetComponent<TeamController>();
    }
        
    public void DeactiveScreenCamera()
    {
        sceneCamera.gameObject.SetActive(false);
    }

    public int GetMyTeamControllerId(int playerId)
    {
        return GetMyTeamController(playerId).GetId();
    }

    public int GetOpponentsTeamControllerId(int playerId)
    {
        return GetOpponentsTeamController(playerId).GetId();
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
        GameState team1GameOverValue = (losingTeamId == TeamController.TEAM1) ? GameState.GAME_LOST : GameState.GAME_WON;
        GameState team2GameOverValue = (losingTeamId == TeamController.TEAM2) ? GameState.GAME_LOST : GameState.GAME_WON;

        teamController1.SetGameOver(team1GameOverValue);
		teamController2.SetGameOver(team2GameOverValue);

        GameRestart();
    }

    public void GameRestart()
    {
        restart = true;
    }

    private void DestroyAllTroops()
    {
        for (int team = 1; team <= 2; team++)
        {
            for (int lane = 0; lane <= 4; lane++)
            {
                GameObject[] troops = GameObject.FindGameObjectsWithTag(string.Format(ENEMY_TAG, team, lane));
                for (int i = 0; i < troops.Length; i++) { NetworkServer.Destroy(troops[i]); }
            }
        }
    }

    private void CheckTime()
    {
        if (!isServer) return;

        if (Time.time > coinIncreaseTime)
        {
            coinIncreaseTime = Time.time + Params.COIN_INCREASE_INTERVAL;
            teamController1.CmdIncreaseCoinPerInterval(Params.COIN_BOOST);
			teamController2.CmdIncreaseCoinPerInterval(Params.COIN_BOOST);
        }
    }

    private void Update()
    {
        CheckTime();   
        if (restart)
        {
            DestroyAllTroops();
            
            if (Input.GetKeyDown(KeyCode.R))
            {
                teamController1.SetGameOver(GameState.GAME_RESTART);
                teamController2.SetGameOver(GameState.GAME_RESTART);
                teamController1.Restart();
                teamController2.Restart();
                DestroyAllTroops();
                teamController1.CmdResetCoinPerInterval();
                teamController2.CmdResetCoinPerInterval();
                coinIncreaseTime = Time.time + Params.COIN_INCREASE_INTERVAL;
                restart = false;
            }
        }
    }

}