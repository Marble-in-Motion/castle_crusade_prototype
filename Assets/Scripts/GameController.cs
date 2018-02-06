using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameController : NetworkBehaviour
{

    public const string GAME_CONTROLLER_TAG = "GameController";
    public const string ENEMY_TAG = "NPCT1";

    public const int gameRestart = -1;
    public const int gameInProgress = 0;
    public const int gameLost = 1;
    public const int gameWon = 2;

    [SerializeField]
    private List<GameObject> spawnPoints;

    [SerializeField]
    private GameObject team1GameObject;

    [SerializeField]
    private GameObject team2GameObject;

    private Camera sceneCamera;

    private bool restart;


    // Use this for initialization
    void Start()
    {
        sceneCamera = Camera.main;
        sceneCamera.gameObject.SetActive(true);
        //gameOver = 0;
        restart = false;

    }

    public void InitialisePlayer(Player player)
    {
        MovePlayerToSpawn(player);
        sceneCamera.gameObject.SetActive(false);
    }

    public TeamController GetTeamController(int playerId)
    {
        if (CalculateTeamId(playerId) == TeamController.TEAM1)
        {
            return team1GameObject.GetComponent<TeamController>();
        }
        else
        {
            return team2GameObject.GetComponent<TeamController>(); ;
        }
    }

    private void MovePlayerToSpawn(Player player)
    {
        player.transform.position = spawnPoints[player.GetId()].transform.position;
        player.transform.rotation = spawnPoints[player.GetId()].transform.rotation;
    }

    private int CalculateTeamId(int playerId)
    {
        return (playerId % 2 == 0) ? TeamController.TEAM1 : TeamController.TEAM2;
    }

	public void GameIsOver(int losingTeamId) {
        int team1GameOverValue;
        int team2GameOverValue;

        if (losingTeamId == TeamController.TEAM1)
        {
            team1GameOverValue = gameLost;
            team2GameOverValue = gameWon;
        }
        else
        {
            team1GameOverValue = gameWon;
            team2GameOverValue = gameLost;
        }
		team1GameObject.GetComponent<TeamController>().SetGameOver(team1GameOverValue);
		team2GameObject.GetComponent<TeamController>().SetGameOver(team2GameOverValue);

        GameRestart();

    }

    public void GameRestart()
    {
        restart = true;
    }

    private void Update()
    {
        if (restart)
        {
            GameObject[] troops = GameObject.FindGameObjectsWithTag(ENEMY_TAG);
            for (int i = 0; i < troops.Length; i++) { Destroy(troops[i]); }

            if (Input.GetKeyDown(KeyCode.R))
            {
                
                team1GameObject.GetComponent<TeamController>().SetGameOver(gameRestart);
                team2GameObject.GetComponent<TeamController>().SetGameOver(gameRestart);
                team1GameObject.GetComponent<TeamController>().Restart();
                team2GameObject.GetComponent<TeamController>().Restart();
            
                restart = false;
            }
        }
    }

}