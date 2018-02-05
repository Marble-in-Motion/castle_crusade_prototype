using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameController : NetworkBehaviour
{

    public const string GAME_CONTROLLER_TAG = "GameController";

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



	//[SyncVar]
	//private int gameOver;


    // Use this for initialization
    void Start()
    {
        sceneCamera = Camera.main;
        sceneCamera.gameObject.SetActive(true);
		//gameOver = 0;

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
		//gameOver = 1;

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
        //Debug.Log("team1 value = " + team1GameOverValue);
		team2GameObject.GetComponent<TeamController>().SetGameOver(team2GameOverValue);
        //Debug.Log("team2 value = " + team2GameOverValue);

        //StartCoroutine(RestartGame());

    }

    //IEnumerator RestartGame()
    //{
    //    // The Application loads the Scene in the background at the same time as the current Scene.
    //    //This is particularly good for creating loading screens. You could also load the Scene by build //number.
    //    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("gamescene");

    //    //Wait until the last operation fully loads to return anything
    //    while (!asyncLoad.isDone)
    //    {
    //        yield return null;
    //    }
    //}

}