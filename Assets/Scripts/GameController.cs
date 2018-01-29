﻿using System.Collections;
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

    // Update is called once per frame
    void Update()
    {

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
    }

    private int CalculateTeamId(int playerId)
    {
        return (playerId % 2 == 0) ? TeamController.TEAM1 : TeamController.TEAM2;
    }

	public void GameIsOver() {
		Debug.Log ("GAME IS OVER");
		gameOver = 1;
		team1GameObject.GetComponent<TeamController> ().SetGameOver ();
		team2GameObject.GetComponent<TeamController> ().SetGameOver ();
		//tell other team controller the game is over.
	}

}