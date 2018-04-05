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
        List<GameObject> troopsInLane = GetComponentInParent<Player>().FindEnemyTroopsInLane();
        GameObject nearestTroop = FindNearestTroopInPath(troopsInLane);
        if (nearestTroop != null)
        {
            transform.LookAt(nearestTroop.transform.position);
        }
    }

    GameObject FindNearestTroopInPath(List<GameObject> troopsInLane)
    {
        float minDist = float.MaxValue;
        GameObject nearestTroop = null;

        foreach (GameObject troop in troopsInLane)
        {
            if (troop.GetComponent<AIController>().Path == activePath && troop.GetComponent<NPCHealth>().IsAlive())
            {
                float tempDistance = Vector3.Distance(troop.transform.position, transform.position);
                if (tempDistance < minDist)
                {
                    minDist = tempDistance;
                    nearestTroop = troop;
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

    // THIS HAS BEEN DUPLICATED - ADAM FIX PLEASE
    public GameObject AIFindTarget(List<GameObject> troopsInLane)
    {
        float minDist = float.MaxValue;
        GameObject nearestTroop = null;

        foreach (GameObject troop in troopsInLane)
        {
            if (troop.GetComponent<NPCHealth>().IsAlive())
            {
                float tempDistance = Vector3.Distance(troop.transform.position, transform.position);
                if (tempDistance < minDist)
                {
                    minDist = tempDistance;
                    nearestTroop = troop;
                }
            }
        }
        return nearestTroop;

    }

}
