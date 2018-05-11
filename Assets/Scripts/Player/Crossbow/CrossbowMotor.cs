using System.Collections.Generic;
using UnityEngine;

/**
 * Controller for crossbow motor functions
 **/
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

    public List<Vector3> defaultTargets = new List<Vector3>();

    void Start()
    {
        activePath = 1;
    }

    public void ResetAim()
    {
        activePath = 1;
        transform.LookAt(defaultTargets[activePath]);

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
            Vector3 target = nearestTroop.transform.position;
            target.y = target.y + (nearestTroop.transform.lossyScale.y / 3);
            transform.LookAt(target);
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
