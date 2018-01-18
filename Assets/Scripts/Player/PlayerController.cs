using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private float lookSensitivity = 3f;

    private PlayerMotor motor;

    public TeamController teamController;

	[SerializeField]
	public GameObject prefabT1;

    [SerializeField]
    public GameObject prefabT2;

    [SerializeField]
    public GameObject[] spawnLocations;

    public GameObject target;

	public GameObject troopSpawn;

    [SyncVar]
    public int spawnId;

    [SyncVar]
    public int teamNum;

    public GameObject spawnTarget;

    void Start()
    {
        teamController = GameObject.Find("TeamController").GetComponent<TeamController>();
        motor = GetComponent<PlayerMotor>();
        CmdSetUp();
        //this is needed otherwise it does not seem to move client to correct position
        spawnTarget = spawnLocations[spawnId];
		this.transform.position = spawnTarget.transform.position;
    }

    void Update()
    {
        UpdateMovement();
        if (Input.GetKeyDown(KeyCode.A))
        {
            int currency = teamController.buy(10);
            this.GetComponentInChildren<Text>().text = "Coin: " + currency.ToString();
        }
		else if (Input.GetKeyDown(KeyCode.S) && isLocalPlayer) {
			CmdRequestTroopSpawn ();
		}

    }

    public void InitialisePlayerInfo(int spawnId, int teamNum,GameObject troopSpawn, GameObject target)
    {
        this.spawnId = spawnId;
        this.teamNum = teamNum;
        this.troopSpawn = troopSpawn;
        this.target = target;
    }

    [Command]
    private void CmdSetUp()
    {
        teamController.SetPlayerInfo(this.gameObject);
    }

    private void UpdateMovement()
    {
        float yRot = Input.GetAxisRaw("Mouse X");
        Vector3 x = new Vector3(0f, yRot, 0f) * lookSensitivity;
        

        float xRot = Input.GetAxisRaw("Mouse Y");
        Vector3 y = new Vector3(xRot, 0f, 0f) * lookSensitivity;

        motor.Rotate(x, y);
    }

	[Command]
	public void CmdRequestTroopSpawn() {
        teamController.SendTroop(this.gameObject);
	}

    

}


