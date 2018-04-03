using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CrossbowMotor : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    private Vector3 mousePos;

    private int activePath;
    public int ActivePath
    {
        get
        {
            return activePath;
        }
    }

    private SpawnController spawnController;

    private List<Vector3> defaultTargets = new List<Vector3>();

    void Start()
    {
        spawnController = GameObject.FindGameObjectWithTag(SpawnController.SPAWN_CONTROLLER_TAG).GetComponent<SpawnController>();
        activePath = 1;
        SetDefaultTargets();
    }

    void Update()
    {
        LookAtTroop();
    }

    private void SetDefaultTargets()
    {
        Player player = GetComponentInParent<Player>();
        for (int i = 0; i <= 2; i++)
        {
            defaultTargets.Add(spawnController.calculateDefaultSpawn(i, player.LaneId, player.OpponentsTeamId));
        }
    }

    private void LookAtTroop()
    {
        GameObject[] troopsInLane = GetComponentInParent<Player>().FindEnemyTroopsInLane();
        GameObject nearestTroop = FindNearestTroopInPath(troopsInLane);
        if (nearestTroop != null)
        {
            transform.LookAt(nearestTroop.transform.position);
        }

    }

    public GameObject AIFindTarget(GameObject[] troopsInLane)
    {
        float minDist = float.MaxValue;
        GameObject nearestTroop = null;
        
        for (int i = 0; i < troopsInLane.Length; i++)
        {
            if (troopsInLane[i].GetComponent<NPCHealth>().IsAlive())
            {
                GameObject tempTroop = troopsInLane[i];
                float tempDistance = Vector3.Distance(tempTroop.transform.position, transform.position);
                if (tempDistance < minDist)
                {
                    minDist = tempDistance;
                    nearestTroop = tempTroop;
                }
            }
        }
        return nearestTroop;
       
    }

    GameObject FindNearestTroopInPath(GameObject[] troopsInLane)
    {
        float minDist = float.MaxValue;
        GameObject nearestTroop = null;

        for (int i = 0; i < troopsInLane.Length; i++)
        {
            if (troopsInLane[i].GetComponent<AIController>().GetPath() == activePath && troopsInLane[i].GetComponent<NPCHealth>().IsAlive())
            {
                GameObject tempTroop = troopsInLane[i];
                float tempDistance = Vector3.Distance(tempTroop.transform.position, transform.position);
                if (tempDistance < minDist)
                {
                    minDist = tempDistance;
                    nearestTroop = tempTroop;
                }
            }
        }
        return nearestTroop;
    }

    public void MoveRight()
    {
        if (activePath < 2)
        {
            activePath += 1;
            transform.LookAt(defaultTargets[activePath]);
        }
    }

    public void MoveLeft()
    {
        if (activePath > 0)
        {
            activePath -= 1;
            transform.LookAt(defaultTargets[activePath]);
        }
    }

}
