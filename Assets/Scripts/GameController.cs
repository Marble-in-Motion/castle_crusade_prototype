using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameController : NetworkBehaviour
{

    public const string GAME_CONTROLLER_TAG = "GameController";

    [SerializeField]
    private List<GameObject> spawnPoints;

    [SerializeField]
    private GameObject team1GameObject;

    [SerializeField]
    private GameObject team2GameObject;

    private Camera sceneCamera;

	[SyncVar]
	private int gameOver;


    // Use this for initialization
    void Start()
    {
        sceneCamera = Camera.main;
        sceneCamera.gameObject.SetActive(true);
		gameOver = 0;

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
        Debug.Log(player.GetId());
        player.transform.position = spawnPoints[player.GetId()].transform.position;
        player.transform.rotation = spawnPoints[player.GetId()].transform.rotation;
    }

    private int CalculateTeamId(int playerId)
    {
        return (playerId % 2 == 0) ? TeamController.TEAM1 : TeamController.TEAM2;
    }

	public void GameIsOver(int losingTeamId) {
		Debug.Log("GAME IS OVER");
		gameOver = 1;

        int team1GameOverValue;
        int team2GameOverValue;
        if (losingTeamId == 1)
        {
            Debug.Log("1 lost");
            team1GameOverValue = 1;
            team2GameOverValue = 2;
        }
        else
        {
            team1GameOverValue = 2;
            team2GameOverValue = 1;
        }
		team1GameObject.GetComponent<TeamController>().SetGameOver(team1GameOverValue);
		team2GameObject.GetComponent<TeamController>().SetGameOver(team2GameOverValue);
	}

}