﻿using System.Collections;
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
        return (CalculateTeamId(playerId) == TeamController.TEAM1)
            ? team1GameObject.GetComponent<TeamController>()
            : team2GameObject.GetComponent<TeamController>();
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
        GameState team1GameOverValue = (losingTeamId == TeamController.TEAM1) ? GameState.GAME_LOST : GameState.GAME_WON;
        GameState team2GameOverValue = (losingTeamId == TeamController.TEAM2) ? GameState.GAME_LOST : GameState.GAME_WON; ;

        team1GameObject.GetComponent<TeamController>().SetGameOver(team1GameOverValue);
		team2GameObject.GetComponent<TeamController>().SetGameOver(team2GameOverValue);

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
            for (int lane = 1; lane <= 5; lane++)
            {
                GameObject[] troops = GameObject.FindGameObjectsWithTag(string.Format(ENEMY_TAG, team, lane));
                for (int i = 0; i < troops.Length; i++) { Destroy(troops[i]); }
            }
        }
    }

    private void Update()
    {
        if (restart)
        {
            DestroyAllTroops();
            

            if (Input.GetKeyDown(KeyCode.R))
            {
                team1GameObject.GetComponent<TeamController>().SetGameOver(GameController.GameState.GAME_RESTART);
                team2GameObject.GetComponent<TeamController>().SetGameOver(GameController.GameState.GAME_RESTART);
                team1GameObject.GetComponent<TeamController>().Restart();
                team2GameObject.GetComponent<TeamController>().Restart();
            
                restart = false;
            }
        }
    }

}