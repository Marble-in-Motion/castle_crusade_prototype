using Assets.Scripts.Team;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameController : NetworkBehaviour {

    public const string GAME_CONTROLLER_TAG = "GameController";

    [SerializeField]
    private List<GameObject> spawnPoints;

    private ITeamController team1;

    private ITeamController team2;

    private Camera sceneCamera;


    // Use this for initialization
    void Start() {
        sceneCamera = Camera.main;
        sceneCamera.gameObject.SetActive(true);

        team1 = new TeamController(TeamController.TEAM1);
        team2 = new TeamController(TeamController.TEAM2);

        // playerSpawns - get them bruv
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void InitialisePlayer(Player player)
    {
        AddPlayerToTeam(player);
        MovePlayerToSpawn(player);
        sceneCamera.gameObject.SetActive(false);
    }

    public ITeamController GetTeamController(int playerId) {
        if (team1.HasPlayer(playerId))
        {
            return team1;
        } else if (team2.HasPlayer(playerId))
        {
            return team2;
        }
        throw new System.Exception();
    }

    private void AddPlayerToTeam(Player player)
    {
        int playerId = player.GetId();
        ITeamController team = (playerId % 2 == 0) ? team1 : team2;
        team.AddPlayer(playerId);
    }

    private void MovePlayerToSpawn(Player player)
    {
        Debug.Log(player.GetId());
        player.transform.position = spawnPoints[player.GetId()].transform.position;
    }

   
}
