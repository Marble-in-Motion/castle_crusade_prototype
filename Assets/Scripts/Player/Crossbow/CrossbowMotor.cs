using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
public class CrossbowMotor : MonoBehaviour
{
    private int activePath;
    public int ActivePath
    {
        get
        {
            return activePath;
        }
    }

    private List<Vector3> defaultTargets = new List<Vector3>();

    void Start()
    {
        activePath = 1;
    }

    void Update()
    {
        LookAtTroop();
    }

    public void SetDefaultTarget(Vector3 target)
    {
        defaultTargets.Add(target);
    }

    private void LookAtTroop()
    {
        Debug.Log("activePath: " + activePath);
        GameObject[] troopsInLane = GetComponentInParent<Player>().FindEnemyTroopsInLane();
        Debug.Log("troop count: " + troopsInLane.Length);
        GameObject nearestTroop = FindNearestTroopInPath(troopsInLane);
        if (nearestTroop != null)
        {
            transform.LookAt(nearestTroop.transform.position);
        }
    }

    GameObject FindNearestTroopInPath(GameObject[] troopsInLane)
    {
        float minDist = float.MaxValue;
        GameObject nearestTroop = null;

        for (int i = 0; i < troopsInLane.Length; i++)
        {
            if (troopsInLane[i].GetComponent<AIController>().Path == activePath && troopsInLane[i].GetComponent<NPCHealth>().IsAlive())
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


    // AI IMPLEMENTATION ##############################

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

}
